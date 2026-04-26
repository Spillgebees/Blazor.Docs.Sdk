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
