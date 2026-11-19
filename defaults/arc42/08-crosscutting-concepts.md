---
title: "08 Crosscutting Concepts"
date: 2026-11-19
status: Accepted
tags: [arc42, crosscutting-concepts, caching, feature-flags, idempotency, api-versioning, logging, audit, background-jobs, testing, code-style, domain, time, concurrency, real-time, secrets, validation]
---
# 08 Crosscutting Concepts

The rules every module follows for concerns that cut across the
[building blocks](05-building-block-view.md). One answer per concern; each section links the
ADR that decides it. A project's own `08-crosscutting-concepts.md` links this page and adds
only concepts of its own.

## Caching

Decided by [ADR 0006](adr/0006-persistence-and-messaging.md) (the source of truth) and
[ADR 0007](adr/0007-observability-and-resilience.md) (the metrics).

- Cache only for a named read-performance or cost goal. **Cache-aside** is the pattern.
- The cache lives in an adapter or a dedicated read service, never in a handler or an entity.
- Cache read models, reference data, expensive external lookups and computed views. Never
  cache write-side aggregate state, an authorization decision, or data whose invalidation has
  no owner.
- Keys are explicit and versioned; every entry has a bounded TTL.
- Define invalidation before adding the entry: evict on the integration event when the module
  already emits one, otherwise a short TTL plus eviction on known writes.
- Record hit and miss metrics. Cache content is disposable; the backing store stays the truth.

## Feature flags

Decided by [ADR 0002](adr/0002-aspire-configuration-and-infrastructure.md) (flags are
validated options) and [ADR 0007](adr/0007-observability-and-resilience.md) (a risky rollout
is watched).

- A flag hides incomplete or high-risk work that has to merge before it is released; it is not
  a configuration switch.
- Every flag has an owner and a removal point, and is deleted once the feature is released.
- Prefer one coarse release flag per feature over scattered low-level toggles; never nest them.
- A flag never bypasses a security or data-integrity rule.
- A flagged rollout is paired with telemetry and a rollback path ([07](07-deployment-view.md)).

## Idempotency

Decided by [ADR 0006](adr/0006-persistence-and-messaging.md) (at-least-once delivery,
idempotent consumers) and [ADR 0004](adr/0004-http-api.md) (the command endpoints).

- A command endpoint with an expensive or externally visible side effect is idempotent:
  payments, registrations and invitations, webhook-triggered commands, and any endpoint a
  browser or mobile client retries.
- Use an explicit idempotency key or a naturally unique business key, and store enough of the
  outcome to return the same result for a duplicate.
- Duplicate detection survives a restart, and sits at the application boundary, never only in
  the UI.
- Every message consumer and background job is idempotent.
- No retry is added to a command that is not idempotent yet.

## API versioning

Decided by [ADR 0004](adr/0004-http-api.md).

- Evolve the contract additively within one version: new fields are optional, request and
  response types are explicit.
- Remove or rename a field only through a documented deprecation; deprecate before removing.
- A new version only when a change cannot stay compatible (behaviour or meaning changes); the
  old version stays supported for a documented transition period.
- The Problem Details shape is the same in every version. A solution uses one versioning
  scheme throughout.
- Generated clients ([ADR 0004](adr/0004-http-api.md)) are regenerated in the change that
  alters the OpenAPI document.

## Logging and audit

Decided by [ADR 0007](adr/0007-observability-and-resilience.md) (diagnostic logs) and
[ADR 0008](adr/0008-security.md) (audit records).

- **Diagnostic logs** are structured `[LoggerMessage]` source-generated methods, never string
  interpolation. They carry the module and feature, the trace id, the failure reason with its
  dependency, and the state changes support needs.
- **Audit records** are not log events. Every write and every denied attempt writes one with
  who (user, and agent when one acts for the user), what, which resource, the outcome, when,
  and the trace id. They are stored and queried apart from diagnostic logs.
- Secrets, tokens and sensitive personal data are redacted from both.

## Background jobs

