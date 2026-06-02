---
title: "End-to-End Testing"
date: 2026-06-01
status: Accepted
tags: [testing, e2e, quality, web-api, recommendations]
---
## Recommendation: End-to-End Testing

## Purpose

Define how to validate complete user and system journeys with one shared end-to-end test project per solution.

## Recommendation

- Use one E2E test project per solution: `tests/Company.Product.E2ETests`.
- Validate critical end-user workflows across API, persistence, messaging, and integrations.
- Keep E2E suites smaller than unit and integration suites; prioritize highest business risk scenarios.
- Run E2E tests against an environment started through Aspire AppHost for topology parity with runtime composition.

## Black-Box vs White-Box Position

- E2E tests in Aspire-hosted environments are **black-box by nature**.
- Assert externally observable outcomes only: API responses, UI behavior, contract-visible state transitions, and emitted events.
- Avoid white-box assertions tied to internal class structure, private state, or implementation-specific call chains.

## Aspire Testing Conventions

- Start the full test environment via Aspire AppHost instead of hand-crafted startup scripts.
- Prefer Aspire testing infrastructure (for example `Aspire.Hosting.Testing` and `DistributedApplicationTestingBuilder`) when bootstrapping test environments in code.
- Reuse the same ServiceDefaults, resource wiring, and dependency graph used by the solution's AppHost.
- Keep E2E suites focused on business journeys while Aspire manages service orchestration and dependency readiness.

## Aspire Monitoring Setup

- Enable Aspire dashboard telemetry for each E2E run (logs, traces, and metrics).
- Configure OpenTelemetry exporters used by the solution so traces and logs are available during test execution.
- Capture browser console logs and page errors from the UI test runner and persist them as CI artifacts.
- Capture browser network failures (failed requests, status codes, timing) and correlate them with server-side traces using correlation/request IDs.
- On failure, publish a diagnostic bundle: screenshots, browser logs, network logs, service logs, and trace references.

## E2E vs Integration Approach

- Use integration tests for deeper boundary validation and faster feedback loops.
- Use E2E tests for high-value cross-system journeys and production-like confidence.
- If a workflow can be validated sufficiently in integration tests, avoid duplicating it in E2E unless it is release-critical.

## Monitoring and Observability Recommendations

- Execute E2E runs with Aspire dashboard telemetry, logs, and traces enabled.
- Capture and publish diagnostics (logs, traces, screenshots, browser console logs, and browser network logs) for failed scenarios in CI.
- Include smoke-level observability checks for critical journeys (for example request trace exists, error signals emitted on failure path).
- Keep observability checks outcome-focused and stable; avoid brittle assertions on full trace trees.

## Scope

E2E tests should cover:

- Primary business journeys (happy path).
- Critical failure journeys (dependency outage, invalid state transition, compensating action).
- Security-sensitive journeys (authentication, authorization, boundary checks).
- Observability smoke checks for critical flows (trace/log/metric emission where required).

## Project Layout

```text
tests/
  Company.Product.E2ETests/
    Journeys/
    Fixtures/
    Environments/
    Helpers/
```

## Test Design Guidelines

- Write scenario-first tests using business language.
- Keep data setup explicit and isolated per scenario.
- Minimize brittle UI-level assertions unless UI is the contract under test.
- Prefer API and contract-level assertions for service-focused systems.

## Execution Strategy

- Execute a smoke subset on pull requests.
- Execute full E2E suites on main branch and release pipelines.
- Capture diagnostics (screenshots, logs, traces) for failed scenarios.

## References

- ADR 0003: Strong Recommendation to Adopt .NET Aspire for ASP.NET Web Services
- Recommendation: Unit testing with xUnit, Moq and Bogus
- Recommendation: Integration Testing
