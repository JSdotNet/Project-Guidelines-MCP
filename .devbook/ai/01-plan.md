# Plan

```meta
status: trial
type: stage
```

## Backlog plan items

```meta
status: trial
type: practice
stage: [plan]
date: 2026-09-27
```

A migration or other multi-step goal is written as a Backlog plan: ordered items, each a
self-contained prompt with its prerequisites, pasted into a fresh agent session and run by
the `backlog-run-plan-item` skill.

- **Used for** — sequencing work that spans several pull requests, such as the
  `devbook-migration` plan.
- **Adopted by** — the maintainer, for the `devbook-migration` plan only.
- **Evidence** — `repo-rules` landed as #60 and `devbook-setup` followed it from a pasted item.
- **Limits** — single-PR changes are asked for directly, without a plan.
