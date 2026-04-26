using System.Diagnostics;

namespace Dysh.IntegrationTests.Helpers;

/// <summary>
/// Provides utility methods related to Git operations, such as checking
/// for Git availability and determining if the current directory is
/// inside a Git repository.
/// </summary>
internal static class GitHelpers
{
    /// <summary>
    /// Checks whether Git is available on the system by attempting to run the "git" command.
    /// </summary>
    /// <returns>
    /// true if Git is available; otherwise, false.
    /// </returns>
    public static bool IsGitAvailable()
    {
        try
        {
            Process? p = Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
            });
            p?.WaitForExit();
            return p?.ExitCode == 0;
        }
        catch { return false; }
    }

    /// <summary>
    /// Determines whether the current directory is inside a Git repository.
    /// </summary>
    /// <returns>
    /// true if the current directory is inside a Git repository; otherwise, false.
    /// </returns>
    public static bool IsInsideGitRepo()
    {
        try
        {
            Process? p = Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse --git-dir",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            });
            p?.WaitForExit();
            return p?.ExitCode == 0;
        }
        catch { return false; }
    }
}