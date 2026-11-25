# Gap analysis report

## Gap table

| # | Area | The repository | The default | Doc id | Priority |
|---|---|---|---|---|---|
| 1 | Solution shape | Layered `Domain`/`Application`/`Infrastructure` projects | Modular monolith with feature slices; a small app has one module | `0003-solution-structure` | High |
| 2 | Build policy | No lock files | Central Package Management with lock files and a locked restore in CI | `0001-platform-and-build` | High |

Areas to cover: solution shape, module layout, API, persistence, configuration and secrets,
observability, security, frontend, tests, build policy, agent setup.

## Uncovered

List each concern no document governs, with the ADR title proposed for it.

## Plan

Order the gaps so a fix never builds on a gap still open: build and structure first, then
cross-cutting concerns, then style. Each item names its gap numbers and the doc ids the fix
follows. Re-run the analysis after the plan lands to confirm each gap closed.
