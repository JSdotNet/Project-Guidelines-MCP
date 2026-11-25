# Migration plan templates

## Plan

```markdown
# Migration: <area> to the defaults

## End state
| Area | As-is | To-be | Doc id |
|---|---|---|---|

## Steps
| # | Step (one pull request) | Depends on | Proof | Doc id |
|---|---|---|---|---|
| 1 | Add Central Package Management and lock files | — | Locked restore passes in CI | 0001-platform-and-build |
| 2 | Move the Orders feature into its module's feature slice | 1 | Orders tests pass unchanged | 0003-solution-structure |

## Risks
| Risk | Mitigation |
|---|---|
```

## Progress update

```markdown
## Migration update: <date>
- Done: steps 1–2 (PRs …)
- Next: step 3
- Blocked: … (what unblocks it)
- Gaps still open: …
```

## Rules of thumb

- One module or feature slice at a time; the old and new shape may coexist between steps,
  never inside one slice.
- Tests move with the code they cover and pass before the old code is deleted.
- A performance-sensitive path gets a measurement before and after its step.
