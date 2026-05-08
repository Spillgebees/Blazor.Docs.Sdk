using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using AwesomeAssertions;
using Spillgebees.Blazor.Docs.Sdk;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Build;

public class ApiManifestToolIntegrationTests
{
    [Test]
    public async Task Should_generate_manifest_only_for_docs_api_project_reference()
    {
        // arrange
        var fixture = await CreateFixtureAsync();

        try
        {
            // act
            await ExecuteProcessAsync(
                "dotnet",
                $"msbuild \"{fixture.DocsProjectPath}\" -nologo -restore -t:DocsSdk_GenerateApiManifest",
                TimeSpan.FromSeconds(60)
            );

            // assert
            var manifestPath = Path.Combine(
                fixture.DocsProjectDirectory,
                "obj",
                "DocsSdk",
                "manifests",
                "DocumentedLibrary.json"
            );
            File.Exists(manifestPath).Should().BeTrue();
            File.Exists(
                    Path.Combine(fixture.DocsProjectDirectory, "obj", "DocsSdk", "manifests", "UnoptedLibrary.json")
                )
                .Should()
                .BeFalse();
        }
        finally
        {
            TryDeleteDirectory(fixture.RootDirectory);
        }
    }

    [Test]
    public async Task Should_generate_roslyn_manifest_with_public_surface_and_signatures()
    {
        // arrange
        var fixture = await CreateFixtureAsync();

        try
        {
            // act
            await ExecuteProcessAsync(
                "dotnet",
                $"msbuild \"{fixture.DocsProjectPath}\" -nologo -restore -t:DocsSdk_GenerateApiManifest",
                TimeSpan.FromSeconds(60)
            );

            var manifest = await ReadManifestAsync(fixture);

            // assert
            manifest.Types.Select(x => x.Id).Should().NotContain("DocumentedLibrary.InternalHelper");
            manifest.Types.Select(x => x.Name).Should().NotContain("_Imports");

            var component = manifest.Types.Single(x => x.Id == "DocumentedLibrary.TrackedEntityLayer`1");
            component.Name.Should().Be("TrackedEntityLayer<TItem>");
            component.Slug.Should().Be("TrackedEntityLayer-1");
            component.Kind.Should().Be("component");
            component
                .SummaryHtml.Should()
                .Contain(
                    """<a href="https://example.com/map" target="_blank" rel="noopener noreferrer">map docs</a>"""
                );
            component.SummaryHtml.Should().Contain("""<a href="api/MapMode"><code>MapMode</code></a>""");
            component.SummaryHtml.Should().Contain("<ul><li>Supports <code>tracked items</code>.</li></ul>");
            component
                .RemarksHtml.Should()
                .Contain("<pre><code>var layer = new TrackedEntityLayer&lt;string&gt;();</code></pre>");
            component.GenericParameters.Single().Constraints.Should().Contain("class");

            component
                .Properties.Single(x => x.Name == "Items")
                .Type.Should()
                .Be("System.Collections.Generic.IReadOnlyList<TItem>?");
            component.Properties.Single(x => x.Name == "Items").IsParameter.Should().BeTrue();
            component.Properties.Single(x => x.Name == "Theme").IsCascadingParameter.Should().BeTrue();

            var method = component.Methods.Single(x => x.Name == "Find" && x.Parameters.Count == 2);
            method.ReturnType.Should().Be("TItem?");
            method.Parameters[0].Name.Should().Be("id");
            method.Parameters[0].Type.Should().Be("string");
            method.Parameters[0].Summary.Should().Be("Entity id.");
            method.Parameters[0].SummaryHtml.Should().Be("Entity id.");
            method.Parameters[1].Name.Should().Be("includeInactive");
            method.Parameters[1].Type.Should().Be("bool");
            method.Parameters[1].IsOptional.Should().BeTrue();
            method.Parameters[1].DefaultValue.Should().Be("false");

            component.Methods.Count(x => x.Name == "Find").Should().Be(2);

            var enumType = manifest.Types.Single(x => x.Id == "DocumentedLibrary.MapMode");
            enumType.EnumValues.Single(x => x.Name == "Hybrid").Value.Should().Be(42);
        }
        finally
        {
            TryDeleteDirectory(fixture.RootDirectory);
        }
    }

