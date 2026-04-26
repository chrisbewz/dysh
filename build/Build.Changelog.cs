sealed partial class Build : IHazChangelog
{
    string IHazChangelog.ChangelogFile => AbsolutePath.Create(RootDirectory / "CHANGELOG.md");
}