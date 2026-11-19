---
title: "ADR 0007: Observability and Resilience"
date: 2026-11-19
status: Accepted
tags: [observability, opentelemetry, logging, loggermessage, service-defaults, resilience, polly, http, adr]
---
# ADR 0007: Observability and Resilience

## Context

Telemetry and resilience wired by hand in each service drift apart and are easy to forget.
Aspire's ServiceDefaults project already configures OpenTelemetry and the standard HTTP
resilience handler for every service that calls `AddServiceDefaults()`. Interpolated log
messages allocate on every call and lose their structure.

## Decision

**Observability.**

- OpenTelemetry logs, traces and metrics are configured once in ServiceDefaults; services do
  not wire exporters themselves.
- The Aspire dashboard is the local backend; deployed environments get their exporter through
  the AppHost (ADR 0002).
- Log messages are `[LoggerMessage]` source-generated methods, never string interpolation.

**Resilience.**

- HTTP clients use `AddStandardResilienceHandler()` from ServiceDefaults: timeouts, retries
  with jittered backoff and a circuit breaker.
- Polly v8 resilience pipelines are used only for non-HTTP adapters (a broker, cache or SDK
  client), configured in that adapter.
- Resilience stays at the adapter boundary; handlers and domain code have no retry loops.

## Consequences

### Positive

- Every service emits the same telemetry and gets the same HTTP resilience with no per-service
  code.
- Structured, allocation-free logging that the dashboard and agents can query.

### Negative

- A service that needs different HTTP resilience overrides the standard handler's options
  instead of composing its own.
- `[LoggerMessage]` methods add a declaration per message.

## References

- [Aspire service defaults](https://aspire.dev/fundamentals/service-defaults/)
- [Compile-time logging source generation](https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator)
- [Build resilient HTTP apps](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)
- ADR 0002: Aspire, Configuration and Infrastructure

Replaces: ADR 0010 (Adopt OpenTelemetry for Comprehensive Observability), ADR 0015 (Resilience Strategy for Outbound Dependencies).
