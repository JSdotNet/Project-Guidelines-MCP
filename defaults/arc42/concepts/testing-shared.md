---
title: "Testing Shared Instructions"
date: 2026-06-22
status: Accepted
tags: [testing, unit-testing, integration-testing, e2e, architecture, recommendations]
---
## Recommendation: Testing Shared Instructions

## Purpose

Define shared testing guidance that applies to all test types: unit, integration, end-to-end, and architecture.

## Test Strategy

- Use a four-layer test strategy:
  - Unit tests: validate behavior in isolation.
  - Integration tests: validate component collaboration and runtime wiring.
  - End-to-end tests: validate critical business journeys across the deployed topology.
  - Architecture tests: validate structural rules such as dependency direction and module boundaries.
- Keep test-type boundaries explicit: avoid duplicating the same scenario across layers unless the higher layer adds distinct risk coverage.
- Run tests in CI in increasing cost order: unit, integration, architecture, then end-to-end.

## Test-Type Boundaries

- Unit tests should not require infrastructure startup.
- Integration tests should focus on boundaries and contracts rather than full business-journey coverage.
- End-to-end tests should stay small and business-critical.
- Architecture tests should assert policy rules, not behavior of use cases.

## Project Layout and Structure

- Keep production projects under `src/` and test projects under repository-root `tests/`.
- Use one default test project per test type and solution:
  - `tests/Company.Product.{Module}.UnitTests` (one unit test project per module)
  - `tests/Company.Product.IntegrationTests`
  - `tests/Company.Product.E2ETests`
  - `tests/Company.Product.ArchitectureTests`
- For very large solutions, module-specific integration or architecture test projects are allowed when differing policy or operational boundaries justify the split.

## Shared Test Stack

All test types use a consistent tooling foundation:

- **Test framework**: Use xUnit for all test projects (unit, integration, architecture, end-to-end).
- **Mocking**: You may depend on Moq for mocking interfaces and collaborators when appropriate for the test type.
- **Test data**: Use Bogus to generate realistic fake data. Prefer deterministic seeds in tests that assert on generated values.
- **Assertions**: Prefer AwesomeAssertions-style fluent domain helpers for outcome, validation, event, and collection checks; use primitive xUnit assertions only when no domain helper exists.
- **Test project setup**: Each test project targets net10.0 and references Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio (PrivateAssets=all), and coverlet.collector (PrivateAssets=all).

## Shared Test Patterns

### AAA (Arrange-Act-Assert) Structure

All tests must explicitly mark the sections:

- `// Arrange`: only setup and test data creation
- `// Act`: one operation under test
- `// Assert`: verify outcomes

### Naming Convention

Use behavior-focused test names that convey intent:

- Unit tests: `Should_ExpectedBehavior_When_StateUnderTest`
- Integration tests: `DescribeScenario_ShouldProduceOutcome_WhenCondition`
- Architecture tests: `ModuleOrLayer_Should_EnforcePolicy`

Keep names explicit and avoid vague or implementation-first names.

### Test Data Factories

Encapsulate test object creation behind factory methods or builder classes to reduce duplication and improve maintainability:

```csharp
public static class OrderFactory
{
    private static readonly Faker faker = new();

    public static Order Create(
        Guid? id = null,
        Customer? customer = null,
        IReadOnlyList<OrderLine>? lines = null)
    {
        var cust = customer ?? new Customer(faker.Random.Guid(), faker.Person.FullName);
        var orderLines = lines ?? Enumerable.Range(0, faker.Random.Int(1, 3))
            .Select(_ => new OrderLine(faker.Commerce.Ean13(), faker.Random.Int(1, 5)))
            .ToList();
        return Order.Create(id ?? Guid.NewGuid(), cust, orderLines);
    }
}
```

### TestDataHelper Pattern

Use a static TestDataHelper for realistic random data and boundary-value generation:

```csharp
public static class TestDataHelper
{
    private static readonly Faker Faker = new();

    public static decimal RandomAmount(decimal min = 100m, decimal max = 50_000m)
        => Faker.Random.Decimal(min, max);

    public static string RandomReference()
        => Faker.Random.AlphaNumeric(12).ToUpperInvariant();
}
```

### Dependency Hygiene

- Avoid adding other test libraries without explicit approval via an ADR or issue/PR discussion.
- Keep the test stack minimal and standard across all modules.

## References

- Recommendation: Unit testing with xUnit, Moq and Bogus
- Recommendation: Integration Testing
- Recommendation: End-to-End Testing
- Recommendation: Architecture Testing for Layer and Module Boundaries
