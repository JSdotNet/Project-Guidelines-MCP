---
title: "Shared Technologies"
date: 2026-11-25
status: Accepted
tags: [tech, ratings, dotnet, csharp, openapi, oidc, opentelemetry]
---
# Shared Technologies

The platform and the contracts every layer uses. Ratings and layers:
[technology-graph.md](technology-graph.md).

## .NET

- **Rating:** adopted · **Version:** 11, pinned in `global.json` with `latestFeature`
- **Used for:** the runtime of every app, its tests and its scripts.
- **Why:** the newest GA release, STS included ([ADR 0001](../arc42/adr/0001-platform-and-build.md)).

## C#

- **Rating:** adopted · **Version:** 15, the SDK default; no preview features
- **Depends on:** [.NET](#net)
- **Used for:** all production code, tests and file-based scripts.
- **Why:** [ADR 0001](../arc42/adr/0001-platform-and-build.md); `Result<T>` is a union
  ([ADR 0005](../arc42/adr/0005-application-layer.md)).

## OpenAPI

- **Rating:** adopted · **Version:** 3.2
- **Used for:** the contract every Api publishes and every client is generated from.
- **Why:** [ADR 0004](../arc42/adr/0004-http-api.md), [ADR 0009](../arc42/adr/0009-frontend.md).

## OpenID Connect and OAuth 2.1

- **Rating:** adopted
- **Used for:** user sign-in at an external identity provider, and the protected-resource
  role of every MCP server.
- **Why:** [ADR 0008](../arc42/adr/0008-security.md), [ADR 0011](../arc42/adr/0011-setting-up-mcp.md).
  The provider itself is rated in the app's own `tech/`.

## OpenTelemetry

- **Rating:** adopted · **Version:** 1.21.0
- **Depends on:** [.NET](#net)
- **Used for:** traces, metrics and logs from every service, wired by ServiceDefaults and read
  in the Aspire dashboard locally.
- **Why:** [ADR 0007](../arc42/adr/0007-observability-and-resilience.md).
