using AwesomeAssertions;
using Microsoft.AspNetCore.Components;
using Spillgebees.Blazor.Docs.Sdk.Components;
using Spillgebees.Blazor.Docs.Sdk.Navigation;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Components;

public class DocSiteTests
{
    private static NavSection[] CreateTestNav() => [new("Getting Started", [new NavPage("Install", "/install")])];

    private static BunitContext CreateContext()
    {
        var ctx = new BunitContext();
        ctx.JSInterop.Setup<string>("Spillgebees.DocsSdk.getTheme").SetResult("dark");
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.setTheme", _ => true);
        return ctx;
    }

    [Test]
    public void Should_render_project_name()
    {
        // arrange
        using var ctx = CreateContext();

        // act
        var cut = ctx.Render<DocSite>(parameters =>
            parameters
                .Add(p => p.ProjectName, "MyProject")
                .Add(p => p.LogoEmoji, "🚀")
                .Add(p => p.GitHubUrl, "https://github.com/test/repo")
                .Add(p => p.Navigation, CreateTestNav())
        );

        // assert
        cut.Markup.Should().Contain("MyProject");
    }

    [Test]
    public void Should_render_logo_emoji()
    {
        // arrange
        using var ctx = CreateContext();

        // act
        var cut = ctx.Render<DocSite>(parameters =>
            parameters
                .Add(p => p.ProjectName, "MyProject")
                .Add(p => p.LogoEmoji, "🚀")
                .Add(p => p.GitHubUrl, "https://github.com/test/repo")
                .Add(p => p.Navigation, CreateTestNav())
        );

        // assert
        cut.Markup.Should().Contain("🚀");
    }

    [Test]
    public void Should_render_child_content()
    {
        // arrange
        using var ctx = CreateContext();

        // act
        var cut = ctx.Render<DocSite>(parameters =>
            parameters
                .Add(p => p.ProjectName, "MyProject")
                .Add(p => p.LogoEmoji, "🚀")
                .Add(p => p.GitHubUrl, "https://github.com/test/repo")
                .Add(p => p.Navigation, CreateTestNav())
                .Add(
                    p => p.ChildContent,
                    (RenderFragment)(
                        builder =>
                        {
                            builder.AddMarkupContent(0, "<p>Hello from child</p>");
                        }
                    )
                )
        );

        // assert
        cut.Markup.Should().Contain("Hello from child");
    }

    [Test]
    public void Should_render_sidebar()
    {
        // arrange
        using var ctx = CreateContext();

        // act
        var cut = ctx.Render<DocSite>(parameters =>
            parameters
                .Add(p => p.ProjectName, "MyProject")
                .Add(p => p.LogoEmoji, "🚀")
                .Add(p => p.GitHubUrl, "https://github.com/test/repo")
                .Add(p => p.Navigation, CreateTestNav())
        );

        // assert
        var nav = cut.Find("nav");
        nav.Should().NotBeNull();
    }

    [Test]
    public void Should_render_footer()
    {
        // arrange
        using var ctx = CreateContext();

        // act
        var cut = ctx.Render<DocSite>(parameters =>
            parameters
                .Add(p => p.ProjectName, "MyProject")
                .Add(p => p.LogoEmoji, "🚀")
                .Add(p => p.GitHubUrl, "https://github.com/test/repo")
                .Add(p => p.Navigation, CreateTestNav())
        );

        // assert
        var footer = cut.Find("footer");
        footer.Should().NotBeNull();
    }
}
