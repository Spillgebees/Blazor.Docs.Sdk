namespace Spillgebees.Blazor.Docs.Sdk.Build;

using System.Reflection;
using System.Text.Json;

/// <summary>
/// Locates embedded resources across loaded assemblies, resolving the mismatch where
/// MSBuild targets embed resources into the docs project assembly while runtime code
/// references the library assembly.
/// </summary>
internal static class EmbeddedResourceLocator
{
    /// <summary>
    /// Finds all manifest resource names matching a prefix across all loaded assemblies.
    /// Searches <paramref name="primaryAssembly"/> first for efficiency.
    /// </summary>
    internal static IReadOnlyList<(Assembly Assembly, string ResourceName)> FindResources(
        string prefix,
        Assembly? primaryAssembly = null)
    {
        var results = new List<(Assembly, string)>();
        var searched = new HashSet<Assembly>();

        if (primaryAssembly is not null)
        {
            CollectMatching(primaryAssembly, prefix, results);
            searched.Add(primaryAssembly);
        }

        // If we found results in the primary assembly, no need to search further
        if (results.Count > 0)
        {
            return results;
        }

        // Search remaining loaded assemblies
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!searched.Add(assembly))
            {
                continue;
            }

            try
            {
                CollectMatching(assembly, prefix, results);
            }
            catch
            {
                // Skip assemblies that can't be reflected (dynamic, etc.)
            }
        }

        return results;
    }

    /// <summary>
    /// Loads and deserializes an <see cref="ApiManifest"/> from the embedded resource
    /// named <c>ApiManifest:{assemblyName}</c>. Searches <paramref name="primaryAssembly"/>
    /// first, then falls back to all loaded assemblies.
    /// </summary>
    internal static ApiManifest? LoadApiManifest(string assemblyName, Assembly? primaryAssembly = null)
    {
        var resourceName = $"ApiManifest:{assemblyName}";

        // Try primary assembly first
        if (primaryAssembly is not null)
        {
            var stream = primaryAssembly.GetManifestResourceStream(resourceName);
            if (stream is not null)
            {
                using (stream)
                {
                    return JsonSerializer.Deserialize<ApiManifest>(stream);
                }
            }
        }

        // Search all loaded assemblies
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly == primaryAssembly)
            {
                continue;
            }

            try
            {
                var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is not null)
                {
                    using (stream)
                    {
                        return JsonSerializer.Deserialize<ApiManifest>(stream);
                    }
                }
            }
            catch
            {
                // Skip assemblies that can't be reflected
            }
        }

        return null;
    }

    private static void CollectMatching(
        Assembly assembly,
        string prefix,
        List<(Assembly, string)> results)
    {
        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                results.Add((assembly, name));
            }
        }
    }
}