Decided by [ADR 0006](adr/0006-persistence-and-messaging.md) (outbox, idempotent consumers),
[ADR 0007](adr/0007-observability-and-resilience.md) (telemetry, resilience) and
[ADR 0008](adr/0008-security.md) (worker identity).

- Long-lived work is a `BackgroundService` hosted in the module's Api; scheduling and
  transport stay in that host or its adapter. A job orchestrates and holds no domain rule.
- Work that must survive a restart goes through the outbox or a durable queue, never an
  in-memory list.
- Every job is idempotent and honours its `CancellationToken` end to end; a loop always has a
  delay or backoff.
- Retries live at the job runner or the adapter; repeated failures go to a dead-letter or
  another operator-visible path. Payloads are small and explicit.
- Each job emits logs, traces and metrics, and runs under its own narrowly scoped identity.

## Validation

Decided by [ADR 0004](adr/0004-http-api.md).

- **At the edge**, the built-in minimal-API validation (`AddValidation()`) checks transport
  shape: required fields, format, simple ranges. A failure is a 400 Problem Details with an
  `errors` extension.
- **In the domain**, factories and behaviour methods enforce invariants and state transitions
  and return `Result` ([domain model](building-blocks/domain-model.md)). Edge validation never
  stands in for them.
- FluentValidation is not used.

## Testing

Decided by [ADR 0010](adr/0010-testing.md); the harness services are a
[building block](building-blocks/test-harness.md).

| Type | Proves | Runs against |
|---|---|---|
| Unit | One behaviour in isolation | Concrete domain objects; NSubstitute only for ports |
| Architecture | Module and slice boundaries (ADR 0003) | Compiled assemblies |
| Integration | One module's Api, adapters and persistence | `WebApplicationFactory` with real dependencies in throwaway containers, `Fake*` adapters for services the app does not own |
| E2E (with a UI) | The critical business journeys | The app started by `aspire start`, driven by Playwright |

- **Layout.** All under the root `tests/`: `Company.Product.{Module}.UnitTests`,
  `Company.Product.ArchitectureTests`, `Company.Product.IntegrationTests` and
  `Company.Product.E2ETests`. A module gets its own integration or architecture project only
  when its policy differs, and says why.
- **Stack.** xUnit v3 on Microsoft Testing Platform, NSubstitute, Bogus, `FakeTimeProvider`.
  Another test library needs an ADR first.
- **Shape.** Every test marks `// Arrange`, `// Act`, `// Assert`, with one act. Names state
  behaviour: `Should_ExpectedBehavior_When_State` for unit tests,
  `Scenario_ShouldOutcome_WhenCondition` for integration tests, and
  `ModuleOrLayer_Should_EnforcePolicy` for architecture tests.
- **Data.** Builders and factories create test objects; a test never news up an aggregate a
  builder exists for. Bogus is seeded wherever a test asserts on a generated value.
- **Unit.** Test-first. Each behaviour gets a happy path, a critical failure and an edge case.
  `[Theory]` covers variations. Prefer real domain objects over substitutes.
- **Architecture.** NetArchTest rules written as policy: domain code references neither EF Core
  nor ASP.NET Core; modules reach each other only through `Abstractions` or integration events;
  the shared kernel depends on no module. Every assembly under test has an `AssemblyReference`
  marker type. A rule changes in the same pull request as the ADR that changes the policy.
- **Integration.** Gray-box: assert on the HTTP response, the persisted state and the emitted
  event, never on private detail. Migrations run for real. Only services outside the app are
  faked.
- **E2E.** Black-box and few: the primary journeys, the critical failures and the security
  journeys. The suite reads the URL Aspire assigned and fails on errors in `aspire otel logs`
  and the browser logs. On failure it keeps screenshots, browser and network logs, and trace
  ids.
- **CI.** Cheapest first (unit, architecture, integration, then E2E); every type blocks a
  merge.

## Code style and domain code

Decided by [ADR 0001](adr/0001-platform-and-build.md) (one `.editorconfig`, style enforced in
the build) and [ADR 0005](adr/0005-application-layer.md) (handlers and the domain). The
aggregate shape is in [domain model](building-blocks/domain-model.md).

