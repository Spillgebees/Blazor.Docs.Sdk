using AwesomeAssertions;
using Spillgebees.Blazor.Docs.Sdk.Navigation;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Navigation;

public class NavSectionTests
{
    [Test]
    public void Should_store_nav_page_properties()
    {
        // arrange & act
        var page = new NavPage("Installation", "/installation");

        // assert
        page.Title.Should().Be("Installation");
        page.Href.Should().Be("/installation");
    }

    [Test]
    public void Should_store_nav_section_properties()
    {
        // arrange
        var pages = new[] { new NavPage("Install", "/install") };

        // act
        var section = new NavSection("Getting Started", pages);

        // assert
        section.Title.Should().Be("Getting Started");
        section.Pages.Should().HaveCount(1);
    }

    [Test]
    public void Should_support_empty_pages()
    {
        // arrange & act
        var section = new NavSection("Empty", []);

        // assert
        section.Pages.Should().BeEmpty();
    }
}
