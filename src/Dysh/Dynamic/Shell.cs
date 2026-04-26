using Dysh.Adapters;
using Dysh.Core;

namespace Dysh.Dynamic;

/// <summary>
/// Static factory for creating <c>dynamic</c> shell instances.
///
/// <para>Usage:</para>
/// <code>
/// dynamic shell = Shell.Create();
/// CommandResult result = await shell.git("status");
/// </code>
/// </summary>
public static class Shell
{
    /// <summary>
    /// Provides a default dynamic shell instance configured with the
    /// <see cref="NativeShellAdapter"/> and default options.
    /// </summary>
    /// <remarks>
    /// This property serves as a convenient and ready-to-use dynamic shell instance,
    /// allowing seamless execution of shell commands without requiring explicit configuration.
    /// Utilizing the default instance enables quick prototyping or straightforward
    /// command executions with the underlying auto-detected OS shell.
    /// </remarks>
    /// <value>A <c>dynamic</c> shell instance represented by <see cref="ShellProxy"/>.</value>
    public static dynamic Default => Create();
    
    /// <summary>
    /// Creates a shell instance using the <see cref="NativeShellAdapter"/> (auto-detected OS shell)
    /// and default options.
    /// </summary>
    /// <returns>A <c>dynamic</c> <see cref="ShellProxy"/> instance.</returns>
    public static dynamic Create() => Create(_ => { });

    /// <summary>
    /// Creates a shell instance with custom <see cref="ShellOptions"/>.
    /// </summary>
    /// <param name="configure">A delegate to configure the shell options.</param>
    /// <returns>A <c>dynamic</c> <see cref="ShellProxy"/> instance.</returns>
    public static dynamic Create(Action<ShellOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        ShellOptions options = new ShellOptions();
        configure(options);

        IShellAdapter adapter = ResolveAdapter(options);
        return new ShellProxy(adapter);
    }

    private static IShellAdapter ResolveAdapter(ShellOptions options)
    {
        // Custom adapter always wins
        if (options.CustomAdapter is not null)
            return options.CustomAdapter;

        return options.ShellKind switch
        {
            ShellKind.Bash   => new BashShellAdapter(options),
            ShellKind.Sh     => new ShShellAdapter(options),
            ShellKind.Pwsh   => new PwshShellAdapter(options),
            ShellKind.Cmd    => new CmdShellAdapter(options),
            ShellKind.Native => new NativeShellAdapter(options),
            _                => new NativeShellAdapter(options),
        };
    }
}
