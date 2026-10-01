---
title: "Instruction-File Setup: One AGENTS.md, Wrapped per Host"
date: 2026-09-28
status: Accepted
tags: [instructions, agents-md, claude-code, copilot, path-scoped-rules, mcp, routing, recommendations]
---
# Recommendation: Instruction-File Setup — One AGENTS.md, Wrapped per Host

## Purpose

Define how a repository gives Claude Code and GitHub Copilot the same standing instructions from a
single authored copy: one root `AGENTS.md`, a thin wrapper per host, and path-scoped rules for
anything that applies to one kind of file only.

This replaces the earlier advice to keep a Copilot-only `.github/copilot-instructions.md`. A
host-specific root file drifts from its sibling the moment a second host is used; one root file with a
pointer per host cannot drift.

## Recommendation

- Author the repository's standing rules **once**, in a root `AGENTS.md`.
- Make `CLAUDE.md` an `@AGENTS.md` import and `.github/copilot-instructions.md` a one-sentence pointer
  at `AGENTS.md`. Neither restates a rule.
- Author a rule that applies to one kind of file once under `.agents/rules/<topic>.md`, with one
  wrapper per host under `.claude/rules/` and `.github/instructions/`.
- Keep instruction files to **selection and routing**: which source is authoritative, which tool or
  skill handles which task, and what happens when one is unavailable. Architecture, coding, testing and
  operational guidance stays in the MCP-served documents.
- Commit all of it. Nothing personal or machine-local belongs in an instruction file.
- Record the project's own architecture, domain, technology, design and AI adoption in `.devbook/`, not
  in `AGENTS.md` — see *Recommendation: Devbook Adoption*.

## The Layout

```
AGENTS.md                                     the standing rules. One copy.
CLAUDE.md                                     @AGENTS.md + one paragraph
.github/copilot-instructions.md               one sentence: read AGENTS.md
.agents/rules/<topic>.md                      a path-scoped rule. One copy. Host-neutral.
  ├── .claude/rules/<topic>.md                wrapper: paths:   → pointer
  └── .github/instructions/<topic>.instructions.md
                                              wrapper: applyTo: → pointer
.devbook/                                     the project's own record (optional; see Devbook Adoption)
```

| File | Holds | Never holds |
|---|---|---|
| `AGENTS.md` | Guidance authority and fallback, repository structure, contribution workflow, dependency rules, one pointer per path-scoped rule | A rule for one kind of file; a restated ADR or recommendation |
| `CLAUDE.md` | `@AGENTS.md` and a paragraph saying so | Any rule |
| `.github/copilot-instructions.md` | One sentence pointing at `AGENTS.md` | Any rule |
| `.agents/rules/<topic>.md` | One topic's rules, with `name`, `description` and `paths` front matter | Rules for the whole repository |

## AGENTS.md: the One Root File

`AGENTS.md` is the cross-host standard for a repository's root instructions, and both hosts can be
pointed at it. Write it imperative and concise; delete any sentence that does not change what an agent
does.

It carries only what applies to the whole repository:

- Which MCP server is authoritative for which topic, the order to consult sources in, and the fallback
  when a server is unreachable (see *Tool and MCP Selection Policy*).
- The repository structure, as a table of paths and what each holds.
- The contribution workflow: branch names, commit convention, merge requirements, and the pull-request
  skill when one is required.
- Dependency rules.
- One line per path-scoped rule, naming the rule and the paths it covers.

A plugin that writes its own section into `AGENTS.md` (devbook does) keeps it between its own markers;
edit outside the markers only.

## Host Wrappers

Each host gets a wrapper whose only job is to load `AGENTS.md`.

`CLAUDE.md` — Claude Code expands the import at launch:

```markdown
@AGENTS.md

`AGENTS.md` holds this repository's standing rules; the import above expands it at launch, so
everything it says applies here. Claude-specific notes, if any are ever needed, go below.
```

`.github/copilot-instructions.md` — GitHub Copilot reads this file on every request:

```markdown
Read [AGENTS.md](../AGENTS.md) and follow it. It holds this repository's standing rules, and
everything it says applies here. Copilot-specific notes, if any are ever needed, go below.
```

Add a host-specific note below the wrapper text only when it is true of that host alone. A rule that
applies to both hosts belongs in `AGENTS.md`.

## Path-Scoped Rules

A rule that applies to one kind of file — the documents under `docs/`, the server code under `src/` —
is authored once and wrapped per host, so it loads only when an agent works on a matching file and
never crowds the root file.

- The rule: `.agents/rules/<topic>.md`, with `name`, `description` and a `paths` glob list in its front
  matter, and a body of at most 60 lines.
