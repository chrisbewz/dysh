using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;

[GitHubActions(
    name: Workflows.CiWorkflow,
    image: GitHubActionsImage.UbuntuLatest,
    images: [GitHubActionsImage.WindowsLatest, GitHubActionsImage.MacOsLatest],
    FetchDepth = 0,
    AutoGenerate = true,
    OnPushBranches = [MasterBranch, DevelopBranch, $"{ReleaseBranchPrefix}/**"],
    OnPullRequestBranches = [MasterBranch, DevelopBranch],
    InvokedTargets = [nameof(ITest.Test), nameof(IPack.Pack)],
    ImportSecrets = [nameof(IPublish.NuGetApiKey)])]
[GitHubActions(
    name: Workflows.PublishWorkflow,
    image: GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    FetchDepth = 0,
    OnPushTags = ["v*"],
    InvokedTargets =  [nameof(IPublish.Publish)],
    ImportSecrets =  [nameof(PublicNuGetApiKey)])]
[GitHubActions(
    Workflows.AlphaDeployment,
    GitHubActionsImage.UbuntuLatest,
    FetchDepth = 0,
    OnPushBranches = [DevelopBranch],
    InvokedTargets = [nameof(IPublish.Publish)],
    EnableGitHubToken = true,
    PublishArtifacts = false,
    ImportSecrets = [nameof(FeedzNuGetApiKey)])]
sealed partial class Build
{
    [CI] readonly GitHubActions GitHubActions;
    
    public static class Workflows
    {
        public const string AlphaDeployment = "alpha-deployment";
    
        public const string CiWorkflow = "ci";
    
        public const string PublishWorkflow = "publish";
    }
}