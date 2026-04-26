using Dysh.Core;

namespace Dysh.Adapters;

/// <summary>
/// Defines the contract for shell execution backends.
/// Implement this interface to support custom shells, sandboxed environments, or test doubles.
/// </summary>
public interface IShellAdapter
{
    /// <summary>
    /// Executes the command described by <paramref name="descriptor"/> and returns
    /// the full result after the process exits.
    /// </summary>
    /// <param name="descriptor">The fully resolved command to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="CommandResult"/> with exit code, stdout, stderr, and elapsed time.</returns>
    Task<CommandResult> ExecuteAsync(
        CommandDescriptor descriptor,
        CancellationToken ct = default);

    /// <summary>
    /// Executes the command and streams stdout lines as they are produced,
    /// without buffering the full output.
    /// </summary>
    /// <param name="descriptor">The fully resolved command to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An async sequence of stdout lines.</returns>
    IAsyncEnumerable<string> StreamAsync(
        CommandDescriptor descriptor,
        CancellationToken ct = default);
}
