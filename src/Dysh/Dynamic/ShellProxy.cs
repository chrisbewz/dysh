using System.Dynamic;
using Dysh.Adapters;
using Dysh.Core;

namespace Dysh.Dynamic;

/// <summary>
/// The central DLR object. Returned by <see cref="Shell.Create()"/> as <c>dynamic</c>.
///
/// <para>
/// Intercepts two DLR operations:
/// <list type="bullet">
///   <item>
///     <term><see cref="TryInvokeMember"/></term>
///     <description>
///       Handles direct invocations like <c>shell.git("status")</c>, treating the
///       member name as the executable.
///     </description>
///   </item>
///   <item>
///     <term><see cref="TryGetMember"/></term>
///     <description>
///       Handles member access like <c>shell.git</c>, returning a
///       <see cref="SubcommandProxy"/> for chaining (e.g. <c>shell.git.log()</c>).
///     </description>
///   </item>
/// </list>
/// </para>
/// </summary>
public sealed class ShellProxy : DynamicObject
{
    private readonly IShellAdapter _adapter;

    internal ShellProxy(IShellAdapter adapter)
    {
        _adapter = adapter;
    }

    /// <summary>
    /// Intercepts <c>shell.executable(args)</c>.
    /// The member name becomes the executable; all args are forwarded to <see cref="ArgumentParser"/>.
    /// Returns a <see cref="Task{CommandResult}"/> — awaitable via <c>dynamic</c>.
    /// </summary>
    public override bool TryInvokeMember(
        InvokeMemberBinder binder,
        object?[]? args,
        out object? result)
    {
        object?[] callArgs = args ?? [];
        IList<string> argNames = new List<string>(binder.CallInfo.ArgumentNames);
        CancellationToken ct = ArgumentParser.ExtractCancellationToken(ref callArgs, ref argNames);

        CommandDescriptor descriptor = ArgumentParser.Map(
            executable:    binder.Name,
            subcommand:    null,
            args:          callArgs,
            argumentNames: argNames);

        result = _adapter.ExecuteAsync(descriptor, ct);
        return true;
    }

    /// <summary>
    /// Intercepts <c>shell.executable</c> (no invocation, just member access).
    /// Returns a <see cref="SubcommandProxy"/> bound to the executable name,
    /// enabling chaining like <c>shell.git.log()</c>.
    /// </summary>
    public override bool TryGetMember(
        GetMemberBinder binder,
        out object? result)
    {
        result = new SubcommandProxy(_adapter, binder.Name);
        return true;
    }
}
