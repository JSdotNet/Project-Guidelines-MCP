---
title: "Database Migration Workflow"
date: 2026-06-04
status: Accepted
tags: [database, migrations, ef-core, deployment, recommendations]
---
# Recommendation: Database Migration Workflow

## Purpose

Define a safe, reviewable workflow for schema evolution in projects that follow module-owned persistence.

## Recommendation

- Keep migrations with the owning module's data adapter project.
- Generate migrations in the same change set as the model change.
- Review migration intent like code, not as generated noise.
- Do not run destructive automatic migrations at production startup.

## Workflow rules

- Name migrations by business intent, not timestamps alone.
- Review data-loss operations explicitly.
- Separate local-development convenience from production deployment behavior.
- Capture rollback or mitigation expectations for risky schema changes.

## References

- ADR 0014: Persistence Strategy and Repository Boundaries
- Recommendation: Integration Testing
