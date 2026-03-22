using AwesomeAssertions;
using Spillgebees.Blazor.Docs.Sdk.Build;
using Spillgebees.Blazor.Docs.Sdk.Navigation;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Navigation;

public class ApiReferenceNavTests
{
    [Test]
    public void Should_generate_nav_pages_from_manifest()
    {
        // arrange
        var manifest = new ApiManifest
        {
            AssemblyName = "TestLib",
            Types =
            [
                new ApiTypeInfo
                {
                    Name = "SgbMap",
                    FullName = "Spillgebees.Blazor.Map.SgbMap",
                    Namespace = "Spillgebees.Blazor.Map",
                    Kind = "class",
                },
                new ApiTypeInfo
                {
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
        pages[0].Href.Should().Be("/api/Spillgebees.Blazor.Map.SgbMap");
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
