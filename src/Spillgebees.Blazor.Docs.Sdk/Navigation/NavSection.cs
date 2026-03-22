namespace Spillgebees.Blazor.Docs.Sdk.Navigation;

/// <summary>
/// Represents a section in the documentation sidebar, grouping related pages under a common title.
/// </summary>
/// <param name="Title">The display title of the section.</param>
/// <param name="Pages">The pages contained within this section.</param>
public sealed record NavSection(string Title, IReadOnlyList<NavPage> Pages);