    [Test]
    public async Task Packed_documented_library_should_not_contain_docs_sdk_manifest_artifacts()
    {
        // arrange
        var fixture = await CreateFixtureAsync();
        var packageDirectory = Path.Combine(fixture.RootDirectory, "packages");
        Directory.CreateDirectory(packageDirectory);

        try
        {
            // act
            await ExecuteProcessAsync(
                "dotnet",
                $"pack \"{fixture.DocumentedProjectPath}\" -nologo -o \"{packageDirectory}\"",
                TimeSpan.FromSeconds(60)
            );

            // assert
            var packagePath = Directory.GetFiles(packageDirectory, "DocumentedLibrary.*.nupkg").Single();
            using var archive = ZipFile.OpenRead(packagePath);
            archive.Entries.Select(x => x.FullName).Should().NotContain(path => IsDocsSdkManifestArtifact(path));
        }
        finally
        {
            TryDeleteDirectory(fixture.RootDirectory);
        }
    }

    private static async Task<ApiManifest> ReadManifestAsync(ApiManifestFixture fixture)
    {
        var manifestPath = Path.Combine(
            fixture.DocsProjectDirectory,
            "obj",
            "DocsSdk",
            "manifests",
            "DocumentedLibrary.json"
        );
        await using var stream = File.OpenRead(manifestPath);
        return (await JsonSerializer.DeserializeAsync<ApiManifest>(stream))!;
    }

    private static async Task<ApiManifestFixture> CreateFixtureAsync()
    {
        var rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "docs-sdk-api-manifest-tests",
            Guid.NewGuid().ToString("N")
        );
        var docsProjectDirectory = Path.Combine(rootDirectory, "DocsSite");
        var documentedProjectDirectory = Path.Combine(rootDirectory, "DocumentedLibrary");
        var unoptedProjectDirectory = Path.Combine(rootDirectory, "UnoptedLibrary");

        Directory.CreateDirectory(docsProjectDirectory);
        Directory.CreateDirectory(documentedProjectDirectory);
        Directory.CreateDirectory(unoptedProjectDirectory);

        var targetsPath = LocateRepositoryFile("src/Spillgebees.Blazor.Docs.Sdk/Spillgebees.Blazor.Docs.Sdk.targets");
        var normalizedTargetsPath = targetsPath.Replace("\\", "/", StringComparison.Ordinal);

        var docsProjectPath = Path.Combine(docsProjectDirectory, "DocsSite.csproj");
        await File.WriteAllTextAsync(
            docsProjectPath,
            $$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Nullable>enable</Nullable>
                <ImplicitUsings>enable</ImplicitUsings>
              </PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="../DocumentedLibrary/DocumentedLibrary.csproj" DocsApi="true" />
                <ProjectReference Include="../UnoptedLibrary/UnoptedLibrary.csproj" />
              </ItemGroup>
              <Import Project="{{normalizedTargetsPath}}" />
            </Project>
            """
        );

        await File.WriteAllTextAsync(
            Path.Combine(documentedProjectDirectory, "_Imports.razor"),
            """
            @using System
            """
        );

        var documentedProjectPath = Path.Combine(documentedProjectDirectory, "DocumentedLibrary.csproj");
        await File.WriteAllTextAsync(
            documentedProjectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Nullable>enable</Nullable>
                <ImplicitUsings>enable</ImplicitUsings>
                <GenerateDocumentationFile>true</GenerateDocumentationFile>
                <PackageId>DocumentedLibrary</PackageId>
                <Version>1.0.0</Version>
              </PropertyGroup>
              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
              </ItemGroup>
            </Project>
            """
        );

