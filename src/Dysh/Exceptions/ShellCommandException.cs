using Dysh.Core;

namespace Dysh.Exceptions;

/// <summary>
/// Thrown by <see cref="CommandResult.EnsureSuccess"/> when a command exits with a non-zero code.
/// </summary>
public sealed class ShellCommandException : Exception
{
    /// <summary>The result that caused this exception.</summary>
    public CommandResult Result { get; }

    /// <inheritdoc/>
    public ShellCommandException(CommandResult result)
        : base($"Shell command failed with exit code {result.ExitCode}.\n{result.StandardError}".TrimEnd())
    {
        Result = result;
    }

    /// <inheritdoc/>
    public ShellCommandException(CommandResult result, Exception innerException)
        : base($"Shell command failed with exit code {result.ExitCode}.\n{result.StandardError}".TrimEnd(), innerException)
    {
        Result = result;
    }
}
