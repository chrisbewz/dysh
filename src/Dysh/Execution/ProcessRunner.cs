using System.Diagnostics;
using System.Runtime.CompilerServices;
using Dysh.Core;

namespace Dysh.Execution;

/// <summary>
/// Wraps <see cref="Process"/> to provide async execution, streaming, and cancellation
/// over arbitrary executables. Shell adapters delegate to this class.
/// </summary>
internal sealed class ProcessRunner
{
    /// <summary>
    /// Executes a process and captures the full stdout/stderr output.
    /// </summary>
    internal static async Task<CommandResult> RunAsync(
        string executable,
        IEnumerable<string> arguments,
        string? workingDirectory,
        IReadOnlyDictionary<string, string> environmentVariables,
        TimeSpan? timeout,
        CancellationToken ct)
    {
        ProcessStartInfo startInfo = BuildStartInfo(executable, arguments, workingDirectory, environmentVariables);

        using Process process = new Process { StartInfo = startInfo };

        Stopwatch sw = Stopwatch.StartNew();

        process.Start();

        // Read stdout and stderr concurrently to avoid deadlocks on full buffers
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        Task<string> stderrTask = process.StandardError.ReadToEndAsync(ct);

        using CancellationTokenSource? timeoutCts  = timeout.HasValue
            ? new CancellationTokenSource(timeout.Value)
            : null;

        using CancellationTokenSource? linkedCts = timeoutCts is not null
            ? CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token)
            : null;

        CancellationToken effectiveCt = linkedCts?.Token ?? ct;

        try
        {
            await process.WaitForExitAsync(effectiveCt).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeoutCts?.IsCancellationRequested == true)
        {
            TryKill(process);
            throw new TimeoutException(
                $"Process '{executable}' did not complete within {timeout!.Value.TotalSeconds:F1}s.");
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }

        sw.Stop();

        string stdout = await stdoutTask.ConfigureAwait(false);
        string stderr = await stderrTask.ConfigureAwait(false);

        return new CommandResult
        {
            ExitCode       = process.ExitCode,
            StandardOutput = stdout,
            StandardError  = stderr,
            Elapsed        = sw.Elapsed,
        };
    }

    /// <summary>
    /// Executes a process and yields stdout lines as they arrive.
    /// </summary>
    internal static async IAsyncEnumerable<string> StreamAsync(
        string executable,
        IEnumerable<string> arguments,
        string? workingDirectory,
        IReadOnlyDictionary<string, string> environmentVariables,
        TimeSpan? timeout,
        [EnumeratorCancellation] CancellationToken ct)
    {
        ProcessStartInfo startInfo = BuildStartInfo(executable, arguments, workingDirectory, environmentVariables);

        using Process process = new Process { StartInfo = startInfo };

        process.Start();

        while (await process.StandardOutput.ReadLineAsync(ct).ConfigureAwait(false) is { } line)
        {
            ct.ThrowIfCancellationRequested();
            yield return line;
        }

        await process.WaitForExitAsync(ct).ConfigureAwait(false);
    }

    private static ProcessStartInfo BuildStartInfo(
        string executable,
        IEnumerable<string> arguments,
        string? workingDirectory,
        IReadOnlyDictionary<string, string> environmentVariables)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName               = executable,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true,
            WorkingDirectory       = workingDirectory ?? string.Empty,
        };

        foreach (string arg in arguments)
            startInfo.ArgumentList.Add(arg);

        foreach ((string key, string value) in environmentVariables)
            startInfo.Environment[key] = value;

        return startInfo;
    }

    private static void TryKill(Process process)
    {
        try { process.Kill(entireProcessTree: true); }
        catch { /* process may have already exited */ }
    }
}
