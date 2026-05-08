using Helpers;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;

sealed partial class Build: IReportCoverage, ITest
{
    const string MainTestProject = "Dysh.UnitTests";
    
    const string IntegrationTestProject = "Dysh.IntegrationTests";
    
    Configure<DotNetTestSettings, Project> ITest.TestProjectSettings => (outer, v) => outer
        .SetNoBuild(true)
        .SetNoRestore(true)
        .EnableCollectCoverage()
        .SetCoverletOutputFormat(CoverletOutputFormat.opencover)
        .When(s => v.Name == MainTestProject, settings => settings
        .SetCoverletOutput(CoverageDir / "unit.opencover.xml")
        .SetResultsDirectory(TestResultsDir / "unit"))
        .When(s => v.Name == IntegrationTestProject, settings => settings
        .SetCoverletOutput(CoverageDir / "integration.opencover.xml")
        .SetResultsDirectory(TestResultsDir / "integration"));
    
    Target ITest.Test => _ => _
        .Inherit<ITest>(t => t.Test)
        .Description("Runs all test suites (unit + integration).");

    bool IReportCoverage.CreateCoverageHtmlReport => true;
    
    bool IReportCoverage.ReportToCodecov => false;
    
    [Parameter]
    int ITest.TestDegreeOfParallelism => 1;

    Configure<DotNetTestSettings> ITest.TestSettings => _ => _
        .SetProcessEnvironmentVariable("NUKE_TELEMETRY_OPTOUT", bool.TrueString);
    
    IEnumerable<Project> ITest.TestProjects => Partition.GetCurrent(this.As<IHazSolution>().Solution.GetAllProjects("*Tests"));
}
