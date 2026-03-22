using AwesomeAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Spillgebees.Blazor.Docs.Sdk.Components;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Components;

public class StubComponent : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.AddContent(0, "stub");
    }
}

public class ExampleViewTests
{
    [Test]
    public void Should_render_child_content_in_preview_mode()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.copyToClipboard", _ => true);
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.highlightElement", _ => true);

        // act
        var cut = ctx.Render<ExampleView<StubComponent>>(parameters =>
            parameters.AddChildContent("<span id=\"child\">child content</span>")
        );

        // assert
        cut.Find("#child").TextContent.Should().Be("child content");
    }

    [Test]
    public void Should_show_source_not_available_when_code_mode_and_no_embedded_resource()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.highlightElement", _ => true);

        // act
        var cut = ctx.Render<ExampleView<StubComponent>>();
        cut.Find(".example-view-mode:last-of-type").Click();

        // assert
        cut.Find(".example-view-no-source").TextContent.Trim().Should().Be("source not available");
    }

    [Test]
    public void Should_render_preview_and_code_toggle_buttons()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.highlightElement", _ => true);

        // act
        var cut = ctx.Render<ExampleView<StubComponent>>();

        // assert
        var modes = cut.FindAll(".example-view-mode");
        modes.Should().HaveCount(2);
        modes[0].TextContent.Should().Be("preview");
        modes[1].TextContent.Should().Be("code");
    }
}
