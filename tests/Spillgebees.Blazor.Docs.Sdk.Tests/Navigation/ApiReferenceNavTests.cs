using AwesomeAssertions;
using Spillgebees.Blazor.Docs.Sdk;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Navigation;

public class ApiReferenceNavTests
{
    [Test]
    public void Should_generate_sorted_relative_nav_pages_from_manifest()
    {
        // arrange
        var manifest = new ApiManifest
        {
            AssemblyName = "TestLib",
            Types =
            [
                new ApiTypeInfo
                {
                    Id = "Spillgebees.Blazor.Map.SgbMap",
                    Slug = "SgbMap",
                    Name = "SgbMap",
                    FullName = "Spillgebees.Blazor.Map.SgbMap",
                    Namespace = "Spillgebees.Blazor.Map",
                    Kind = "class",
                },
                new ApiTypeInfo
                {
                    Id = "Spillgebees.Blazor.Map.TrackedDataSource",
                    Slug = "TrackedDataSource",
                    Name = "TrackedDataSource",
                    FullName = "Spillgebees.Blazor.Map.TrackedDataSource",
                    Namespace = "Spillgebees.Blazor.Map",
                    Kind = "class",
                },
            ],
        };

        // act
        var pages = ApiReferenceNav.FromManifest(manifest);

        // assert
        pages.Should().HaveCount(2);
        pages[0].Title.Should().Be("SgbMap");
        pages[0].Href.Should().Be("api/SgbMap");
        pages.Should().OnlyContain(x => !x.Href.StartsWith("/", StringComparison.Ordinal));
    }

    [Test]
    public void Should_resolve_type_by_slug_and_legacy_full_name()
    {
        // arrange
        var manifest = new ApiManifest
        {
            AssemblyName = "TestLib",
            Types =
            [
                new ApiTypeInfo
                {
                    Id = "Spillgebees.Blazor.Map.Components.Layers.TrackedEntityLayer`1",
                    Slug = "TrackedEntityLayer-1",
                    Name = "TrackedEntityLayer<TItem>",
                    FullName = "Spillgebees.Blazor.Map.Components.Layers.TrackedEntityLayer`1",
                    Namespace = "Spillgebees.Blazor.Map.Components.Layers",
                    Kind = "component",
                },
            ],
        };

        // act
        var slugMatch = ApiReferenceNav.FindType(manifest, "TrackedEntityLayer-1");
        var legacyMatch = ApiReferenceNav.FindType(
            manifest,
            "Spillgebees.Blazor.Map.Components.Layers.TrackedEntityLayer`1"
        );

        // assert
        slugMatch.Should().NotBeNull();
        legacyMatch.Should().NotBeNull();
        slugMatch.Should().BeSameAs(legacyMatch);
    }

    [Test]
    public void Should_return_empty_when_no_types()
    {
        // arrange
        var manifest = new ApiManifest { AssemblyName = "TestLib", Types = [] };

        // act
        var pages = ApiReferenceNav.FromManifest(manifest);

        // assert
        pages.Should().BeEmpty();
    }
}
