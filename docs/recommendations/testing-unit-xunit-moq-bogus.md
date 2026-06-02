---
title: "Unit testing with xUnit, Moq and Bogus"
date: 2025-11-12
status: Accepted
tags: [testing, xunit, moq, bogus, unit-testing, assertions]
---
# Recommendation: Unit Testing with xUnit, Moq, and Bogus

---

## Purpose

Provide consistent, lightweight, and capable unit testing guidance for .NET 10 C# projects in this repository and projects following these guidelines.

## Recommendation

- Process: Follow TDD for unit behavior changes (write failing test first, implement minimal fix, then refactor).
- Test framework: Use xUnit for all unit test projects.
- Mocking: You may depend on Moq for mocking interfaces and collaborators.
- Assertions: Prefer AwesomeAssertions-style fluent domain helpers for outcome, validation, event, and collection checks; use primitive xUnit assertions only when no domain helper exists.
- Test data: Use Bogus to generate realistic fake data. Prefer deterministic seeds in tests that assert on generated values.
- Test scope: Keep each test class focused on one feature or public method/command.
- Method naming: Use `Should_ExpectedBehavior_When_StateUnderTest` for test methods. Keep names behavior-oriented and explicit.
- Parameterized tests: Prefer `[Theory]` with `InlineData` for scenario variations, and include a final scenario-description argument when it improves readability.
- Arrangement style: Every test must include explicit `// Arrange`, `// Act`, and `// Assert` comments.
- Builder-first arrangement: Do not instantiate aggregates/entities/specifications directly in tests when a builder exists.
- TestDataHelper: Centralize random primitives and common specs in a static TestDataHelper with sensible defaults and override points.
- Data factories: Encapsulate test object creation behind factory methods or builder classes. Configure factories to use Bogus internally for object graphs and random-but-plausible defaults.
- Test completeness baseline: For each behavior under test, include at least one happy-path test, one critical validation/failure test, and one edge-case test.
- Coverage: Maintain ≥80% unit test coverage for Core and Server (as per Testing Strategy). Enforce via CI with coverlet collector or equivalent.
- Dependency hygiene: Avoid adding other test libraries without explicit approval via an ADR or issue/PR discussion. Keep the test stack minimal and standard across modules.

## Test Project Placement and Structure

- Keep production projects under `src/` and keep test projects under repository-root `tests/`.
- Use one unit test project per module: `tests/Company.Product.{Module}.UnitTests`.
- Use one shared integration test project per solution: `tests/Company.Product.IntegrationTests`.
- Use one shared architecture test project per solution by default: `tests/Company.Product.ArchitectureTests`.
- Use one shared end-to-end test project per solution: `tests/Company.Product.E2ETests`.
- For very large solutions, module-specific integration or architecture test projects are allowed when justified by differing policies or operational boundaries.

## Rationale

- xUnit integrates well with .NET tooling, offers parallelization controls, and a simple attribute model.
- Moq is widely used, ergonomic for interface mocking, and reduces brittle hand-written fakes when interaction behavior matters.
- Bogus produces realistic values and can reduce duplication in test data setup. Centralized factories improve readability and encourage reuse.

## Usage Patterns

### Project setup (example)

- Test project targets net10.0
- References: Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio (PrivateAssets=all), coverlet.collector (PrivateAssets=all)
- Optionally reference Moq and Bogus as needed by the tests.

### Factory pattern (recommended)

Place factories under `Tests/Factories` or similar.

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
            .Select(_ => new OrderLine(faker.Commerce.Ean13(), faker.Random.Int(1,5)))
            .ToList();
        return Order.Create(id ?? Guid.NewGuid(), cust, orderLines);
    }
}
```

### Builder-first arrangement (required when builders exist)

Prefer builder classes over direct aggregate/entity instantiation to keep tests intention-revealing and reusable.

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

### TestDataHelper pattern

Use a static TestDataHelper for realistic random data and boundary-value generation.

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

### AAA annotation standard

All tests must explicitly mark the sections:

- `// Arrange`: only setup and test data creation
- `// Act`: one operation under test
- `// Assert`: verify outcomes

### Test completeness baseline

For each behavior, provide at least:

1. Happy path: success outcome and resulting state
2. Failure path: critical validation/guardrail
3. Edge case: boundary/null/duplicate scenario

Configure a deterministic seed when asserting on specific values:

```csharp
[Fact]
public void CreateOrder_UsesDeterministicData_WhenSeeded()
{
    Randomizer.Seed = new Random(1234); // Bogus deterministic seed
    var order = OrderFactory.Create();
    Assert.NotNull(order);
}
```

### Using Moq for ports

```csharp
var repo = new Mock<IOrderRepository>();
repo.Setup(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);
```

## Anti-patterns to avoid

- Spreading ad-hoc object construction across tests; use factories/builders.
- Relying on random non-deterministic data for assertions; seed when asserting values.
- Over-mocking behavior that’s better validated via domain logic; prefer pure domain tests with concrete objects where feasible.
- Directly newing aggregates/entities in tests when builders already exist.
- Mixing many unrelated behaviors in one test class.
- Omitting explicit `// Arrange`, `// Act`, and `// Assert` section markers.
- Using vague or implementation-first test names instead of behavior-focused names.
- Shipping only happy-path tests without failure and edge-case coverage for the same behavior.
- Introducing additional test frameworks or assertion libraries without prior approval.

## Approval for additional dependencies

If you need additional test libraries (snapshot testing, specialized generators, etc.), open an ADR or GitHub issue to request approval before adding the dependency. Include justification and impact.

## References

- ADR 0001: Adopt .NET 10
- Testing Strategy in `.github/copilot-instructions.md`
- Moq: <https://github.com/moq/moq>
- Bogus: <https://github.com/bchavez/Bogus>
- xUnit: <https://xunit.net/>
