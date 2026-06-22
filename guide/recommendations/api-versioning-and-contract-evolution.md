---
title: "API Versioning and Contract Evolution"
date: 2026-06-04
status: Accepted
tags: [api, versioning, contracts, backward-compatibility, recommendations]
---
# Recommendation: API Versioning and Contract Evolution

## Purpose

Define how HTTP APIs evolve without surprising consumers or creating unnecessary breaking changes.

## Recommendation

- Prefer backward-compatible contract evolution within a stable API version.
- Add new fields as optional where possible.
- Remove or rename fields only through a deliberate versioning or deprecation path.
- Version only when compatibility cannot be preserved.

## Baseline rules

- Keep request and response DTOs explicit and additive-first.
- Document deprecations before removal.
- Keep Problem Details shapes stable across versions.
- Versioning strategy must be consistent within a solution; do not mix incompatible approaches casually.

## Preferred versioning order

1. No new version when an additive change is sufficient.
2. New version when behavior or schema meaning changes incompatibly.
3. Old version remains supported for a documented transition period.

## References

- ADR 0007: Recommend Minimal APIs Over Controller-Based APIs
- ADR 0017: HTTP Error Contract and Problem Details
- ADR 0009: Feature Slices Within Module Projects
