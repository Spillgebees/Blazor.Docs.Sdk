namespace Spillgebees.Blazor.Docs.Sdk;

/// <summary>
/// Represents a single navigable page in the documentation sidebar.
/// </summary>
/// <param name="Title">The display title of the page.</param>
/// <param name="Href">The URL path of the page.</param>
public sealed record NavPage(string Title, string Href);
