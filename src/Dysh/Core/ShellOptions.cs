using Dysh.Adapters;

namespace Dysh.Core;

/// <summary>
/// Fluent configuration for a <see cref="Dynamic.ShellProxy"/> instance.
/// All methods return a new <see cref="ShellOptions"/> — the type is immutable.
/// </summary>
public sealed class ShellOptions
{
    private readonly Dictionary<string, string> _environmentVariables;

    /// <summary>Creates a default <see cref="ShellOptions"/> using <see cref="ShellKind.Native"/>.</summary>
    public ShellOptions()
    {
        ShellKind            = ShellKind.Native;
        _environmentVariables = new Dictionary<string, string>();
    }

    private ShellOptions(
        string? workingDirectory,
        Dictionary<string, string> environmentVariables,
        ShellKind shellKind,
        IShellAdapter? customAdapter,
        TimeSpan? timeout)
    {
        WorkingDirectory     = workingDirectory;
        _environmentVariables = environmentVariables;
        ShellKind            = shellKind;
        CustomAdapter        = customAdapter;
        Timeout              = timeout;
    }

    /// <summary>Sets the working directory for all commands in this shell instance.</summary>
    public ShellOptions WithWorkingDirectory(string path)
    {
#if NETSTANDARD2_0
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
#else
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
#endif
        return new ShellOptions(path, _environmentVariables, ShellKind, CustomAdapter, Timeout);
    }

    /// <summary>Adds or overrides a single environment variable.</summary>
    public ShellOptions WithEnvironmentVariable(string key, string value)
    {
#if NETSTANDARD2_0
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
#else
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
#endif
        Dictionary<string, string> env = new Dictionary<string, string>(_environmentVariables) { [key] = value };
        return new ShellOptions(WorkingDirectory, env, ShellKind, CustomAdapter, Timeout);
    }

    /// <summary>Selects the shell to use via <see cref="ShellKind"/>.</summary>
    public ShellOptions WithShell(ShellKind kind)
    {
        return new ShellOptions(WorkingDirectory, _environmentVariables, kind, CustomAdapter, Timeout);
    }

    /// <summary>
    /// Injects a custom <see cref="IShellAdapter"/> implementation.
    /// Takes precedence over <see cref="WithShell"/>.
    /// Useful for testing and custom shell wrappers.
    /// </summary>
    public ShellOptions WithAdapter(IShellAdapter adapter)
    {
#if NETSTANDARD2_0
        if (adapter is null) throw new ArgumentNullException(nameof(adapter));
#else
        ArgumentNullException.ThrowIfNull(adapter);
#endif
        return new ShellOptions(WorkingDirectory, _environmentVariables, ShellKind, adapter, Timeout);
    }

    /// <summary>Sets a timeout after which the process is killed and a <see cref="TimeoutException"/> is thrown.</summary>
    public ShellOptions WithTimeout(TimeSpan timeout)
    {
        return timeout <= TimeSpan.Zero 
            ? throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive.") 
            : new ShellOptions(WorkingDirectory, _environmentVariables, ShellKind, CustomAdapter, timeout);
    }

    internal string? WorkingDirectory { get; }

    internal IReadOnlyDictionary<string, string> EnvironmentVariables => _environmentVariables;
    internal ShellKind ShellKind { get; }

    internal IShellAdapter? CustomAdapter { get; }

    internal TimeSpan? Timeout { get; }
}
