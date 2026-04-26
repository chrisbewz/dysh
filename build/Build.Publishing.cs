using Helpers;
using Nuke.Common.CI.GitHubActions;

sealed partial class Build: IPublish
{
    string PublicNuGetSource => "https://api.nuget.org/v3/index.json";
    
    string FeedzNuGetSource => "https://f.feedz.io/bzko/dysh-preview/nuget/index.json";
    
    Target IPublish.Publish => _ => _
        .Inherit<IPublish>(t => t.Publish)
        .Requires(() => IsPublicRelease || this.As<IHazGitRepository>().GitRepository.IsOnDevelopBranch() && Host is GitHubActions && GitHubActions.Workflow == Workflows.AlphaDeployment)
        .WhenSkipped(DependencyBehavior.Execute);
    
    [Parameter] [Secret] readonly string PublicNuGetApiKey;
    [Parameter] [Secret] readonly string FeedzNuGetApiKey;

    bool IsPublicRelease => GitRepository.IsOnMasterBranch() || GitRepository.IsOnReleaseBranch();
    
    string IPublish.NuGetSource => IsPublicRelease ? PublicNuGetSource : FeedzNuGetSource;
    
    string IPublish.NuGetApiKey => IsPublicRelease ? PublicNuGetApiKey : FeedzNuGetApiKey;
}