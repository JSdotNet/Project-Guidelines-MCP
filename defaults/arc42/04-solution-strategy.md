---
title: "04 Solution Strategy"
date: 2026-11-19
status: Accepted
tags: [arc42, solution-strategy, modular-monolith, feature-slices, cqrs, ddd, domain-model]
---
# 04 Solution Strategy

One page on how a JSdotNet application meets its goals. The parts are drawn in the
[building-block view](05-building-block-view.md); the decisions behind them are the ADRs cited.

| Goal | Approach | Source |
|---|---|---|
| Every app reads the same way | One shape: a modular monolith with feature slices per module; a small app has one module | ADR 0003 |
| A change touches one place | One folder per use case: endpoint, command or query, handler and types together | ADR 0003, 0004 |
| Business rules cannot be bypassed | A rich domain model guards its invariants; handlers orchestrate and never decide | ADR 0005, [domain model](building-blocks/domain-model.md) |
| Modules evolve apart | A module owns its data and contract; others reach it only through `Abstractions` or integration events | ADR 0003, 0006 |
| Expected outcomes are explicit | Handlers return `Result<T>`, matched exhaustively at the edge into Problem Details | ADR 0004, 0005 |
| One model from laptop to Azure | The AppHost models every resource; ServiceDefaults wires telemetry, health and resilience | ADR 0002, 0007 |

## Pragmatic domain-driven design

DDD is applied where it protects correctness, and left out where it only adds ceremony.

- **Under-applied**, logic scatters into services and handlers, and an invalid state can be
  saved. **Over-applied**, every primitive becomes a value object and the code drowns in
  wrappers. The default sits between: rich entities, few value objects.
- **The domain model is the source of truth.** Every state change goes through a method on
  an aggregate root that checks the rule and returns a `Result`. Nothing sets a property from
  outside.
- **Handlers orchestrate.** A command handler loads the aggregate, calls one domain method,
  saves, and maps the outcome. It holds no business rule.
- **Two validation layers.** Transport shape (required, range, format) is checked at the edge
  by the built-in minimal-API validation (ADR 0004); invariants and state rules live in the
  domain. FluentValidation is not used.
- **Value objects only when they earn it**: several fields that belong together, or a rule
  that spans them. A single `Guid`, name or number stays a primitive.
- **Consistency stops at the module.** One transaction changes one module; a workflow across
  modules is eventual, through integration events from the outbox (ADR 0006).

The rules, patterns and examples are in [domain model](building-blocks/domain-model.md).

## Moving an existing anemic model

1. Find the business rules that live in services and handlers.
2. Move each into a method on the entity that owns the state, returning `Result`.
3. Add a `Create` factory where construction needs validation.
4. Make setters private and expose collections as `IReadOnlyList<T>`.
5. Reduce the handlers to load, call, save; add unit tests on the entity.

## Evolution

A module that outgrows the monolith is extracted in order: stabilize its `Abstractions`,
replace in-process calls with integration events, isolate its database and migrations, then
deploy its Api on its own. The module layout does not change, so extraction is a move, not a
rewrite.
