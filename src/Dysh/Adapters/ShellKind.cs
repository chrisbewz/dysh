namespace Dysh.Adapters;

/// <summary>
/// Identifies the shell to use for command execution.
/// </summary>
public enum ShellKind
{
    /// <summary>
    /// Automatically selects the appropriate shell for the current OS.
    /// Resolves to <c>bash</c> on Linux/macOS and <c>cmd.exe</c> on Windows.
    /// </summary>
    Native,

    /// <summary>
    /// <c>/bin/bash</c> — Linux and macOS.
    /// </summary>
    Bash,

    /// <summary>
    /// <c>/bin/sh</c> — POSIX-compatible shell, Linux and macOS.
    /// </summary>
    Sh,

    /// <summary>
    /// <c>pwsh</c> — PowerShell Core, cross-platform.
    /// </summary>
    Pwsh,

    /// <summary>
    /// <c>cmd.exe</c> — Windows Command Prompt.
    /// </summary>
    Cmd,
}
