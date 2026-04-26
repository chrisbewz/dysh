using Dysh.Core;

namespace Dysh.Adapters;

/// <summary>Executes commands via <c>cmd.exe /c</c> (Windows only).</summary>
internal sealed class CmdShellAdapter : ShellAdapterBase
{
    protected override string ShellExecutable => "cmd.exe";
    protected override IReadOnlyList<string> ShellPrefix => ["/c"];

    /// <inheritdoc/>
    public CmdShellAdapter(ShellOptions options) : base(options) { }
}