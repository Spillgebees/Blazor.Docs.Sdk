using AwesomeAssertions;
using Spillgebees.Blazor.Docs.Sdk.Components;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Components;

public class OnThisPageTests
{
    [Test]
    public async Task Should_render_toggle_button()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.Setup<OnThisPageSection[]>("Spillgebees.DocsSdk.getSections")
            .SetResult([new("introduction", "introduction"), new("usage", "usage")]);
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.scrollToElement", _ => true);

        // act
        var cut = ctx.Render<OnThisPage>();
        await cut.InvokeAsync(() => cut.Instance.GetType());

        // assert
        var button = cut.Find("button.doc-on-this-page-toggle");
        button.Should().NotBeNull();
    }

    [Test]
    public async Task Should_render_section_links_when_open()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.Setup<OnThisPageSection[]>("Spillgebees.DocsSdk.getSections")
            .SetResult([new("introduction", "introduction"), new("usage", "usage")]);
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.scrollToElement", _ => true);

        // act
        var cut = ctx.Render<OnThisPage>();
        await cut.InvokeAsync(() => cut.Instance.GetType());
        cut.Find("button.doc-on-this-page-toggle").Click();

        // assert
        var links = cut.FindAll("button.doc-on-this-page-link");
        links.Should().HaveCount(2);
        links[0].TextContent.Trim().Should().Be("introduction");
        links[1].TextContent.Trim().Should().Be("usage");
    }
}
