---
title: "ADR 0014: Persistence Strategy and Repository Boundaries"
date: 2026-06-04
status: Accepted
tags: [persistence, repositories, ef-core, modular-monolith, data, adr]
---
# ADR 0014: Persistence Strategy and Repository Boundaries

## Context

The current guidance defines module boundaries, CQRS, feature slices, and modular-monolith structure, but it does not yet define how persistence should be implemented inside those boundaries. Teams still need a shared answer for:

1. Whether Entity Framework Core is allowed and where it belongs.
2. How repositories relate to aggregates and query models.
3. Who owns schema, migrations, and database access within a module.
4. How to prevent persistence concerns from leaking into domain code.

Without an explicit persistence ADR, projects will drift into active-record patterns, cross-module table access, and inconsistent migration ownership.

## Decision

We ADOPT a persistence model built on **module-owned data access** with **EF Core allowed only in adapter/data projects**.

### 1. Persistence belongs to the module that owns the data

Each module owns:

- Its database schema objects or schema segment.
- Its migrations.
- Its repository implementations.
- Its read-model/query access patterns.

Other modules do not read or write another module's tables directly. Cross-module collaboration uses abstractions or integration events.

### 2. EF Core stays in data adapter projects

EF Core is permitted in projects such as:

- `Company.Product.{Module}.Data.SqlServer`
- `Company.Product.{Module}.Data.PostgreSql`

EF Core is not allowed in:

- Domain-centric module projects
- Abstractions projects
- API projects

Domain entities remain free of EF Core attributes and base types. Mapping is configured externally in the data adapter.

### 3. Repositories are aggregate-focused write ports

Repositories model the write side of the application:

- One repository per aggregate root or aggregate cluster where practical.
- Repository interfaces live in the module implementation project near the domain/application code that consumes them.
- Repository implementations live in the data adapter project.

Repository responsibilities:

- Load aggregates required for command handling.
- Persist aggregate state changes.
- Enforce aggregate-oriented transaction boundaries.

Repositories should not become general-purpose query services.

### 4. Queries may use dedicated read models

Read paths may bypass aggregate reconstruction when business invariants are not being enforced. Query handlers may use:

- EF Core projections
- Dapper
- Plain SQL
- Database views

This is allowed only inside the owning module and only for read concerns. Query models are optimized for retrieval, not used as domain entities.

### 5. Migrations are owned and reviewed per module

Every module that owns relational storage owns its migrations. Migration files are stored with the module's data adapter project.

Rules:

- Migrations are created in the same pull request as the model change that requires them.
- Startup-time automatic destructive migration is prohibited in production.
- Cross-module migrations are prohibited unless an explicit superseding ADR allows them.

### 6. Schema-per-module is the default for modular monoliths

For relational modular monoliths, the default persistence shape is:

- One shared database instance when operationally convenient.
- One logical schema per module where the database engine supports it.
- Strict ownership rules even when the physical database is shared.

Database-per-module is allowed when extraction readiness or operational constraints justify it.

## Consequences

### Positive

1. **Clear ownership**: Data access and schema changes are owned by the module that owns the business capability.
2. **Domain purity**: Domain code stays persistence-agnostic.
3. **Pragmatic CQRS**: Read models can be optimized without compromising write-side consistency.
4. **Better extraction readiness**: Modules are less coupled through shared tables and shared ORM models.

### Negative

1. **More explicit mapping**: Adapter projects carry additional mapping/configuration work.
2. **Less convenience for shared reporting**: Cross-module queries need deliberate integration or reporting patterns.
3. **Potential duplicate projections**: Read models may overlap when different modules need different views.

## References

- ADR 0005: Modular Monolith Project Structure
- ADR 0006: Recommendation to Implement CQRS for ASP.NET API Projects
- ADR 0008: Adopt Vertical Slice Architecture for Feature Organization
- ADR 0009: Feature Slices Within Module Projects
- Design: Modular Monolith Architecture Design
