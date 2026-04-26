using Dysh.Exceptions;

namespace Dysh.Core;

/// <summary>
/// Represents the result of a shell command execution.
/// </summary>
public sealed record CommandResult
{
    /// <summary>The process exit code.</summary>
    public int ExitCode { get; init; }

    /// <summary>Full content of the standard output stream.</summary>
    public string StandardOutput { get; init; } = string.Empty;

    /// <summary>Full content of the standard error stream.</summary>
    public string StandardError { get; init; } = string.Empty;

    /// <summary>Total time elapsed during execution.</summary>
    public TimeSpan Elapsed { get; init; }

    /// <summary>Returns true if the process exited with code 0.</summary>
    public bool Success => ExitCode == 0;

    /// <summary>
    /// Returns this result if successful; otherwise throws <see cref="ShellCommandException"/>.
    /// </summary>
    /// <exception cref="ShellCommandException">Thrown when <see cref="ExitCode"/> is not zero.</exception>
    public CommandResult EnsureSuccess()
    {
        return !Success ? throw new ShellCommandException(this) : this;
    }

    /// <summary>
    /// Returns a string representation including the exit code and output lengths,
    /// useful for debugging.
    /// </summary>
    public override string ToString() =>
        $"CommandResult {{ ExitCode={ExitCode}, Success={Success}, Elapsed={Elapsed.TotalMilliseconds:F0}ms }}";
}
