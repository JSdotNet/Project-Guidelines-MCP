---
title: "ADR 0006: Persistence and Messaging"
date: 2026-11-19
status: Accepted
tags: [persistence, ef-core, migrations, concurrency, messaging, integration-events, outbox, idempotency, adr]
---
# ADR 0006: Persistence and Messaging

## Context

Modules must own their data and collaborate without shared tables. Mixing data-access
libraries, running migrations from app startup, and publishing events outside the database
transaction each lead to drift, lost updates or lost messages. The database and broker an app
needs depend on the app, so the defaults fix the patterns, not the products.

## Decision

**Persistence.**

- EF Core is the only data-access library. It lives only in `{Module}.Data.{Provider}`
  projects (ADR 0003); domain and API projects do not reference it, and mapping is configured
  outside the entities.
- Each module owns its schema, migrations and repositories, and never touches another
  module's tables.
- Each app chooses its database and records the choice in its own ADR; Aspire hosts it
  locally (ADR 0002).
- Every aggregate has a concurrency token. A stale write returns 409 Problem Details
  (ADR 0004), and clients send back the version they read.
- Migrations run through a migration-runner resource in the AppHost that the API waits for;
  the API never migrates on startup.
- Roll back a deployment only while the schema is unchanged. After a migration has run, fix
  forward.

**Messaging** is fixed as patterns only:

- Modules publish integration events: past-tense, minimal, versioned contracts.
- A transactional outbox writes the event in the same transaction as the state change, and a
  dispatcher publishes it afterwards.
- Delivery is at-least-once; every consumer is idempotent.
- No broker or messaging package is named here. Each app chooses them in its own ADR.

## Consequences

### Positive

- One data-access stack and one migration path across apps.
- Lost updates and lost events are prevented by design rather than by care.
- Apps pick the database and broker that fit, without leaving the defaults.

### Negative

- EF Core-only rules out hand-tuned SQL libraries; a hot path uses EF Core raw SQL instead.
- An outbox table, dispatcher and idempotency store per publishing or consuming module.
- Fix-forward needs migrations that are safe to run before the code that uses them.

## References

- [EF Core concurrency conflicts](https://learn.microsoft.com/ef/core/saving/concurrency)
- [Transactional outbox pattern](https://learn.microsoft.com/azure/architecture/databases/guide/transactional-outbox-cosmos)
- ADR 0002: Aspire, Configuration and Infrastructure
- ADR 0003: Solution Structure
- ADR 0004: HTTP API

Replaces: ADR 0014 (Persistence Strategy and Repository Boundaries), ADR 0016 (Messaging and Integration-Event Delivery).
