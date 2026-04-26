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
    public void Should_not_configure_source_extraction_with_assign_target_paths_only()
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
        configuredTargets.Should().NotBeEquivalentTo(["AssignTargetPaths"]);
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
    public void Should_not_configure_api_manifest_generation_with_assign_target_paths_only()
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
        configuredTargets.Should().NotBeEquivalentTo(["AssignTargetPaths"]);
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
        using var process = Process.Start(startInfo);
        process.Should().NotBeNull();

        var outputTask = process!.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        var output = await outputTask;
        var error = await errorTask;

        // assert
        process.ExitCode.Should().Be(0, $"MSBuild output:\n{output}\nMSBuild errors:\n{error}");
    }

    private static async Task<ExtractionFixture> CreateExtractionFixtureAsync(
        bool includeReferencedProject,
        bool includeDuplicatePureCsType
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
            Path.Combine(projectDirectory, "Sources", "TrainCatalog.cs"),
            """
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
