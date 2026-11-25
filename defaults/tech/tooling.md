---
title: "Tooling Technologies"
date: 2026-11-25
status: Accepted
tags: [tech, ratings, tooling, aspire, testing, xunit, nsubstitute, bogus, testcontainers, netarchtest, playwright, moq]
---
# Tooling Technologies

Orchestration, build and test tooling. Ratings and layers:
[technology-graph.md](technology-graph.md).

## Aspire

- **Rating:** adopted · **Version:** 17.0.0
- **Depends on:** [.NET](shared.md#net)
- **Used for:** the AppHost (`Microsoft.NET.Sdk` with `Aspire.Hosting.AppHost` and
  `Aspire.Hosting.Dotnet`, projects added with `AddDotnetProject`), local orchestration, the
  dashboard, `aspire publish` and `aspire deploy`, and the agent skills from `aspire agent init`.
- **Why:** [ADR 0002](../arc42/adr/0002-aspire-configuration-and-infrastructure.md),
  [ADR 0013](../arc42/adr/0013-agent-ready-repository.md).

## Aspire.AppHost.Sdk and AddProject

- **Rating:** hold
- **Used for:** nothing. Obsolete in Aspire 17; an existing AppHost migrates to
  `Microsoft.NET.Sdk` and `AddDotnetProject`.
- **Why:** [ADR 0002](../arc42/adr/0002-aspire-configuration-and-infrastructure.md).

## xUnit v3 on Microsoft Testing Platform

- **Rating:** adopted · **Version:** 4.0.3 (`xunit.v3`)
- **Depends on:** [.NET](shared.md#net)
- **Used for:** unit, architecture and integration tests.
- **Why:** [ADR 0010](../arc42/adr/0010-testing.md).

## NSubstitute

- **Rating:** adopted · **Version:** 6.2.0
- **Depends on:** [xUnit v3 on Microsoft Testing Platform](#xunit-v3-on-microsoft-testing-platform)
- **Used for:** test doubles in unit tests.
- **Why:** [ADR 0010](../arc42/adr/0010-testing.md).

## Moq

- **Rating:** hold
- **Used for:** nothing in a new app; NSubstitute replaces it.
- **Why:** [ADR 0010](../arc42/adr/0010-testing.md).

## Bogus

- **Rating:** adopted · **Version:** 35.6.5
- **Depends on:** [xUnit v3 on Microsoft Testing Platform](#xunit-v3-on-microsoft-testing-platform)
- **Used for:** test-data builders.
- **Why:** [ADR 0010](../arc42/adr/0010-testing.md).

## FakeTimeProvider

- **Rating:** adopted · **Version:** 11.0.0 (`Microsoft.Extensions.TimeProvider.Testing`)
- **Depends on:** [.NET](shared.md#net)
- **Used for:** controlling time in tests in place of the injected `TimeProvider`.
- **Why:** [ADR 0005](../arc42/adr/0005-application-layer.md), [ADR 0010](../arc42/adr/0010-testing.md).

## Testcontainers

- **Rating:** adopted · **Version:** 4.17.0
- **Depends on:** [xUnit v3 on Microsoft Testing Platform](#xunit-v3-on-microsoft-testing-platform)
- **Used for:** real dependencies in throwaway containers for integration tests on
  `WebApplicationFactory` (`Microsoft.AspNetCore.Mvc.Testing`).
- **Why:** [ADR 0010](../arc42/adr/0010-testing.md).

## NetArchTest

- **Rating:** adopted · **Version:** 1.3.2 (`NetArchTest.Rules`)
- **Depends on:** [xUnit v3 on Microsoft Testing Platform](#xunit-v3-on-microsoft-testing-platform)
- **Used for:** architecture tests that enforce the module and slice boundaries.
- **Why:** [ADR 0003](../arc42/adr/0003-solution-structure.md), [ADR 0010](../arc42/adr/0010-testing.md).

## Playwright Test

- **Rating:** adopted (`@playwright/test`)
- **Depends on:** [Aspire](#aspire)
- **Used for:** the E2E suite, started through `aspire start` detached and isolated.
- **Why:** [ADR 0010](../arc42/adr/0010-testing.md), which also lists what is on watch.
