// Copyright 2023 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE



// partial class Build : ISignPackages
// {
//     public IEnumerable<AbsolutePath> SignPathPackages => this.As<IPack>().PackagesDirectory.GlobFiles("*.nupkg");
//
//     public Target SignPackages => _ => _
//         .Inherit<ISignPackages>()
//         .OnlyWhenStatic(() => IsPublicRelease)
//         .OnlyWhenStatic(() => EnvironmentInfo.IsWin);
// }