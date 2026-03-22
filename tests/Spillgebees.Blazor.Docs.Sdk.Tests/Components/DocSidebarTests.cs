using AwesomeAssertions;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Spillgebees.Blazor.Docs.Sdk.Components;
using Spillgebees.Blazor.Docs.Sdk.Navigation;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Components;

public class DocSidebarTests
{
    private static NavSection[] CreateTestNav() =>
        [
            new("Getting Started", [new NavPage("Install", "/install"), new NavPage("Quick Start", "/quick-start")]),
            new("Components", [new NavPage("Map", "/components/map")]),
        ];

    private static IRenderedComponent<DocSidebar> RenderSidebar(BunitContext ctx) =>
        ctx.Render<DocSidebar>(parameters =>
            parameters.Add(p => p.Navigation, CreateTestNav()).Add(p => p.GitHubUrl, "https://github.com/test/repo")
        );

    [Test]
    public void Should_render_all_section_titles_with_comment_prefix()
    {
        // arrange
        using var ctx = new BunitContext();

        // act
        var cut = RenderSidebar(ctx);

        // assert
        cut.Markup.Should().Contain("// getting_started");
        cut.Markup.Should().Contain("// components");
    }

    [Test]
    public void Should_render_all_page_links()
    {
        // arrange
        using var ctx = new BunitContext();

        // act
        var cut = RenderSidebar(ctx);

        // assert
        var links = cut.FindAll("a");
        links.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Test]
    public void Should_render_nav_element()
    {
        // arrange
        using var ctx = new BunitContext();

        // act
        var cut = RenderSidebar(ctx);

        // assert
        var nav = cut.Find("nav");
        nav.Should().NotBeNull();
    }

    [Test]
    public void Should_highlight_active_page()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.Services.GetRequiredService<BunitNavigationManager>().NavigateTo("/install");

        // act
        var cut = RenderSidebar(ctx);

        // assert
        var activeLink = cut.Find("a.active");
        activeLink.TextContent.Should().Be("Install");
    }

    [Test]
    public void Should_render_footer_in_sidebar()
    {
        // arrange
        using var ctx = new BunitContext();

        // act
        var cut = RenderSidebar(ctx);

        // assert
        var footerDiv = cut.Find(".doc-sidebar-footer");
        footerDiv.Should().NotBeNull();
        var footer = cut.Find("footer");
        footer.Should().NotBeNull();
    }
}
