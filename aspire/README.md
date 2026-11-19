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

Pick **JSdotNet app defaults** in the template picker and accept the source when the CLI asks.
Without the source, run the template directly:

```powershell
aspire new --template-repository https://github.com/JSdotNet/app-defaults --template-path aspire/templates
```

`aspire agent init` then registers the defaults skills in the agent's own plugin settings, and
`aspire add` finds the shared hosting integration. `aspire update --config-source` refreshes an
app that accepted the source; setting `configSource.optOut` to `true` in the app's
`aspire.config.json` opts it out.

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

## Trust and pinning

Owned by the JSdotNet maintainers; changes land through reviewed pull requests to `main`
(AGENTS.md). Templates and plugins run code on a developer's machine, so accept the source only
from this repository. The sources follow `main` rather than a commit pin, because a file cannot
pin the commit that contains it; pin `ASPIRE_INIT_SOURCE_REF` to a reviewed commit to freeze
what a team receives. The integration package is versioned by GitVersion and published by the
`publish-nuget` workflow.
