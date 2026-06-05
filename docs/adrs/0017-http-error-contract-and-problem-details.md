---
title: "ADR 0017: HTTP Error Contract and Problem Details"
date: 2026-06-04
status: Accepted
tags: [http, problem-details, errors, result-pattern, minimal-api, adr]
---
# ADR 0017: HTTP Error Contract and Problem Details

## Context

ADR 0004 standardizes `Result` and `Result<T>` for expected application outcomes, but the repository does not yet define a uniform HTTP error contract for API clients. Teams still need a shared answer for:

1. How `Result` maps to HTTP status codes.
2. How validation, domain, technical, and authorization failures differ at the API boundary.
3. Whether RFC 7807 Problem Details is required.

## Decision

We REQUIRE ASP.NET APIs to expose failures using **RFC 7807 Problem Details** and to map `Result` outcomes consistently at the delivery boundary.

### 1. Expected failures map from `Result`

The application layer returns `Result` or `Result<T>` for expected failures such as:

- Validation failures
- Not-found outcomes
- Forbidden outcomes
- Business rule failures

The delivery layer translates those outcomes into HTTP responses.

Recommended baseline mapping:

| Application outcome | HTTP status |
|---|---|
| Validation failure | `400 Bad Request` |
| Not found | `404 Not Found` |
| Forbidden | `403 Forbidden` |
| Conflict/business rule conflict | `409 Conflict` |
| Unauthorized | `401 Unauthorized` |

### 2. Problem Details is the standard error body

All non-success HTTP responses from application APIs use Problem Details-compatible payloads.

Required fields:

- `type`
- `title`
- `status`
- `detail`
- `traceId`

Optional extensions may include:

- `code`
- `errors`
- `correlationId`
- `resourceId`

### 3. Unexpected exceptions are handled once at the boundary

Unexpected technical failures are converted to `500 Internal Server Error` Problem Details responses by centralized exception handling middleware or equivalent pipeline wiring.

Rules:

- Do not scatter broad try/catch blocks through endpoints and handlers.
- Log unexpected exceptions once at the boundary.
- Do not leak stack traces or internal implementation details to clients.

### 4. Validation details are structured

Boundary validation failures may include an `errors` extension keyed by field name or validation rule. Domain invariants remain enforced in the domain model even when boundary validation exists.

## Consequences

### Positive

1. **Predictable API contracts**: Clients get one consistent failure shape.
2. **Clean layering**: The application layer stays HTTP-agnostic while APIs still expose a standard contract.
3. **Better diagnosability**: Trace and correlation identifiers travel with failures.
4. **Safer error exposure**: Internal details remain server-side.

### Negative

1. **More translation code**: Endpoints or shared mapping helpers must convert results deliberately.
2. **Client updates may be needed**: Existing clients with ad-hoc error parsing may need to adjust.

## References

- ADR 0004: Standardize Result Objects for Expected Application Outcomes
- ADR 0007: Recommend Minimal APIs Over Controller-Based APIs
- ADR 0010: Adopt OpenTelemetry for Comprehensive Observability
- ADR 0013: Authorization & Zero Trust Security Model