        await File.WriteAllTextAsync(
            Path.Combine(documentedProjectDirectory, "TrackedEntityLayer.cs"),
            """
            namespace DocumentedLibrary;

            using Microsoft.AspNetCore.Components;

            /// <summary>
            /// Tracks entities on a <see href="https://example.com/map">map docs</see>.
            /// <para>See <see cref="T:DocumentedLibrary.MapMode" /> for display modes.</para>
            /// <list type="bullet">
            /// <item><description>Supports <c>tracked items</c>.</description></item>
            /// </list>
            /// </summary>
            /// <remarks>
            /// <code>
            /// var layer = new TrackedEntityLayer&lt;string&gt;();
            /// </code>
            /// </remarks>
            /// <typeparam name="TItem">Entity type.</typeparam>
            public class TrackedEntityLayer<TItem> : ComponentBase
                where TItem : class
            {
                /// <summary>
                /// Gets or sets tracked items.
                /// </summary>
                [Parameter]
                public IReadOnlyList<TItem>? Items { get; set; }

                /// <summary>
                /// Gets or sets the inherited theme.
                /// </summary>
                [CascadingParameter]
                public string? Theme { get; set; }

                /// <summary>
                /// Finds an entity by <paramref name="id" />.
                /// </summary>
                /// <param name="id">Entity id.</param>
                /// <param name="includeInactive">Whether inactive entities are searched.</param>
                public TItem? Find(string id, bool includeInactive = false) => null;

                /// <summary>
                /// Finds an entity by numeric id.
                /// </summary>
                public TItem? Find(int id) => null;
            }
            """
        );

        await File.WriteAllTextAsync(
            Path.Combine(documentedProjectDirectory, "MapMode.cs"),
            """
            namespace DocumentedLibrary;

            /// <summary>
            /// Map display modes.
            /// </summary>
            public enum MapMode
            {
                /// <summary>Road mode.</summary>
                Road = 1,

                /// <summary>Hybrid mode.</summary>
                Hybrid = 42,
            }
            """
        );

        await File.WriteAllTextAsync(
            Path.Combine(documentedProjectDirectory, "InternalHelper.cs"),
            """
            namespace DocumentedLibrary;

            /// <summary>
            /// This documented helper must not appear in the manifest.
            /// </summary>
            internal class InternalHelper;
            """
        );

        await File.WriteAllTextAsync(
            Path.Combine(unoptedProjectDirectory, "UnoptedLibrary.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Nullable>enable</Nullable>
                <ImplicitUsings>enable</ImplicitUsings>
              </PropertyGroup>
            </Project>
            """
        );
        await File.WriteAllTextAsync(
            Path.Combine(unoptedProjectDirectory, "UnoptedType.cs"),
            """
            namespace UnoptedLibrary;

            public class UnoptedType;
            """
        );

        return new ApiManifestFixture(rootDirectory, docsProjectDirectory, docsProjectPath, documentedProjectPath);
    }

    private static async Task ExecuteProcessAsync(string fileName, string arguments, TimeSpan timeout)
    {
        using var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            }
        );
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

        if (timedOut)
        {
            throw new InvalidOperationException(
                $"Process timed out after {timeout}. Output:\n{output}\nErrors:\n{error}"
            );
        }

        process
            .ExitCode.Should()
            .Be(0, $"process exited with code {process.ExitCode}. Output:\n{output}\nErrors:\n{error}");
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static bool IsDocsSdkManifestArtifact(string path)
    {
        var fileName = Path.GetFileName(path);
        var isJson = fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        return isJson
            && (
                path.Contains("DocsSdk", StringComparison.OrdinalIgnoreCase)
                || path.Contains("ApiManifest", StringComparison.OrdinalIgnoreCase)
                || path.Contains("/manifests/", StringComparison.OrdinalIgnoreCase)
            );
    }

    private static string LocateRepositoryFile(string relativePath)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

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

    private sealed record ApiManifestFixture(
        string RootDirectory,
        string DocsProjectDirectory,
        string DocsProjectPath,
        string DocumentedProjectPath
    );
}
