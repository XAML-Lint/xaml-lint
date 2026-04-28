using System.Net;
using System.Net.Http;
using XamlLint.Cli.Update;

namespace XamlLint.Cli.Tests.Update;

public sealed class NuGetVersionProbeTest
{
    [Fact]
    public async Task Returns_highest_stable_version_from_versions_array()
    {
        using var http = HttpFor(HttpStatusCode.OK,
            """{"versions":["0.1.0","1.0.0","1.1.0","1.2.0"]}""");

        var result = await NuGetVersionProbe.GetLatestStableAsync(http, TestContext.Current.CancellationToken);

        result.LatestVersion.Should().Be("1.2.0");
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task Filters_out_prerelease_versions()
    {
        using var http = HttpFor(HttpStatusCode.OK,
            """{"versions":["1.0.0","1.1.0","1.2.0-alpha.1","1.2.0-rc.1"]}""");

        var result = await NuGetVersionProbe.GetLatestStableAsync(http, TestContext.Current.CancellationToken);

        result.LatestVersion.Should().Be("1.1.0");
    }

    [Fact]
    public async Task Returns_error_on_non_success_status()
    {
        using var http = HttpFor(HttpStatusCode.NotFound, "");

        var result = await NuGetVersionProbe.GetLatestStableAsync(http, TestContext.Current.CancellationToken);

        result.LatestVersion.Should().BeNull();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Returns_error_on_transport_failure()
    {
        var handler = new ThrowingHandler(new HttpRequestException("boom"));
        using var http = new HttpClient(handler);

        var result = await NuGetVersionProbe.GetLatestStableAsync(http, TestContext.Current.CancellationToken);

        result.LatestVersion.Should().BeNull();
        result.Error.Should().Contain("boom");
    }

    [Fact]
    public async Task Returns_error_when_versions_array_is_empty()
    {
        using var http = HttpFor(HttpStatusCode.OK, """{"versions":[]}""");

        var result = await NuGetVersionProbe.GetLatestStableAsync(http, TestContext.Current.CancellationToken);

        result.LatestVersion.Should().BeNull();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Returns_error_when_only_prereleases_exist()
    {
        using var http = HttpFor(HttpStatusCode.OK,
            """{"versions":["1.0.0-alpha","1.0.0-beta"]}""");

        var result = await NuGetVersionProbe.GetLatestStableAsync(http, TestContext.Current.CancellationToken);

        result.LatestVersion.Should().BeNull();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    private static HttpClient HttpFor(HttpStatusCode status, string body)
    {
        var handler = new StubHandler(req => new HttpResponseMessage(status)
        {
            Content = new StringContent(body),
        });
        return new HttpClient(handler);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;
        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => _respond = respond;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(_respond(request));
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        private readonly Exception _ex;
        public ThrowingHandler(Exception ex) => _ex = ex;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => throw _ex;
    }
}
