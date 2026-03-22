using AwesomeAssertions;
using Spillgebees.Blazor.Docs.Sdk.Components;

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
}
