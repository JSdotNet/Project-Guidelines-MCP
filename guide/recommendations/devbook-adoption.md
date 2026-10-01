---
title: "Devbook Adoption: Start with the devbook Plugin Alone"
date: 2026-09-28
status: Accepted
tags: [devbook, documentation, arc42, domain, agents-md, claude-code, copilot, ci, recommendations]
---
# Recommendation: Devbook Adoption — Start with the devbook Plugin Alone

## Purpose

Define how a project records its own architecture, domain, technology, design and AI adoption in
`.devbook/`, and the smallest setup that keeps that record valid: the `devbook` plugin, its `init` and
`update` skills, and its check in CI.

The guidance this server publishes (`guide/`, `design/`) is what projects should do. `.devbook/` is what
one project actually is. Keep the two apart: a consuming project reads the published guidance through
MCP and writes its own record in `.devbook/`.

## Recommendation

- Adopt devbook with the **`devbook` plugin alone**: `devbook:init` to adopt, `devbook:update` to move
  forward, `devbook:validate` in CI.
- Install the delivery engine and the layered plugins later, as a deliberate step up, when the project
  needs what they add — not as the starting point.
- Commit everything devbook writes into the repository. Keep anything personal in your own devbook
  config directory, never in the clone.
- Keep the instruction files in the shape of *Recommendation: Instruction-File Setup*; devbook writes
  its section into the root `AGENTS.md` and its rules beside the project's own under `.agents/rules/`.

## What .devbook/ Holds

| Folder | Holds |
|---|---|
| `.devbook/arc42/` | Structure, decisions and technical debt |
| `.devbook/domain/` | Bounded contexts and the ubiquitous language |
| `.devbook/tech/` | The technology graph and its ratings |
| `.devbook/design/` | Design principles, tokens and component guidelines |
| `.devbook/ai/` | How the team works with AI, stage by stage — a record of practice, never an instruction |

Each folder is addressed Markdown chapters. Every chapter carries a fenced `meta` block, written in the
same change as its content, and each folder has its own rule under `.agents/rules/devbook-<folder>.md`.
Agents load the chapters a task names and follow their references — never a folder whole.

Adopt the folders the project will actually write. A folder can be added or dropped later with
`devbook:update`.

## The Minimal Shape

1. **Install the marketplace and enable `devbook`.**

   ```bash
   claude plugin marketplace add JSdotNet/devbook
   ```

   Then enable the `devbook` plugin from `/plugin`. The same plugin loads in GitHub Copilot.

2. **Adopt with `devbook:init`.** It asks which folders to adopt, shows its plan before writing, and
   then installs:
   - the checker under `.devbook/_tools/devbook-meta/` and the `tech/` inventory scripts under
     `.devbook/_tools/devbook-tech/`;
   - `.github/workflows/devbook-meta.yml`, filtered to the adopted folders;
   - the devbook folder rules as `.agents/rules/devbook-*.md`, each with a wrapper per host;
   - devbook's section of `AGENTS.md`, between its markers;
   - one starting chapter per adopted folder, with a valid `meta` block;
   - the stamp under `components.devbook` in `.devbook/config.json`.

   Existing `CLAUDE.md` and `.github/copilot-instructions.md` wrappers are left alone. Add an `"id"` as
   the first key of `.devbook/config.json` — a stable, lower-case name for the repository that personal
   settings are filed under — and never rename it.

3. **Validate in CI.** The installed workflow runs the check on every pull request that touches
   `.devbook/`:

   ```bash
   node .devbook/_tools/devbook-meta/build.mjs --check
   ```

   It writes nothing. When it fails, run `devbook:validate` to repair the broken references and `meta`
   blocks it reports. Run the check locally before committing, and add it to the coding agent's
   `pre_flight` (see *Config Guideline: github-app.yml*) so an agent cannot open a pull request with a
   broken chapter.

4. **Move forward with `devbook:update`.** After upgrading the plugin or changing which folders are
   adopted, `devbook:update` refreshes the checker, the rules and the `AGENTS.md` section, runs
   outstanding migrations and re-stamps. Never edit a `devbook-*` rule or the text between devbook's
   markers by hand: the next update reports it customized and stops refreshing it.

## Writing Chapters Without a Flow Engine

With only `devbook` installed, each folder's rule is the procedure: an agent editing a chapter follows
`.agents/rules/devbook-<folder>.md` and `devbook-chapter-metadata.md` directly.

The converters help between chapters and code without writing either:

- `devbook:capture-specs` reads the implementation and its tests and delivers a capture plan; you write
  the chapter from it.
- `devbook:verify-change` reports whether a chapter and its code still agree.
- `devbook:apply-change` derives a change brief from an agreed chapter.
- `devbook:tech-update` inventories packages for `tech/`.

## The Optional Step Up

The same marketplace ships plugins layered over `devbook`. Each is worth adopting when its problem is
real in the project:

| Plugin | Adds | Adopt when |
|---|---|---|
| `delivery` | The delivery engine: `flow-*` procedures that carry a change through gated stages to a validated pull request | Changes should run through a fixed, gated procedure rather than the folder rules alone |
| `devbook-config` | Owns `.devbook/config.json`, moves the whole stack forward, diagnoses it | More than one component of the stack is installed |
| `devbook-derived` | Committed `_meta/` indexes and a reference-graph canvas | A reader that cannot run the checker needs the graph |
| `devbook-procedures` | Repository-owned start, show, capture, debug and estimate skills | The engine needs to start and show the application |
| `devbook-collaboration` | Review, approval and hand-off over chapters | Chapters are reviewed and approved by named people |
| `delivery-surface-*`, `delivery-schedule` | Run dashboards and unattended scheduled runs | Flows run often enough to watch, or should run on a cadence |

Starting with the whole stack front-loads configuration a project has not yet needed; starting with
`devbook` alone proves the record first.

## Team-Shared and Personal

- Commit `.devbook/`, `.agents/rules/`, `.claude/rules/`, `.github/instructions/` and the workflow. A
  rule or chapter reaches a teammate's session only if it is in the clone.
- Keep personal settings in your devbook config directory — `$XDG_CONFIG_HOME/devbook` when set, else
  `%APPDATA%\devbook` on Windows and `~/.config/devbook` elsewhere — or under `repos/<id>/` there for one
  repository. An `AGENTS.local.md` there holds instructions for your machine only.
- Never add an `AGENTS.local.md` or a `.gitignore` entry for personal devbook files to the repository.

## Anti-Patterns to Avoid

- Installing the delivery engine and every layered plugin before the project has written a chapter.
- Writing a chapter without its `meta` block, or leaving the check out of CI.
- Loading a whole devbook folder as baseline context instead of the chapters a task needs.
- Treating `ai/` as instructions for agents; it records how the team works, it does not direct it.
- Restating a published ADR or recommendation in `.devbook/`; cite it, and record only this project's
  own decisions there.
- Editing devbook-managed files instead of running `devbook:update`.

## Worked Example

[JSdotNet/Project-Guidelines-MCP](https://github.com/JSdotNet/Project-Guidelines-MCP) runs this minimal
shape: all five folders adopted with the `devbook` plugin alone, `devbook-meta.yml` in CI, the
`devbook-*` rules beside its own `guide`, `design-content` and `mcp-server` rules, and no delivery
engine.

## References

- Recommendation: Instruction-File Setup
- Config Guideline: github-app.yml
- devbook marketplace: https://github.com/JSdotNet/devbook
