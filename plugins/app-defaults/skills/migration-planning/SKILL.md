---
name: migration-planning
description: "Plan and execute incremental refactoring to align code with guidelines. Break into phases, manage dependencies, and track progress."
---

# Migration planning

Turns a list of gaps into steps that each land on their own and leave the build green. Plan
and update templates: [reference.md](reference.md).

## Workflow

1. **Start from the gaps**: the `gap-analysis` report, or the deviations the server's
   `review-against-defaults` prompt listed. Each carries its doc id.
2. **Read the target** for each gap with `get_guide`, so every step names the default it
   moves toward.
3. **Describe the end state** per area: as-is, to-be, and the doc id.
4. **Cut it into steps** that each ship as one pull request, keep the build and tests
   passing, and can be stopped after without leaving the code half-moved.
5. **Order by dependency**: build policy and solution structure first, then one module or
   feature slice at a time, then cross-cutting concerns.
6. **Give each step its proof**: the tests that must pass before and after, and the check that
   shows the gap closed.
7. **Track progress** with the update template, and re-run `gap-analysis` at the end.

A step that needs a decision the defaults leave open is preceded by the application's own
ADR, drafted with `propose-adr`.
