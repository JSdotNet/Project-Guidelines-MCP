---
title: "ADR 0005: Application Layer"
date: 2026-11-19
status: Accepted
tags: [cqrs, handlers, result-pattern, unions, csharp-15, time-provider, application, adr]
---
# ADR 0005: Application Layer

## Context

Endpoint code that mixes HTTP, orchestration and domain logic is hard to test and reuse.
Mediator libraries add indirection and a dependency for what is a method call. Class-based
`Result` types let callers read `Value` without checking for failure. C# 15 unions let the
compiler prove that every outcome is handled. Code that reads the clock directly cannot be
tested deterministically.

## Decision

**Handlers.** Each use case is a command or query record handled by a hand-rolled handler
class, registered in DI and called directly by its endpoint. No MediatR or other mediator.
Handlers carry no HTTP types and return DTOs, never domain entities.

**Expected outcomes** are a C# 15 union `Result<T>`, matched with an exhaustive `switch`;
CS8509 is an error (ADR 0001), so an unhandled outcome does not compile. Unexpected technical
faults stay exceptions.

```csharp
public union Result<T>(Ok<T>, NotFound, Invalid, Conflict);

return await handler.Handle(command, ct) switch
{
    Ok<OrderDto> ok => TypedResults.Ok(ok.Value),
    NotFound     => TypedResults.Problem(statusCode: 404),
    Invalid i    => TypedResults.ValidationProblem(i.Errors),
    Conflict     => TypedResults.Problem(statusCode: 409),
};
```

Existing class-based `Result` types are migrated to the union in this rollout.

**Time.** `TimeProvider` is injected wherever code needs the current time; `DateTime.Now` and
`DateTime.UtcNow` are not used. Tests use `FakeTimeProvider`.

## Consequences

### Positive

- Handlers test without HTTP or a mediator, and the call chain is plain to follow.
- Adding an outcome breaks every place that must handle it, at compile time.
- Time-dependent behaviour is deterministic in tests.

### Negative

- A little registration and endpoint plumbing per handler, which a mediator would hide.
- Migrating existing `Result` types touches every handler and endpoint that uses them.

## References

- [C# unions](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union)
- [TimeProvider](https://learn.microsoft.com/dotnet/standard/datetime/timeprovider-overview)
- ADR 0004: HTTP API

Replaces: ADR 0004 (Standardize Result Objects for Expected Application Outcomes), ADR 0006 (CQRS Recommendation for ASP.NET API).
