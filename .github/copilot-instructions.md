# Copilot Repository Instructions: project-coding-guidelines

> Purpose: This repo defines authoritative design, architecture, style and structural guidance for modern .NET C# projects. Guidance lives in `docs/` and is served programmatically via the `jsdotnet-coding-guidelines` MCP server. This file governs how to maintain **this repository** — not how to write .NET code.

---

## Guidance Authority & Precedence

**Architectural and coding guidance lives in the docs, not in this file.**

When answering questions about architecture, patterns, C# style, testing, observability, or any technical decision:

1. **MCP server first** — call `jsdotnet-coding-guidelines` tools (`SearchDocuments`, `ListDocuments`, `GetDocument`) to retrieve authoritative content from `docs/`.
2. **Local docs second** — if MCP is unavailable, read `docs/index.json` and the referenced markdown files directly.
3. **Disclose uncertainty** — if neither source is reachable, say so explicitly. Do not invent or recall guidance from memory alone.

When giving architectural advice, always cite the source document (ADR number, doc ID, or relative path).

If a question touches an area with no existing document, suggest creating a new ADR or recommendation rather than answering from first principles.

---

## 1. Scope & Mission

This repo is the single source of truth for JSdotNet project guidelines. Content is organized under `docs/` and exposed via an MCP server so AI agents can consult it during code generation and review.

This instructions file covers only **repository-operational** concerns: structure, contribution workflow, doc conventions, and how to maintain the MCP server codebase.

---

## 2. Repository Structure (Authoritative)

```
/README.md
/docs/
  index.json                   - Document metadata index (MUST be updated when docs change)
  adrs/                        - Each ADR: `NNNN-title.md` (sequential 0001, 0002 ...)
  designs/                     - Deeper design explorations & diagrams
  recommendations/             - Prescriptive guidance & best practices
  structures/                  - Example folder/file scaffolds & templates
/src/                          - MCP Server production code
/tests/                        - MCP Server test projects
/JSdotNet.Project.Guidelines.slnx
/.github/copilot-instructions.md - This file
```

### Important: index.json Maintenance

The `docs/index.json` file is the authoritative document registry for the MCP Server.

**CRITICAL REQUIREMENT**: Whenever you add, modify, or remove documentation files in `docs/`, you MUST regenerate `docs/index.json` to reflect those changes.

To regenerate the index:
1. Run through all markdown files in `docs/` subdirectories
2. Extract metadata from front matter (title, tags)
3. Generate entries with: id, title, category, relativePath, tags
4. Update the `generated` timestamp
5. Save to `docs/index.json`

Example structure:
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

Without an up-to-date index.json, the MCP Server falls back to expensive directory traversal.

---

## 3. Document Taxonomy & Conventions

### ADRs (`docs/adrs`)
Format (MADR-ish simplified):
```
# ADR NNNN: Concise Title
Date: YYYY-MM-DD
Status: {Proposed|Accepted|Deprecated|Superseded by NNNN}
Context
Decision
Consequences (Positive/Negative)
References
```
Rules:
- Immutable once accepted except for status; use superseding ADRs for changes.
- Use present tense in the decision section.
- Provide justification, not restatement of context.

### Designs (`docs/designs`)
- Exploratory or future-facing. Can evolve freely.
- Use diagrams (Mermaid preferred) and clearly labeled sections: Problem, Forces, Proposed Solution, Variants, Risks.

### Recommendations (`docs/recommendations`)
- Prescriptive, stable guidance (e.g., "Error handling approach").
- Keep narrowly scoped; link back to the originating ADR where applicable.

### Structures (`docs/structures`)
- Canonical directory/file scaffolds (e.g., API service, background worker, library pack).
- Show minimal code shells with comments for where domain logic goes.

### Markdown Date Maintenance
- Update the `Date:` field on meaningful content changes.
- Do not update dates for typo fixes, grammar cleanup, or formatting-only edits.

---

## 4. MCP Server: Usage by Copilot

The `jsdotnet-coding-guidelines` MCP server (NuGet: `JSdotNet.Project.Guidelines.McpServer`) exposes:

| Tool | Purpose |
|------|---------|
| `ListDocuments` | List all available docs with metadata (title, category, tags) |
| `SearchDocuments` | Keyword search across all docs |
| `GetDocument` | Fetch full content of a specific doc by ID |

**Usage examples:**
- "What is the recommended project structure?" -> `SearchDocuments("project structure")`
- "List all ADRs" -> `ListDocuments` filtered by category `adrs`
- "Fetch ADR 0005" -> `GetDocument("0005-modular-monolith-structure")`

---

## 5. MCP Server: Maintainer Invariants

These rules govern the MCP server implementation itself (applies when modifying `src/`):

- Serve guidance only; do not embed business logic in the server.
- Always include source doc IDs in responses for traceability.
- `FileSystemDocumentCatalog` is used when running locally (local `docs/`); `GitHubDocumentCatalog` is used when installed as a global tool (fetches from GitHub).
- Cache results; avoid redundant GitHub API calls.
- Rate limits & caching strategy to be documented in a future ADR.

---

## 6. Contribution Workflow

- Branch naming: `feature/<short-phrase>`, `fix/<issue-id>`, `docs/<topic>`, `adr/<NNNN-title>`.
- ADR process: Open PR with ADR in `adrs/` as `NNNN-title.md` (reserve number sequentially). Review ensures clarity of consequences.
- Commit messages: Conventional Commits (`feat:`, `fix:`, `docs:`, `refactor:`, `chore:` etc.).
- Merge requires: passing CI build + review from at least 1 maintainer.
- **Code coverage**: Maintain at least 80% line coverage for `JSdotNet.Project.Guidelines.Docs` and `JSdotNet.Project.Guidelines.McpServer`; enforced in CI via coverlet.

---

## 7. Dependency Management

- Pin versions for critical libraries (logging, DI, resilience) — update via a dedicated `chore:` PR.
- Prefer BCL over third-party packages where equivalent.
- All package versions managed centrally (see ADR 0002).

---

## 8. Formatting & Analysis Tooling

Tooling decisions to be established via ADR. Candidates:
- `dotnet format` for style enforcement.
- Roslyn analyzers: `Microsoft.CodeAnalysis.NetAnalyzers`, `StyleCop.Analyzers`.
- Security: `DevSkim`, GitHub code scanning.

---

## 9. How Copilot Should Behave in This Repo

1. **Always query the MCP server first** for architectural or coding guidance before answering from memory.
2. **Cite sources** — reference ADR numbers or doc IDs when giving architectural advice (e.g., `// ADR-0007: circuit breaker at adapter layer`).
3. **Flag conflicts** — if a request conflicts with established guidance, highlight the conflict and offer the compliant alternative.
4. **Suggest new docs** — if guidance is missing, recommend creating an ADR or recommendation rather than improvising.
5. **Never expose secrets** — remind users to externalize configuration; never commit credentials.
6. **On MCP unavailable** — say "I cannot verify this against the guidelines (MCP server unreachable)" rather than guessing.

---

## 10. Maintenance Notes

Review this file after major ADR shifts or when the MCP server toolset changes significantly.

Version History:
- 2025-11-10: Initial creation.
- 2026-06-05: Refactored — inline guidance removed; MCP server now live as authoritative source.

---

End of Copilot repository instructions.
