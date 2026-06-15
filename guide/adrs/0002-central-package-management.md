---
title: "ADR 0002: Central Package Management for .NET Solutions"
date: 2026-05-28
status: Accepted
tags: [nuget, package-management, dependencies, dotnet, adr, maintainability]
---
# ADR 0002: Central Package Management for .NET Solutions

## Context

The repository defines conventions for modular .NET solutions with multiple projects. As the number of projects and modules grows, package versions can drift when each project declares versions independently inside its own project file.

Version drift increases maintenance overhead, creates inconsistent transitive dependency graphs, and can introduce hard-to-diagnose build/runtime issues. Manual synchronization of package versions across many project files is error-prone.

NuGet Central Package Management (CPM) provides a native mechanism to declare package versions once and share them across the solution.

## Decision

We ADOPT NuGet Central Package Management for all multi-project .NET solutions in this repository.

1. Package versions MUST be declared centrally in a root-level `Directory.Packages.props` file.
2. Project files SHOULD reference packages without local `Version` attributes unless an explicit and documented exception is required.
3. New package additions MUST include a central version entry.
4. Version upgrades MUST be applied centrally and validated across impacted projects.
5. Exceptions (project-specific version overrides) MUST be rare, justified, and documented in the project file with a short rationale comment.

### Hierarchical structure for package sets

To keep versions aligned while allowing different default package sets (for example, test-only packages), use a hierarchical props layout:

1. Keep all `PackageVersion` entries in root `Directory.Packages.props` as the single version catalog.
2. Use root `Directory.Packages.props` to enable CPM and enforce central version governance.
3. Put shared build defaults (for example, `TargetFramework` and `LangVersion`) in root `Directory.Build.props`.
4. Add child `Directory.Build.props` files under `src/` and `test/` to apply subtree-specific package sets via `PackageReference`.
5. In child `Directory.Build.props` files, import the root `Directory.Build.props` because only the nearest file is auto-loaded.
6. Put test-only defaults in `test/Directory.Build.props` (for example, `xunit.v3`, `Microsoft.NET.Test.Sdk`, test utilities, and test analyzers).
7. Keep production-only defaults in `src/Directory.Build.props` so test dependencies do not leak into production projects.

This pattern keeps version governance centralized while allowing scoped package baselines per subtree.

### Example

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
    <CentralPackageVersionEnabled>false</CentralPackageVersionEnabled>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Microsoft.Extensions.Hosting" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
    <PackageVersion Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="9.0.0" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
    <PackageVersion Include="xunit.v3" Version="3.2.1" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.1.5" />
    <PackageVersion Include="coverlet.collector" Version="6.0.4" />
  </ItemGroup>
</Project>
```

### Hierarchical example (root + src/test build props)

Root `Directory.Packages.props`:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
    <CentralPackageVersionEnabled>false</CentralPackageVersionEnabled>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Microsoft.Extensions.Hosting" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
    <PackageVersion Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="9.0.0" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
    <PackageVersion Include="xunit.v3" Version="3.2.1" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.1.5" />
    <PackageVersion Include="coverlet.collector" Version="6.0.4" />
  </ItemGroup>
</Project>
```

Root `Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>14.0</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

`src/Directory.Build.props`:

```xml
<Project>
  <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))" />

  <ItemGroup>
    <!-- Shared production project references go here -->
    <!-- <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" /> -->
  </ItemGroup>
</Project>
```

`test/Directory.Build.props`:

```xml
<Project>
  <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))" />

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>
</Project>
```

`test/SomeFeature.Tests.csproj` (inherits defaults; no local versions):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
</Project>
```

## Consequences

Positive:

- Single source of truth for package versions.
- Reduced version drift and dependency inconsistency between projects.
- Faster and safer dependency upgrades.
- Cleaner project files with less repetitive metadata.
- Better visibility for security and compliance reviews.

Negative/Trade-offs:

- Initial migration effort is required for existing projects.
- Teams need discipline to update central entries when adding packages.
- Rare package version exceptions require explicit handling and review.

## Migration considerations

- Create root `Directory.Packages.props` as the central version catalog.
- Move package versions from project files into central `PackageVersion` entries.
- Define `TargetFramework` and `LangVersion` in root `Directory.Build.props`.
- Add child `Directory.Build.props` files in `src/` and `test/` for subtree defaults when needed.
- Remove redundant `Version` attributes from `PackageReference` items.
- Verify restore/build/test for the full solution after migration.
- Add CI validation to detect unexpected package version overrides where practical.

## References

- NuGet Central Package Management documentation
- ADR 0001: Adopt .NET 10 as Target Framework
- ADR 0005: Modular Monolith Project Structure
