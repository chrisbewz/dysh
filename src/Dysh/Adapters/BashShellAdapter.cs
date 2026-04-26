using Dysh.Core;

namespace Dysh.Adapters;

/// <summary>Executes commands via <c>/bin/bash -c</c>.</summary>
internal sealed class BashShellAdapter : ShellAdapterBase
{
    protected override string ShellExecutable => "/bin/bash";
    protected override IReadOnlyList<string> ShellPrefix => ["-c"];

    /// <inheritdoc/>
    public BashShellAdapter(ShellOptions options) : base(options) { }
}