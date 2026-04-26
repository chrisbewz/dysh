using Dysh.Core;
using Dysh.Execution;

namespace Dysh.Adapters;

/// <summary>
/// Base implementation of <see cref="IShellAdapter"/> that handles argument assembly
/// and delegates process execution to <see cref="ProcessRunner"/>.
///
/// Concrete adapters only need to provide the shell executable and
/// the argument prefix required to pass a command string to the shell.
/// </summary>
public abstract class ShellAdapterBase : IShellAdapter
{
    private readonly ShellOptions _options;

    /// <summary>The shell executable path, e.g. <c>/bin/bash</c> or <c>cmd.exe</c>.</summary>
    protected abstract string ShellExecutable { get; }

    /// <summary>
    /// Arguments that precede the command string when invoking the shell.
    /// For bash: <c>["-c"]</c>. For cmd.exe: <c>["/c"]</c>. For pwsh: <c>["-Command"]</c>.
    /// </summary>
    protected abstract IReadOnlyList<string> ShellPrefix { get; }

    /// <summary>Initializes the adapter with the provided options.</summary>
    protected ShellAdapterBase(ShellOptions options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public Task<CommandResult> ExecuteAsync(
        CommandDescriptor descriptor,
        CancellationToken ct = default)
    {
        (IEnumerable<string> args, IReadOnlyDictionary<string, string> mergedEnv) = BuildInvocation(descriptor);

        return ProcessRunner.RunAsync(
            ShellExecutable,
            args,
            descriptor.WorkingDirectory ?? _options.WorkingDirectory,
            mergedEnv,
            _options.Timeout,
            ct);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<string> StreamAsync(
        CommandDescriptor descriptor,
        CancellationToken ct = default)
    {
        (IEnumerable<string> args, IReadOnlyDictionary<string, string> mergedEnv) = BuildInvocation(descriptor);

        return ProcessRunner.StreamAsync(
            ShellExecutable,
            args,
            descriptor.WorkingDirectory ?? _options.WorkingDirectory,
            mergedEnv,
            _options.Timeout,
            ct);
    }

    /// <summary>
    /// Assembles the full argument list and merged environment for this invocation.
    /// </summary>
    private (IEnumerable<string> args, IReadOnlyDictionary<string, string> env) BuildInvocation(
        CommandDescriptor descriptor)
    {
        // Build the command string: "git log --oneline -n 10"
        string commandString = descriptor.ToCommandLine();

        // Full arg list: shell prefix + command string
        // e.g. bash -c "git log --oneline -n 10"
        IEnumerable<string> args = ShellPrefix.Append(commandString);

        // Merge global env from options with per-command env (descriptor wins)
        Dictionary<string, string> mergedEnv = _options.EnvironmentVariables
            .Concat(descriptor.EnvironmentVariables)
            .GroupBy(kv => kv.Key)
            .ToDictionary(g => g.Key, g => g.Last().Value);

        return (args, mergedEnv);
    }
}
