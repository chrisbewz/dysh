using Helpers;
using NuGet.Packaging;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;

sealed partial class Build: IPack
{
    AbsolutePath IPack.PackagesDirectory => NuGetDir;
    
    const string BasePackagePrefix = "Dysh";
    
    IEnumerable<AbsolutePath> NuGetPackageFiles
        => From<IPack>().PackagesDirectory.GlobFiles("*.nupkg");
    
    Configure<DotNetPackSettings> IPack.PackSettings => s => s
        .SetProject(this.As<IHazSolution>().Solution.GetProject(BasePackagePrefix))
        .SetNoBuild(true)
        .SetNoRestore(true)
        .When(s => Host is Terminal or GitHubActions { Workflow: Workflows.AlphaDeployment }, s => s
            .SetVersion(DefaultDeploymentVersion))
        .When(s => !IsPublicRelease, s => s
            .SetPackageReleaseNotes(string.Empty));
    
    Target DeletePackages => _ => _
        .DependentFor<IPublish>()
        .After<IPack>()
        .OnlyWhenStatic(() => Host is Terminal or GitHubActions { Workflow: Workflows.AlphaDeployment })
        .Executes(() =>
        {
            switch (Host)
            {
                case Terminal:
                {
                    AbsolutePath packagesDirectory = NuGetPackageResolver.GetPackagesDirectory(packagesConfigFile: BuildProjectFile);
                    IReadOnlyCollection<AbsolutePath> packageDirectories = packagesDirectory.GlobDirectories($"{BasePackagePrefix}.*/{DefaultDeploymentVersion}");
                    packageDirectories.DeleteDirectories();
                    break;
                }
                case Nuke.Common.CI.GitHubActions.GitHubActions:
                {
                    void DeletePackage(string id, string version)
                        => DotNet(
                            $"nuget delete {id} {version} --source {FeedzNuGetSource} --api-key {FeedzNuGetApiKey} --non-interactive",
                            logOutput: false);

                    IEnumerable<string> packageIds = NuGetPackageFiles.Select(x => new PackageArchiveReader(x).NuspecReader.GetId());
                    foreach (string packageId in packageIds)
                        ControlFlow.SuppressErrors(() => DeletePackage(packageId, DefaultDeploymentVersion), logWarning: false);
                    break;
                }
            }
        });
}
