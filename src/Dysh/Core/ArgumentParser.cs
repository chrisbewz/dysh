namespace Dysh.Core;

/// <summary>
/// Translates raw DLR invocation arguments into a <see cref="CommandDescriptor"/>.
///
/// <para>Argument mapping convention:</para>
/// <list type="bullet">
///   <item>Positional args → passed as-is</item>
///   <item>Named arg, 1 char, value is <c>true</c> → <c>-f</c></item>
///   <item>Named arg, 1 char, with value → <c>-f value</c></item>
///   <item>Named arg, N chars, value is <c>true</c> → <c>--flag</c></item>
///   <item>Named arg, N chars, with value → <c>--flag value</c></item>
///   <item>Named arg with <c>null</c> value → ignored</item>
/// </list>
/// </summary>
public static class ArgumentParser
{
    /// <summary>
    /// Builds a <see cref="CommandDescriptor"/> from the intercepted DLR call.
    /// </summary>
    /// <param name="executable">The executable name (from TryGetMember or TryInvokeMember).</param>
    /// <param name="subcommand">Optional subcommand from member chaining.</param>
    /// <param name="args">Raw positional and named arguments from the DLR binder.</param>
    /// <param name="argumentNames">
    /// Named argument names in order, aligned to the tail of <paramref name="args"/>.
    /// The DLR convention is: positional args first, then named args.
    /// </param>
    /// <param name="workingDirectory">Optional working directory override.</param>
    /// <param name="environmentVariables">Optional environment variables override.</param>
    public static CommandDescriptor Map(
        string executable,
        string? subcommand,
        object?[] args,
        IList<string> argumentNames,
        string? workingDirectory = null,
        IReadOnlyDictionary<string, string>? environmentVariables = null)
    {
        int positionalCount = args.Length - argumentNames.Count;
        List<string> resolved = [];

        // Positional arguments — passed through as-is
        for (int i = 0; i < positionalCount; i++)
        {
            if (args[i] is null) continue;
            resolved.Add(Convert.ToString(args[i])!);
        }

        // Named arguments — mapped to CLI flags
        for (int i = 0; i < argumentNames.Count; i++)
        {
            string name  = argumentNames[i];
            object? value = args[positionalCount + i];

            if (value is null) continue;

            string flag = name.Length == 1
                ? $"-{name}"
                : $"--{name}";

            if (value is bool b)
            {
                // bool true  → emit flag only
                // bool false → skip entirely
                if (b) resolved.Add(flag);
            }
            else
            {
                resolved.Add(flag);
                resolved.Add(Convert.ToString(value)!);
            }
        }

        return new CommandDescriptor
        {
            Executable           = executable,
            Subcommand           = subcommand,
            Arguments            = resolved,
            WorkingDirectory     = workingDirectory,
            EnvironmentVariables = environmentVariables ?? new Dictionary<string, string>(),
        };
    }
}
