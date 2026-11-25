---
title: "Backend Technologies"
date: 2026-11-25
status: Accepted
tags: [tech, ratings, backend, aspnet-core, minimal-apis, ef-core, resilience, authentication, mcp, agent-framework, microsoft-extensions-ai]
---
# Backend Technologies

The Api host, application layer, persistence, security, MCP and agents. Ratings and layers:
[technology-graph.md](technology-graph.md).

## ASP.NET Core Minimal APIs

- **Rating:** adopted · **Version:** 11.0.0 (`Microsoft.AspNetCore.OpenApi`)
- **Depends on:** [.NET](shared.md#net), [OpenAPI](shared.md#openapi)
- **Used for:** endpoint classes per feature slice, built-in `AddValidation()`, native OpenAPI
  with Scalar in development, and Problem Details for every error.
- **Why:** [ADR 0004](../arc42/adr/0004-http-api.md).

## Entity Framework Core

- **Rating:** adopted · **Version:** 11.0.0
- **Depends on:** [.NET](shared.md#net)
- **Used for:** all data access, only in `{Module}.Data.{Provider}` projects. The provider
  package is the app's choice.
- **Why:** [ADR 0006](../arc42/adr/0006-persistence-and-messaging.md).

## Microsoft.Extensions.Http.Resilience

- **Rating:** adopted · **Version:** 11.0.0
- **Depends on:** [.NET](shared.md#net)
- **Used for:** the standard resilience handler on every HTTP client, from ServiceDefaults.
  Polly v8 pipelines only for non-HTTP adapters.
- **Why:** [ADR 0007](../arc42/adr/0007-observability-and-resilience.md).

## JWT bearer and OpenID Connect handlers

- **Rating:** adopted · **Version:** 11.0.0
- **Depends on:** [ASP.NET Core Minimal APIs](#aspnet-core-minimal-apis),
  [OpenID Connect and OAuth 2.1](shared.md#openid-connect-and-oauth-21)
- **Used for:** full token validation. `Microsoft.Identity.Web` for Entra ID;
  `Microsoft.AspNetCore.Authentication.JwtBearer` and `.OpenIdConnect` for other providers.
- **Why:** [ADR 0008](../arc42/adr/0008-security.md).

## MCP C# SDK

- **Rating:** adopted · **Version:** 2.x (`ModelContextProtocol`)
- **Depends on:** [ASP.NET Core Minimal APIs](#aspnet-core-minimal-apis)
- **Used for:** an app's MCP server, stateless Streamable HTTP inside its Api host.
- **Why:** [ADR 0011](../arc42/adr/0011-setting-up-mcp.md).

## Microsoft.Extensions.AI

- **Rating:** adopted
- **Depends on:** [.NET](shared.md#net)
- **Used for:** `IChatClient`, the only model abstraction application code sees.
- **Why:** [ADR 0012](../arc42/adr/0012-agents-in-applications.md).

## Microsoft Agent Framework

- **Rating:** adopted · **Version:** 1.x
- **Depends on:** [Microsoft.Extensions.AI](#microsoftextensionsai)
- **Used for:** agents and agent workflows inside an app, models from Microsoft Foundry behind
  one adapter.
- **Why:** [ADR 0012](../arc42/adr/0012-agents-in-applications.md).

## MediatR

- **Rating:** hold
- **Used for:** nothing in a new app. Handlers are plain classes called by their endpoint.
- **Why:** [ADR 0005](../arc42/adr/0005-application-layer.md).

## FluentValidation

- **Rating:** hold
- **Used for:** nothing in a new app. Transport validation is built-in minimal-API validation,
  invariants live in the domain.
- **Why:** [ADR 0004](../arc42/adr/0004-http-api.md).

## Dapper

- **Rating:** hold
- **Used for:** nothing in a new app. A hot path uses EF Core raw SQL.
- **Why:** EF Core is the only data-access library
  ([ADR 0006](../arc42/adr/0006-persistence-and-messaging.md)).

## Aspire.Azure.AI.OpenAI and Aspire.Azure.AI.Inference

- **Rating:** hold
- **Used for:** nothing. Discontinued; `Aspire.Azure.AI.Projects` and
  `Aspire.Azure.AI.Extensions.OpenAI` replace them.
- **Why:** [ADR 0012](../arc42/adr/0012-agents-in-applications.md).
