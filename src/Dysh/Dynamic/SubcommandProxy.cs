using System.Dynamic;
using Dysh.Adapters;
using Dysh.Core;

namespace Dysh.Dynamic;

/// <summary>
/// Represents a partially-resolved command chain, e.g. the <c>git</c> in <c>shell.git.log()</c>.
///
/// <para>
/// When a member is accessed on a <see cref="ShellProxy"/> (e.g. <c>shell.git</c>), the DLR
/// returns a <see cref="SubcommandProxy"/> bound to that name. Invoking a method on it
/// (e.g. <c>.log()</c>) completes the chain and dispatches the command.
/// </para>
/// </summary>
public sealed class SubcommandProxy : DynamicObject
{
    private readonly IShellAdapter _adapter;
    private readonly string _executable;

    internal SubcommandProxy(IShellAdapter adapter, string executable)
    {
        _adapter    = adapter;
        _executable = executable;
    }

    /// <summary>
    /// Intercepts a method call on the proxy, treating the method name as the subcommand.
    /// e.g. <c>shell.git.log(n: 10)</c> → executable=<c>git</c>, subcommand=<c>log</c>.
    /// </summary>
    public override bool TryInvokeMember(
        InvokeMemberBinder binder,
        object?[]? args,
        out object? result)
    {
        CommandDescriptor descriptor = ArgumentParser.Map(
            executable:           _executable,
            subcommand:           binder.Name,
            args:                 args ?? [],
            argumentNames:        binder.CallInfo.ArgumentNames);

        result = _adapter.ExecuteAsync(descriptor);
        return true;
    }

    /// <summary>
    /// Supports direct invocation of the proxy itself, e.g. after further chaining.
    /// Not commonly needed but completes the DLR contract.
    /// </summary>
    public override bool TryInvoke(
        InvokeBinder binder,
        object?[]? args,
        out object? result)
    {
        CommandDescriptor descriptor = ArgumentParser.Map(
            executable:    _executable,
            subcommand:    null,
            args:          args ?? [],
            argumentNames: binder.CallInfo.ArgumentNames);

        result = _adapter.ExecuteAsync(descriptor);
        return true;
    }
}
