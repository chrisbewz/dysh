using Nuke.Common.Tools.GitVersion;

sealed partial class Build: IHazGitVersion
{ 
    GitVersion GitVersion => ((IHazGitVersion)this).Versioning;
    
    string MajorMinorPatchVersion => Major ? $"{GitVersion.Major + 1}.0.0" : GitVersion.MajorMinorPatch;
    string MilestoneTitle => $"v{MajorMinorPatchVersion}";
    
    const string DefaultDeploymentVersion = "999.9.9";
    
}
