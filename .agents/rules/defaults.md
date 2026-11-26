---
name: defaults
description: Taxonomy, front matter and index rules for the guidance documents under defaults/.
paths:
  - "defaults/arc42/**"
  - "defaults/ai/**"
  - "defaults/tech/**"
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
| `defaults/arc42/02-constraints.md`, `04-solution-strategy.md`, `07-deployment-view.md`, `08-crosscutting-concepts.md` | arc42 chapters on givens, approach, deployment and crosscutting concepts | `designs` |
| `defaults/arc42/05-building-block-view.md`, `building-blocks/` | The one solution structure and one page per block | `structures` |
| `defaults/ai/` | Instruction-file setup and skill authoring | `ai` |
| `defaults/ai/config/` | One standard configuration file per doc, named `<file>.md` | `config` |
| `defaults/tech/` | Technology ratings: `technology-graph.md` plus one file per layer | `tech` |
| `defaults/design/` | The UX style guide | `design` |

- **ADRs** — `NNNN-title.md`, numbered sequentially; title `ADR NNNN: Concise Title`;
  `status` is Proposed, Accepted, Deprecated, or Superseded by NNNN; sections Context,
  Decision, Consequences (Positive/Negative), References. Rewrite an ADR in place when its
  decision changes; git keeps the old text. When two ADRs overlap, merge them into one; the
  absorbed number is retired, never reused, and
  [adr/README.md](../../defaults/arc42/adr/README.md) maps it to the ADR that absorbed it.
  Write the decision in present tense and justify it rather than restating the context.
- **arc42 chapters** — named `NN-chapter-name.md` after the arc42 chapter they fill; one
  answer per concern, no variants. `workspace.dsl` beside them is the C4 seed and is not
  indexed.
- **Building blocks** — one block per file: its responsibility, layout, a minimal code shell
  using `Company.Product` placeholders or the `Contoso.Orders` example, and its rules.
- **Technology ratings** — devbook's `tech/` shape without `meta` blocks: one
  `## <Technology>` chapter per technology with its rating (`adopted` or `hold`), version,
  depends-on links and the ADR behind it. An ADR changes first; `tech/` records the outcome,
  and `technology-graph.md`'s graph changes in the same edit.
- **Recommendations** — prescriptive, stable, narrowly scoped guidance; link the originating
  ADR where one exists.

## `defaults/index.json`

The one document registry the `jsdotnet-app-defaults` MCP server reads, every category in one
catalog, `design` among them. Without it the server falls back to directory traversal.
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
