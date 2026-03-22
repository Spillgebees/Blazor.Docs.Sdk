using AwesomeAssertions;
using Spillgebees.Blazor.Docs.Sdk.Components;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Components;

public class CodeBlockTests
{
    [Test]
    public void Should_render_single_file_as_plain_text()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.copyToClipboard", _ => true);
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.highlightElement", _ => true);
        var files = new Dictionary<string, string> { ["Program.cs"] = "var x = 1;" };

        // act
        var cut = ctx.Render<CodeBlock>(parameters => parameters.Add(p => p.Files, files));

        // assert
        var code = cut.Find("pre code");
        code.TextContent.Should().Be("var x = 1;");
    }

    [Test]
    public void Should_render_tabs_for_multiple_files()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.copyToClipboard", _ => true);
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.highlightElement", _ => true);
        var files = new Dictionary<string, string> { ["Program.cs"] = "hello", ["Startup.cs"] = "world" };

        // act
        var cut = ctx.Render<CodeBlock>(parameters => parameters.Add(p => p.Files, files));

        // assert
        var tabs = cut.FindAll(".code-block-tab");
        tabs.Should().HaveCount(2);
        tabs[0].TextContent.Should().Be("Program.cs");
        tabs[1].TextContent.Should().Be("Startup.cs");
    }

    [Test]
    public void Should_render_copy_button()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.copyToClipboard", _ => true);
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.highlightElement", _ => true);
        var files = new Dictionary<string, string> { ["Program.cs"] = "hello" };

        // act
        var cut = ctx.Render<CodeBlock>(parameters => parameters.Add(p => p.Files, files));

        // assert
        var copyButton = cut.Find(".code-block-copy");
        copyButton.Should().NotBeNull();
    }

    [Test]
    public void Should_render_tab_for_single_file()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.copyToClipboard", _ => true);
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.highlightElement", _ => true);
        var files = new Dictionary<string, string> { ["Program.cs"] = "hello" };

        // act
        var cut = ctx.Render<CodeBlock>(parameters => parameters.Add(p => p.Files, files));

        // assert
        var tabs = cut.FindAll(".code-block-tab");
        tabs.Should().HaveCount(1);
        tabs[0].TextContent.Should().Be("Program.cs");
    }

    [Test]
    public void Should_render_bar_with_tabs_and_copy()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.copyToClipboard", _ => true);
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.highlightElement", _ => true);
        var files = new Dictionary<string, string> { ["Program.cs"] = "hello", ["Startup.cs"] = "world" };

        // act
        var cut = ctx.Render<CodeBlock>(parameters => parameters.Add(p => p.Files, files));

        // assert
        var bar = cut.Find(".code-block-bar");
        bar.Should().NotBeNull();
        var copy = cut.Find(".code-block-copy");
        copy.Should().NotBeNull();
    }
}
