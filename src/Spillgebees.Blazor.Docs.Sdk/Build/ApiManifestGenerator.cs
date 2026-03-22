namespace Spillgebees.Blazor.Docs.Sdk.Build;

using System.Reflection;
using System.Xml.Linq;

/// <summary>
/// Generates an API manifest from a compiled assembly and optional XML documentation.
/// </summary>
public static class ApiManifestGenerator
{
    private static readonly HashSet<string> _excludedObjectMethods = ["ToString", "Equals", "GetHashCode", "GetType"];

    /// <summary>
    /// Generates an API manifest by reflecting over the assembly's public types.
    /// </summary>
    public static ApiManifest Generate(Assembly assembly, string? xmlDocPath)
    {
        var xmlDocs =
            xmlDocPath is not null && File.Exists(xmlDocPath)
                ? ParseXmlDocs(xmlDocPath)
                : new Dictionary<string, string>();

        var manifest = new ApiManifest { AssemblyName = assembly.GetName().Name ?? "" };

        foreach (var type in assembly.GetExportedTypes())
        {
            var typeInfo = new ApiTypeInfo
            {
                Name = FormatTypeName(type),
                FullName = type.FullName ?? type.Name,
                Namespace = type.Namespace ?? "",
                Kind = GetTypeKind(type),
                Summary = GetDoc(xmlDocs, $"T:{type.FullName}"),
                BaseType =
                    type.BaseType is not null && type.BaseType != typeof(object) ? FormatTypeName(type.BaseType) : null,
                Interfaces = type.GetInterfaces().Select(FormatTypeName).ToList(),
            };

            if (type.IsGenericTypeDefinition)
            {
                typeInfo.GenericParameters = type.GetGenericArguments()
                    .Select(g => new ApiGenericParameter
                    {
                        Name = g.Name,
                        Constraints = g.GetGenericParameterConstraints().Select(FormatTypeName).ToList(),
                    })
                    .ToList();
            }

            if (type.IsEnum)
            {
                typeInfo.EnumValues = Enum.GetNames(type)
                    .Select(name => new ApiEnumValue
                    {
                        Name = name,
                        Value = Convert.ToInt64(
                            Enum.Parse(type, name),
                            System.Globalization.CultureInfo.InvariantCulture
                        ),
                        Summary = GetDoc(xmlDocs, $"F:{type.FullName}.{name}"),
                    })
                    .ToList();
            }
            else
            {
                typeInfo.Properties = type.GetProperties(
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
                    )
                    .Select(p => new ApiPropertyInfo
                    {
                        Name = p.Name,
                        Type = FormatTypeName(p.PropertyType),
                        Summary = GetDoc(xmlDocs, $"P:{type.FullName}.{p.Name}"),
                        IsParameter = p.GetCustomAttributes().Any(a => a.GetType().Name == "ParameterAttribute"),
                    })
                    .ToList();

                typeInfo.Methods = type.GetMethods(
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
                    )
                    .Where(m => !m.IsSpecialName && !_excludedObjectMethods.Contains(m.Name))
                    .Select(m => new ApiMethodInfo
                    {
                        Name = m.Name,
                        ReturnType = FormatTypeName(m.ReturnType),
                        Summary = GetDoc(xmlDocs, $"M:{type.FullName}.{m.Name}"),
                        Parameters = m.GetParameters()
                            .Select(p => new ApiParameterInfo
                            {
                                Name = p.Name ?? "",
                                Type = FormatTypeName(p.ParameterType),
                            })
                            .ToList(),
                    })
                    .ToList();

                typeInfo.Events = type.GetEvents(
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
                    )
                    .Select(e => new ApiEventInfo
                    {
                        Name = e.Name,
                        Type = FormatTypeName(e.EventHandlerType ?? typeof(EventHandler)),
                        Summary = GetDoc(xmlDocs, $"E:{type.FullName}.{e.Name}"),
                    })
                    .ToList();
            }

            manifest.Types.Add(typeInfo);
        }

        return manifest;
    }

    private static Dictionary<string, string> ParseXmlDocs(string path)
    {
        var docs = new Dictionary<string, string>();
        var doc = XDocument.Load(path);
        var members = doc.Descendants("member");

        foreach (var member in members)
        {
            var name = member.Attribute("name")?.Value;
            var summary = member.Element("summary")?.Value.Trim();
            if (name is not null && summary is not null)
            {
                docs[name] = summary;
            }
        }

        return docs;
    }

    private static string? GetDoc(Dictionary<string, string> docs, string key) =>
        docs.TryGetValue(key, out var value) ? value : null;

    private static string FormatTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var name = type.Name[..type.Name.IndexOf('`')];
            var args = string.Join(", ", type.GetGenericArguments().Select(FormatTypeName));
            return $"{name}<{args}>";
        }

        return type.Name;
    }

    private static string GetTypeKind(Type type) =>
        type switch
        {
            _ when type.IsEnum => "enum",
            _ when type.IsInterface => "interface",
            _ when type.IsValueType => "struct",
            _ => "class",
        };
}
