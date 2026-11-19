---
title: "ADR 0004: HTTP API"
date: 2026-11-19
status: Accepted
tags: [minimal-api, openapi, scalar, validation, problem-details, http, errors, client-generation, adr]
---
# ADR 0004: HTTP API

## Context

Controllers, endpoint lambdas, FluentValidation and ad-hoc error bodies each solved part of
the HTTP edge in a different way. .NET 11 ships built-in minimal-API validation with async
DataAnnotations support and OpenAPI 3.2 generation, which covers what the third-party pieces
were there for.

## Decision

**Endpoints.** Minimal APIs, organized as endpoint classes per feature slice (ADR 0003): each
maps its routes and calls one handler (ADR 0005). Controllers are not used. Native OpenAPI
generates an OpenAPI 3.2 document and Scalar renders it in development.

**Validation.** Transport validation runs at the edge with the built-in minimal-API validation
(`builder.Services.AddValidation()`, including async DataAnnotations validation). Business
invariants live in the domain model. FluentValidation is not used.

**Errors.** Every non-success response is RFC 9457 Problem Details with `type`, `title`,
`status`, `detail` and `traceId`. Expected outcomes map from `Result<T>` in one exhaustive
`switch`:

| Outcome | Status |
|---|---|
| Validation failure | 400, with an `errors` extension |
| Unauthorized / forbidden | 401 / 403 |
| Not found | 404 |
| Conflict, including a stale concurrency version (ADR 0006) | 409 |

Unexpected exceptions are handled once by the exception handler and become a 500 Problem
Details without internals.

**Clients.** Every client, frontend and service-to-service, is generated from the OpenAPI
document; no hand-written HTTP client duplicates a contract.

## Consequences

### Positive

- One endpoint style, one validation path and one error shape across every app.
- Clients break at build time, not at run time, when a contract changes.
- Fewer third-party packages on the request path.

### Negative

- Rules that need services or cross-field logic must fit async DataAnnotations or move into
  the handler or domain.
- Client generators must support OpenAPI 3.2.

## References

- [Minimal APIs overview](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/overview)
- [OpenAPI in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/overview)
- [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457)
- ADR 0005: Application Layer

Replaces: ADR 0007 (Minimal APIs Over Controller-Based APIs), ADR 0017 (HTTP Error Contract and Problem Details).
