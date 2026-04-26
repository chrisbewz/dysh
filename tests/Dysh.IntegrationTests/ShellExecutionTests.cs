using System.Diagnostics;
using Dysh.Dynamic;
using Dysh.Core;
using Dysh.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace Dysh.IntegrationTests;

/// <summary>
/// Integration tests that execute real processes.
/// These run on CI against both Linux and Windows runners.
/// </summary>
public sealed class ShellExecutionTests
{
    [Fact]
    public async Task Execute_EchoCommand_ReturnsOutput()
    {
        dynamic shell = Shell.Create();

        CommandResult result = await shell.echo("hello dysh");

        result.Success.Should().BeTrue();
        result.StandardOutput.Trim().Should().Be("hello dysh");
    }

    [Fact]
    public async Task Execute_ExitCodeZero_SuccessIsTrue()
    {
        dynamic shell = Shell.Create();

        CommandResult result = await shell.echo("ok");

        result.ExitCode.Should().Be(0);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_Elapsed_IsPositive()
    {
        dynamic shell = Shell.Create();

        CommandResult result = await shell.echo("timing");

        result.Elapsed.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task Execute_GitVersion_ReturnsVersionString()
    {
        Debug.Assert(GitHelpers.IsGitAvailable(), "git not available in this environment");

        dynamic shell = Shell.Create();

        CommandResult result = await shell.git("--version");

        result.Success.Should().BeTrue();
        result.StandardOutput.Should().Contain("git version");
    }

    [Fact]
    public async Task Execute_Subcommand_GitLog_WorksWithFlags()
    {
        Debug.Assert(GitHelpers.IsGitAvailable(), "git not available in this environment");
        Debug.Assert(GitHelpers.IsInsideGitRepo(), "not inside a git repo");

        dynamic shell = Shell.Create();

        CommandResult result = await shell.git.log(n: 1, oneline: true);

        result.Success.Should().BeTrue();
        result.StandardOutput.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Execute_WithCancellation_ThrowsOperationCancelled()
    {
        using CancellationTokenSource cts = new CancellationTokenSource();
        await cts.CancelAsync();

        dynamic shell = Shell.Create();

        Func<Task> act = async () =>
        {
            CommandResult _ = await shell.echo("should not run");
        };

        // Awaiting a cancelled token before the process starts should throw
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Execute_WithCustomAdapter_UsesInjectedAdapter()
    {
        FakeShellAdapter fakeAdapter = new FakeShellAdapter(new CommandResult
        {
            ExitCode       = 0,
            StandardOutput = "from fake",
            Elapsed        = TimeSpan.FromMilliseconds(1),
        });

        dynamic shell = Shell.Create(opt => opt.WithAdapter(fakeAdapter));

        CommandResult result = await shell.anything("args");

        result.StandardOutput.Should().Be("from fake");
        fakeAdapter.WasCalled.Should().BeTrue();
    }
}