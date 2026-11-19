---
title: "Building Block: ServiceDefaults"
date: 2026-11-19
status: Accepted
tags: [building-blocks, aspire, service-defaults, opentelemetry, health-checks, resilience, structure]
---
# Building Block: ServiceDefaults

`Company.Product.ServiceDefaults` is the one place every service gets its telemetry, health
checks, service discovery and HTTP resilience (ADR 0007). Part of the
[building-block view](../05-building-block-view.md).

## Use

```csharp
builder.AddServiceDefaults();   // in every service's Program.cs
app.MapDefaultEndpoints();      // /health and /alive
```

## What it wires

| Concern | Default |
|---|---|
| Telemetry | OpenTelemetry logs, traces and metrics; OTLP exporter set by the AppHost; the Aspire dashboard locally |
| Module signals | Registers every module's `ActivitySource` and `Meter` by name |
| Health | `/health` (ready) and `/alive` (live) endpoints |
| Discovery | Service discovery for every `HttpClient` |
| Resilience | `AddStandardResilienceHandler()` on every `HttpClient`: timeout, jittered retry, circuit breaker |

## Rules

- Services never configure exporters, health endpoints or HTTP resilience themselves.
- Polly v8 pipelines for non-HTTP adapters live in that adapter, not here.
- Nothing domain-specific goes in this project; it references no module.
