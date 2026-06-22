---
title: "ADR 0019: C# Standalone Script Files"
date: 2026-06-05
status: Accepted
tags: [scripts, csharp, dotnet, dotnet-10, automation, tooling, devops, file-based-apps]
---
# ADR 0019: C# Standalone Script Files

## Context

Projects frequently require auxiliary automation: seeding databases, generating documentation indexes, scaffolding artefacts, running one-off data migrations, or orchestrating build steps that fall outside the main application. Common approaches carry trade-offs:

- **Bash / PowerShell** — cross-platform friction; no static typing; no NuGet ecosystem.
- **Python** — separate runtime to install and version; inconsistency with .NET toolchain.
- **Dedicated console project** — full project overhead (`.csproj`, solution entry, restore lock file) for what is often a single-purpose, short-lived script.

.NET 10 introduced **file-based apps**: fully standalone `.cs` files with embedded `#:` directives that declare NuGet packages, MSBuild properties, and included files — no project file required. Scripts run via `dotnet run script.cs`. This aligns scripting entirely with the .NET toolchain while eliminating project boilerplate.

## Decision

### Script Runtime

All project scripts are authored as **.NET 10 file-based apps**: standalone `.cs` files using top-level statements and `#:` directives at the top of the file. No `.csproj` is created.

```csharp
// scripts/seed-database.cs
// seed-database — Populates the local development database with reference data.
//
// Prerequisites:
//   DB_CONNECTION — PostgreSQL connection string
//
// Usage:
//   dotnet run scripts/seed-database.cs
//   dotnet run scripts/seed-database.cs -- --truncate

#:package Npgsql
#:package Microsoft.Extensions.Configuration.Json

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

if (args.Length == 0 && Environment.GetEnvironmentVariable("DB_CONNECTION") is null)
{
    Console.Error.WriteLine("Error: DB_CONNECTION environment variable is not set.");
    return 1;
}

await SeedAsync(args, cts.Token);

static async Task SeedAsync(string[] args, CancellationToken ct)
{
    // implementation
}
```

Run with:

```bash
dotnet run scripts/seed-database.cs
dotnet run scripts/seed-database.cs -- --truncate
```

### Folder Layout

```
/scripts/
  Directory.Build.props           # Isolates scripts from solution-level MSBuild settings
  seed-database.cs
  generate-index.cs
  <verb-noun>.cs
```

Rules:

