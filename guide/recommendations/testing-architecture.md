---
title: "Architecture Testing for Layer and Module Boundaries"
date: 2026-06-22
status: Accepted
tags: [architecture, testing, clean-architecture, netarchtest, boundaries, modular-monolith]
---
# Recommendation: Architecture Testing for Layer and Module Boundaries

## Purpose

Define a repeatable approach for automated architecture tests that enforce dependency direction, module boundaries, and forbidden technology references in .NET modular solutions.

## Recommendation

- Shared guidance for all test types is defined in **Recommendation: Testing Shared**.
- Add architecture tests to every solution that follows modular monolith and feature-slice guidance.
- Use NetArchTest for dependency and namespace-level rules.
- Run architecture tests in CI on every pull request.
- Keep architecture tests independent from implementation details such as method bodies or algorithm behavior.
- Prefer one shared architecture test project when all modules follow the same policy; use per-module architecture test projects when module policies intentionally differ.
- Require a marker type in every production assembly under test (for example `AssemblyReference.cs`) so architecture tests can resolve assemblies without brittle string-based lookup.

## What to Validate

### 1. Layer dependency direction

For each module, enforce the direction established by ADR 0005 and ADR 0009:

- Domain-facing code must not depend on outer delivery or persistence concerns.
- API and data adapter projects must not leak their frameworks into domain-centric code.
- Module boundary contracts must be consumed through the module's public Abstractions surface.

### 2. Forbidden dependencies

Define explicit deny-lists for layers that must remain framework-agnostic, for example:

- `Microsoft.EntityFrameworkCore` forbidden in domain-centric assemblies.
- ASP.NET types forbidden in domain and handler-centric assemblies.
- Infrastructure SDKs forbidden outside adapter/data projects.

### 3. Module boundary integrity

Enforce that:

- Modules do not reference another module's internal implementation assembly.
- Cross-module collaboration uses Abstractions contracts or integration events.
- Shared kernel libraries do not introduce reverse dependencies into concrete modules.

## Project Placement

Recommended options:

- Shared policy project: `tests/ArchitectureTests`
- Module-specific policy project: `tests/{Module}.ArchitectureTests`

General test placement policy: keep test projects under the repository root `tests/` folder (Unit, Integration, Architecture, and E2E) and keep production code projects under `src/`.

Each production project validated by architecture tests should include a marker file like `AssemblyReference.cs`:

```csharp
namespace Company.Product.Orders;

public sealed class AssemblyReference;
```

Use the shared project when the same rules apply everywhere. Split by module only when a module has intentionally different constraints and that exception is documented.

## Test Design Principles

- Write tests as policy statements, not implementation assertions.
- Keep rules stable and descriptive to reduce churn from refactors.
- Name tests by policy intent, for example: `Orders_Module_Should_Not_Depend_On_Profiles_Module_Implementation`.
- Emit failed type names in assertion messages to speed up remediation.

## Example (NetArchTest)

```csharp
using NetArchTest.Rules;
using Xunit;

public sealed class ArchitectureTests
{
    private const string EfCore = "Microsoft.EntityFrameworkCore";

    [Fact]
    public void Domain_Should_Not_Depend_On_EfCore()
    {
        var result = Types
            .InAssembly(typeof(Orders.Domain.AssemblyReference).Assembly)
            .ShouldNot()
            .HaveDependencyOn(EfCore)
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain must remain persistence-agnostic.");
    }

    [Fact]
    public void Api_Should_Not_Depend_On_Other_Module_Implementation()
    {
        var result = Types
            .InAssembly(typeof(Orders.Api.Program).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Company.Product.Profiles")
            .GetResult();

        Assert.True(result.IsSuccessful, "Cross-module calls should target Abstractions contracts.");
    }
}
```

Adapt assembly markers and namespace prefixes to your solution naming convention.

## CI Enforcement

- Execute architecture tests with `dotnet test` as part of pull-request validation.
- Treat architecture test failures as blocking.
- Keep rule additions backward-compatible unless a new ADR explicitly changes boundary policy.

## When to Change Rules

Change architecture test rules only when:

- A new ADR changes structural policy.
- A module receives a documented exception.
- A technology migration changes the forbidden dependency set.

Rule changes should be made in the same pull request as the architecture decision update.

## References

- ADR 0005: Modular Monolith Project Structure
- ADR 0006: Recommendation to Implement CQRS for ASP.NET API Projects
- ADR 0008: Adopt Vertical Slice Architecture for Feature Organization
- ADR 0009: Feature Slices Within Module Projects
- Recommendation: Testing Shared
- Recommendation: Unit Testing with xUnit, Moq and Bogus
- Structure: Feature Slices Module Structure
- NetArchTest: <https://github.com/BenMorris/NetArchTest>
