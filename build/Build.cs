using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Ci);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Collect code coverage. Default is 'true'")]
    readonly bool Cover = true;

    [Solution]
    readonly Solution Solution;

    //[MinVer]
    //readonly MinVer MinVer;

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    AbsolutePath TestsDirectory => ArtifactsDirectory / "tests";

    AbsolutePath CoverageFile => ArtifactsDirectory / "coverage" / "coverage";

    AbsolutePath NugetDirectory => ArtifactsDirectory / "nuget";

    AbsolutePath DotnetDirectory => RootDirectory / "dotnet";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            ArtifactsDirectory.CreateOrCleanDirectory();
            DotNetClean(v => v.SetProject(DotnetDirectory));
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });


    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });


    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution.GetProject("Allors.Embedded.Tests"))
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore()
                .AddLoggers("trx;LogFileName=Allors.Embedded.Tests.trx")
                .EnableProcessLogOutput()
                .SetResultsDirectory(TestsDirectory)
                .When(Cover, _ => _
                    .EnableCollectCoverage()
                    .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                    .SetCoverletOutput(CoverageFile)
                    .SetExcludeByFile("*.g.cs")
                    .When(IsServerBuild, _ => _
                        .EnableUseSourceLink()))
            );
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(Solution.GetProject("Allors.Embedded"))
                .SetConfiguration(Configuration)
                .EnableIncludeSource()
                .EnableIncludeSymbols()
                .SetOutputDirectory(NugetDirectory));
        });

    Target CiNonWin => _ => _
        .DependsOn(Test);

    Target Ci => _ => _
        .DependsOn(Pack, Test);
}
