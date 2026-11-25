# JSdotNet App Defaults

The default setup for a new JSdotNet .NET application: one golden setup, with one answer per
concern and no alternatives listed. The decisions live in [defaults/](defaults/) as ADRs and
arc42 chapters a project's `.devbook/` links to instead of copying. The `jsdotnet-app-defaults`
MCP server serves them to agents, and [aspire/](aspire/) publishes them as an Aspire 17 config
source, so `aspire new` scaffolds an app that already follows them.

> Credits: this project builds on the original HexMaster Design Guidelines work by Eduard
> Keilholz ([nikneem/hexmaster-design-guidelines](https://github.com/nikneem/hexmaster-design-guidelines))
> and contributors.

## The golden setup

| Concern | Default | ADR |
|---|---|---|
| Runtime | The newest GA .NET release, STS included (today .NET 11, C# 15); `global.json` pinned with `latestFeature`; no preview features | [0001](defaults/arc42/adr/0001-platform-and-build.md) |
| Build and packages | `.slnx` only; `Directory.Build.props` with `TreatWarningsAsErrors` always on and CS8509 an error; Central Package Management with lock files and transitive pinning; C# file-based apps in `scripts/` | [0001](defaults/arc42/adr/0001-platform-and-build.md) |
| Orchestration | Aspire 17, mandatory: a `Microsoft.NET.Sdk` AppHost with `Aspire.Hosting.Dotnet` and `AddDotnetProject`, started with `aspire run`; this repository is the Aspire config source | [0002](defaults/arc42/adr/0002-aspire-configuration-and-infrastructure.md) |
| Configuration and infrastructure | Aspire parameters and secrets per environment; the AppHost is the infrastructure code, `aspire publish` generates it and `aspire deploy` applies it | [0002](defaults/arc42/adr/0002-aspire-configuration-and-infrastructure.md) |
| Solution shape | Modular monolith with feature slices; a small app has one module; data access in `{Module}.Data.{Provider}`; tests under root `tests/` | [0003](defaults/arc42/adr/0003-solution-structure.md) |
| HTTP API | Minimal APIs with endpoint classes, native OpenAPI and Scalar; built-in `AddValidation()` at the edge; Problem Details for every error | [0004](defaults/arc42/adr/0004-http-api.md) |
| Application flow | Hand-rolled CQRS handlers (no mediator) returning a `Result<T>` union | [0005](defaults/arc42/adr/0005-application-layer.md) |
| Persistence and messaging | EF Core only, database chosen per app in its own ADR; a concurrency token on every aggregate; integration events through a transactional outbox, broker chosen per app | [0006](defaults/arc42/adr/0006-persistence-and-messaging.md) |
| Observability and resilience | OpenTelemetry and the standard resilience handler from ServiceDefaults; the Aspire dashboard locally; `[LoggerMessage]` logging | [0007](defaults/arc42/adr/0007-observability-and-resilience.md) |
| Security | External OIDC identity provider, full JWT validation, zero-trust authorization policies | [0008](defaults/arc42/adr/0008-security.md) |
| Frontend | Rules, not a framework: its own app under `src/App/`, an Aspire resource, a client generated from OpenAPI, `defaults/design/` tokens as CSS variables; Blazor or React + Vite, picked in the app's own ADR | [0009](defaults/arc42/adr/0009-frontend.md) |
| Tests | xUnit v3 on Microsoft Testing Platform with NSubstitute and Bogus; integration tests on `WebApplicationFactory` with throwaway containers; Playwright E2E through `aspire start` | [0010](defaults/arc42/adr/0010-testing.md) |
| MCP in an app | C# SDK 2.x, stateless Streamable HTTP in the Api host, its own skill, OAuth 2.1 with agent and user identities checked on every call, audited | [0011](defaults/arc42/adr/0011-setting-up-mcp.md) |
| Agents in an app | Microsoft Agent Framework on `Microsoft.Extensions.AI`, models from Foundry behind one adapter, tools only from the app's own MCP tools or handlers | [0012](defaults/arc42/adr/0012-agents-in-applications.md) |
| Agent-ready repository | One `AGENTS.md` wrapped per host, path-scoped rules, Aspire skills CLI first, this server through `dnx` | [0013](defaults/arc42/adr/0013-agent-ready-repository.md) |

The arc42 chapters around the ADRs — [constraints](defaults/arc42/02-constraints.md),
[solution strategy](defaults/arc42/04-solution-strategy.md),
[building blocks](defaults/arc42/05-building-block-view.md),
[deployment](defaults/arc42/07-deployment-view.md) and
[crosscutting concepts](defaults/arc42/08-crosscutting-concepts.md) — and the
[UX style guide](defaults/design/) fill in the detail. Retired ADR numbers are mapped in
[defaults/arc42/adr/README.md](defaults/arc42/adr/README.md).

## Use it from a project

**The MCP server.** Add it to the project's `.mcp.json`; `dnx` (in the .NET SDK) runs the
`JSdotNet.AppDefaults.Mcp` package without a global install:

```json
{
  "mcpServers": {
    "jsdotnet-app-defaults": {
      "type": "stdio",
      "command": "dnx",
      "args": ["JSdotNet.AppDefaults.Mcp", "--yes"]
    }
  }
}
```

It serves the tools `list_guides`, `search_guides` and `get_guide`, every document as a
`defaults://` resource, and the prompts `new-app`, `review-against-defaults` and `propose-adr`.
Per-host setup (Claude Code, GitHub Copilot CLI, VS Code, Visual Studio, the Copilot coding
agent), the agent plugin and running from source are in [docs/install.md](docs/install.md).

**The Aspire config source.** Point the Aspire CLI at this repository and `aspire new`,
`aspire agent init` and `aspire add` offer the golden-setup template, the shared hosting
integration and the defaults skills:

```powershell
$env:ASPIRE_INIT_SOURCE_URL = 'https://github.com/JSdotNet/app-defaults.git'
$env:ASPIRE_INIT_SOURCE_PATH = 'aspire/aspire.config.json'
$env:ASPIRE_INIT_SOURCE_README_URL = 'https://github.com/JSdotNet/app-defaults/blob/main/aspire/README.md'
```

The source follows `main`; set `ASPIRE_INIT_SOURCE_REF` to a reviewed commit id to pin it.
[aspire/README.md](aspire/README.md) says what accepting the source changes and how to opt out.

**From a devbook project.** A decision the organization already records is linked, not
copied. In the project's `.devbook/arc42/09-architecture-decisions.md`, link these ADRs and
record only the app's own decisions — its database, broker, identity provider and frontend
framework — as its own ADRs:

```markdown
## Organization defaults

This application follows the JSdotNet app defaults at commit `<sha>`:
[ADRs 0001–0013](https://github.com/JSdotNet/app-defaults/tree/<sha>/defaults/arc42/adr).
Deviations are recorded below as this application's own ADRs.
```

Its other arc42 chapters link the matching `defaults/arc42/` chapter the same way. How to adopt
devbook itself is the [devbook](https://github.com/JSdotNet/devbook) repository's to say.

## Start a new app

```powershell
$env:ASPIRE_INIT_SOURCE_URL = 'https://github.com/JSdotNet/app-defaults.git'
$env:ASPIRE_INIT_SOURCE_PATH = 'aspire/aspire.config.json'
aspire new
```

Pick **JSdotNet app defaults** and accept the source. Without the source, run the template
directly:

```powershell
aspire new --template-repository https://github.com/JSdotNet/app-defaults --template-path aspire/templates
```

The template scaffolds the golden setup: an AppHost with `AddDotnetProject`, ServiceDefaults,
one module (Api, module, `Data.SqlServer`), the test projects, the ADR 0001 build files,
`.editorconfig`, `AGENTS.md` with its host wrappers and a `dnx` `.mcp.json`. Then run
`aspire agent init` for the skills and record the app's own choices as ADRs.

## Change a default

A default changes through a pull request that rewrites the ADR in place (git keeps the old
text) or, where no ADR covers the topic, adds the next number. Update
[defaults/index.json](defaults/index.json) in the same change. Branch names, commit style, the
rules per folder and the review bar are in [AGENTS.md](AGENTS.md).

```powershell
dotnet build JSdotNet.AppDefaults.slnx
```

```powershell
dotnet test JSdotNet.AppDefaults.slnx
```

Merging to `main` publishes the `JSdotNet.AppDefaults.Mcp` package through
[publish-nuget.yml](.github/workflows/publish-nuget.yml) when its source changed; the server
reads documents from `main`, so a merged default reaches every project at once.
A merge by [auto-merge.yml](.github/workflows/auto-merge.yml) uses `GITHUB_TOKEN` when the
repository has no `AUTO_MERGE_TOKEN` secret, and such a push starts no workflow, so auto-merge
dispatches `publish-nuget.yml` itself with `since` set to `main`'s commit before the merge. A
manual dispatch without `since` publishes every package, the retired notices included.
