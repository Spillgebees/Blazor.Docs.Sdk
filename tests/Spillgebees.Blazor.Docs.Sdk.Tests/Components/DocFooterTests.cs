using AwesomeAssertions;
using Spillgebees.Blazor.Docs.Sdk.Components;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Components;

public class DocFooterTests
{
    [Test]
    public void Should_render_made_by_text()
    {
        // arrange
        using var ctx = new BunitContext();

        // act
        var cut = ctx.Render<DocFooter>(parameters =>
            parameters.Add(p => p.GitHubUrl, "https://github.com/Spillgebees/Blazor.Map")
        );

        // assert
        cut.Markup.Should().Contain("made by");
        cut.Markup.Should().Contain("igotinfected");
    }

    [Test]
    public void Should_render_strawberry_emoji()
    {
        // arrange
        using var ctx = new BunitContext();

        // act
        var cut = ctx.Render<DocFooter>(parameters =>
            parameters.Add(p => p.GitHubUrl, "https://github.com/Spillgebees/Blazor.Map")
        );

        // assert
        cut.Markup.Should().Contain("🍓");
    }

    [Test]
    public void Should_render_mit_license()
    {
        // arrange
        using var ctx = new BunitContext();

        // act
        var cut = ctx.Render<DocFooter>(parameters =>
            parameters.Add(p => p.GitHubUrl, "https://github.com/Spillgebees/Blazor.Map")
        );

        // assert
        cut.Markup.Should().Contain("MIT");
    }

    [Test]
    public void Should_render_github_link_with_correct_url()
    {
        // arrange
        using var ctx = new BunitContext();

        // act
        var cut = ctx.Render<DocFooter>(parameters =>
            parameters.Add(p => p.GitHubUrl, "https://github.com/Spillgebees/Blazor.Map")
        );

        // assert
        var link = cut.Find("a[href='https://github.com/Spillgebees/Blazor.Map']");
        link.Should().NotBeNull();
    }
}
