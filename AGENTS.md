# Blazor.Docs.Sdk — Project Agent Instructions

## Project Overview

**Spillgebees.Blazor.Docs.Sdk** is a shared Razor Class Library NuGet package that provides everything needed to build and deploy documentation sites for Spillgebees Blazor component libraries. Each component repo (Blazor.Map, Blazor.RichTextEditor, etc.) hosts its own docs site using this SDK.

## Architecture

### Solution structure

```text
Blazor.Docs.Sdk.slnx                                      # XML solution (root)
├── src/Spillgebees.Blazor.Docs.Sdk/                       # Razor Class Library (NuGet package)
│   ├── Components/                                        # Shell layout, ExampleView, ApiDoc, etc.
│   ├── Navigation/                                        # NavSection, NavPage, ApiReferenceNav models
│   ├── Build/                                             # MSBuild-time source extraction & API manifest generation
│   └── wwwroot/                                           # CSS, fonts (Inter Variable, JetBrains Mono Variable)
├── src/Spillgebees.Blazor.Docs.Sdk.Assets/                # TypeScript/Vite build -- compiles CSS/JS assets into wwwroot
└── tests/Spillgebees.Blazor.Docs.Sdk.Tests/               # TUnit + bUnit tests
```

### SDK components

- **DocSite** — root layout: sidebar, top bar, footer, theme toggle
- **ExampleView** — renders live Blazor component + syntax-highlighted source tabs
- **ApiDoc** — auto-generated API documentation from XML doc comments + reflection
- **CodeBlock** — syntax-highlighted code display with copy button
- **ThemeToggle** — dark/light mode switch with localStorage persistence

### Build-time infrastructure

- **Source extraction** — MSBuild targets discover `ExampleView<TComponent>` references, locate source files, highlight with highlight.js, embed as assembly resources
- **API manifest generation** — MSBuild targets process XML doc files + reflection into a JSON manifest embedded as a resource

### .NET target

The library targets `net10.0` (configured in `src/General.targets`).
ASP.NET Core package versions are pinned in `src/Directory.Packages.props`.

## Testing

- **Framework:** TUnit + AwesomeAssertions + bUnit
- **Run tests:** `dotnet test --solution Blazor.Docs.Sdk.slnx`

## Dev tooling

- **CSharpier:** formats `.cs`, `.csproj`, `.props`, `.targets`, `.slnx`, `.xml`
- **Husky.Net:** pre-commit hook runs CSharpier on staged files
