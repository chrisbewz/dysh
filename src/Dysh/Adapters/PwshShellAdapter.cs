using Dysh.Core;

namespace Dysh.Adapters;

/// <summary>Executes commands via <c>pwsh -Command</c> (PowerShell Core, cross-platform).</summary>
internal sealed class PwshShellAdapter : ShellAdapterBase
{
    protected override string ShellExecutable => "pwsh";
    protected override IReadOnlyList<string> ShellPrefix => ["-Command"];

    /// <inheritdoc/>
    public PwshShellAdapter(ShellOptions options) : base(options) { }
}