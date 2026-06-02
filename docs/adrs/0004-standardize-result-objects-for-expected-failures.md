---
title: "ADR 0004: Standardize Result Objects for Expected Application Outcomes"
date: 2026-06-01
status: Accepted
tags: [result-pattern, error-handling, domain, application, adr]
---
# ADR 0004: Standardize Result Objects for Expected Application Outcomes

Date: 2026-06-01
Status: Accepted

## Context
The codebase currently mixes styles for expected failures and business validation outcomes. Some examples use exceptions for expected rule failures, while others suggest returning explicit `Result` values.

This inconsistency increases cognitive load and leads to unclear API contracts across handlers, endpoints, and tests.

## Decision
Adopt Result objects as the standard contract for expected outcomes at application boundaries.

1. Application handlers and use cases return `Result` / `Result<T>` for expected business outcomes.
2. Domain models continue to enforce invariants internally; domain violations may raise domain-specific exceptions internally when needed.
3. Application layer translates domain exceptions to failed `Result` objects before crossing boundaries.
4. Delivery adapters (HTTP, gRPC, messaging) map `Result` states to transport-specific responses.
5. Unexpected technical faults (I/O failures, infrastructure outages, serialization faults) remain exception-driven.

### Result Shape (Guideline)
A `Result` type should represent:
- Success flag
- Optional value (`Result<T>`)
- Machine-readable error code
- Human-readable error message
- Optional validation details

Example:

```csharp
public sealed record Result(bool IsSuccess, string? ErrorCode = null, string? ErrorMessage = null)
{
    public static Result Success() => new(true);
    public static Result Failure(string code, string message) => new(false, code, message);
}

public sealed record Result<T>(bool IsSuccess, T? Value = default, string? ErrorCode = null, string? ErrorMessage = null)
{
    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Failure(string code, string message) => new(false, default, code, message);
}
```

## Consequences
### Positive
- Clear and explicit contracts for expected outcomes.
- Easier testing of success/failure paths without exception-first assertions.
- Consistent translation from application outcomes to transport responses.
- Reduced exception noise for normal business rule failures.

### Negative
- Additional plumbing to map exceptions and validation details into Result objects.
- Requires discipline to avoid mixing Result and exception flows in the same boundary.

### Mitigations
- Provide shared Result helpers and conversion methods.
- Add code-review checks for boundary consistency.
- Keep exception usage focused on unexpected technical failures.

## References
- Design: Pragmatic Domain-Driven Design Approach
- ADR 0006: Recommendation to Implement CQRS for ASP.NET API Projects
- Recommendation: Unit Testing with xUnit, Moq, and Bogus



