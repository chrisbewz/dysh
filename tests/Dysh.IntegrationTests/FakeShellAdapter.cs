using Dysh.Adapters;
using Dysh.Core;

namespace Dysh.IntegrationTests;

/// <summary>
/// A test double for <see cref="IShellAdapter"/> that returns a predetermined result.
/// </summary>
internal sealed class FakeShellAdapter(CommandResult result) : IShellAdapter
{
    public bool WasCalled { get; private set; }

    public Task<CommandResult> ExecuteAsync(
        Dysh.Core.CommandDescriptor descriptor,
        CancellationToken ct = default)
    {
        WasCalled = true;
        return Task.FromResult(result);
    }

    public async IAsyncEnumerable<string> StreamAsync(
        Dysh.Core.CommandDescriptor descriptor,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        WasCalled = true;
        yield return result.StandardOutput;
        await Task.CompletedTask;
    }
}