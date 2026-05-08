using Nuke.Common.Tools.GitHub;
using Octokit;

sealed partial class Build
{
    Target ICreateGitHubRelease.CreateGitHubRelease => _ => _
        .Inherit<ICreateGitHubRelease>()
        .TriggeredBy<IPublish>()
        .ProceedAfterFailure()
        .OnlyWhenStatic(() => GitRepository.IsOnMasterBranch())
        .Executes(async () =>
        {
            IReadOnlyList<Issue> issues = await GitRepository.GetGitHubMilestoneIssues(MilestoneTitle);
            foreach (Issue issue in issues)
                await GitHubActions.CreateComment(issue.Number, $"Released in {MilestoneTitle}!");
        });
    
    string ICreateGitHubRelease.Name => MajorMinorPatchVersion;
    IEnumerable<AbsolutePath> ICreateGitHubRelease.AssetFiles => NuGetPackageFiles;
}
