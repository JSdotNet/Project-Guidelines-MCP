# Design

```meta
status: draft
related: [.devbook/arc42/01-introduction-and-goals.md]
```

This repository ships no user interface of its own: its users are coding agents, reached
through MCP tools. The UX style guide under the root `design/` folder is product content —
what `jsdotnet-design-ux-guidelines` serves to other repositories — and is governed by
`.agents/rules/design-content.md`, not by this folder.

This folder records how the product presents itself to its callers.

## Principles

```meta
status: draft
```

- **Answer with the source.** Every tool response names the doc ids it came from, so a
  caller can cite and re-fetch them.
- **One vocabulary across servers.** A tool that does the same thing on two servers has the
  same name and the same parameters on both.
- **Fail visibly.** A server that cannot reach its content says so rather than returning an
  empty result that reads as "no guidance".
