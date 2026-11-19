---
title: "ADR 0010: Testing"
date: 2026-11-19
status: Accepted
tags: [testing, xunit, microsoft-testing-platform, nsubstitute, bogus, integration-tests, webapplicationfactory, testcontainers, e2e, playwright, aspire, architecture-tests, adr]
---
# ADR 0010: Testing

## Context

The guidance offered four testing profiles, Moq and NSubstitute side by side, and integration
tests that started the whole AppHost. Starting every resource for a test of one module is slow
and flaky, while mocking a database hides the bugs integration tests exist to find. End-to-end
tests that hard-code a port collide when two worktrees run at once.

## Decision

**One test set per app**, all under the root `tests/` folder (ADR 0003): unit, architecture
and integration tests always, and E2E tests when the app has a UI.

**Unit and architecture tests.** xUnit v3 on Microsoft Testing Platform. NSubstitute for test
doubles and Bogus builders for test data; Moq is not used. `FakeTimeProvider` stands in for
time. Architecture tests enforce the module and slice boundaries of ADR 0003.

**Integration tests** host one module's Api in-process with `WebApplicationFactory`:

- Real dependencies (database, cache, broker) run in throwaway containers per test run.
- External services the app does not own are replaced by `Fake*` adapters; each also has a
  harness project the AppHost can start in its place for local runs.
- They never start the AppHost.

**E2E tests are the only tests that run through Aspire.** A Playwright (`@playwright/test`)
suite whose global setup:

1. runs `aspire start` detached and isolated, so each worktree gets its own ports;
2. reads the frontend URL Aspire assigned, never a hard-coded port;
3. after the run, fails on errors in `aspire otel logs` and in the browser logs each frontend
   sends through `.WithBrowserLogs()` (ADR 0002).

Devbook journeys reuse this suite rather than a second one.

## Consequences

### Positive

- Unit and integration tests stay fast and run without Aspire or a UI.
- Integration tests find real database and serialization bugs, not mock behaviour.
- E2E runs from parallel worktrees do not collide, and a server-side error fails the run even
  when the page looks fine.

### Negative

- Integration tests need a container runtime on every developer machine and CI agent.
- Every external service needs a `Fake*` adapter and a harness project.
- E2E tests are slower and depend on the Aspire CLI and a browser install.

## References

- [xUnit v3 with Microsoft Testing Platform](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
- [Integration tests in ASP.NET Core](https://learn.microsoft.com/aspnet/core/test/integration-tests)
- [Playwright Test](https://playwright.dev/docs/intro)
- [Aspire CLI](https://aspire.dev/reference/cli/overview/)
- ADR 0002: Aspire, Configuration and Infrastructure
- ADR 0003: Solution Structure

Replaces: none; new in this set.
