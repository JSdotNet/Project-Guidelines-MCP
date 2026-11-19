# JSdotNet Project Guidelines

Design, architecture, style and structure guidelines for modern .NET (C#) projects, organized as ADRs, building blocks, concepts, AI setup and a UX style guide under `defaults/`. An MCP Server in `src/` exposes these documents for tools/agents.

> Credits: This project builds on the original HexMaster Design Guidelines work by Eduard Keilholz (GitHub: nikneem) and contributors.
> Original repository: https://github.com/nikneem/hexmaster-design-guidelines

## MCP Server (C#, .NET 10)

One MCP server, `jsdotnet-app-defaults` (package `JSdotNet.AppDefaults.Mcp`), serves every
document under `defaults/`. Design is one category of that catalog, not a second server.

### Use it from a project

.NET 10 runs the package without installing it:

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

The package has package type `McpServer` and ships `.mcp/server.json`, so MCP hosts and the MCP
Registry can read its metadata. It fetches the documents from this repository's `defaults/` on
GitHub; set `JSDOTNET_DOCS_PATH` to a local `defaults/` folder (or a repository root holding one)
to serve those instead, and `GITHUB_TOKEN` to raise the GitHub rate limit.

### What it serves

| Primitive | Names |
|---|---|
| Tools | `list_guides`, `list_guides_by_type` (`adrs`, `designs`, `recommendations`, `structures`, `config`, `design`), `search_guides`, `search_guides_by_tag`, `get_guide`, `get_usage_logs` |
| Resources | every document at `defaults://<relative path without .md>`, e.g. `defaults://arc42/adr/0001-platform-and-build` |
| Prompts | `new-app`, `review-against-defaults`, `propose-adr` |

List and read results carry `ttlMs` (five minutes). `ServerInfo.Version` reports the package
version GitVersion stamps at build time.

### Run from source

From the repository root, serving the local `defaults/`:

```powershell
$env:JSDOTNET_DOCS_PATH = "."; dotnet run --project .\src\JSdotNet.AppDefaults.Mcp\JSdotNet.AppDefaults.Mcp.csproj
```

This repository's own [.mcp.json](.mcp.json) starts it that way. The server uses stdio: logs go
to stderr, JSON-RPC to stdout. See [src/JSdotNet.AppDefaults.Mcp/README.md](src/JSdotNet.AppDefaults.Mcp/README.md).

### Publish Results Server

A second MCP server (`src/JSdotNet.MCP.Publish`) lets an agent publish its results to a **configurable file location** instead of serving documentation.

```powershell
dotnet run --project .\src\JSdotNet.MCP.Publish\JSdotNet.MCP.Publish.csproj -- --Publish:RootPath=D:\reports
```

Tools: `publish_result`, `append_result`, `list_published`, `read_published`, `delete_published`, `get_publish_location`, `get_usage_logs`.

The publish root is resolved from `Publish:RootPath` (command line or `Publish__RootPath` environment variable), then `JSDOTNET_PUBLISH_PATH`, and finally a per-user default folder (`%LOCALAPPDATA%\JSdotNet\PublishedResults`). All tool paths are relative to that root — absolute paths and `..` segments are rejected. See [src/JSdotNet.MCP.Publish/README.md](src/JSdotNet.MCP.Publish/README.md) for details.

### Registering new documents

This repository uses `defaults/index.json` as the canonical registry for MCP document discovery.

When you add, rename, edit, or remove markdown files in `defaults/`, also update `defaults/index.json` so metadata and paths stay in sync.

## Repo structure

```
AGENTS.md                         the repository's standing rules (the one root instruction file)
CLAUDE.md                         @AGENTS.md import for Claude Code
.github/copilot-instructions.md   one-sentence pointer at AGENTS.md for GitHub Copilot
.agents/rules/                    path-scoped rules, wrapped per host in .claude/rules/ and .github/instructions/
defaults/                         arc42/ (chapters 02, 04, 05, workspace.dsl, adr/, building-blocks/, concepts/), ai/, design/ and index.json
defaults/design/                  UX style guide served by the design server
plugins/                          guidelines and design agent plugins
src/                              JSdotNet.MCP.Shared, JSdotNet.AppDefaults.Mcp, JSdotNet.MCP.Publish
tests/                            one test project per src/ project
JSdotNet.MCP.slnx
```

## ADRs

The decisions are a set of 13 ADRs, rewritten in place when a decision changes:

- [0001: Platform and Build](defaults/arc42/adr/0001-platform-and-build.md) (Accepted)
- [0002: Aspire, Configuration and Infrastructure](defaults/arc42/adr/0002-aspire-configuration-and-infrastructure.md) (Accepted)
- [0003: Solution Structure](defaults/arc42/adr/0003-solution-structure.md) (Accepted)
- [0004: HTTP API](defaults/arc42/adr/0004-http-api.md) (Accepted)
- [0005: Application Layer](defaults/arc42/adr/0005-application-layer.md) (Accepted)
- [0006: Persistence and Messaging](defaults/arc42/adr/0006-persistence-and-messaging.md) (Accepted)
- [0007: Observability and Resilience](defaults/arc42/adr/0007-observability-and-resilience.md) (Accepted)
- [0008: Security](defaults/arc42/adr/0008-security.md) (Accepted)
- [0009: Frontend](defaults/arc42/adr/0009-frontend.md) (Accepted)
- [0010: Testing](defaults/arc42/adr/0010-testing.md) (Accepted)
- [0011: Setting Up MCP](defaults/arc42/adr/0011-setting-up-mcp.md) (Accepted)
- [0012: Agents in Applications](defaults/arc42/adr/0012-agents-in-applications.md) (Accepted)
- [0013: Agent-Ready Repository](defaults/arc42/adr/0013-agent-ready-repository.md) (Accepted)

The old numbers 0001 to 0019 are retired and never reused; [defaults/arc42/adr/README.md](defaults/arc42/adr/README.md) maps each one to the ADR that replaced it.

## Architecture

The arc42 chapters a project links to from its own `.devbook/arc42/`:

| Chapter | Holds |
|---|---|
| [02 Constraints](defaults/arc42/02-constraints.md) | The fixed givens: .NET, Aspire, EF Core, Azure |
| [04 Solution Strategy](defaults/arc42/04-solution-strategy.md) | Goals to approach, pragmatic domain-driven design, module extraction |
| [05 Building Block View](defaults/arc42/05-building-block-view.md) | The one solution structure, dependency rules and naming |
| [workspace.dsl](defaults/arc42/workspace.dsl) | The C4 seed (Structurizr DSL) a project starts from |

Building blocks, one page each: [Module](defaults/arc42/building-blocks/module.md),
[Domain model](defaults/arc42/building-blocks/domain-model.md),
[Api](defaults/arc42/building-blocks/api.md),
[Data.{Provider}](defaults/arc42/building-blocks/data-provider.md),
[AppHost](defaults/arc42/building-blocks/apphost.md),
[ServiceDefaults](defaults/arc42/building-blocks/service-defaults.md),
[Test harness](defaults/arc42/building-blocks/test-harness.md),
[Notifications](defaults/arc42/building-blocks/notifications.md),
[File storage](defaults/arc42/building-blocks/file-storage.md).

## Recommendations

- [Instruction-File Setup: One AGENTS.md, Wrapped per Host](defaults/ai/copilot-instruction-file-setup.md)
- [Devbook Adoption: Start with the devbook Plugin Alone](defaults/arc42/concepts/devbook-adoption.md)
- [Blazor Frontend Framework Guidance](defaults/arc42/concepts/blazor-frontend-framework-guidance.md)
- [C# Coding Style](defaults/arc42/concepts/csharp-coding-style.md)
- [Object Calisthenics for Domain Code](defaults/arc42/concepts/object-calisthenics-for-domain.md)
- [Specification Pattern for Business Rules](defaults/arc42/concepts/specification-pattern-usage.md)
- [Architecture Testing for Layer and Module Boundaries](defaults/arc42/concepts/testing-architecture.md)
- [End-to-End Testing](defaults/arc42/concepts/testing-end-to-end.md)
- [Integration Testing](defaults/arc42/concepts/testing-integration.md)
- [Testing Shared Instructions](defaults/arc42/concepts/testing-shared.md)
- [Unit Testing with xUnit, Moq, and Bogus](defaults/arc42/concepts/testing-unit.md)

---

## Development

### Building and Testing

```bash
# Build the solution
dotnet build JSdotNet.MCP.slnx

# Run all tests
dotnet test JSdotNet.MCP.slnx

# Run tests with coverage
dotnet test JSdotNet.MCP.slnx --collect:"XPlat Code Coverage" --results-directory ./coverage --settings coverlet.runsettings

# Generate coverage report
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:"Html"
```

### Code Coverage Requirements

- **Core Library** (`JSdotNet.MCP.Shared`): ≥80% line coverage
- **Tests**: All tests must pass
- Coverage reports are automatically generated in CI/CD

---

## CI/CD Workflows

### Build and Publish Workflow

**Workflow**: `.github/workflows/publish-nuget.yml`

Triggers on push to `main` branch when files in `src/` change.

Steps:

1. **Versioning** – GitVersion generates semantic version
2. **Build** – Compiles solution in Release configuration with version info
3. **Test** – Runs all unit tests with 80% coverage enforcement
4. **Coverage Report** – Generates coverage summary
5. **Package** – Creates NuGet package for the MCP Server
6. **Publish** – Pushes package to NuGet.org
7. **Release** – Creates GitHub release with version tag and artifacts

Semantic Versioning Strategy (GitHubFlow):

- **Main branch**: 1.0.0, 1.0.1, 1.0.2... (patch increments)
- **Feature branches** (`feature/*`): 1.1.0-alpha.1, 1.1.0-alpha.2... (minor with alpha pre-release)
- **Release branches** (`release/*`): 1.0.0-beta.1, 1.0.0-beta.2... (beta pre-release)

Configuration: `GitVersion.yml` at repository root.

### NuGet Packages

Published to NuGet.org:

- **JSdotNet.AppDefaults.Mcp** – application defaults MCP server (`jsdotnet-app-defaults`), run with `dnx JSdotNet.AppDefaults.Mcp --yes`
- **JSdotNet.MCP.Publish** – publish-results MCP server (`jsdotnet-publish-mcpserver`)

Each package is published independently: the release workflow only pushes a package when its own
source (or the shared `JSdotNet.MCP.Shared` project) changed on `main`.

Package features:

- .NET 10 tool, runnable with `dnx`
- Automatic document discovery from filesystem or GitHub
- ModelContextProtocol SDK integration
- MIT license

### Setup Requirements

To enable automated publishing, add the following GitHub secret:

- `NUGET_API_KEY` – NuGet.org API key with push permissions

Navigate to: Repository Settings → Secrets and variables → Actions → New repository secret

### Local Version Testing

To check what version GitVersion would generate locally:

```bash
# Install GitVersion tool
dotnet tool install --global GitVersion.Tool

# Run in repository root
dotnet-gitversion
```

## Notes

- All code and examples target `.NET 10`.
- The MCP Server uses the `FileSystemDocumentCatalog` for local development and `GitHubDocumentCatalog` for published scenarios.
- Coverage threshold is enforced at 80% for core library code.
- CI/CD pipeline only triggers on changes to `src/` folder when pushed to `main` branch.

