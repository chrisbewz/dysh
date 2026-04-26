sealed partial class Build
{
    [Parameter("Build configuration. Default: Debug (local) / Release (CI)")]
    readonly Configuration Configuration = IsLocalBuild
        ? Configuration.Debug
        : Configuration.Release;
    
    [Parameter] 
    readonly bool Major;
}
