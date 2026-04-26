# dysh

> Dynamic shell command execution for .NET — invoke shell commands as if they were native C# methods.

[![NuGet](https://img.shields.io/nuget/v/dysh.svg)](https://www.nuget.org/packages/dysh)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-512BD4)](https://dotnet.microsoft.com)

---

**dysh** is a C# library that leverages the .NET Dynamic Language Runtime (DLR) to let you execute shell commands with a fluent, natural syntax — inspired by Python's [sh](https://github.com/amoffat/sh) library. It is the spiritual successor to [manojlds/cmd](https://github.com/manojlds/cmd), rebuilt for modern .NET with async-first design and cross-platform shell abstraction.

```csharp
dynamic shell = Shell.Create();

// Run a command
CommandResult result = await shell.git("status");

// Subcommands
CommandResult log = await shell.git.log(n: 10, oneline: true);

// Stream output in real time
await foreach (var line in shell.docker.logs(f: true, "my-container"))
    Console.WriteLine(line);
```

---

## Features

- **DLR-powered syntax** — any shell command becomes a C# method call via `dynamic`
- **Async-first** — all executions return `Task<CommandResult>` or `IAsyncEnumerable<string>`
- **Cross-platform shell abstraction** — auto-detects the appropriate shell (`bash`, `pwsh`, `cmd.exe`)
- **Subcommand chaining** — `shell.git.log()` maps naturally to `git log`
- **Named argument mapping** — single-char named args map to `-f`, multi-char to `--flag`
- **Structured results** — exit code, stdout, stderr, and elapsed time in one object
- **CancellationToken support** — full async cancellation throughout
- **Scoped execution** — set working directory and environment variables per instance

---

## Installation

```bash
dotnet add package dysh
```

---

## Getting Started

### Basic usage

```csharp
using Dysh;

dynamic shell = Shell.Create();

CommandResult result = await shell.git("status");

if (result.Success)
    Console.WriteLine(result.StandardOutput);
else
    Console.WriteLine(result.StandardError);
```

### Subcommands

```csharp
// Equivalent to: git log --oneline -n 10
CommandResult log = await shell.git.log(n: 10, oneline: true);
```

### Argument mapping

```csharp
// Positional args → passed as-is
await shell.ls("/home/user");

// Single-char named arg → -v
await shell.tar(x: true, z: true, f: "archive.tar.gz");
// → tar -x -z -f archive.tar.gz

// Multi-char named arg → --output
await shell.ffmpeg(input: "video.mp4", output: "out.mp4");
// → ffmpeg --input video.mp4 --output out.mp4
```

### Streaming output

```csharp
// Stream stdout line by line as it's produced
await foreach (var line in shell.ping("8.8.8.8", c: 4))
    Console.WriteLine(line);
```

### Scoped shell with options

```csharp
dynamic scoped = Shell.Create(options => options
    .WithWorkingDirectory("/var/app")
    .WithEnvironmentVariable("NODE_ENV", "production")
    .WithShell(ShellKind.Bash));

await scoped.npm("install");
await scoped.npm.run("build");
```

### Error handling

```csharp
// Check manually
var result = await shell.git("push");
if (!result.Success)
    Console.Error.WriteLine($"Exit {result.ExitCode}: {result.StandardError}");

// Or throw on failure
await shell.git("push").EnsureSuccess();
```

---

## Shell Support

| Shell | Identifier | Platform |
|---|---|---|
| Auto-detect | `ShellKind.Native` *(default)* | Cross-platform |
| Bash | `ShellKind.Bash` | Linux / macOS |
| sh | `ShellKind.Sh` | Linux / macOS |
| PowerShell | `ShellKind.Pwsh` | Cross-platform |
| cmd.exe | `ShellKind.Cmd` | Windows |

The default `Native` adapter resolves to `bash` on Linux/macOS and `cmd.exe` on Windows. You can always override this explicitly via `ShellOptions`.

---

## How it works

dysh implements `DynamicObject` and overrides `TryInvokeMember` and `TryGetMember` to intercept method calls at runtime via the DLR. Each call is translated into a `CommandDescriptor` — a structured representation of the executable, subcommands, arguments, and environment — which is then handed off to an `IShellAdapter` for execution via `System.Diagnostics.Process`.

```
dynamic shell.git.log(n: 10)
        │
        ▼
  TryGetMember("git")       → SubcommandProxy("git")
        │
        ▼
  TryInvokeMember("log")    → CommandDescriptor { Executable="git", Subcommand="log", Args=["-n","10"] }
        │
        ▼
  IShellAdapter.ExecuteAsync(descriptor)
        │
        ▼
  ProcessRunner             → System.Diagnostics.Process
        │
        ▼
  CommandResult { ExitCode, StandardOutput, StandardError, Elapsed }
```

---

## Acknowledgements

dysh is the spiritual successor to [manojlds/cmd](https://github.com/manojlds/cmd) and draws inspiration from Python's [sh](https://github.com/amoffat/sh). The original `cmd` library was a creative showcase of the C# DLR — dysh aims to carry that spirit forward as a maintained, production-grade library.

---

## License

MIT © dysh contributors
