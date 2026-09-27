# AGENTS.md

This repository is the single source of truth for JSdotNet .NET C# project guidelines. The
guidance lives in `guide/` and is served by the guidelines MCP server built from `src/`. This
file governs how to maintain **this repository** — not how to write .NET code.

## Guidance authority

Architectural and coding guidance lives in `guide/`, not here. For any question about
architecture, patterns, C# style, testing, observability, or another technical decision:

1. **MCP server first** — call `jsdotnet-coding-guidelines` (`search_guides`, `list_guides`,
   `get_guide`). Server setup: [guide/config/.mcp.json.md](guide/config/.mcp.json.md).
2. **Local docs second** — when the server is unavailable, read `guide/index.json` and the
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
| `guide/index.json` | Document metadata index — update it whenever a doc changes |
| `guide/adrs/` | ADRs as `NNNN-title.md`, numbered sequentially (0001, 0002, …) |
| `guide/designs/` | Deeper design explorations and diagrams |
| `guide/recommendations/` | Prescriptive guidance and best practices |
| `guide/structures/` | Example folder/file scaffolds and templates |
| `guide/config/` | Standard configuration files (`.mcp.json`, `github-app.yml`) |
| `design/` | UX style guide served by the design MCP server |
| `plugins/` | Agent plugins (`guidelines`, `design`) and their skills |
| `scripts/` | C# standalone automation scripts |
| `src/` | MCP server production code |
| `tests/` | MCP server test projects |
| `JSdotNet.MCP.slnx` | Solution file |
| `.agents/rules/` | Path-scoped rules, wrapped per host in `.claude/rules/` and `.github/instructions/` |
| `.devbook/` | Devbook folders about this repository itself, and their checker — see *Devbook folders* below |
| `AGENTS.md` | This file; `CLAUDE.md` and `.github/copilot-instructions.md` wrap it per host |

## Path-scoped rules

Rules for one kind of file live in [.agents/rules/](.agents/rules/README.md), wrapped per host
under `.claude/rules/` and `.github/instructions/`:

- `guide/` taxonomy, front matter and `guide/index.json`: [guide.md](.agents/rules/guide.md).
- `design/` style-guide authoring and `design/index.json`: [design-content.md](.agents/rules/design-content.md).
- `src/` and `tests/` server invariants and coverage floor: [mcp-server.md](.agents/rules/mcp-server.md).

## Contribution workflow

- Branches: `feature/<short-phrase>`, `fix/<issue-id>`, `guide/<topic>`, `adr/<NNNN-title>`.
- ADRs: open a PR adding `guide/adrs/NNNN-title.md`, reserving the next number; review checks
  that the consequences are clear.
- Commits: Conventional Commits (`feat:`, `fix:`, `docs:`, `refactor:`, `chore:`, …).
- Merge needs a passing CI build and review from at least one maintainer.
- Create every pull request with the `pr-jsdotnet` skill
  ([.github/skills/pr-jsdotnet/SKILL.md](.github/skills/pr-jsdotnet/SKILL.md)), never the
  built-in PR tool, so it is authored with JSdotNet credentials via `gh pr create`.

## Dependencies

- Manage every package version centrally (ADR 0002).
- Pin versions of critical libraries (logging, DI, resilience) and update them in a dedicated
  `chore:` PR.
- Prefer the BCL over a third-party package where they are equivalent.

Revisit this file after a major ADR shift or a significant change to the MCP toolset.

<!-- devbook:begin -->
## Devbook folders

Written by `devbook:init` and kept by `devbook:update`. Edit outside these markers; an edit inside them makes the
next reconcile report the section as customized and leave it alone.

This repository keeps its devbook as addressed Markdown chapters. Treat the folders as
task-scoped context, never baseline context: load the chapters a task names, walk
`related` and `depends-on` from them, and never load a folder whole.

| Folder | Holds | Rules |
| --- | --- | --- |
| `.devbook/arc42/` | Structure, decisions, and technical debt | `devbook-arc42.md` |
| `.devbook/domain/` | Bounded contexts and the ubiquitous language | `devbook-domain.md` |
| `.devbook/tech/` | The technology graph and its ratings | `devbook-tech.md` |
| `.devbook/design/` | Design principles, tokens, and component guidelines | `devbook-design.md` |
| `.devbook/ai/` | How the team works with AI, stage by stage; it records a way of working and never instructs one | `devbook-ai.md` |

Every chapter carries a fenced `meta` block; write it in the same change as the content,
per `devbook-chapter-metadata.md`. Skip `annotation` fences when loading a
chapter as context: they hold review notes, not content.

Run the check before committing; it writes nothing:

    node .devbook/_tools/devbook-meta/build.mjs --check

An annotation fence is written only through `.devbook/_tools/devbook-meta/annotations.mjs`.

Nothing personal lives in this repository. Your own settings live under your devbook
config directory — `$XDG_CONFIG_HOME/devbook` when set, else `%APPDATA%\devbook` on
Windows and `~/.config/devbook` elsewhere — for every repository, or under `repos/<id>/`
there for this one, `<id>` being the `id` in `.devbook/config.json`. `AGENTS.local.md`
in either place holds instructions for your machine only: read it when it exists and
treat it as this file's last word. What else lives there, each plugin says for itself.
Put no secret in it — your home directory is not private.
<!-- devbook:end -->
