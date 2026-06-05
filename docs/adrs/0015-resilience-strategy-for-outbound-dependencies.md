---
title: "ADR 0015: Resilience Strategy for Outbound Dependencies"
date: 2026-06-04
status: Accepted
tags: [resilience, polly, http, messaging, retries, timeouts, adr]
---
# ADR 0015: Resilience Strategy for Outbound Dependencies

## Context

The repository already recommends clean boundaries and observability, but it does not define how adapters should handle unreliable outbound calls to APIs, queues, caches, or databases. Teams need a consistent answer for:

1. When retries are appropriate.
2. When timeouts and circuit breakers are required.
3. Where resilience logic belongs.
4. How to avoid retry storms and hidden side effects.

## Decision

We STANDARDIZE resilience policies at the **adapter boundary only**, using **Polly-based policy composition** for outbound dependencies.

### 1. Resilience logic belongs in adapters

Retries, timeouts, circuit breakers, fallback behavior, and bulkheads are applied only where the application crosses a technical boundary:

- Outbound HTTP clients
- Message producers/consumers
- Cache clients
- External SDKs

Domain models, command handlers, and query handlers do not implement retry loops.

### 2. Timeouts are mandatory for outbound I/O

All outbound network calls must have an explicit timeout. Infinite or default timeout behavior is not allowed for production-facing integrations.

Timeout rules:

- Prefer short, purposeful timeouts per dependency.
- Use separate connect/request or overall operation timeouts when the client stack supports it.
- Surface timeout failures explicitly with structured logging and trace context.

### 3. Retries are opt-in and only for transient failures

Retries are allowed only when the operation is safe to retry or is protected by idempotency guarantees.

Allowed retry scenarios:

- Transient network failures
- HTTP `408`, `429`, and `5xx` responses where the contract supports retry
- Broker/client transient connection failures

Retries are prohibited for:

- Validation failures
- Authentication/authorization failures
- Known business rule failures
- Non-idempotent commands without idempotency protection

### 4. Circuit breakers protect degraded dependencies

Use circuit breakers for dependencies whose repeated failure would otherwise:

- Exhaust threads or sockets
- Cascade latency across requests
- Obscure the dependency as the source of failure

Circuit-breaker state changes must be observable through logs and metrics.

### 5. Use jittered backoff, not aggressive fixed retries

When retries are used:

- Prefer exponential backoff with jitter.
- Keep retry counts low.
- Avoid nested retry policies across call layers.

### 6. Fallbacks are explicit and rare

Fallback logic is allowed only when the degraded behavior is intentionally designed, documented, and safe. A fallback must never masquerade as a successful primary-path result.

## Consequences

### Positive

1. **Consistent failure handling**: Teams apply the same resilience model across adapters.
2. **Clear architecture boundaries**: Resilience stays out of the domain and use-case core.
3. **Better production behavior**: Timeouts and circuit breakers prevent slow dependency failures from spreading.
4. **Traceable degradation**: Policy behavior is observable and diagnosable.

### Negative

1. **Policy tuning required**: Poor thresholds can still cause failures or unnecessary load.
2. **More infrastructure registration**: Each external dependency needs deliberate policy wiring.
3. **No blanket retry convenience**: Teams must think about safety and idempotency first.

## References

- ADR 0003: Strong Recommendation to Adopt .NET Aspire for ASP.NET Web Services
- ADR 0010: Adopt OpenTelemetry for Comprehensive Observability
- Recommendation: Integration Testing
