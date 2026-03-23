# Spillgebees.Blazor.Docs.Sdk

[![Build & test](https://github.com/Spillgebees/Blazor.Docs.Sdk/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/Spillgebees/Blazor.Docs.Sdk/actions/workflows/build-and-test.yml)
[![NuGet](https://img.shields.io/nuget/v/Spillgebees.Blazor.Docs.Sdk?label=nuget)](https://www.nuget.org/packages/Spillgebees.Blazor.Docs.Sdk)

A Razor Class Library NuGet package for building and deploying documentation sites for Spillgebees Blazor component libraries. Each component repo hosts its own docs site by referencing this SDK, without a combined repo or companion PRs.

## Features

- Shell layout with collapsible sidebar, top bar, and footer
- Dark/light theme toggle with `localStorage` persistence
- Live component rendering alongside syntax-highlighted source code via `ExampleView`
- Auto-generated API reference pages from XML doc comments and reflection
- Build-time source extraction and API manifest generation via MSBuild targets (runs automatically for direct package consumers)
- Self-hosted Inter Variable and JetBrains Mono fonts
- Reusable GitHub Actions workflow for deploying to GitHub Pages

## Package Sources

This package is published to [nuget.org](https://www.nuget.org/packages/Spillgebees.Blazor.Docs.Sdk). No special feed configuration is needed.

## Quick Start

### 1. Install the NuGet package

```xml
<PackageReference Include="Spillgebees.Blazor.Docs.Sdk" />
```

### 2. Add a ProjectReference to your library

```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Spillgebees.Blazor.Docs.Sdk" />
    <ProjectReference Include="../YourLibrary/YourLibrary.csproj" />
    <ProjectReference Include="../YourLibrary.Samples.Shared/YourLibrary.Samples.Shared.csproj" />
  </ItemGroup>
</Project>
```

### 3. Create `MainLayout.razor` with `DocSite`

```razor
@inherits LayoutComponentBase

<DocSite ProjectName="Your.Library"
         LogoEmoji="🍓"
         GitHubUrl="https://github.com/Spillgebees/Your.Library"
         Navigation="@Navigation">
    @Body
</DocSite>

@code {
    private static readonly NavSection[] Navigation =
    [
        new("Getting Started", [
            new NavPage("Home", ""),
            new NavPage("Installation", "installation"),
        ]),
        new("Components", [
            new NavPage("MyComponent", "components/my-component"),
        ]),
        new("API Reference", ApiReferenceNav.Generate<MyComponent>()),
    ];
}
```

### 4. Add the CSS link to `wwwroot/index.html`

```html
<link rel="stylesheet" href="_content/Spillgebees.Blazor.Docs.Sdk/docs-sdk.css" />
```

### 5. Create pages with `@page` directives

```razor
@page "/installation"

<h1>Installation</h1>
<p>Add the NuGet package to your project.</p>
```

## Components

| Component | Description |
|-----------|-------------|
| `DocSite` | Root layout component wrapping the entire app with sidebar, top bar, and footer |
| `ExampleView` | Renders a live Blazor component alongside its build-time-extracted, syntax-highlighted source code |
| `CodeBlock` | Displays a syntax-highlighted code snippet with a copy-to-clipboard button |
| `ApiDoc` | Auto-generated API documentation page for a type, sourced from XML doc comments and reflection |
| `ThemeToggle` | Switches between dark and light mode and persists the preference to `localStorage` |
| `DocSidebar` | Collapsible navigation sidebar driven by the `Navigation` parameter on `DocSite` |
| `DocFooter` | Footer displaying the GitHub link configured on `DocSite` |

## Running the Demo

```bash
dotnet run --project samples/Spillgebees.Blazor.Docs.Sdk.Demo/
```

## Deployment

The SDK provides a reusable GitHub Actions workflow at `.github/workflows/deploy-docs.yml` for deploying to GitHub Pages. Consuming repos reference it as follows:

```yaml
# .github/workflows/deploy-docs.yml (in the consuming repo)
name: Deploy Docs

on:
  push:
    branches: [main]

jobs:
  deploy:
    uses: Spillgebees/Blazor.Docs.Sdk/.github/workflows/deploy-docs.yml@main
    with:
      docs-project-path: src/Docs/Docs.csproj
      base-href: /Your.Library/
```

The workflow publishes the WASM app, patches the `<base href>` for the GitHub Pages subpath, and deploys to `spillgebees.github.io/{repo-name}/`.

## License

MIT
