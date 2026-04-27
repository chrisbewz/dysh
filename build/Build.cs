using Helpers;
using Nuke.Common.CI;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities.Collections;

[ShutdownDotNetAfterServerBuild]
sealed partial class Build : NukeBuild, IHazSolution
{
    [Solution(GenerateProjects = true)] 
    readonly Nuke.Common.ProjectModel.Solution Solution;
    
    Nuke.Common.ProjectModel.Solution IHazSolution.Solution => Solution;
    
    public static int Main() => Execute<Build>(x => ((IPack)x).Pack);

    public Target Clean => _ => _
        .Before(this.As<IRestore>().Restore)
        .Description("Removes build outputs and artifact directories.")
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(d => d.DeleteDirectory());
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(d => d.DeleteDirectory());
            ArtifactsDir.CreateOrCleanDirectory();
        });

    Target IRestore.Restore => _ => _
        .Description("Restores NuGet packages for the entire solution.")
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(this.As<IHazSolution>().Solution));
        });

    Target ICompile.Compile => _ => _
        .DependsOn(this.As<IRestore>().Restore)
        .Description("Compiles the solution with the version derived from GitVersion.")
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(this.As<IHazSolution>().Solution)
                .SetConfiguration(Configuration)
                .SetNoRestore(true)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion));
        });
}
