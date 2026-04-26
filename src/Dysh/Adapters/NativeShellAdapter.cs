using Dysh.Core;

namespace Dysh.Adapters;

/// <summary>
/// Auto-detects the appropriate shell for the current OS and delegates to it.
/// Resolves to <see cref="BashShellAdapter"/> on Linux/macOS and
/// <see cref="CmdShellAdapter"/> on Windows.
/// This is the default adapter used by <see cref="Dynamic.Shell.Create()"/>.
/// </summary>
internal sealed class NativeShellAdapter : IShellAdapter
{
    private readonly IShellAdapter _inner;

    /// <inheritdoc/>
    public NativeShellAdapter(ShellOptions options)
    {
        _inner = OperatingSystem.IsWindows()
            ? new CmdShellAdapter(options)
            : new BashShellAdapter(options);
    }

    /// <inheritdoc/>
    public Task<Core.CommandResult> ExecuteAsync(
        Core.CommandDescriptor descriptor,
        CancellationToken ct = default)
        => _inner.ExecuteAsync(descriptor, ct);

    /// <inheritdoc/>
    public IAsyncEnumerable<string> StreamAsync(
        Core.CommandDescriptor descriptor,
        CancellationToken ct = default)
        => _inner.StreamAsync(descriptor, ct);
}