- Each script is a **single `.cs` file** directly in `/scripts/`, named `<verb-noun>` in kebab-case.
- Scripts are **not added to the solution** (`.slnx`); they are independent tooling artefacts.
- Shared utility logic is extracted only when multiple scripts need it; use `#:include helpers.cs` (requires .NET SDK 10.0.300 or later — see [Shared Helpers](#shared-helpers) below).

### Naming Convention

| Concern | Convention | Examples |
|---|---|---|
| Script file | `verb-noun.cs`, kebab-case | `seed-database.cs`, `generate-index.cs`, `publish-docs.cs` |
| Shared helpers | Descriptive noun, kebab-case `.cs` file in `_shared/` | `_shared/database-helper.cs`, `_shared/index-builder.cs` |

### NuGet Package References

NuGet packages are declared with `#:package` directives at the top of the file:

```csharp
#:package Npgsql                         // version resolved from Directory.Packages.props
#:package Serilog@4.2.0                  // explicit version for packages not in central management
#:package Spectre.Console@*              // latest version (use sparingly)
```

**Version alignment with central package management**: when a package is listed in `Directory.Packages.props`, omit the version in the `#:package` directive. The `Directory.Packages.props` file at the solution root is inherited by scripts (via the isolated `scripts/Directory.Build.props`) and its declared versions are used automatically. This keeps script package versions consistent with the rest of the solution.

For packages not present in `Directory.Packages.props`, pin an explicit version.

### Isolating Scripts from Solution MSBuild Settings

The solution root `Directory.Build.props` sets properties such as `RootNamespace` and `TargetFramework` intended for production projects. Scripts inherit parent `Directory.Build.props` files by default, so a local override is placed in `scripts/` to keep scripts cleanly separated:

```xml
<!-- scripts/Directory.Build.props -->
<Project>
  <!--
    Import the solution-level props to inherit TargetFramework, LangVersion,
    Nullable, and ImplicitUsings. Central package versions from
    Directory.Packages.props are also inherited, enabling version-less
    #:package directives for packages already declared there.
  -->
  <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))" />

  <PropertyGroup>
    <!-- Scripts use top-level statements; a namespace root is not meaningful. -->
    <RootNamespace>Scripts</RootNamespace>
    <!--
      Restore lock files are for repeatable builds of production projects.
      Standalone scripts manage their own transient restore cache.
    -->
    <RestorePackagesWithLockFile>false</RestorePackagesWithLockFile>
  </PropertyGroup>
</Project>
```

### Coding Standards

Scripts follow the same C# coding standards as the main solution. Mandatory rules:

1. **Nullable reference types** — inherited from `Directory.Build.props`; do not disable with `#nullable disable`.
2. **Cancellation support** — scripts performing async I/O must accept and propagate a `CancellationToken`. Wire `Console.CancelKeyPress` at the entry point (see example above).
3. **No domain/application references** — scripts must not reference domain, application, or infrastructure projects from the main solution. Scripts are DevOps tooling only. Use `#:project` sparingly and only for shared infrastructure utilities, never for application domain code.
4. **Guard clauses** — validate required arguments and environment variables at the top; exit with a descriptive message to `stderr` and a non-zero exit code on invalid input.
5. **Structured exit codes** — exit `0` on success, non-zero on failure.
6. **No secrets in source** — connection strings, API keys, and tokens are read from environment variables. Never hardcode credentials.

```csharp
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? throw new InvalidOperationException("DB_CONNECTION environment variable is not set.");
```

### Documentation Header

Each script begins with a comment block describing purpose, prerequisites, and usage — before any `#:` directives except the shebang:

```csharp
// generate-index — Regenerates docs/index.json from all markdown files under docs/.
//
// Prerequisites:
//   Run from the repository root.
//
// Usage:
//   dotnet run scripts/generate-index.cs
//   dotnet run scripts/generate-index.cs -- --dry-run

#:package YamlDotNet
```

### Shared Helpers

Reusable logic shared across multiple scripts lives in `/scripts/_shared/`. Include a shared file using the `#:include` directive:

```csharp
#:include _shared/database-helper.cs

// shared file adds types and local functions; top-level statements remain in the main script
```

> **Note**: `#:include` requires .NET SDK 10.0.300 or later. For earlier SDK versions, copy shared utilities into each script or extract them to a project referenced via `#:project`.

## Consequences

### Positive

1. **Zero boilerplate**: no `.csproj`, no solution entry, no restore lock file per script.
2. **Single runtime**: developers only need .NET 10 SDK — no Python, Bash, or Node.js.
3. **Full C# ecosystem**: NuGet packages, async/await, nullable types, and IDE tooling (IntelliSense, debugging via `dotnet run --debug`) available out of the box.
4. **Version consistency**: `#:package` without a version automatically aligns with `Directory.Packages.props`, keeping script dependencies in sync with the solution.
5. **Type safety**: compile-time errors catch bugs before scripts run in CI.
6. **Consistency**: scripts follow the same language standards and patterns as the main codebase.
7. **Discoverable**: all scripts are co-located in a flat `/scripts/` directory.

### Negative

1. **Startup overhead**: .NET cold-start is slower than an equivalent Bash one-liner for trivial tasks. Acceptable for the automation scenarios this convention targets; subsequent runs benefit from build caching.
2. **No solution integration**: scripts are not visible as projects in IDEs by default; open the file directly or run via `dotnet run`.
3. **`#:include` SDK floor**: shared helper inclusion requires SDK 10.0.300+; teams on earlier SDK patches must use workarounds.
4. **Package version drift for non-CPM packages**: packages pinned with explicit versions in `#:package` can drift from actual used versions if not reviewed on dependency updates.

### Mitigation Strategies

- Run `dotnet build scripts/seed-database.cs` before parallel CI invocations to warm the build cache.
- Add a note in `CONTRIBUTING.md` reminding contributors to update `#:package` explicit versions when upgrading the corresponding entry in `Directory.Packages.props`.
- Enforce SDK minimum via `global.json` if `#:include` is used.

## References

- [File-based apps — .NET SDK documentation](https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps)
- [Announcing dotnet run app.cs — .NET Blog, May 2025](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/)
- [Top-level statements in C#](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/top-level-statements)
- ADR 0001: Adopt .NET 10 as Target Framework
- ADR 0002: Central Package Management
