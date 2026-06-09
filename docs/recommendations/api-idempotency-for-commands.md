---
title: "API Idempotency for Commands"
date: 2026-06-04
status: Accepted
tags: [api, idempotency, commands, payments, reliability, recommendations]
---
# Recommendation: API Idempotency for Commands

## Purpose

Define when command endpoints should be idempotent and how that requirement should be implemented safely.

## Recommendation

- Require idempotency for command endpoints that create or trigger expensive or externally visible side effects.
- Use explicit idempotency keys or naturally unique business keys.
- Store enough request outcome state to return the same logical result for duplicates.

## Typical candidates

- Payment or billing commands
- Registration or invitation flows
- Webhook-triggered commands
- Public APIs exposed to mobile or browser clients where retries are expected

## Design rules

- Idempotency belongs at the delivery/application boundary, not in the UI only.
- Duplicate detection must survive process restarts when the endpoint matters operationally.
- Do not implement retries for non-idempotent commands without first adding idempotency protection.

## References

- ADR 0015: Resilience Strategy for Outbound Dependencies
- ADR 0016: Messaging and Integration-Event Delivery
- ADR 0017: HTTP Error Contract and Problem Details
