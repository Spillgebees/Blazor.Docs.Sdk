namespace Spillgebees.Blazor.Docs.Sdk.Build;

using System.Text.RegularExpressions;

/// <summary>
/// Discovers ExampleView type references in Razor files and produces syntax-highlighted HTML.
/// </summary>
public static partial class SourceExtractor
{
    [GeneratedRegex("""TComponent="(\w+)""")]
    private static partial Regex TComponentRegex();

    [GeneratedRegex("""typeof\((\w+)\)""")]
    private static partial Regex TypeOfRegex();

    [GeneratedRegex(@"AdditionalSources=""@\((.*?)\)""")]
    private static partial Regex AdditionalSourcesRegex();

    /// <summary>
    /// Scans Razor content for ExampleView TComponent and AdditionalSources type references.
    /// </summary>
    public static HashSet<string> DiscoverReferencedTypes(string razorContent)
    {
        var types = new HashSet<string>();

        foreach (Match match in TComponentRegex().Matches(razorContent))
        {
            types.Add(match.Groups[1].Value);
        }

        foreach (Match additionalMatch in AdditionalSourcesRegex().Matches(razorContent))
        {
            var inner = additionalMatch.Groups[1].Value;
            foreach (Match typeMatch in TypeOfRegex().Matches(inner))
            {
                types.Add(typeMatch.Groups[1].Value);
            }
        }

        return types;
    }

    /// <summary>
    /// Returns the C# source unchanged (highlighting is handled client-side by highlight.js).
    /// </summary>
    public static string HighlightCSharp(string source) => source;

    /// <summary>
    /// Returns the Razor source unchanged (highlighting is handled client-side by highlight.js).
    /// </summary>
    public static string HighlightRazor(string source) => source;
}
