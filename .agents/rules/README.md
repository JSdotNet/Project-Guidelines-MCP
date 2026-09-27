# Repository rules

A rule that applies to one kind of file is authored **once** here and wrapped per host. A new
host adds another wrapper; it never adds a second copy.

```
.agents/rules/<topic>.md                      the rule. One copy. Host-neutral.
  ├── .claude/rules/<topic>.md                wrapper: paths:   → pointer
  └── .github/instructions/<topic>.instructions.md
                                              wrapper: applyTo: → pointer
```

The rule declares `name`, `description`, and a `paths` list, and keeps its body within 60
lines. A wrapper carries frontmatter and one sentence of body — "Read
`.agents/rules/<topic>.md` and follow it before editing this file." — and never restates the
rule. The Claude wrapper copies `paths` verbatim; the Copilot wrapper's `applyTo` is that list
joined with commas, and its `description` is copied from the rule. Check that `applyTo` equals
`paths.join(",")` for every pair before committing.

| Topic | Paths | Covers |
|---|---|---|
| [guide](guide.md) | `guide/**` | Doc taxonomy, front matter, `guide/index.json` |
| [design-content](design-content.md) | `design/**` | Style-guide authoring, customization boundary, `design/index.json` |
| [mcp-server](mcp-server.md) | `src/**`, `tests/**` | Server invariants and the coverage floor |

The `devbook-*.md` rules and their wrappers are copied in verbatim by `devbook:init` and
refreshed by `devbook:update`, which list them in `.devbook/config.json`. They are exempt from
the 60-line budget; never edit one here, or the next update reports it customized and stops
refreshing it.

Rules that apply to the whole repository stay in [`AGENTS.md`](../../AGENTS.md), which points
here for each topic.

`.agents/rules/` is not a ratified standard. `AGENTS.md` is the standard for the *root* file
and defines no globs; [agents.md#179](https://github.com/agentsmd/agents.md/issues/179) is the
open proposal for glob-scoped rules, and its `name` / `description` / `paths` shape is what
this convention uses.

A rule fires when a host **reads** a matching file, so authoring a new file from scratch may
not trigger it. Open a sibling first, or read the rule directly.

All three directories are committed, so every clone and every worktree under
`.claude/worktrees/` picks them up. A rule is therefore never the place for anything personal
or machine-local.
