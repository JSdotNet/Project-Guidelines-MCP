---
title: "ADR 0001: Platform and Build"
date: 2026-11-19
status: Accepted
tags: [dotnet, csharp, global-json, central-package-management, nuget, build, analyzers, editorconfig, scripts, file-based-apps, adr]
---
# ADR 0001: Platform and Build

## Context

.NET now gives an STS release the same 24 months of support as an LTS, counted from the next
release, so .NET 11 (STS) and .NET 10 (LTS) both reach end of support in November 2028.
Staying on the LTS buys no support time and delays language features by a year. Version drift
between projects, unpinned transitive packages and warnings that pile up make every upgrade
harder, and brand-new package versions are a supply-chain risk.

## Decision

**Runtime.** Every app follows the newest GA .NET release, STS included: today .NET 11 and
C# 15. A repository moves within three months of GA. `global.json` pins the SDK with
`"rollForward": "latestFeature"`. The C# language version is the SDK default; preview
features (`LangVersion preview`, `EnablePreviewFeatures`) are never used.

**Packages.** Central Package Management: every version lives in the root
`Directory.Packages.props`, with `CentralPackageTransitivePinningEnabled` on and
`RestorePackagesWithLockFile` committing a `packages.lock.json` per project. NuGet's minimum
package publication age setting is on, so a version published only hours ago is not restored.

**Build policy**, set once in the root `Directory.Build.props`:

```xml
<AnalysisLevel>latest</AnalysisLevel>
<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
<WarningsAsErrors>$(WarningsAsErrors);CS8509</WarningsAsErrors>
```

- One root `.editorconfig` holds every style and analyzer severity; no nested ones.
- `TreatWarningsAsErrors` is always on, in every configuration. A non-exhaustive `switch`
  (CS8509) is an error, which ADR 0005's `Result<T>` relies on.
- Aspire's analyzers are part of the build like any other; an obsolete Aspire API fails it
  (ADR 0002).
- CI restores with `--locked-mode`, so a lock file that does not match fails the build.

**Scripts.** Repository automation is C# file-based apps: one `verb-noun.cs` per script in
`scripts/`, run with `dotnet run scripts/<name>.cs`, packages declared with `#:package`
(version-less when `Directory.Packages.props` lists them), never added to the solution.
`scripts/Directory.Build.props` isolates them from the production build settings.

## Consequences

### Positive

- Language and runtime features arrive a year sooner at no cost in support time.
- One place for versions and one for style; a broken lock file or new warning stops CI
  instead of reaching `main`.
- Scripts use the same language, packages and tooling as the apps.

### Negative

- One major upgrade per year instead of one per two years.
- Warnings as errors makes analyzer and SDK updates break the build until fixed; they land
  in their own `chore:` pull request.
- The publication-age delay holds back an urgent security fix until it ages or is allowed
  explicitly.

## References

- [.NET support policy](https://dotnet.microsoft.com/platform/support/policy/dotnet-core)
- [Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management)
- [File-based apps](https://learn.microsoft.com/dotnet/core/sdk/file-based-apps)
- ADR 0002: Aspire, Configuration and Infrastructure
- ADR 0005: Application Layer

Replaces: ADR 0001 (Adopt .NET 10), ADR 0002 (Central Package Management), ADR 0019 (C# Standalone Script Files).
