using System.Diagnostics;
using System.Xml.Linq;
using AwesomeAssertions;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Build;

public class BuildTargetsTests
{
    [Test]
    public void Should_schedule_source_extraction_before_prepare_resource_names()
    {
        // arrange
        var targetsPath = LocateRepositoryFile("src/Spillgebees.Blazor.Docs.Sdk/Spillgebees.Blazor.Docs.Sdk.targets");
        var document = XDocument.Load(targetsPath);
        var sourceTarget = document
            .Root!.Elements("Target")
            .Single(x => x.Attribute("Name")?.Value == "DocsSdk_ExtractSources");
        var beforeTargets = sourceTarget.Attribute("BeforeTargets")?.Value ?? string.Empty;

        // act
        var configuredTargets = beforeTargets
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // assert
        configuredTargets.Should().Contain("PrepareResourceNames");
        configuredTargets.Should().Contain("AssignTargetPaths");
    }

    [Test]
    public void Should_schedule_source_extraction_before_prepare_resource_names_and_assign_target_paths()
    {
        // arrange
        var targetsPath = LocateRepositoryFile("src/Spillgebees.Blazor.Docs.Sdk/Spillgebees.Blazor.Docs.Sdk.targets");
        var document = XDocument.Load(targetsPath);
        var sourceTarget = document
            .Root!.Elements("Target")
            .Single(x => x.Attribute("Name")?.Value == "DocsSdk_ExtractSources");
        var beforeTargets = sourceTarget.Attribute("BeforeTargets")?.Value ?? string.Empty;

        // act
        var configuredTargets = beforeTargets
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // assert
        configuredTargets.Should().BeEquivalentTo(["PrepareResourceNames", "AssignTargetPaths"]);
    }

    [Test]
    public void Should_schedule_api_manifest_generation_before_prepare_resource_names()
    {
        // arrange
        var targetsPath = LocateRepositoryFile("src/Spillgebees.Blazor.Docs.Sdk/Spillgebees.Blazor.Docs.Sdk.targets");
        var document = XDocument.Load(targetsPath);
        var apiManifestTarget = document
            .Root!.Elements("Target")
            .Single(x => x.Attribute("Name")?.Value == "DocsSdk_GenerateApiManifest");
        var beforeTargets = apiManifestTarget.Attribute("BeforeTargets")?.Value ?? string.Empty;

        // act
        var configuredTargets = beforeTargets
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // assert
        configuredTargets.Should().Contain("PrepareResourceNames");
        configuredTargets.Should().Contain("AssignTargetPaths");
    }

    [Test]
    public void Should_schedule_api_manifest_generation_before_prepare_resource_names_and_assign_target_paths()
    {
        // arrange
        var targetsPath = LocateRepositoryFile("src/Spillgebees.Blazor.Docs.Sdk/Spillgebees.Blazor.Docs.Sdk.targets");
        var document = XDocument.Load(targetsPath);
        var apiManifestTarget = document
            .Root!.Elements("Target")
            .Single(x => x.Attribute("Name")?.Value == "DocsSdk_GenerateApiManifest");
        var beforeTargets = apiManifestTarget.Attribute("BeforeTargets")?.Value ?? string.Empty;

        // act
        var configuredTargets = beforeTargets
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // assert
        configuredTargets.Should().BeEquivalentTo(["PrepareResourceNames", "AssignTargetPaths"]);
    }

    [Test]
    public async Task Should_fail_with_timeout_and_include_captured_output()
    {
        // arrange
        var startInfo = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = "-lc \"echo ready; sleep 5\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        // act
        var action = async () => await ExecuteProcessAsync(startInfo, TimeSpan.FromMilliseconds(200));

        // assert
        var exception = await action.Should().ThrowAsync<Exception>();
        exception.Which.Message.Should().Contain("timed out");
        exception.Which.Message.Should().Contain("MSBuild output:\nready");
        exception.Which.Message.Should().Contain("MSBuild errors:\n");
    }

    [Test]
    public async Task Should_fail_with_non_zero_exit_and_include_captured_output()
    {
        // arrange
        var startInfo = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = "-lc \"echo out; echo err 1>&2; exit 17\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        // act
        var action = async () => await ExecuteProcessAsync(startInfo, TimeSpan.FromSeconds(5));

        // assert
        var exception = await action.Should().ThrowAsync<Exception>();
        exception.Which.Message.Should().Contain("exited with code 17");
        exception.Which.Message.Should().Contain("MSBuild output:\nout");
        exception.Which.Message.Should().Contain("MSBuild errors:\nerr");
    }

