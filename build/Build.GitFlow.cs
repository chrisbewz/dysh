// Copyright 2023 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using JetBrains.Annotations;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Octokit;
using Serilog;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.GitVersion.GitVersionTasks;

sealed partial class Build
{
    [Parameter] readonly bool AutoStash = true;
    
    Target Milestone => _ => _
        .Unlisted()
        .OnlyWhenStatic(() => GitRepository.IsOnReleaseBranch() || GitRepository.IsOnHotfixBranch())
        .Executes(async () =>
        {
            Milestone? milestone = await GitRepository.GetGitHubMilestone(MilestoneTitle);
            if (milestone == null)
                return;

            Assert.True(milestone.OpenIssues == 0);
            Assert.True(milestone.ClosedIssues != 0);
            Assert.True(milestone.State == ItemState.Closed);
        });

    Target Changelog => _ => _
        .Unlisted()
        .DependsOn(Milestone)
        .OnlyWhenStatic(() => GitRepository.IsOnReleaseBranch() || GitRepository.IsOnHotfixBranch())
        .Executes(() =>
        {
            string? changelogFile = From<IHazChangelog>().ChangelogFile;
            FinalizeChangelog(changelogFile, MajorMinorPatchVersion, GitRepository);
            Log.Information("Please review CHANGELOG.md and press any key to continue ...");
            System.Console.ReadKey();

            Git($"add {changelogFile}");
            Git($"commit -m \"chore: {Path.GetFileName(changelogFile)} for {MajorMinorPatchVersion}\"");
        });

    [UsedImplicitly]
    Target Release => _ => _
        .DependsOn(Changelog)
        .Requires(() => !GitRepository.IsOnReleaseBranch() || GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            if (!GitRepository.IsOnReleaseBranch())
                Checkout($"{ReleaseBranchPrefix}/{MajorMinorPatchVersion}", start: DevelopBranch);
            else
                FinishReleaseOrHotfix();
        });

    [UsedImplicitly]
    Target Hotfix => _ => _
        .DependsOn(Changelog)
        .Requires(() => !GitRepository.IsOnHotfixBranch() || GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            GitVersion? masterVersion = GitVersion(s => s
                .SetFramework("netcoreapp3.1")
                .SetUrl(RootDirectory)
                .SetBranch(MasterBranch)
                .EnableNoFetch()
                .DisableProcessOutputLogging()).Result;

            if (!GitRepository.IsOnHotfixBranch())
                Checkout($"{HotfixBranchPrefix}/{masterVersion.Major}.{masterVersion.Minor}.{masterVersion.Patch + 1}", start: MasterBranch);
            else
                FinishReleaseOrHotfix();
        });

    void FinishReleaseOrHotfix()
    {
        Git($"checkout {MasterBranch}");
        Git($"merge --no-ff --no-edit {GitRepository.Branch}");
        Git($"tag {MilestoneTitle}");

        Git($"checkout {DevelopBranch}");
        Git($"merge --no-ff --no-edit {GitRepository.Branch}");

        Git($"branch -D {GitRepository.Branch}");

        Git($"push origin {MasterBranch} {DevelopBranch} {MilestoneTitle}");
    }

    void Checkout(string branch, string start)
    {
        bool hasTrackedChanges = Git("status --short").Any(x => !x.Text.StartsWith("??"));
        bool stashed = false;

        if (hasTrackedChanges && AutoStash)
        {
            Git("stash push");
            stashed = true;
        }

        Git($"checkout -b {branch} {start}");

        if (stashed)
            Git("stash pop");
    }
}
