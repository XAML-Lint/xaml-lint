using System.Text.Json;

namespace XamlLint.Cli.Update;

internal readonly record struct NuGetProbeResult(string? LatestVersion, string? Error);

internal static class NuGetVersionProbe
{
    private const string FlatContainerUrl =
        "https://api.nuget.org/v3-flatcontainer/xaml-lint/index.json";

    public static async Task<NuGetProbeResult> GetLatestStableAsync(
        HttpClient http,
        CancellationToken ct = default)
    {
        try
        {
            using var response = await http.GetAsync(FlatContainerUrl, ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return new NuGetProbeResult(null, $"NuGet returned HTTP {(int)response.StatusCode}.");

            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);

            if (!doc.RootElement.TryGetProperty("versions", out var versionsElem) ||
                versionsElem.ValueKind != JsonValueKind.Array)
            {
                return new NuGetProbeResult(null, "NuGet response did not contain a 'versions' array.");
            }

            var stable = new List<Version>();
            foreach (var v in versionsElem.EnumerateArray())
            {
                var raw = v.GetString();
                if (string.IsNullOrEmpty(raw)) continue;
                if (raw.Contains('-')) continue; // pre-release
                if (Version.TryParse(raw, out var parsed)) stable.Add(parsed);
            }

            if (stable.Count == 0)
                return new NuGetProbeResult(null, "No stable versions found on NuGet.");

            stable.Sort();
            var latest = stable[^1];
            return new NuGetProbeResult(FormatVersion(latest), null);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            return new NuGetProbeResult(null, $"Failed to query NuGet: {ex.Message}");
        }
    }

    private static string FormatVersion(Version v) =>
        v.Build < 0 ? $"{v.Major}.{v.Minor}" : $"{v.Major}.{v.Minor}.{v.Build}";
}
