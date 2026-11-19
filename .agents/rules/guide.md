---
name: guide
description: Taxonomy, front matter and index rules for the guidance documents under guide/.
paths:
  - "guide/**"
---

# Guide documents

Every doc opens with front matter carrying `title`, `date`, `status` and `tags`. Update `date`
on a meaningful content change, not on a typo, grammar, or formatting-only edit.

## Taxonomy

- **ADRs (`guide/adrs`)** — `NNNN-title.md`, numbered sequentially; title
  `ADR NNNN: Concise Title`; `status` is Proposed, Accepted, Deprecated, or Superseded by NNNN;
  sections Context, Decision, Consequences (Positive/Negative), References. Rewrite an ADR in
  place when its decision changes; git keeps the old text. When two ADRs overlap, merge them
  into one; the absorbed number is retired, never reused, and
  [adrs/README.md](../../guide/adrs/README.md) maps it to the ADR that absorbed it. Write the
  decision in present tense and justify it rather than restating the context.
- **Designs (`guide/designs`)** — exploratory or future-facing and free to evolve. Use Mermaid
  diagrams and the sections Problem, Forces, Proposed Solution, Variants, Risks.
- **Recommendations (`guide/recommendations`)** — prescriptive, stable, narrowly scoped
  guidance (e.g. "Error handling approach"); link the originating ADR where one exists.
- **Structures (`guide/structures`)** — canonical directory/file scaffolds (API service,
  background worker, library pack) with minimal code shells and comments marking where domain
  logic goes.
- **Config (`guide/config`)** — one standard configuration file per doc, named
  `<file>.md` after the file it governs.

## `guide/index.json`

The MCP server's document registry; without a current one the server falls back to directory
traversal. Whenever you add, modify, or remove a file in `guide/`, update it in the same
change:

1. Walk every markdown file in the `guide/` subdirectories.
2. Read `title` and `tags` from its front matter.
3. Emit an entry with `id` (the file name without `.md`), `title`, a one-sentence
   `description` of what the doc decides or prescribes, `category` (the subdirectory),
   `relativePath` (relative to `guide/`), and `tags`.
4. Update the `generated` timestamp and save.

```json
{
  "version": "1.0",
  "generated": "2025-11-19T10:30:00Z",
  "documents": [
    {
      "id": "0001-platform-and-build",
      "title": "ADR 0001: Platform and Build",
      "description": "Follows the newest GA .NET release, pinned in global.json, with Central Package Management.",
      "category": "adrs",
      "relativePath": "adrs/0001-platform-and-build.md",
      "tags": ["dotnet", "adr"]
    }
  ]
}
```