- The Claude Code wrapper: `.claude/rules/<topic>.md`, with `paths` copied verbatim.
- The Copilot wrapper: `.github/instructions/<topic>.instructions.md`, with `applyTo` set to the
  `paths` list joined with commas and `description` copied from the rule.
- Each wrapper's body is one sentence: "Read `.agents/rules/<topic>.md` and follow it before editing
  this file." It never restates the rule.
- Check that `applyTo` equals `paths.join(",")` for every pair before committing.
- Keep a `README.md` in `.agents/rules/` with a table of topics, their paths, and what each covers.

A rule fires when a host **reads** a matching file, so creating a new file from scratch may not trigger
it; open a sibling first or read the rule directly. `.agents/rules/` is a convention rather than a
ratified standard — `AGENTS.md` defines no globs, and glob-scoped rules are an open proposal
([agents.md#179](https://github.com/agentsmd/agents.md/issues/179)) whose `name` / `description` /
`paths` shape this convention follows.

## Tool and MCP Selection Policy

Use a stable selection order:

1. Repository MCP guidance for architecture, design, coding, testing, structure and governance.
2. Installed skills and agents whose declared specialty matches the task.
3. External or platform MCP servers for vendor, framework or product documentation.
4. Direct repository inspection and built-in tools for workspace state, code search, diffs and local
   validation.

Name the expected MCP servers in `AGENTS.md` by their authoritative server IDs, and take those IDs and
commands from *Config Guideline: .mcp.json* rather than restating the table.

**⚠️ Do not use the deprecated command `jsdotnet-project-guidelines-mcpserver`** (deprecated package
`jsdotnet.project.guidelines.mcpserver`, last v1.0.6). If it appears in a generated instruction file or
config, replace it with `jsdotnet-guidelines-mcpserver`.

Rules:

- Query the authoritative MCP server before answering a repository-policy question from memory.
- Cite the document ID, ADR number or relative path behind every architectural answer.
- Link or route to MCP guidance; never copy it into an instruction file.

### MCP Fallback

If the repository MCP server is unavailable:

1. Read the checked-in document index and the markdown files it references, when they are present.
2. If those are unavailable too, state that the guidance could not be verified.
3. Never invent repository policy from memory when the authoritative source cannot be reached.

## Agent and Flow Routing

- Name a skill or agent in `AGENTS.md` only when the repository requires it for a task category — for
  example, the skill every pull request must be created with.
- Route only to what the repository actually installs. A plugin that routes tasks itself — the devbook
  delivery engine's `flow-*` skills arrive with their own routing context — needs no copy of that
  routing in `AGENTS.md`.
- Say **when** to select a skill or agent, never **how** it performs its work; that stays in the skill.
- When a preferred skill or agent is unavailable, keep the same governance checkpoints with the closest
  lower-level option and state the reduced assurance explicitly.

## Migrating from a Copilot-Only File

1. Move the repository-wide content of `.github/copilot-instructions.md` into a new root `AGENTS.md`,
   rewritten imperative and concise. Drop version history and speculative sections.
2. Move each rule that applies to one kind of file into `.agents/rules/<topic>.md` with its two
   wrappers, leaving a one-line pointer in `AGENTS.md`.
3. Reduce `.github/copilot-instructions.md` to the pointer above and add `CLAUDE.md` beside it.
4. Check that nothing from the old file was lost except what was dropped on purpose, and list the drops
   in the commit message.

## Anti-Patterns to Avoid

- A rule stated in two places — in `AGENTS.md` and a path-scoped rule, or in a wrapper and its rule.
- A host-specific root file carrying rules the other host never sees.
- An instruction file that duplicates ADRs, recommendations, design documents or skill playbooks.
- Listing tools or agents without saying when each is selected.
- A fallback that silently changes policy or hides the loss of an authoritative source.
- A personal preference, local path or machine-specific setting in a committed instruction file.

## Worked Example

[JSdotNet/Project-Guidelines-MCP](https://github.com/JSdotNet/Project-Guidelines-MCP) uses exactly this
layout: its `AGENTS.md` is the only root instruction file, `CLAUDE.md` and
`.github/copilot-instructions.md` wrap it, and `.agents/rules/` holds the `guide`, `design-content` and
`mcp-server` rules beside the `devbook-*` rules that `devbook:init` installed.

## References

- Recommendation: Devbook Adoption
- Recommendation: Agent Skill Authoring
- Config Guideline: .mcp.json
- Config Guideline: github-app.yml
- AGENTS.md: https://agents.md
