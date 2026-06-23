---
title: "Unit testing with xUnit, Moq and Bogus"
date: 2026-06-22
status: Accepted
tags: [testing, xunit, moq, bogus, unit-testing, assertions]
---
# Recommendation: Unit Testing with xUnit, Moq, and Bogus

---

## Purpose

Provide consistent, lightweight, and capable unit testing guidance for .NET 10 C# projects in this repository and projects following these guidelines.

## Recommendation

Shared guidance for all test types is defined in **Recommendation: Testing Shared**. The following are unit-test–specific practices:

- Process: Follow TDD for unit behavior changes (write failing test first, implement minimal fix, then refactor).
- Test scope: Keep each test class focused on one feature or public method/command.
- Parameterized tests: Prefer `[Theory]` with `InlineData` for scenario variations, and include a final scenario-description argument when it improves readability.
- Builder-first arrangement: Do not instantiate aggregates/entities/specifications directly in tests when a builder exists. Instead, use builder classes to keep tests intention-revealing.
- Assertion strategy: Prefer AwesomeAssertions-style fluent domain helpers for outcome, validation, event, and collection checks; use primitive xUnit assertions only when no domain helper exists.
- Deterministic test data: When using Bogus for assertions, prefer deterministic seeds to ensure repeatable test results.
- Test completeness baseline: For each behavior under test, include at least one happy-path test, one critical validation/failure test, and one edge-case test.
- Coverage: Maintain ≥80% unit test coverage for Core and Server (as per Testing Strategy). Enforce via CI with coverlet collector or equivalent.
- Over-mocking avoidance: Prefer pure domain tests with concrete objects; use Moq only when testing interaction behavior with interfaces/ports is necessary.

## Rationale

- xUnit integrates well with .NET tooling, offers parallelization controls, and a simple attribute model.
- Moq is ergonomic for interface mocking and reduces brittle hand-written fakes when interaction behavior matters.
- Bogus produces realistic values and can reduce duplication in test data setup.

## Unit-Specific Test Patterns

### Builder-first arrangement (required when builders exist)

Prefer builder classes over direct aggregate/entity instantiation to keep tests intention-revealing and reusable:

```csharp
[Fact]
public void Should_RegisterOrder_When_CommandIsValid()
{
    // Arrange
    var command = OrderCommandBuilder.New().WithValidDefaults().Build();
    var sut = OrderServiceBuilder.New().Build();

    // Act
    var result = sut.Register(command);

    // Assert
    Assert.True(result.IsSuccess);
}
```

### Assertion strategy (AwesomeAssertions)

- Use domain-focused fluent assertions when available (for example: `ShouldBeSuccess`, `ShouldHaveValue`, `ShouldHaveValidationError`, `ShouldHaveEvent`).
- Avoid generic checks like `Assert.True(result.IsSuccess)` or manual event counting when an equivalent domain helper exists.
- If a scenario lacks an assertion helper, add one in the shared assertion library with a descriptive failure message.

### Deterministic Bogus seeding

Configure a deterministic seed when asserting on specific generated values:

```csharp
[Fact]
public void CreateOrder_UsesDeterministicData_WhenSeeded()
{
    Randomizer.Seed = new Random(1234); // Bogus deterministic seed
    var order = OrderFactory.Create();
    Assert.NotNull(order);
}
```

### Using Moq for interfaces

```csharp
var repo = new Mock<IOrderRepository>();
repo.Setup(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);
```

Use Moq primarily for interface-based dependencies and collaborators under test. Prefer concrete domain objects where feasible to avoid over-mocking.

## Anti-patterns to avoid

- Spreading ad-hoc object construction across tests; use factories/builders (see Shared Instructions).
- Relying on random non-deterministic data for assertions; seed when asserting values.
- Over-mocking behavior that's better validated via domain logic; prefer pure domain tests with concrete objects where feasible.
- Directly newing aggregates/entities in tests when builders already exist.
- Mixing many unrelated behaviors in one test class.
- Omitting explicit `// Arrange`, `// Act`, and `// Assert` section markers (see Shared Instructions).
- Using vague or implementation-first test names instead of behavior-focused names.
- Shipping only happy-path tests without failure and edge-case coverage for the same behavior.
- Introducing additional test frameworks or assertion libraries without prior approval.

## Approval for additional dependencies

If you need additional test libraries (snapshot testing, specialized generators, etc.), open an ADR or GitHub issue to request approval before adding the dependency. Include justification and impact.

Dependency hygiene rules are defined in **Recommendation: Testing Shared Instructions**.

## References

- ADR 0001: Adopt .NET 10
- Testing Strategy in `.github/copilot-instructions.md`
- Recommendation: Testing Shared
- Moq: <https://github.com/moq/moq>
- Bogus: <https://github.com/bchavez/Bogus>
- xUnit: <https://xunit.net/>
