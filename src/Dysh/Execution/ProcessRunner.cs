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
#if NETSTANDARD2_0
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();
#else
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        Task<string> stderrTask = process.StandardError.ReadToEndAsync(ct);
#endif

        using CancellationTokenSource? timeoutCts  = timeout.HasValue
            ? new CancellationTokenSource(timeout.Value)
            : null;

        using CancellationTokenSource? linkedCts = timeoutCts is not null
            ? CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token)
            : null;

        CancellationToken effectiveCt = linkedCts?.Token ?? ct;

        try
        {
#if NETSTANDARD2_0
            await WaitForExitAsync(process, effectiveCt).ConfigureAwait(false);
#else
            await process.WaitForExitAsync(effectiveCt).ConfigureAwait(false);
#endif
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

#if NETSTANDARD2_0
        while (await process.StandardOutput.ReadLineAsync().ConfigureAwait(false) is { } line)
#else
        while (await process.StandardOutput.ReadLineAsync(ct).ConfigureAwait(false) is { } line)
#endif
        {
            ct.ThrowIfCancellationRequested();
            yield return line;
        }

#if NETSTANDARD2_0
        await WaitForExitAsync(process, ct).ConfigureAwait(false);
#else
        await process.WaitForExitAsync(ct).ConfigureAwait(false);
#endif
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

#if NETSTANDARD2_0
        startInfo.Arguments = BuildArgumentString(arguments);
#else
        foreach (string arg in arguments)
            startInfo.ArgumentList.Add(arg);
#endif

        foreach (KeyValuePair<string, string> kv in environmentVariables)
            startInfo.Environment[kv.Key] = kv.Value;

        return startInfo;
    }

    private static void TryKill(Process process)
    {
        try
        {
#if NETSTANDARD2_0
            process.Kill();
#else
            process.Kill(entireProcessTree: true);
#endif
        }
        catch { /* process may have already exited */ }
    }

#if NETSTANDARD2_0
    /// <summary>
    /// Polyfill for <c>Process.WaitForExitAsync</c> (unavailable in netstandard2.0).
    /// Subscribes to the <see cref="Process.Exited"/> event and completes when the
    /// process exits or the <paramref name="ct"/> is cancelled.
    /// </summary>
    private static Task WaitForExitAsync(Process process, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        process.EnableRaisingEvents = true;
        process.Exited += (_, _) => tcs.TrySetResult(true);

        // Handle race: process may have already exited before we subscribed
        if (process.HasExited)
            tcs.TrySetResult(true);

        var reg = ct.Register(() => tcs.TrySetCanceled(ct));

        return tcs.Task.ContinueWith(
            t => { reg.Dispose(); return t; },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default).Unwrap();
    }

    /// <summary>
    /// Builds a quoted argument string for <see cref="ProcessStartInfo.Arguments"/>
    /// on netstandard2.0, which lacks <c>ArgumentList</c>.
    /// </summary>
    private static string BuildArgumentString(IEnumerable<string> args)
    {
        var parts = new System.Text.StringBuilder();
        foreach (string arg in args)
        {
            if (parts.Length > 0) parts.Append(' ');

            bool needsQuotes = arg.Length == 0 || arg.IndexOf(' ') >= 0 || arg.IndexOf('"') >= 0 || arg.IndexOf('\t') >= 0;
            if (needsQuotes)
            {
                parts.Append('"');
                parts.Append(arg.Replace("\\", "\\\\").Replace("\"", "\\\""));
                parts.Append('"');
            }
            else
            {
                parts.Append(arg);
            }
        }
        return parts.ToString();
    }
#endif
}
