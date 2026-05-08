using AwesomeAssertions;
using Spillgebees.Blazor.Docs.Sdk;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Build;

public class SourceExtractorTests
{
    [Test]
    public void Should_find_example_view_type_references()
    {
        // arrange
        var razorContent = """
            <ExampleView TComponent="TrainTrackingExample" Title="Train Tracking">
                <TrainTrackingExample />
            </ExampleView>
            """;

        // act
        var types = SourceExtractor.DiscoverReferencedTypes(razorContent);

        // assert
        types.Should().Contain("TrainTrackingExample");
    }

    [Test]
    public void Should_find_additional_sources_type_references()
    {
        // arrange
        var razorContent = """
            <ExampleView TComponent="TrainTrackingExample"
                         AdditionalSources="@(typeof(TrainCatalog), typeof(TrainSim))">
                <TrainTrackingExample />
            </ExampleView>
            """;

        // act
        var types = SourceExtractor.DiscoverReferencedTypes(razorContent);

        // assert
        types.Should().Contain("TrainTrackingExample");
        types.Should().Contain("TrainCatalog");
        types.Should().Contain("TrainSim");
    }

    [Test]
    public void Should_find_additional_sources_type_references_with_fully_qualified_names()
    {
        // arrange
        var razorContent = """
            <ExampleView TComponent="TrainTrackingExample"
                         AdditionalSources="@(typeof(Samples.Trains.TrainTrackingPresentation), typeof(Samples.Trains.TrainSampleSimulation))">
                <TrainTrackingExample />
            </ExampleView>
            """;

        // act
        var types = SourceExtractor.DiscoverReferencedTypes(razorContent);

        // assert
        types.Should().Contain("TrainTrackingExample");
        types.Should().Contain("TrainTrackingPresentation");
        types.Should().Contain("TrainSampleSimulation");
    }

    [Test]
    public void Should_not_add_invalid_additional_source_entries_without_typeof()
    {
        // arrange
        var razorContent = """
            <ExampleView TComponent="TrainTrackingExample"
                         AdditionalSources="@(new[] { \"TrainTrackingPresentation\" })">
                <TrainTrackingExample />
            </ExampleView>
            """;

        // act
        var types = SourceExtractor.DiscoverReferencedTypes(razorContent);

        // assert
        types.Should().ContainSingle().Which.Should().Be("TrainTrackingExample");
    }

    [Test]
    public void Should_return_source_unchanged_for_csharp()
    {
        // arrange
        var source = "var x = 1;";

        // act
        var result = SourceExtractor.HighlightCSharp(source);

        // assert
        result.Should().Be(source);
    }

    [Test]
    public void Should_return_empty_when_no_example_views()
    {
        // arrange
        var razorContent = "<p>No examples here</p>";

        // act
        var types = SourceExtractor.DiscoverReferencedTypes(razorContent);

        // assert
        types.Should().BeEmpty();
    }
}