**All C#** follows the
[Microsoft C# coding conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions),
plus:

- File-scoped namespaces, `using` directives outside them, one type per file, files organized
  by feature.
- Classes are `sealed` unless inheritance is designed.
- Guard clauses at the top of public methods (`ArgumentNullException.ThrowIfNull`), with
  `nameof` for parameter names.
- `var` when the type is obvious from the right-hand side.
- `async` all the way; never `.Result` or `.Wait()`.
- Full names, no abbreviations. Comments say why, not what. No commented-out code.

**Domain code** (aggregates, entities, value objects, domain services) also keeps these rules;
handlers follow them where they fit, while DTOs, configuration and infrastructure are exempt:

- One level of indentation per method, and no `else`: return early instead.
- State changes only through behaviour methods; properties have `private set` or `init`.
- A collection that carries behaviour is a first-class collection type; collections are
  exposed as `IReadOnlyList<T>`.
- A value object wraps a primitive only when it carries a rule or groups fields; a lone `Guid`,
  name or number stays a primitive ([04](04-solution-strategy.md)).
- Small classes: about ten methods and fifty lines. At most two injected collaborators besides
  the logger; more means a missing facade.
- One dot per line in a chain that walks across objects.
- A **specification** (`ISpecification<T>`) is used only for a rule with several conditions, a
  rule shared between aggregates, or a rule that is composed with `And`/`Or`/`Not`. A simple
  guard stays inline. A specification is named in the ubiquitous language, lives in the feature
  folder of the aggregate that uses it, and is tested on its own.

## Time

Decided by [ADR 0005](adr/0005-application-layer.md).

- Inject `TimeProvider` wherever code needs the current time; `DateTime.Now` and
  `DateTime.UtcNow` are not used.
- Store and compare instants as `DateTimeOffset` in UTC; convert to a time zone only at the
  edge.
- Delays and timers take the `TimeProvider` too, so tests drive them with `FakeTimeProvider`.

## Concurrency

Decided by [ADR 0006](adr/0006-persistence-and-messaging.md) (the token) and
[ADR 0004](adr/0004-http-api.md) (the 409).

- Every aggregate has a concurrency token, configured in its `Data.{Provider}` project.
- Clients read the version with the resource and send it back with every change.
- A stale write returns 409 Problem Details; the client re-reads and retries or asks the user.
  The server never merges silently or lets the last write win.
- A workflow across modules is eventual through integration events, never a distributed lock.

## Real-time updates

Decided by [ADR 0004](adr/0004-http-api.md) (the client re-reads through the generated API
client) and [ADR 0002](adr/0002-aspire-configuration-and-infrastructure.md) (the transport is
an Aspire resource). The same nudge drives push [notifications](building-blocks/notifications.md).

- **The server pushes a nudge; the client re-reads.** A nudge names what changed (kind and id)
  and nothing more. The client fetches the new state through the normal API, so authorization,
  versioning and concurrency apply exactly as on any other read.
- **The push never carries the data.** A lost or duplicate nudge costs one extra read, never a
  wrong screen.
- The nudge is sent after the change commits, from the outbox ([ADR 0006](adr/0006-persistence-and-messaging.md)).
- A client that reconnects re-reads what it shows, because nudges sent while it was away are
  gone.
- The transport (SignalR, Web PubSub, server-sent events) is the app's choice, recorded in its
  own ADR and hosted through the AppHost.

## Secrets at rest

Decided by [ADR 0002](adr/0002-aspire-configuration-and-infrastructure.md) and
[ADR 0008](adr/0008-security.md).

- Managed identity wherever the target supports it, so there is no secret to keep.
- Locally, Aspire parameters and user secrets are the source; deployed, secrets are set per
  environment with `aspire secret set --environment` and land in Key Vault through
  `aspire deploy` ([07](07-deployment-view.md)). Nothing secret is committed.
- A token the app must keep (a refresh token, a third-party credential) is protected with Data
  Protection before it is stored.
- An API token the app issues is stored only as a hash and shown once.
- Secrets never appear in a log, a trace, an audit record or a Problem Details response.
