# Code

```meta
status: adopted
type: stage
```

## Agent-authored pull requests

```meta
status: adopted
type: practice
stage: [code]
date: 2026-09-27
```

Changes to `guide/`, `design/`, `src/` and `tests/` are authored by a coding agent — GitHub
Copilot or Claude Code — in a session that ends in a pull request a maintainer reviews.

- **Used for** — guidance documents, MCP server fixes and features, dependency bumps.
- **Adopted by** — the maintainer, for nearly every change.
- **Evidence** — every merged pull request from #36 to #60 carries a Copilot or Claude co-author
  trailer.
- **Limits** — merge still needs a passing CI build and a maintainer's review.

## Path-scoped rules

```meta
status: adopted
type: guardrail
stage: [code]
date: 2026-09-27
```

Rules for one kind of file live once under `.agents/rules/`, wrapped per host, so either agent
gets the rule when it reads a matching file.

- **Used for** — the `guide/` taxonomy, the `design/` style guide, the server invariants, and
  the devbook folders.
- **Adopted by** — every agent session in this repository, Copilot and Claude alike.
- **Evidence** — #58 moved the standing rules to `AGENTS.md`; #60 split the path-scoped ones out.
- **Limits** — a rule fires on a read, so a file authored from scratch may not trigger it.

## Pull requests through pr-jsdotnet

```meta
status: adopted
type: guardrail
stage: [code]
date: 2026-09-27
```

Every pull request is opened with the `pr-jsdotnet` skill through `gh pr create`, never an
agent's built-in pull request tool, so it is authored with JSdotNet credentials.

- **Used for** — opening every pull request an agent session ends in.
- **Adopted by** — every agent session, per `AGENTS.md`.
- **Evidence** — introduced in #51.
- **Limits** — none.
