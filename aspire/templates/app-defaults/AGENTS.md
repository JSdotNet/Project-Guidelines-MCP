# AGENTS.md

Company.Product follows the JSdotNet application defaults. This file holds the repository's
standing rules; `CLAUDE.md` and `.github/copilot-instructions.md` only point here.

## Defaults first

For any question about architecture, patterns, C# style, testing or observability, ask the
`jsdotnet-app-defaults` MCP server (`search_guides`, `get_guide`; set up in `.mcp.json`) and
cite the ADR it answers from. When it is unreachable, say so instead of answering from memory.
A decision this app makes differently is recorded in its own ADR.

## Structure

| Path | Holds |
|---|---|
| `src/Aspire/Company.Product.AppHost` | The Aspire model of every resource (ADR 0002) |
| `src/Aspire/Company.Product.ServiceDefaults` | Telemetry, health, discovery and resilience (ADR 0007) |
| `src/Core/Company.Product.Core` | `Result<T>` and the handler interfaces (ADR 0005) |
| `src/ModuleName/` | The ModuleName module: domain and slices, `Abstractions`, `Api`, `Data.SqlServer` (ADR 0003) |
| `tests/` | Unit, integration and architecture tests; nothing test-only lives in `src/` (ADR 0010) |

## Working here

- Run the app with `aspire start` (detached) or `aspire run`; read its state with
  `aspire describe`, `aspire logs` and `aspire otel logs`, and stop it with `aspire stop`.
- Build with `dotnet build`; warnings are errors (ADR 0001). Never suppress an Aspire or
  analyzer diagnostic to get a green build.
- Package versions live only in `Directory.Packages.props`; restore writes a `packages.lock.json`
  per project, committed with the change, and CI restores with `--locked-mode`.
- A new use case is a feature slice: a command or query and its handler in the module, an
  endpoint class in the Api, and its tests.
- Settings and secrets are Aspire parameters; never commit a secret.
