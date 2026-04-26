namespace Dysh.Core;

/// <summary>
/// An immutable description of a shell command to be executed.
/// Produced by <see cref="ArgumentParser"/> from a DLR invocation
/// and consumed by <see cref="Adapters.IShellAdapter"/>.
/// </summary>
public sealed record CommandDescriptor
{
    /// <summary>The executable name, e.g. "git", "docker", "npm".</summary>
    public required string Executable { get; init; }

    /// <summary>
    /// Optional subcommand, e.g. "log" in <c>git log</c>.
    /// Null when the command was invoked directly without chaining.
    /// </summary>
    public string? Subcommand { get; init; }

    /// <summary>
    /// Ordered list of resolved arguments, already formatted as strings
    /// (flags, values, positional args). Ready to be passed to the process.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; init; } = [];

    /// <summary>
    /// Environment variables to set for this process invocation.
    /// Merged on top of the current process environment.
    /// </summary>
    public IReadOnlyDictionary<string, string> EnvironmentVariables { get; init; }
        = new Dictionary<string, string>();

    /// <summary>
    /// Working directory for the process. Null means inherit from the current process.
    /// </summary>
    public string? WorkingDirectory { get; init; }

    /// <summary>
    /// Builds the full command line string for display or logging purposes.
    /// Not used for actual process invocation.
    /// </summary>
    public string ToCommandLine()
    {
        List<string> parts =
        [
            Executable
        ];

        if (Subcommand is not null)
            parts.Add(Subcommand);

        parts.AddRange(Arguments);

        return string.Join(' ', parts);
    }

    /// <inheritdoc/>
    public override string ToString() => ToCommandLine();
}
