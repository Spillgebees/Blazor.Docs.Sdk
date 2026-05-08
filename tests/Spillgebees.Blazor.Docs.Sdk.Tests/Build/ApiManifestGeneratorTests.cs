using System.Text.Json;
using AwesomeAssertions;
using Spillgebees.Blazor.Docs.Sdk;

namespace Spillgebees.Blazor.Docs.Sdk.Tests.Build;

/// <summary>
/// A public test type for reflection.
/// </summary>
public class SamplePublicClass
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Does some work.
    /// </summary>
    public void DoWork(int count) { }
}

/// <summary>
/// A sample enum for testing.
/// </summary>
public enum SampleEnum
{
    /// <summary>No value.</summary>
    None = 0,

    /// <summary>Active state.</summary>
    Active = 1,
}

public class ApiManifestGeneratorTests
{
    [Test]
    public void Should_include_public_classes()
    {
        // arrange
        var assembly = typeof(SamplePublicClass).Assembly;

        // act
        var manifest = ApiManifestGenerator.Generate(assembly, xmlDocPath: null);

        // assert
        var type = manifest.Types.FirstOrDefault(t => t.Name == "SamplePublicClass");
        type.Should().NotBeNull();
        type!.Kind.Should().Be("class");
    }

    [Test]
    public void Should_include_properties()
    {
        // arrange
        var assembly = typeof(SamplePublicClass).Assembly;

        // act
        var manifest = ApiManifestGenerator.Generate(assembly, xmlDocPath: null);

        // assert
        var type = manifest.Types.First(t => t.Name == "SamplePublicClass");
        var prop = type.Properties.FirstOrDefault(p => p.Name == "Name");
        prop.Should().NotBeNull();
        prop!.Type.Should().Be("String");
    }

    [Test]
    public void Should_include_methods()
    {
        // arrange
        var assembly = typeof(SamplePublicClass).Assembly;

        // act
        var manifest = ApiManifestGenerator.Generate(assembly, xmlDocPath: null);

        // assert
        var type = manifest.Types.First(t => t.Name == "SamplePublicClass");
        var method = type.Methods.FirstOrDefault(m => m.Name == "DoWork");
        method.Should().NotBeNull();
    }

    [Test]
    public void Should_read_xml_doc_summary_for_methods_with_parameters()
    {
        // arrange
        var xmlPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");
        File.WriteAllText(
            xmlPath,
            $$"""
            <doc>
              <members>
                <member name="M:{{typeof(SamplePublicClass).FullName}}.DoWork(System.Int32)">
                  <summary>Parameterized method docs.</summary>
                </member>
              </members>
            </doc>
            """
        );

        try
        {
            // act
            var manifest = ApiManifestGenerator.Generate(typeof(SamplePublicClass).Assembly, xmlPath);

            // assert
            var type = manifest.Types.First(t => t.Name == "SamplePublicClass");
            type.Methods.First(m => m.Name == "DoWork").Summary.Should().Be("Parameterized method docs.");
        }
        finally
        {
            File.Delete(xmlPath);
        }
    }

    [Test]
    public void Should_include_enums_with_values()
    {
        // arrange
        var assembly = typeof(SampleEnum).Assembly;

        // act
        var manifest = ApiManifestGenerator.Generate(assembly, xmlDocPath: null);

        // assert
        var type = manifest.Types.FirstOrDefault(t => t.Name == "SampleEnum");
        type.Should().NotBeNull();
        type!.Kind.Should().Be("enum");
        type.EnumValues.Should().HaveCount(2);
    }

    [Test]
    public void Should_serialize_to_json()
    {
        // arrange
        var assembly = typeof(SamplePublicClass).Assembly;

        // act
        var manifest = ApiManifestGenerator.Generate(assembly, xmlDocPath: null);
        var json = JsonSerializer.Serialize(manifest);

        // assert
        json.Should().Contain("SamplePublicClass");
    }
}
