using AwesomeAssertions;
using Spillgebees.Blazor.Docs.Sdk;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Components;

public class ThemeToggleTests
{
    [Test]
    public void Should_render_toggle_button()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.Setup<string>("Spillgebees.DocsSdk.getTheme").SetResult("dark");
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.setTheme", _ => true);

        // act
        var cut = ctx.Render<ThemeToggle>();

        // assert
        var button = cut.Find("button");
        button.Should().NotBeNull();
    }

    [Test]
    public void Should_have_aria_label_on_button()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.Setup<string>("Spillgebees.DocsSdk.getTheme").SetResult("dark");
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.setTheme", _ => true);

        // act
        var cut = ctx.Render<ThemeToggle>();

        // assert
        var button = cut.Find("button");
        button.GetAttribute("aria-label").Should().NotBeNull();
    }

    [Test]
    public void Should_show_dark_label_when_theme_is_dark()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.Setup<string>("Spillgebees.DocsSdk.getTheme").SetResult("dark");
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.setTheme", _ => true);

        // act
        var cut = ctx.Render<ThemeToggle>();

        // assert
        cut.WaitForAssertion(() => cut.Find(".theme-toggle-label").TextContent.Should().Contain("dark"));
    }

    [Test]
    public void Should_show_light_label_when_theme_is_light()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.Setup<string>("Spillgebees.DocsSdk.getTheme").SetResult("light");
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.setTheme", _ => true);

        // act
        var cut = ctx.Render<ThemeToggle>();

        // assert
        cut.WaitForAssertion(() => cut.Find(".theme-toggle-label").TextContent.Should().Contain("light"));
    }

    [Test]
    public void Should_toggle_theme_on_click()
    {
        // arrange
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.JSInterop.Setup<string>("Spillgebees.DocsSdk.getTheme").SetResult("dark");
        var cut = ctx.Render<ThemeToggle>();
        cut.WaitForAssertion(() => cut.Find(".theme-toggle-label").TextContent.Should().Contain("dark"));

        // act
        cut.Find("button").Click();

        // assert
        cut.WaitForAssertion(() => cut.Find(".theme-toggle-label").TextContent.Should().Contain("light"));
        ctx.JSInterop.Invocations.Should()
            .Contain(invocation => invocation.Identifier == "Spillgebees.DocsSdk.setTheme");
    }
}
