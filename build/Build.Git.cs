using static Nuke.Common.Tools.Git.GitTasks;

sealed partial class Build: IHazGitRepository, ICreateGitHubRelease
{
    const string MasterBranch = "main";
    
    const string DevelopBranch = "develop";
    
    GitRepository GitRepository => From<IHazGitRepository>().GitRepository;
    
    [Parameter] readonly bool AutoStash = true;

    void Checkout(string branch, string start)
    {
        bool hasCleanWorkingCopy = GitHasCleanWorkingCopy();

        if (!hasCleanWorkingCopy && AutoStash)
            Git("stash");

        Git($"checkout -b {branch} {start}");

        if (!hasCleanWorkingCopy && AutoStash)
            Git("stash apply");
    }
}