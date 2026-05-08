sealed partial class Build: IHazGitRepository, ICreateGitHubRelease
{
    const string MasterBranch = "main";
    
    const string DevelopBranch = "develop";
    
    const string ReleaseBranchPrefix = "release";
    
    const string HotfixBranchPrefix = "hotfix";
    
    GitRepository GitRepository => From<IHazGitRepository>().GitRepository;
}
