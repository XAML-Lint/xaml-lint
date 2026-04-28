using System.Diagnostics;

namespace XamlLint.Cli.Update;

internal sealed class DefaultProcessRunner : IProcessRunner
{
    public int Run(string fileName, string[] arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
        };
        foreach (var arg in arguments)
            psi.ArgumentList.Add(arg);

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException($"Failed to start process: {fileName}");
        process.WaitForExit();
        return process.ExitCode;
    }
}
