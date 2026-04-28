namespace XamlLint.Cli.Update;

internal interface IProcessRunner
{
    /// <summary>
    /// Runs <paramref name="fileName"/> with the given arguments. Returns the child exit code.
    /// Stdout and stderr are inherited so the caller's user sees live output.
    /// </summary>
    /// <exception cref="System.ComponentModel.Win32Exception">If <paramref name="fileName"/> is not on PATH.</exception>
    int Run(string fileName, string[] arguments);
}
