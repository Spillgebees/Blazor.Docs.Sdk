using AwesomeAssertions;
using Spillgebees.Blazor.Docs.Sdk;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Components;

public class ApiDocTests
{
    private static ApiTypeInfo CreateTestType() =>
        new()
        {
            Name = "SgbMap",
            FullName = "Spillgebees.Blazor.Map.SgbMap",
            Namespace = "Spillgebees.Blazor.Map",
            Kind = "class",
            Summary = "A map component",
            Properties =
            [
                new ApiPropertyInfo
                {
                    Name = "Center",
                    Type = "LatLng",
                    Summary = "Map center position",
                    IsParameter = true,
                },
            ],
            Methods =
            [
                new ApiMethodInfo
                {
                    Name = "FlyTo",
                    ReturnType = "Task",
                    Summary = "Animate to position",
                    Parameters = [new ApiParameterInfo { Name = "position", Type = "LatLng" }],
                },
            ],
        };

    private static BunitContext CreateContext()
    {
        var ctx = new BunitContext();
        ctx.JSInterop.SetupVoid("Spillgebees.DocsSdk.highlightAll", _ => true);
        return ctx;
    }

    [Test]
    public void Should_render_type_name()
    {
        // arrange
        using var ctx = CreateContext();
        var typeInfo = CreateTestType();

        // act
        var cut = ctx.Render<ApiDoc>(parameters => parameters.Add(p => p.TypeInfo, typeInfo));

        // assert
        cut.Markup.Should().Contain("SgbMap");
    }

    [Test]
    public void Should_render_namespace()
    {
        // arrange
        using var ctx = CreateContext();
        var typeInfo = CreateTestType();

        // act
        var cut = ctx.Render<ApiDoc>(parameters => parameters.Add(p => p.TypeInfo, typeInfo));

        // assert
        cut.Markup.Should().Contain("Spillgebees.Blazor.Map");
    }

    [Test]
    public void Should_render_properties()
    {
        // arrange
        using var ctx = CreateContext();
        var typeInfo = CreateTestType();

        // act
        var cut = ctx.Render<ApiDoc>(parameters => parameters.Add(p => p.TypeInfo, typeInfo));

        // assert
        var definitions = cut.FindAll(".api-doc-definition");
        definitions.Should().NotBeEmpty();
        cut.Markup.Should().Contain("Center");
        cut.Markup.Should().Contain("LatLng");
    }

    [Test]
    public void Should_render_short_type_names_with_full_type_tooltips()
    {
        // arrange
        using var ctx = CreateContext();
        var typeInfo = CreateTestType();
        typeInfo.Properties[0].Type =
            "System.Collections.Generic.IReadOnlyDictionary<string, Spillgebees.Blazor.Docs.Sdk.NavSection>";

        // act
        var cut = ctx.Render<ApiDoc>(parameters => parameters.Add(p => p.TypeInfo, typeInfo));

        // assert
        var typeBadge = cut.Find(".api-doc-definition-type");
        typeBadge.TextContent.Should().Be("IReadOnlyDictionary<string, NavSection>");
        typeBadge
            .GetAttribute("title")
            .Should()
            .Be("System.Collections.Generic.IReadOnlyDictionary<string, Spillgebees.Blazor.Docs.Sdk.NavSection>");
    }

    [Test]
    public void Should_render_methods()
    {
        // arrange
        using var ctx = CreateContext();
        var typeInfo = CreateTestType();

        // act
        var cut = ctx.Render<ApiDoc>(parameters => parameters.Add(p => p.TypeInfo, typeInfo));

        // assert
        cut.Markup.Should().Contain("FlyTo");
        cut.Markup.Should().Contain("Task");
    }

    [Test]
    public void Should_render_rich_xml_documentation_html()
    {
        // arrange
        using var ctx = CreateContext();
        var typeInfo = CreateTestType();
        typeInfo.Summary = "Plain fallback";
        typeInfo.SummaryHtml =
            """Use <a href="https://example.com" target="_blank" rel="noopener noreferrer">docs</a> and <code>code</code>.""";
        typeInfo.Methods[0].Parameters[0].Summary = "Position fallback";
        typeInfo.Methods[0].Parameters[0].SummaryHtml = "The <code>position</code> value.";

        // act
        var cut = ctx.Render<ApiDoc>(parameters => parameters.Add(p => p.TypeInfo, typeInfo));

        // assert
        cut.Markup.Should()
            .Contain("""<a href="https://example.com" target="_blank" rel="noopener noreferrer">docs</a>""");
        cut.Markup.Should().Contain("<code>code</code>");
        cut.Markup.Should().Contain("The <code>position</code> value.");
    }

    [Test]
    public void Should_render_parameter_badge_for_blazor_parameters()
    {
        // arrange
        using var ctx = CreateContext();
        var typeInfo = CreateTestType();

        // act
        var cut = ctx.Render<ApiDoc>(parameters => parameters.Add(p => p.TypeInfo, typeInfo));

        // assert
        var badge = cut.Find(".api-doc-badge-required");
        badge.Should().NotBeNull();
        badge.TextContent.Should().Contain("blazor parameter");
    }

    [Test]
    public void Should_label_public_properties_section_without_confusing_it_with_blazor_parameters()
    {
        // arrange
        using var ctx = CreateContext();
        var typeInfo = CreateTestType();

        // act
        var cut = ctx.Render<ApiDoc>(parameters => parameters.Add(p => p.TypeInfo, typeInfo));

        // assert
        cut.Markup.Should().Contain("// properties");
        cut.Markup.Should().NotContain("// parameters");
    }

    [Test]
    public void Should_render_collapsible_section_toggles()
    {
        // arrange
        using var ctx = CreateContext();
        var typeInfo = CreateTestType();

        // act
        var cut = ctx.Render<ApiDoc>(parameters => parameters.Add(p => p.TypeInfo, typeInfo));

        // assert
        var toggles = cut.FindAll(".api-doc-section-toggle");
        toggles.Should().NotBeEmpty();
    }

    [Test]
    public void Should_collapse_section_when_toggle_is_clicked()
    {
        // arrange
        using var ctx = CreateContext();
        var typeInfo = CreateTestType();
        var cut = ctx.Render<ApiDoc>(parameters => parameters.Add(p => p.TypeInfo, typeInfo));
        var definitionsBefore = cut.FindAll(".api-doc-definitions").Count;

        // act
        cut.FindAll(".api-doc-section-toggle")[0].Click();

        // assert
        var definitionsAfter = cut.FindAll(".api-doc-definitions").Count;
        definitionsAfter.Should().Be(definitionsBefore - 1);
    }
}
