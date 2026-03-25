namespace Spillgebees.Blazor.Docs.Sdk.Build;

using System.Reflection;
using System.Text.Json;

/// <summary>
/// Discovers embedded resources across all loaded assemblies. MSBuild targets embed
/// source files and API manifests into the consuming docs project, but runtime code
/// references the library assembly. This locator bridges that gap by searching across
/// all loaded assemblies when the resource isn't found in the start assembly.
/// </summary>
public static class EmbeddedResourceLocator
{
    /// <summary>
    /// Finds all embedded resources whose logical name starts with <paramref name="resourcePrefix"/>.
    /// Searches <paramref name="startAssembly"/> first, then falls back to all loaded assemblies.
    /// </summary>
    /// <param name="resourcePrefix">The prefix to match against resource names (e.g. <c>SourceEmbed:MyNamespace.MyComponent</c>).</param>
    /// <param name="startAssembly">The assembly to search first.</param>
    /// <returns>Tuples of the assembly containing the resource and the full resource name.</returns>
    public static IEnumerable<(Assembly Assembly, string ResourceName)> FindResources(
        string resourcePrefix,
        Assembly startAssembly
    )
    {
        var results = GetMatchingResources(startAssembly, resourcePrefix);
        if (results.Count > 0)
        {
            return results;
        }

        return FindResourcesAcrossAssemblies(resourcePrefix);
    }

    /// <summary>
    /// Loads and deserializes an <see cref="ApiManifest"/> from an embedded resource with
    /// logical name <c>ApiManifest:{assemblyName}</c>. Searches <paramref name="startAssembly"/>
    /// first, then falls back to all loaded assemblies.
    /// </summary>
    /// <param name="assemblyName">The assembly name used in the resource logical name.</param>
    /// <param name="startAssembly">The assembly to search first.</param>
    /// <returns>The deserialized manifest, or <c>null</c> if the resource was not found.</returns>
    public static ApiManifest? LoadApiManifest(string assemblyName, Assembly startAssembly)
    {
        var resourceName = $"ApiManifest:{assemblyName}";

        var stream = TryGetResourceStream(startAssembly, resourceName);
        if (stream is null)
        {
            stream = FindResourceStreamAcrossAssemblies(resourceName);
        }

        if (stream is null)
        {
            return null;
        }

        using (stream)
        {
            return JsonSerializer.Deserialize<ApiManifest>(stream);
        }
    }

    private static List<(Assembly Assembly, string ResourceName)> GetMatchingResources(
        Assembly assembly,
        string resourcePrefix
    )
    {
        var results = new List<(Assembly, string)>();

        try
        {
            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (name.StartsWith(resourcePrefix, StringComparison.Ordinal))
                {
                    results.Add((assembly, name));
                }
            }
        }
        catch (NotSupportedException)
        {
            // Dynamic assemblies don't support manifest resources
        }

        return results;
    }

    private static List<(Assembly Assembly, string ResourceName)> FindResourcesAcrossAssemblies(
        string resourcePrefix
    )
    {
        var results = new List<(Assembly, string)>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
            {
                continue;
            }

            try
            {
                foreach (var name in assembly.GetManifestResourceNames())
                {
                    if (name.StartsWith(resourcePrefix, StringComparison.Ordinal))
                    {
                        results.Add((assembly, name));
                    }
                }
            }
            catch (NotSupportedException)
            {
                // Dynamic assemblies don't support manifest resources
            }
        }

        return results;
    }

    private static Stream? TryGetResourceStream(Assembly assembly, string resourceName)
    {
        try
        {
            var names = assembly.GetManifestResourceNames();
            if (Array.Exists(names, n => n == resourceName))
            {
                return assembly.GetManifestResourceStream(resourceName);
            }
        }
        catch (NotSupportedException)
        {
            // Dynamic assemblies don't support manifest resources
        }

        return null;
    }

    private static Stream? FindResourceStreamAcrossAssemblies(string resourceName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
            {
                continue;
            }

            var stream = TryGetResourceStream(assembly, resourceName);
            if (stream is not null)
            {
                return stream;
            }
        }

        return null;
    }
}
