using XamlLint.Cli.Commands;
using XamlLint.Cli.Update;

namespace XamlLint.Cli.Tests.Commands;

public sealed class UpdateCommandTest
{
    [Fact]
    public void Bare_invocation_parses_cleanly()
    {
        var cmd = UpdateCommand.Build();
        var result = cmd.Parse(Array.Empty<string>());

        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Check_flag_parses_cleanly()
    {
        var cmd = UpdateCommand.Build();
        var result = cmd.Parse(new[] { "--check" });

        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Unknown_argument_is_a_parse_error()
    {
        var cmd = UpdateCommand.Build();
        var result = cmd.Parse(new[] { "--bogus" });

        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Handle_reports_up_to_date_when_versions_match()
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var http = HttpReturning($$"""{"versions":["{{ToolVersion.Current}}"]}""");
        var runner = new RecordingRunner();

        var exit = UpdateCommand.Handle(
            new UpdateOptions(CheckOnly: false),
            stdout, stderr, () => http, runner);

        exit.Should().Be(0);
        stdout.ToString().Should().Contain("up to date");
        runner.Calls.Should().BeEmpty();
    }

    [Fact]
    public void Handle_runs_dotnet_tool_update_when_newer_available()
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var http = HttpReturning("""{"versions":["1.0.0","99.0.0"]}""");
        var runner = new RecordingRunner { ExitCode = 0 };

        var exit = UpdateCommand.Handle(
            new UpdateOptions(CheckOnly: false),
            stdout, stderr, () => http, runner);

        exit.Should().Be(0);
        stdout.ToString().Should().Contain("New version available");
        stdout.ToString().Should().Contain("Successfully updated to 99.0.0");
        runner.Calls.Should().ContainSingle()
            .Which.Should().BeEquivalentTo((FileName: "dotnet", Args: new[] { "tool", "update", "-g", "xaml-lint" }));
    }

    [Fact]
    public void Handle_check_flag_does_not_invoke_dotnet()
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var http = HttpReturning("""{"versions":["1.0.0","99.0.0"]}""");
        var runner = new RecordingRunner();

        var exit = UpdateCommand.Handle(
            new UpdateOptions(CheckOnly: true),
            stdout, stderr, () => http, runner);

        exit.Should().Be(0);
        stdout.ToString().Should().Contain("New version available");
        stdout.ToString().Should().Contain("dotnet tool update -g xaml-lint");
        runner.Calls.Should().BeEmpty();
    }

    [Fact]
    public void Handle_reports_probe_failure_and_exits_one()
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var http = HttpFailing();
        var runner = new RecordingRunner();

        var exit = UpdateCommand.Handle(
            new UpdateOptions(CheckOnly: false),
            stdout, stderr, () => http, runner);

        exit.Should().Be(1);
        stderr.ToString().Should().Contain("Failed to check for updates");
        stderr.ToString().Should().Contain("dotnet tool update -g xaml-lint");
        runner.Calls.Should().BeEmpty();
    }

    [Fact]
    public void Handle_falls_back_when_dotnet_tool_update_returns_nonzero()
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var http = HttpReturning("""{"versions":["1.0.0","99.0.0"]}""");
        var runner = new RecordingRunner { ExitCode = 1 };

        var exit = UpdateCommand.Handle(
            new UpdateOptions(CheckOnly: false),
            stdout, stderr, () => http, runner);

        exit.Should().Be(1);
        stderr.ToString().Should().Contain("Update failed");
        stderr.ToString().Should().Contain("open a new shell");
    }

    [Fact]
    public void Handle_reports_when_dotnet_is_not_on_PATH()
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var http = HttpReturning("""{"versions":["1.0.0","99.0.0"]}""");
        var runner = new ThrowingRunner(new System.ComponentModel.Win32Exception("not found"));

        var exit = UpdateCommand.Handle(
            new UpdateOptions(CheckOnly: false),
            stdout, stderr, () => http, runner);

        exit.Should().Be(1);
        stderr.ToString().Should().Contain("dotnet").And.Contain("PATH");
    }

    private static HttpClient HttpReturning(string body) =>
        new HttpClient(new StubHandler(_ => new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(body),
        }));

    private static HttpClient HttpFailing() =>
        new HttpClient(new ThrowingHandler(new HttpRequestException("network down")));

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

    private sealed class RecordingRunner : IProcessRunner
    {
        public List<(string FileName, string[] Args)> Calls { get; } = new();
        public int ExitCode { get; set; }
        public int Run(string fileName, string[] arguments)
        {
            Calls.Add((fileName, arguments));
            return ExitCode;
        }
    }

    private sealed class ThrowingRunner : IProcessRunner
    {
        private readonly Exception _ex;
        public ThrowingRunner(Exception ex) => _ex = ex;
        public int Run(string fileName, string[] arguments) => throw _ex;
    }
}
