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
| `AGENTS.md` | This file; `CLAUDE.md` and `.github/copilot-instructions.md` wrap it per host |

## `guide/index.json`

`guide/index.json` is the MCP server's document registry; without a current one the server
falls back to expensive directory traversal. Whenever you add, modify, or remove a file in
`guide/`, regenerate it:

1. Walk every markdown file in the `guide/` subdirectories.
2. Read `title` and `tags` from its front matter.
3. Emit an entry with `id`, `title`, `category`, `relativePath`, `tags`.
4. Update the `generated` timestamp and save.

```json
{
  "version": "1.0",
  "generated": "2025-11-19T10:30:00Z",
  "documents": [
    {
      "id": "document-id",
      "title": "Document Title",
      "category": "adrs",
      "relativePath": "adrs/document.md",
      "tags": ["tag1", "tag2"]
    }
  ]
}
```

## Doc taxonomy

Every doc opens with front matter carrying `title`, `date`, `status` and `tags`. Update `date`
on a meaningful content change, not on a typo, grammar, or formatting-only edit.

- **ADRs (`guide/adrs`)** — title `ADR NNNN: Concise Title`; `status` is Proposed, Accepted,
  Deprecated, or Superseded by NNNN; sections Context, Decision, Consequences
  (Positive/Negative), References. Once accepted, change only the status; record a change as a
  superseding ADR. Write the decision in present tense and justify it rather than restating
  the context.
- **Designs (`guide/designs`)** — exploratory or future-facing and free to evolve. Use Mermaid
  diagrams and the sections Problem, Forces, Proposed Solution, Variants, Risks.
- **Recommendations (`guide/recommendations`)** — prescriptive, stable, narrowly scoped
  guidance (e.g. "Error handling approach"); link the originating ADR where one exists.
- **Structures (`guide/structures`)** — canonical directory/file scaffolds (API service,
  background worker, library pack) with minimal code shells and comments marking where domain
  logic goes.
- **Config (`guide/config`)** — one standard configuration file per doc, named
  `<file>.md` after the file it governs.

## MCP server invariants (`src/`)

- Serve guidance only; keep business logic out of the server.
- Include source doc ids in every response for traceability.
- `FileSystemDocumentCatalog` serves the local `guide/` when running locally;
  `GitHubDocumentCatalog` fetches from GitHub when installed as a global tool.
- Cache results; make no redundant GitHub API calls.

## Contribution workflow

- Branches: `feature/<short-phrase>`, `fix/<issue-id>`, `guide/<topic>`, `adr/<NNNN-title>`.
- ADRs: open a PR adding `guide/adrs/NNNN-title.md`, reserving the next number; review checks
  that the consequences are clear.
- Commits: Conventional Commits (`feat:`, `fix:`, `docs:`, `refactor:`, `chore:`, …).
- Merge needs a passing CI build and review from at least one maintainer.
- Keep line coverage at 80% or more for `JSdotNet.MCP.Shared` and `JSdotNet.MCP.Guidelines`;
  CI enforces it through coverlet.
- Create every pull request with the `pr-jsdotnet` skill
  ([.github/skills/pr-jsdotnet/SKILL.md](.github/skills/pr-jsdotnet/SKILL.md)), never the
  built-in PR tool, so it is authored with JSdotNet credentials via `gh pr create`.

## Dependencies

- Manage every package version centrally (ADR 0002).
- Pin versions of critical libraries (logging, DI, resilience) and update them in a dedicated
  `chore:` PR.
- Prefer the BCL over a third-party package where they are equivalent.

Revisit this file after a major ADR shift or a significant change to the MCP toolset.
