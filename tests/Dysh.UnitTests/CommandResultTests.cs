using Dysh.Core;
using Dysh.Exceptions;
using FluentAssertions;
using Xunit;

namespace Dysh.UnitTests;

public sealed class CommandResultTests
{
    [Fact]
    public void Success_IsTrue_WhenExitCodeIsZero()
    {
        CommandResult result = new CommandResult { ExitCode = 0 };

        result.Success.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(127)]
    [InlineData(-1)]
    public void Success_IsFalse_WhenExitCodeIsNonZero(int exitCode)
    {
        CommandResult result = new CommandResult { ExitCode = exitCode };

        result.Success.Should().BeFalse();
    }

    [Fact]
    public void EnsureSuccess_ReturnsResult_WhenSuccessful()
    {
        CommandResult result = new CommandResult { ExitCode = 0, StandardOutput = "ok" };

        CommandResult returned = result.EnsureSuccess();

        returned.Should().BeSameAs(result);
    }

    [Fact]
    public void EnsureSuccess_ThrowsShellCommandException_WhenFailed()
    {
        CommandResult result = new CommandResult { ExitCode = 1, StandardError = "fatal error" };

        Func<CommandResult> act = () => result.EnsureSuccess();

        act.Should().Throw<ShellCommandException>()
            .Which.Result.Should().BeSameAs(result);
    }

    [Fact]
    public void EnsureSuccess_ExceptionMessage_ContainsExitCodeAndStderr()
    {
        CommandResult result = new CommandResult { ExitCode = 2, StandardError = "not a git repo" };

        Func<CommandResult> act = () => result.EnsureSuccess();

        act.Should().Throw<ShellCommandException>()
            .WithMessage("*exit code 2*")
            .And.Message.Should().Contain("not a git repo");
    }
}
