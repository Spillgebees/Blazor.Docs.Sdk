namespace Spillgebees.Blazor.Docs.Sdk.Navigation;

using System.Text.Json;
using Spillgebees.Blazor.Docs.Sdk.Build;

/// <summary>
/// Provides methods for building API reference navigation pages from an <see cref="ApiManifest"/>.
/// </summary>
public static class ApiReferenceNav
{
    /// <summary>
    /// Converts an <see cref="ApiManifest"/> into a sorted list of <see cref="NavPage"/> entries.
    /// </summary>
    /// <param name="manifest">The API manifest to convert.</param>
    /// <returns>
    /// A read-only list of <see cref="NavPage"/> instances, one per type, ordered by title.
    /// </returns>
    public static IReadOnlyList<NavPage> FromManifest(ApiManifest manifest) =>
        manifest
            .Types.Select(t => new NavPage(t.Name, $"/api/{t.FullName}"))
            .OrderBy(p => p.Title, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Generates API reference navigation pages by loading the embedded <c>ApiManifest</c> resource
    /// from the assembly that contains <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// A type whose assembly carries the embedded <c>ApiManifest:&lt;AssemblyName&gt;</c> resource.
    /// </typeparam>
    /// <returns>
    /// A read-only list of <see cref="NavPage"/> instances built from the manifest, or an empty list
    /// if the resource could not be found or deserialized.
    /// </returns>
    public static IReadOnlyList<NavPage> Generate<T>()
    {
        var assembly = typeof(T).Assembly;
        var resourceName = $"ApiManifest:{assembly.GetName().Name}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return [];
        }

        var manifest = JsonSerializer.Deserialize<ApiManifest>(stream);
        return manifest is null ? [] : FromManifest(manifest);
    }
}
