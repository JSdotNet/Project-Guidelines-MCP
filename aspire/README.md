# Aspire config source

This folder makes the repository an [Aspire 17 configuration source](https://aspire.dev/extensibility/config-sources/):
`aspire new` and `aspire init` offer the JSdotNet golden-setup template, the shared hosting
integration and the defaults skills to every new app (ADR 0002).

## Use it

```powershell
$env:ASPIRE_INIT_SOURCE_URL = 'https://github.com/JSdotNet/app-defaults.git'
$env:ASPIRE_INIT_SOURCE_PATH = 'aspire/aspire.config.json'
$env:ASPIRE_INIT_SOURCE_README_URL = 'https://github.com/JSdotNet/app-defaults/blob/main/aspire/README.md'
aspire new
```

Accept the source when the CLI asks, then pick **JSdotNet app defaults** in the template picker.
Only the interactive picker lists a source's templates; `--non-interactive` accepts no template
name from it. Without the source, or in a script, run the template directly:

```powershell
aspire new --template-repository https://github.com/JSdotNet/app-defaults --template-path aspire/templates
```

`aspire update --use-config-source` binds an existing app to the source. `aspire agent init`
then registers the defaults skills in the agent's own plugin settings (`.claude/settings.json`,
`.github/copilot/settings.json`); it also installs Aspire's telemetry hooks for Copilot and
Claude Code at user level (`~/.copilot/hooks/aspire-telemetry.json`, `~/.claude/settings.json`),
even with `--scope project`. `aspire add` finds the shared hosting integration; with
`--non-interactive`, name it exactly: `aspire add JSdotNet.AppDefaults.Hosting`.
`aspire update --config-source` refreshes an app that accepted the source; setting
`configSource.optOut` to `true` in the app's `aspire.config.json` opts it out.

## What it supplies

| Section | Points at | Gives an app |
|---|---|---|
| `templateSources` | [templates/app-defaults/](templates/app-defaults/) | The golden setup, see below |
| `integrationSources` | `JSdotNet.AppDefaults.Hosting` on nuget.org, built from [integrations/](integrations/JSdotNet.AppDefaults.Hosting/) | `AddDefaultsSqlServer`, `AddDefaultsPostgres`, `WithDefaultsHealthCheck` |
| `skillSources` | The `app-defaults` plugin in this repository's marketplace ([.claude-plugin/marketplace.json](../.claude-plugin/marketplace.json)) | The defaults skills for Claude Code, Copilot, OpenCode and Codex |

The template (`dotnet new` short name `jsdotnet-app`) scaffolds, for `-n Contoso.Orders`:

```text
Contoso.Orders/
├── src/
│   ├── Aspire/Contoso.Orders.AppHost/         # Microsoft.NET.Sdk, AddDotnetProject, SQL Server
│   ├── Aspire/Contoso.Orders.ServiceDefaults/
│   ├── Core/Contoso.Orders.Core/              # union Result<T>, handler interfaces
│   └── Orders/                                # module name = last segment of the app name
│       ├── Contoso.Orders/                    # domain model, feature slices, ports
│       ├── Contoso.Orders.Abstractions/
│       ├── Contoso.Orders.Api/
│       └── Contoso.Orders.Data.SqlServer/
├── tests/                                     # UnitTests, IntegrationTests, ArchitectureTests
├── .editorconfig  .mcp.json (dnx)  AGENTS.md  CLAUDE.md  .github/copilot-instructions.md
├── Directory.Build.props  Directory.Packages.props  global.json  aspire.config.json
└── Contoso.Orders.slnx
```

It targets .NET 11 with warnings as errors (ADR 0001). The database is SQL Server because its
EF Core 11 provider is GA; an app that chooses another database renames `Data.SqlServer` and
records the choice in its own ADR (ADR 0006).

- **AppHost** — calls `AddDefaultsSqlServer` and `WithDefaultsHealthCheck` from
  `JSdotNet.AppDefaults.Hosting`, pinned in `Directory.Packages.props`.
- **`nuget.config`** — nuget.org only, with `minPublishAgeHours="72"` (ADR 0001).
- **Sample slice** — `Item.Version` is the concurrency token: `RenameItem` (`PUT
  /{module}/items/{id}`) takes the version the client read, the aggregate increments it, and a
  stale version or a `DbUpdateConcurrencyException` becomes a 409 Problem Details (ADR 0006).
- **Lock files are not shipped.** The template excludes `packages.lock.json`; the app's first
  `dotnet restore` writes one per project from the template's exact pins, and the app commits
  them with its first change, after which CI restores with `--locked-mode`. Shipping them would
  make every pin change here a lock-file regeneration nothing in this repository checks.

## Trust and pinning

Owned by the JSdotNet maintainers; changes land through reviewed pull requests to `main`
(AGENTS.md). Templates and plugins run code on a developer's machine, so accept the source only
from this repository. The sources follow `main` rather than a commit pin, because a file cannot
pin the commit that contains it; pin `ASPIRE_INIT_SOURCE_REF` to a reviewed commit to freeze
what a team receives. The integration package is versioned by GitVersion and published by the
`publish-nuget` workflow.
