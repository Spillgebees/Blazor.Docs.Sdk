namespace Spillgebees.Blazor.Docs.Sdk.Build;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the API manifest for a single assembly, listing all exported types.
/// </summary>
public sealed class ApiManifest
{
    /// <summary>Gets or sets the name of the assembly described by this manifest.</summary>
    [JsonPropertyName("assemblyName")]
    public string AssemblyName { get; set; } = "";

    /// <summary>Gets or sets the list of public types in the assembly.</summary>
    [JsonPropertyName("types")]
    public List<ApiTypeInfo> Types { get; set; } = [];
}

/// <summary>
/// Represents metadata for a single public type in the API manifest.
/// </summary>
public sealed class ApiTypeInfo
{
    /// <summary>Gets or sets the simple name of the type.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Gets or sets the fully qualified name of the type.</summary>
    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = "";

    /// <summary>Gets or sets the namespace in which the type is declared.</summary>
    [JsonPropertyName("namespace")]
    public string Namespace { get; set; } = "";

    /// <summary>Gets or sets the kind of the type (e.g. class, interface, enum).</summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "";

    /// <summary>Gets or sets the XML doc summary for the type, if present.</summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>Gets or sets the fully qualified name of the base type, if any.</summary>
    [JsonPropertyName("baseType")]
    public string? BaseType { get; set; }

    /// <summary>Gets or sets the list of fully qualified interface names implemented by the type.</summary>
    [JsonPropertyName("interfaces")]
    public List<string> Interfaces { get; set; } = [];

    /// <summary>Gets or sets the generic type parameters declared on the type.</summary>
    [JsonPropertyName("genericParameters")]
    public List<ApiGenericParameter> GenericParameters { get; set; } = [];

    /// <summary>Gets or sets the public properties of the type.</summary>
    [JsonPropertyName("properties")]
    public List<ApiPropertyInfo> Properties { get; set; } = [];

    /// <summary>Gets or sets the public methods of the type.</summary>
    [JsonPropertyName("methods")]
    public List<ApiMethodInfo> Methods { get; set; } = [];

    /// <summary>Gets or sets the public events of the type.</summary>
    [JsonPropertyName("events")]
    public List<ApiEventInfo> Events { get; set; } = [];

    /// <summary>Gets or sets the named values for enum types.</summary>
    [JsonPropertyName("enumValues")]
    public List<ApiEnumValue> EnumValues { get; set; } = [];
}

/// <summary>
/// Represents a single generic type parameter and its constraints.
/// </summary>
public sealed class ApiGenericParameter
{
    /// <summary>Gets or sets the name of the generic parameter (e.g. T).</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Gets or sets the constraint names applied to this parameter.</summary>
    [JsonPropertyName("constraints")]
    public List<string> Constraints { get; set; } = [];
}

/// <summary>
/// Represents metadata for a single public property.
/// </summary>
public sealed class ApiPropertyInfo
{
    /// <summary>Gets or sets the property name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Gets or sets the fully qualified type name of the property.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    /// <summary>Gets or sets the XML doc summary for the property, if present.</summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>Gets or sets a value indicating whether this property is a Blazor component parameter.</summary>
    [JsonPropertyName("isParameter")]
    public bool IsParameter { get; set; }

    /// <summary>Gets or sets the default value expression for the property, if known.</summary>
    [JsonPropertyName("defaultValue")]
    public string? DefaultValue { get; set; }
}

/// <summary>
/// Represents metadata for a single public method.
/// </summary>
public sealed class ApiMethodInfo
{
    /// <summary>Gets or sets the method name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Gets or sets the fully qualified return type name.</summary>
    [JsonPropertyName("returnType")]
    public string ReturnType { get; set; } = "";

    /// <summary>Gets or sets the XML doc summary for the method, if present.</summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>Gets or sets the list of parameters accepted by the method.</summary>
    [JsonPropertyName("parameters")]
    public List<ApiParameterInfo> Parameters { get; set; } = [];
}

/// <summary>
/// Represents a single parameter of a method.
/// </summary>
public sealed class ApiParameterInfo
{
    /// <summary>Gets or sets the parameter name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Gets or sets the fully qualified type name of the parameter.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
}

/// <summary>
/// Represents metadata for a single public event.
/// </summary>
public sealed class ApiEventInfo
{
    /// <summary>Gets or sets the event name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Gets or sets the fully qualified delegate type of the event.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    /// <summary>Gets or sets the XML doc summary for the event, if present.</summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }
}

/// <summary>
/// Represents a single named value in an enum type.
/// </summary>
public sealed class ApiEnumValue
{
    /// <summary>Gets or sets the enum member name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Gets or sets the underlying integer value of the enum member.</summary>
    [JsonPropertyName("value")]
    public long Value { get; set; }

    /// <summary>Gets or sets the XML doc summary for the enum member, if present.</summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }
}
