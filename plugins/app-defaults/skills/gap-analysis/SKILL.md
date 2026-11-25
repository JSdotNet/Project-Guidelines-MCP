---
name: gap-analysis
description: "Audit a .NET repository's structure against the JSdotNet application defaults: solution shape, modules, projects, tests, build policy and Aspire setup, each gap cited by doc id, ending in a prioritised improvement plan. Use when: adopting the defaults in an existing repository, or checking how far a repository has drifted."
---

# Gap analysis

Compares a whole repository with the defaults. For one change, a diff or a design, use the
server's `review-against-defaults` prompt instead. Report format: [reference.md](reference.md).

## Workflow

1. **Read the target shape**: `get_guide("05-building-block-view")`, the building blocks it
   names (`module`, `api`, `data-provider`, `apphost`, `service-defaults`), and ADRs
   `0001-platform-and-build`, `0002-aspire-configuration-and-infrastructure`,
   `0003-solution-structure` and `0010-testing`.
2. **Inventory the repository**: the `.slnx`, every project and its folder, `global.json`,
   `Directory.Build.props`, `Directory.Packages.props` and lock files, the AppHost, the
   `tests/` tree, `AGENTS.md` and `.mcp.json`.
3. **Compare each area** with the document that governs it, and record each gap as: area,
   what the repository does, what the default requires, doc id.
4. **Look up what step 1 did not cover** with `search_guides_by_tag` (for example
   `persistence`, `security`, `opentelemetry`) and read each hit with `get_guide`.
5. **Name what no document covers** and propose an ADR for it with `propose-adr`.
6. **Prioritise**: build and structure gaps first, since later fixes build on them; then
   cross-cutting concerns; then style.
7. **Hand over the plan**: one item per gap, or the `migration-planning` skill when the fix
   spans several steps.
