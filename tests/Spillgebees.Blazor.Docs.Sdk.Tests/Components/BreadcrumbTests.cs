using AwesomeAssertions;
using Spillgebees.Blazor.Docs.Sdk.Components;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Components;

public class BreadcrumbTests
{
    [Test]
    public void Should_render_section_and_page_name()
    {
        // arrange
        using var ctx = new BunitContext();

        // act
        var cut = ctx.Render<Breadcrumb>(p => p.Add(x => x.Section, "components").Add(x => x.PageName, "ExampleView"));

        // assert
        cut.Find(".doc-breadcrumb-segment").TextContent.Should().Be("components");
        cut.Find(".doc-breadcrumb-current").TextContent.Should().Be("ExampleView");
    }

    [Test]
    public void Should_render_api_reference_link_when_provided()
    {
        // arrange
        using var ctx = new BunitContext();

        // act
        var cut = ctx.Render<Breadcrumb>(p =>
            p.Add(x => x.Section, "components")
                .Add(x => x.PageName, "ExampleView")
                .Add(x => x.ApiReferenceUrl, "https://example.com/api")
        );

        // assert
        var link = cut.Find("a.doc-breadcrumb-link");
        link.TextContent.Should().Be("api_ref");
        link.GetAttribute("href").Should().Be("https://example.com/api");
    }

    [Test]
    public void Should_render_source_link_when_provided()
    {
        // arrange
        using var ctx = new BunitContext();

        // act
        var cut = ctx.Render<Breadcrumb>(p =>
            p.Add(x => x.Section, "components")
                .Add(x => x.PageName, "ExampleView")
                .Add(x => x.SourceUrl, "https://github.com/example/repo")
        );

        // assert
        var link = cut.Find("a.doc-breadcrumb-link");
        link.TextContent.Should().Be("source");
        link.GetAttribute("href").Should().Be("https://github.com/example/repo");
    }
}
