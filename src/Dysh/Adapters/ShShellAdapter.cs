using Dysh.Core;

namespace Dysh.Adapters;

/// <summary>Executes commands via <c>/bin/sh -c</c>.</summary>
internal sealed class ShShellAdapter : ShellAdapterBase
{
    protected override string ShellExecutable => "/bin/sh";
    protected override IReadOnlyList<string> ShellPrefix => ["-c"];

    /// <inheritdoc/>
    public ShShellAdapter(ShellOptions options) : base(options) { }
}