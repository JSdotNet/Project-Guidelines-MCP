---
title: "Integration Testing"
date: 2026-06-22
status: Accepted
tags: [testing, integration-testing, aspnet, recommendations]
---
## Recommendation: Integration Testing

## Purpose

Define a consistent approach for integration testing in .NET solutions using one shared integration test project per solution.

## Recommendation

- Shared guidance for all test types is defined in **Recommendation: Testing Shared Instructions**.
- Prefer one shared integration test project per solution: `tests/Company.Product.IntegrationTests`.
- Cover service collaboration, persistence adapters, messaging adapters, and HTTP boundaries.
- Keep test setup close to runtime wiring (database, message broker, cache, external service stubs).
- Prefer realistic infrastructure via containers (for example Testcontainers) over deep mocking.
- Keep integration tests independent from unit-test factories unless those factories are infrastructure-agnostic.

For larger projects, module-specific integration test projects are allowed in addition to the shared project, for example:

- `tests/Company.Product.{Module}.IntegrationTests` for module-local adapter and API behavior.
- `tests/Company.Product.IntegrationTests` for cross-module flows and shared integration scenarios.

## Black-Box vs White-Box Position

- Integration tests are primarily **gray-box**: validate observable behavior at component boundaries while using knowledge of internal contracts and data models.
- Use **black-box assertions** for API outputs, persisted state, and emitted messages.
- Use selective **white-box setup** when needed (for example seed data, inspect storage, assert transactional effects), but avoid asserting private implementation details.

## Mocking Policy

- Only mock **external dependencies** outside your delivery boundary (third-party APIs, SaaS systems, unrecoverable external services).
- Do not mock core internal collaborators under test (repository adapters, message handlers, persistence, DI wiring) when they are part of the integration boundary.
- Prefer containerized/local substitutes over hand-written mocks for infrastructure your system owns.

## Scope

Integration tests should validate:

- API-to-handler-to-adapter flow for representative scenarios.
- Persistence behavior with real schema migrations.
- Cross-component collaboration through contracts and integration events.
- Failure behavior for retries, timeouts, and idempotency where relevant.

## Integration vs E2E Approach

- Integration tests provide faster, boundary-focused confidence for service internals and contracts.
- E2E tests validate complete business journeys across the full runtime topology.
- Keep most coverage in integration tests; reserve E2E for critical end-to-end journeys and release confidence.

## Project Layout

```text
tests/
  Company.Product.IntegrationTests/
    Api/
    Persistence/
    Messaging/
    CrossModule/
    Fixtures/
    Helpers/
```

## Test Design Guidelines

- Name tests by behavior and boundary, for example `CreateOrder_ShouldPersistAndPublishEvent_WhenRequestIsValid`.
- Use dedicated test fixtures for infrastructure startup and teardown.
- Keep assertions focused on externally observable behavior (HTTP response, persisted state, emitted event).
- Avoid asserting internal implementation details.

## CI Guidance

- Run integration tests after unit tests in CI.
- Mark integration tests as required for pull requests touching adapters, API contracts, or persistence.
- Publish logs and test artifacts on failure for diagnosis.

## References

- ADR 0003: Strong Recommendation to Adopt .NET Aspire for ASP.NET Web Services
- Recommendation: Testing Shared Instructions
- Recommendation: Unit testing with xUnit, Moq and Bogus
- Recommendation: End-to-End Testing
- Recommendation: Architecture Testing for Layer and Module Boundaries
