namespace Spillgebees.Blazor.Docs.Sdk.Navigation;

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
    /// for the assembly that contains <typeparamref name="T"/>. Searches that assembly first, then
    /// falls back to all loaded assemblies (e.g. the docs project where the resource is actually embedded).
    /// </summary>
    /// <typeparam name="T">
    /// A type from the library whose API manifest should be loaded.
    /// </typeparam>
    /// <returns>
    /// A read-only list of <see cref="NavPage"/> instances built from the manifest, or an empty list
    /// if the resource could not be found or deserialized.
    /// </returns>
    public static IReadOnlyList<NavPage> Generate<T>()
    {
        var assembly = typeof(T).Assembly;
        var assemblyName = assembly.GetName().Name;

        if (assemblyName is null)
        {
            return [];
        }

        var manifest = EmbeddedResourceLocator.LoadApiManifest(assemblyName, assembly);
        return manifest is null ? [] : FromManifest(manifest);
    }
}
