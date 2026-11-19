---
name: defaults
description: Taxonomy, front matter and index rules for the guidance documents under defaults/.
paths:
  - "defaults/arc42/**"
  - "defaults/ai/**"
  - "defaults/index.json"
---

# Defaults documents

Every doc opens with front matter carrying `title`, `date`, `status` and `tags`. Update `date`
on a meaningful content change, not on a typo, grammar, or formatting-only edit.
`defaults/design/` follows [design-content.md](design-content.md) instead.

## Taxonomy

| Folder | Holds | `category` |
|---|---|---|
| `defaults/arc42/adr/` | ADRs | `adrs` |
| `defaults/arc42/building-blocks/` | Structures and designs, until they merge into one view | `structures`, `designs` |
| `defaults/arc42/concepts/` | Recommendations, until they fold into chapter 08 | `recommendations` |
| `defaults/ai/` | Instruction-file setup and skill authoring | `recommendations` |
| `defaults/ai/config/` | One standard configuration file per doc, named `<file>.md` | `config` |
| `defaults/design/` | The UX style guide | `design` |

- **ADRs** — `NNNN-title.md`, numbered sequentially; title `ADR NNNN: Concise Title`;
  `status` is Proposed, Accepted, Deprecated, or Superseded by NNNN; sections Context,
  Decision, Consequences (Positive/Negative), References. Rewrite an ADR in place when its
  decision changes; git keeps the old text. When two ADRs overlap, merge them into one; the
  absorbed number is retired, never reused, and
  [adr/README.md](../../defaults/arc42/adr/README.md) maps it to the ADR that absorbed it.
  Write the decision in present tense and justify it rather than restating the context.
- **Designs** — exploratory and free to evolve; Mermaid diagrams and the sections Problem,
  Forces, Proposed Solution, Variants, Risks.
- **Structures** — canonical directory/file scaffolds with minimal code shells and comments
  marking where domain logic goes.
- **Recommendations** — prescriptive, stable, narrowly scoped guidance; link the originating
  ADR where one exists.

## `defaults/index.json`

The one document registry both MCP servers read: the design server serves category `design`,
the guidelines server everything else. Without it they fall back to directory traversal.
Whenever you add, modify, or remove a file under `defaults/`, update it in the same change:
one entry per markdown file with `id` (the file name without `.md`, unique across the index),
`title` and `tags` from the front matter, a one-sentence `description` of what the doc decides
or prescribes, `category` from the table above, and `relativePath` relative to `defaults/`.
Update the `generated` timestamp.

```json
{
  "id": "0001-platform-and-build",
  "title": "ADR 0001: Platform and Build",
  "description": "Follows the newest GA .NET release, pinned in global.json, with Central Package Management.",
  "category": "adrs",
  "relativePath": "arc42/adr/0001-platform-and-build.md",
  "tags": ["dotnet", "adr"]
}
```
