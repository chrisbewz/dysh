using Helpers;

sealed partial class Build: IHazArtifacts
{
    AbsolutePath SourceDirectory   => RootDirectory / "src";
    
    AbsolutePath TestsDirectory    => RootDirectory / "tests";
    AbsolutePath ArtifactsDir      => this.As<IHazArtifacts>().ArtifactsDirectory;
    
    AbsolutePath NuGetDir          => ArtifactsDir / "nuget";
    
    AbsolutePath CoverageDir       => ArtifactsDir / "coverage";
    
    AbsolutePath TestResultsDir     => ArtifactsDir / "test-results";
}
