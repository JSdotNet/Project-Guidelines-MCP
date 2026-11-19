---
title: "ADR 0003: Solution Structure"
date: 2026-11-19
status: Accepted
tags: [modular-monolith, feature-slices, vertical-slices, solution-structure, slnx, tests, modules, adr]
---
# ADR 0003: Solution Structure

## Context

The guidance offered a simple and a modular solution shape, two slice layouts (ADR 0008 and
0009) that were both Accepted, `.sln` and `.slnx`, and module-local test projects next to a
root `tests/` folder. Each choice is a variation every reader has to resolve, and code moved
between apps does not fit.

## Decision

**One shape: a modular monolith with feature slices per module.**

- A module owns a business capability, its data and its public contract. Modules talk only
  through those contracts or integration events (ADR 0006), never each other's internals.
- Inside a module, code is organized by feature slice: one folder per use case holding its
  endpoint, command or query, handler and types. Shared technical layers per module stay thin.
- A small app is the same shape with one module. There is no second, "simple" profile.
- A module's data access lives in `{Module}.Data.{Provider}` (for example
  `Contoso.Orders.Data.Postgres`). The provider is the app's choice, recorded in its own ADR.

**Solution files.** `.slnx` only. Tests live under the root `tests/` folder only, one test
project per production project it covers; no test project sits inside `src/`.

## Consequences

### Positive

- Every app reads the same way, and a module can grow or be extracted without a re-layout.
- A feature change touches one slice folder instead of several layers.
- Module boundaries make ownership of data and contracts explicit.

### Negative

- A one-module app carries some structure it does not yet need.
- Boundaries between modules need review discipline; architecture tests enforce them.

## References

- [Solution files (.slnx)](https://learn.microsoft.com/visualstudio/ide/reference/solution-file)
- ADR 0005: Application Layer
- ADR 0006: Persistence and Messaging

Replaces: ADR 0005 (Modular Monolith Project Structure), ADR 0008 (Vertical Slice Architecture), ADR 0009 (Feature Slices Within Module Projects).
