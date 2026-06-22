---
title: "Feature Flags and Rollout Safety"
date: 2026-06-04
status: Accepted
tags: [feature-flags, rollout, deployment, safety, recommendations]
---
# Recommendation: Feature Flags and Rollout Safety

## Purpose

Provide guidance for introducing change safely without long-lived branch divergence or risky all-at-once rollouts.

## Recommendation

- Use feature flags for incomplete or high-risk functionality that must merge before full release.
- Keep flags explicit, discoverable, and short-lived.
- Pair risky rollouts with observability and rollback paths.

## Rules

- Every flag has an owner and intended removal point.
- Avoid deeply nested or permanently accumulating flags.
- Do not use flags to bypass security or data-integrity rules.
- Prefer coarse-grained release flags over scattered low-level toggles when possible.

## References

- ADR 0010: Adopt OpenTelemetry for Comprehensive Observability
- ADR 0015: Resilience Strategy for Outbound Dependencies