    [Test]
    public async Task Should_extract_pure_csharp_additional_sources_with_fully_qualified_typeof()
    {
        // arrange
        var fixture = await CreateExtractionFixtureAsync(
            includeReferencedProject: false,
            includeDuplicatePureCsType: false
        );

        try
        {
            // act
            await ExecuteExtractSourcesTargetAsync(fixture.ProjectPath);

            var capturedItems = await File.ReadAllLinesAsync(fixture.CapturedItemsPath);
            var embeddedSourcePath = Path.Combine(
                fixture.ProjectDirectory,
                "obj",
                "DocsSdk",
                "sources",
                "Samples.Trains.TrainCatalog.TrainCatalog.cs"
            );
            var embeddedSourceContent = await File.ReadAllTextAsync(embeddedSourcePath);

            // assert
            capturedItems
                .Should()
                .Contain(line =>
                    line.StartsWith(
                        "SourceEmbed:Samples.Trains.TrainCatalog:TrainCatalog.cs|",
                        StringComparison.Ordinal
                    )
                );
            embeddedSourceContent.Should().Contain("public const string Marker = \"PRIMARY\";");
        }
        finally
        {
            Directory.Delete(fixture.RootDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Should_not_overwrite_destination_source_when_duplicate_logical_name_is_discovered()
    {
        // arrange
        var fixture = await CreateExtractionFixtureAsync(
            includeReferencedProject: true,
            includeDuplicatePureCsType: true
        );

        try
        {
            // act
            await ExecuteExtractSourcesTargetAsync(fixture.ProjectPath);

            var capturedItems = await File.ReadAllLinesAsync(fixture.CapturedItemsPath);
            var embeddedSourcePath = Path.Combine(
                fixture.ProjectDirectory,
                "obj",
                "DocsSdk",
                "sources",
                "Samples.Trains.TrainCatalog.TrainCatalog.cs"
            );
            var embeddedSourceContent = await File.ReadAllTextAsync(embeddedSourcePath);

            // assert
            capturedItems
                .Count(line =>
                    line.StartsWith(
                        "SourceEmbed:Samples.Trains.TrainCatalog:TrainCatalog.cs|",
                        StringComparison.Ordinal
                    )
                )
                .Should()
                .Be(1);
            embeddedSourceContent.Should().Contain("public const string Marker = \"PRIMARY\";");
            embeddedSourceContent.Should().NotContain("public const string Marker = \"SECONDARY\";");
        }
        finally
        {
            Directory.Delete(fixture.RootDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Should_ignore_commented_csharp_namespace_declaration_when_deriving_logical_name()
    {
        // arrange
        var fixture = await CreateExtractionFixtureAsync(
            includeReferencedProject: false,
            includeDuplicatePureCsType: false,
            trainCatalogSource: """
            // namespace Samples.Commented;

            public class TrainCatalog
            {
                public const string Marker = "PRIMARY";
            }
            """
        );

        try
        {
            // act
            await ExecuteExtractSourcesTargetAsync(fixture.ProjectPath);

            var capturedItems = await File.ReadAllLinesAsync(fixture.CapturedItemsPath);

            // assert
            capturedItems
                .Should()
                .Contain(line =>
                    line.StartsWith(
                        "SourceEmbed:DocsSite.Sources.TrainCatalog:TrainCatalog.cs|",
                        StringComparison.Ordinal
                    )
                );
            capturedItems
                .Should()
                .NotContain(line =>
                    line.StartsWith(
                        "SourceEmbed:Samples.Commented.TrainCatalog:TrainCatalog.cs|",
                        StringComparison.Ordinal
                    )
                );
        }
        finally
        {
            Directory.Delete(fixture.RootDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Should_ignore_multiline_commented_csharp_namespace_declaration_when_deriving_logical_name()
    {
        // arrange
        var fixture = await CreateExtractionFixtureAsync(
            includeReferencedProject: false,
            includeDuplicatePureCsType: false,
            trainCatalogSource: """
            /*
               namespace Samples.Commented;
            */

            public class TrainCatalog
            {
                public const string Marker = "PRIMARY";
            }
            """
        );

        try
        {
            // act
            await ExecuteExtractSourcesTargetAsync(fixture.ProjectPath);

            var capturedItems = await File.ReadAllLinesAsync(fixture.CapturedItemsPath);

            // assert
            capturedItems
                .Should()
                .Contain(line =>
                    line.StartsWith(
                        "SourceEmbed:DocsSite.Sources.TrainCatalog:TrainCatalog.cs|",
                        StringComparison.Ordinal
                    )
                );
            capturedItems
                .Should()
                .NotContain(line =>
                    line.StartsWith(
                        "SourceEmbed:Samples.Commented.TrainCatalog:TrainCatalog.cs|",
                        StringComparison.Ordinal
                    )
                );
        }
        finally
        {
            Directory.Delete(fixture.RootDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Should_ignore_commented_razor_namespace_declaration_when_deriving_logical_name()
    {
        // arrange
        var fixture = await CreateExtractionFixtureAsync(
            includeReferencedProject: false,
            includeDuplicatePureCsType: false,
            trainTrackingExampleSource: """
            @* @namespace Samples.Commented *@

            <div>tracking</div>
            """
        );

        try
        {
            // act
            await ExecuteExtractSourcesTargetAsync(fixture.ProjectPath);

            var capturedItems = await File.ReadAllLinesAsync(fixture.CapturedItemsPath);

            // assert
            capturedItems
                .Should()
                .Contain(line =>
                    line.StartsWith(
                        "SourceEmbed:DocsSite.Sources.TrainTrackingExample:TrainTrackingExample.razor|",
                        StringComparison.Ordinal
                    )
                );
            capturedItems
                .Should()
                .NotContain(line =>
                    line.StartsWith(
                        "SourceEmbed:Samples.Commented.TrainTrackingExample:TrainTrackingExample.razor|",
                        StringComparison.Ordinal
                    )
                );
        }
        finally
        {
            Directory.Delete(fixture.RootDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Should_ignore_multiline_commented_razor_namespace_declaration_when_deriving_logical_name()
    {
        // arrange
        var fixture = await CreateExtractionFixtureAsync(
            includeReferencedProject: false,
            includeDuplicatePureCsType: false,
            trainTrackingExampleSource: """
            @*
              @namespace Samples.Commented
            *@

            <div>tracking</div>
            """
        );

        try
        {
            // act
            await ExecuteExtractSourcesTargetAsync(fixture.ProjectPath);

            var capturedItems = await File.ReadAllLinesAsync(fixture.CapturedItemsPath);

            // assert
            capturedItems
                .Should()
                .Contain(line =>
                    line.StartsWith(
                        "SourceEmbed:DocsSite.Sources.TrainTrackingExample:TrainTrackingExample.razor|",
                        StringComparison.Ordinal
                    )
                );
            capturedItems
                .Should()
                .NotContain(line =>
                    line.StartsWith(
                        "SourceEmbed:Samples.Commented.TrainTrackingExample:TrainTrackingExample.razor|",
                        StringComparison.Ordinal
                    )
                );
        }
        finally
        {
            Directory.Delete(fixture.RootDirectory, recursive: true);
        }
    }

    [Test]
    public void Should_cache_pure_csharp_file_discovery_per_search_directory()
    {
        // arrange
        var targetsPath = LocateRepositoryFile("src/Spillgebees.Blazor.Docs.Sdk/Spillgebees.Blazor.Docs.Sdk.targets");
        var targetsContent = File.ReadAllText(targetsPath);

        // act
        var usesPerTypeScan = targetsContent.Contains(
            "Directory.GetFiles(searchDir, typeName + \".cs\", SearchOption.AllDirectories)",
            StringComparison.Ordinal
        );
        var usesDirectoryWideScan = targetsContent.Contains(
            "Directory.GetFiles(searchDir, \"*.cs\", SearchOption.AllDirectories)",
            StringComparison.Ordinal
        );

        // assert
        usesPerTypeScan.Should().BeFalse();
        usesDirectoryWideScan.Should().BeTrue();
    }

    private static async Task ExecuteExtractSourcesTargetAsync(string projectPath)
    {
        // arrange
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"msbuild \"{projectPath}\" -nologo -restore -t:DocsSdk_ExtractSources",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        // act
        await ExecuteProcessAsync(startInfo, TimeSpan.FromSeconds(30));
    }

    private static async Task ExecuteProcessAsync(ProcessStartInfo startInfo, TimeSpan timeout)
    {
        // arrange
        using var process = Process.Start(startInfo);
        process.Should().NotBeNull();

        var outputTask = process!.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        using var cts = new CancellationTokenSource(timeout);

        var timedOut = false;
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            timedOut = true;
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
        }

        var output = await outputTask;
        var error = await errorTask;

        // assert
        if (timedOut)
        {
            throw new InvalidOperationException(
                $"MSBuild execution timed out after {timeout}. MSBuild output:\n{output}\nMSBuild errors:\n{error}"
            );
        }

        process
            .ExitCode.Should()
            .Be(
                0,
                $"MSBuild process exited with code {process.ExitCode}. MSBuild output:\n{output}\nMSBuild errors:\n{error}"
            );
    }

    private static async Task<ExtractionFixture> CreateExtractionFixtureAsync(
        bool includeReferencedProject,
        bool includeDuplicatePureCsType,
        string? trainCatalogSource = null,
        string? trainTrackingExampleSource = null
    )
    {
        // arrange
        var rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "docs-sdk-build-target-tests",
            Guid.NewGuid().ToString("N")
        );
        var projectDirectory = Path.Combine(rootDirectory, "DocsSite");
        var projectPath = Path.Combine(projectDirectory, "DocsSite.csproj");
        var capturedItemsPath = Path.Combine(projectDirectory, "obj", "captured-embedded-sources.txt");

        Directory.CreateDirectory(projectDirectory);
        Directory.CreateDirectory(Path.Combine(projectDirectory, "Docs"));
        Directory.CreateDirectory(Path.Combine(projectDirectory, "Sources"));

        var targetsPath = LocateRepositoryFile("src/Spillgebees.Blazor.Docs.Sdk/Spillgebees.Blazor.Docs.Sdk.targets");
        var normalizedTargetsPath = targetsPath.Replace("\\", "/", StringComparison.Ordinal);

        var projectReferenceXml = string.Empty;
        if (includeReferencedProject)
        {
            projectReferenceXml = """
                  <ItemGroup>
                    <ProjectReference Include="../ReferenceLibrary/ReferenceLibrary.csproj" />
                  </ItemGroup>
                """;
        }

        var projectContent = $$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <RootNamespace>DocsSite</RootNamespace>
              </PropertyGroup>
            {{projectReferenceXml}}
              <Import Project="{{normalizedTargetsPath}}" />
              <Target Name="CaptureDocsSdkEmbeddedSources" AfterTargets="DocsSdk_ExtractSources">
                <WriteLinesToFile
                  File="$(BaseIntermediateOutputPath)captured-embedded-sources.txt"
                  Lines="@(_DocsSdk_EmbeddedSources->'%(LogicalName)|%(Identity)')"
                  Overwrite="true" />
              </Target>
            </Project>
            """;

        await File.WriteAllTextAsync(projectPath, projectContent);
        await File.WriteAllTextAsync(
            Path.Combine(projectDirectory, "Docs", "Example.razor"),
            """
            <ExampleView TComponent="TrainTrackingExample"
                         AdditionalSources="@(typeof(Samples.Trains.TrainCatalog))">
                <TrainTrackingExample />
            </ExampleView>
            """
        );
        await File.WriteAllTextAsync(
            Path.Combine(projectDirectory, "Sources", "TrainTrackingExample.razor"),
            trainTrackingExampleSource
                ?? """
                <div>tracking</div>
                """
        );
        await File.WriteAllTextAsync(
            Path.Combine(projectDirectory, "Sources", "TrainCatalog.cs"),
            trainCatalogSource
                ?? """
                namespace Samples.Trains;

                public class TrainCatalog
                {
                    public const string Marker = "PRIMARY";
                }
                """
        );

        if (includeReferencedProject)
        {
            var referencedProjectDirectory = Path.Combine(rootDirectory, "ReferenceLibrary");
            Directory.CreateDirectory(referencedProjectDirectory);

            await File.WriteAllTextAsync(
                Path.Combine(referencedProjectDirectory, "ReferenceLibrary.csproj"),
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                  </PropertyGroup>
                </Project>
                """
            );

            if (includeDuplicatePureCsType)
            {
                await File.WriteAllTextAsync(
                    Path.Combine(referencedProjectDirectory, "TrainCatalog.cs"),
                    """
                    namespace Samples.Trains;

                    public class TrainCatalog
                    {
                        public const string Marker = "SECONDARY";
                    }
                    """
                );
            }
        }

        // act
        var fixture = new ExtractionFixture(rootDirectory, projectDirectory, projectPath, capturedItemsPath);

        // assert
        File.Exists(projectPath).Should().BeTrue();

        return fixture;
    }

    private sealed record ExtractionFixture(
        string RootDirectory,
        string ProjectDirectory,
        string ProjectPath,
        string CapturedItemsPath
    );

    private static string LocateRepositoryFile(string relativePath)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var current = new DirectoryInfo(baseDirectory);

        while (current is not null)
        {
            var solutionPath = Path.Combine(current.FullName, "Blazor.Docs.Sdk.slnx");
            if (File.Exists(solutionPath))
            {
                return Path.Combine(current.FullName, relativePath);
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root from test execution directory.");
    }
}
