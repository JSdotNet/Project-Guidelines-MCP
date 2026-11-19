# AGENTS.md

This repository is the single source of truth for JSdotNet .NET C# project guidelines. The
guidance lives in `defaults/` and is served by the MCP server built from `src/`. This
file governs how to maintain **this repository** — not how to write .NET code.

## Guidance authority

Architectural and coding guidance lives in `defaults/`, not here. For any question about
architecture, patterns, C# style, testing, observability, or another technical decision:

1. **MCP server first** — call `jsdotnet-app-defaults` (`search_guides`, `list_guides`,
   `get_guide`). Server setup: [defaults/ai/config/.mcp.json.md](defaults/ai/config/.mcp.json.md).
2. **Local docs second** — when the server is unavailable, read `defaults/index.json` and the
   markdown files it references.
3. **Disclose when neither is reachable** — say "I cannot verify this against the guidelines
   (MCP server unreachable)"; never answer from memory alone.

- Cite the source of every architectural answer: ADR number, doc id, or relative path
  (e.g. `// ADR-0007: circuit breaker at adapter layer`).
- When a request conflicts with established guidance, name the conflict and offer the
  compliant alternative.
- When no document covers the question, propose a new ADR or recommendation instead of
  answering from first principles.
- Externalize configuration and secrets; never commit credentials.

## Repository structure

| Path | Holds |
|---|---|
| `defaults/` | The application defaults both MCP servers serve, shaped like a devbook `.devbook/` |
| `defaults/index.json` | The one document metadata index — update it whenever a doc changes |
| `defaults/arc42/` | arc42 chapters `02`, `04`, `05`, `07` and `08`, and the C4 seed `workspace.dsl` |
| `defaults/arc42/adr/` | ADRs 0001–0013 as `NNNN-title.md`, rewritten in place; `README.md` maps retired numbers |
| `defaults/arc42/building-blocks/` | One page per building block of `05-building-block-view.md` |
| `defaults/ai/` | Instruction-file setup, skill authoring, and standard config files in `config/` |
| `defaults/design/` | UX style guide, served as the `design` category |
| `aspire/` | Aspire 17 config source: `aspire.config.json`, the golden-setup template and the hosting integration |
| `plugins/app-defaults/` | The one agent plugin: `skills/<name>/SKILL.md` and both host manifests, targeting server `jsdotnet-app-defaults` |
| `scripts/` | C# standalone automation scripts |
| `src/` | MCP server production code |
| `tests/` | MCP server test projects |
| `JSdotNet.MCP.slnx` | Solution file |
| `.agents/rules/` | Path-scoped rules, wrapped per host in `.claude/rules/` and `.github/instructions/` |
| `AGENTS.md` | This file; `CLAUDE.md` and `.github/copilot-instructions.md` wrap it per host |

## Path-scoped rules

Rules for one kind of file live in [.agents/rules/](.agents/rules/README.md), wrapped per host
under `.claude/rules/` and `.github/instructions/`:

- `defaults/` taxonomy, front matter and `defaults/index.json`: [defaults.md](.agents/rules/defaults.md).
- `defaults/design/` style-guide authoring: [design-content.md](.agents/rules/design-content.md).
- `src/` and `tests/` server invariants and coverage floor: [mcp-server.md](.agents/rules/mcp-server.md).

## Contribution workflow

- Branches: `feature/<short-phrase>`, `fix/<issue-id>`, `guide/<topic>`, `adr/<NNNN-title>`.
- ADRs: open a PR adding `defaults/arc42/adr/NNNN-title.md`, reserving the next number; review checks
  that the consequences are clear.
- Commits: Conventional Commits (`feat:`, `fix:`, `docs:`, `refactor:`, `chore:`, …).
- Merge needs a passing CI build and review from at least one maintainer.
- Create every pull request with the `pr-jsdotnet` skill
  ([.github/skills/pr-jsdotnet/SKILL.md](.github/skills/pr-jsdotnet/SKILL.md)), never the
  built-in PR tool, so it is authored with JSdotNet credentials via `gh pr create`.

## Dependencies

- Manage every package version centrally (ADR 0001, `0001-platform-and-build`).
- Pin versions of critical libraries (logging, DI, resilience) and update them in a dedicated
  `chore:` PR.
- Prefer the BCL over a third-party package where they are equivalent.

Revisit this file after a major ADR shift or a significant change to the MCP toolset.
