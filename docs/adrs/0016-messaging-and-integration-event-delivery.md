---
title: "ADR 0016: Messaging and Integration-Event Delivery"
date: 2026-06-04
status: Accepted
tags: [messaging, integration-events, outbox, inbox, idempotency, modular-monolith, adr]
---
# ADR 0016: Messaging and Integration-Event Delivery

## Context

The modular-monolith guidance allows cross-module collaboration through abstractions and integration events, but it does not yet define:

1. When to choose synchronous calls versus integration events.
2. Who owns event contracts.
3. What delivery guarantees are expected.
4. How to handle duplicate messages and partial failures.

Without that guidance, teams may overuse synchronous coupling or publish events unreliably.

## Decision

We ADOPT **integration events for cross-module asynchronous workflows** and require **outbox + idempotent consumption** whenever durable delivery matters.

### 1. Choose the collaboration style intentionally

Use direct abstractions-based calls when:

- The caller needs an immediate response.
- The dependency is required to complete the use case.
- The collaboration stays within an acceptable synchronous coupling boundary.

Use integration events when:

- The work is asynchronous or eventual by nature.
- Multiple consumers may react independently.
- Failure of one downstream step should not roll back the initiating module's local transaction.

### 2. Event contracts are public, minimal, and versioned

Integration-event contracts live in a shared contract surface, typically:

- A shared integration-events project, or
- A module abstractions project when the module is the authoritative publisher

Rules:

- Events describe facts that already happened.
- Events carry only the data needed by consumers.
- Event names use past tense (`OrderSubmittedEvent`, `ConferencePublishedEvent`).
- Breaking changes require a new contract version or a superseding event.

### 3. Durable publishing uses the outbox pattern

When an event must not be lost, the publishing module writes the domain state change and the pending integration event in the same local transaction. A background dispatcher then publishes from the outbox.

This is the default for:

- Cross-module business workflows
- External broker publication
- Auditable state transitions

### 4. Consumers are idempotent

Handlers of integration events must tolerate duplicate delivery.

Required approaches:

- Record processed message IDs, or
- Use natural/business idempotency keys, or
- Make the state transition itself idempotent

Exactly-once delivery is not assumed. The system is designed for at-least-once delivery.

### 5. Failed handlers are retried at the messaging boundary

Transient failures in message processing are retried by the consumer adapter or worker boundary, not by domain code. Poison-message handling must move failing messages to an operator-visible dead-letter path.

## Consequences

### Positive

1. **Lower coupling**: Modules can collaborate without direct implementation references.
2. **Safer delivery**: The outbox pattern prevents lost events during transaction boundaries.
3. **Operational clarity**: Duplicate delivery and dead-letter scenarios are treated as expected realities.
4. **Better extraction readiness**: The same event patterns remain valid if a module later becomes a separate service.

### Negative

1. **More moving parts**: Durable messaging introduces outbox tables, dispatchers, and dead-letter handling.
2. **Eventual consistency**: Consumers observe changes asynchronously, not instantly.
3. **Versioning overhead**: Event contracts need deliberate compatibility management.

## References

- ADR 0005: Modular Monolith Project Structure
- ADR 0009: Feature Slices Within Module Projects
- Design: Modular Monolith Architecture Design
