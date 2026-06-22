---
title: "Background Jobs and Hosted Services"
date: 2026-06-04
status: Accepted
tags: [background-jobs, hosted-services, workers, idempotency, recommendations]
---
# Recommendation: Background Jobs and Hosted Services

## Purpose

Standardize how background work is implemented, scheduled, and operated in .NET solutions.

## Recommendation

- Use `BackgroundService` or worker-hosted services for long-lived background processing.
- Keep scheduling and transport concerns in the host/adapter layer.
- Make every background job idempotent.
- Respect cancellation tokens end to end.
- Emit logs, traces, and metrics for each recurring or message-driven job.

## Design rules

- Background services orchestrate work; they do not hold domain logic.
- Prefer durable queues or outbox-driven dispatch when work must survive process restarts.
- Use dedicated service identities for jobs that cross security boundaries.
- Avoid hidden infinite loops without delay, backoff, or cancellation checks.

## Failure handling

- Retries belong at the job runner or adapter boundary.
- Use dead-letter or operator-visible failure paths for repeated failures.
- Keep job payloads small and explicit.

## References

- ADR 0015: Resilience Strategy for Outbound Dependencies
- ADR 0016: Messaging and Integration-Event Delivery
- ADR 0010: Adopt OpenTelemetry for Comprehensive Observability
