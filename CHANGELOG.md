# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [Unreleased]

## [0.1.0] - 2026-05-08

### Added
- DLR-powered shell execution — any shell command becomes a C# method call via `dynamic Shell.Create()`
- Async-first API — executions return `Task<CommandResult>` or `IAsyncEnumerable<string>` for streaming output
- Subcommand chaining — `shell.git.log()` maps naturally to `git log`
- Named argument mapping — single-char args map to `-f`, multi-char to `--flag`
- Cross-platform shell abstraction — auto-detects `bash`, `pwsh`, or `cmd.exe` via `ShellKind.Native`
- `ShellKind` enum with explicit support for `Bash`, `Sh`, `Pwsh`, and `Cmd`
- Scoped execution via `Shell.Create(options => ...)` — configurable working directory, environment variables, and shell kind
- `CommandResult` — structured result with exit code, stdout, stderr, and elapsed time
- `EnsureSuccess()` extension to throw on non-zero exit codes
- Full `CancellationToken` support throughout async execution
- Target frameworks: `net8.0`, `net9.0`, and `netstandard2.0`
