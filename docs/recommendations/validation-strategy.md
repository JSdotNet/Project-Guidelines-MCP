---
title: "Validation Strategy"
date: 2026-06-04
status: Accepted
tags: [validation, fluentvalidation, minimal-api, domain, recommendations]
---
# Recommendation: Validation Strategy

## Purpose

Clarify the boundary between input validation, application validation, and domain invariant enforcement.

## Recommendation

- Validate external input at the delivery boundary.
- Enforce business invariants in the domain model.
- Keep validation messages consistent with the API error contract.
- Use FluentValidation or equivalent boundary validators only for transport/application validation, not domain truth.

## Validation layers

### 1. Boundary validation

Use endpoint filters, request validators, or equivalent mechanisms for:

- Required fields
- Shape and format checks
- Simple range checks
- Fast rejection of malformed input

### 2. Domain validation

Use domain behavior and factories for:

- Invariant enforcement
- State transition rules
- Aggregate consistency

Do not rely on request validators alone to protect domain correctness.

## References

- ADR 0004: Standardize Result Objects for Expected Application Outcomes
- ADR 0007: Recommend Minimal APIs Over Controller-Based APIs
- ADR 0017: HTTP Error Contract and Problem Details
