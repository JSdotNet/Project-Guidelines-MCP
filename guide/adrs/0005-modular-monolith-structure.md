---
title: "ADR 0005: Modular Monolith Project Structure"
date: 2025-11-10
status: Accepted
tags: [architecture, modular-monolith, adr, design, modularity, hexagonal]
---
# ADR 0005: Modular Monolith Project Structure

## Context

The repository aims to provide guidance for .NET 10 C# solutions employing Hexagonal / Clean Architecture while remaining maintainable and evolvable. Many teams start with a single project or a coarse set of layered projects (Domain, Application, Infrastructure). As functionality grows, unrelated domains/features can become tightly coupled, increasing build times, merge conflicts, and cognitive load. A full microservice split may be premature, introducing deployment, observability, and transactional complexity. We require a structure that enforces boundaries, supports independent evolution of domain modules, enables targeted testing, and keeps operational simplicity.

## Decision

Adopt a Modular Monolith physical project structure. Each domain/feature/module resides in its own folder and set of projects forming an internal boundary.

### Top-Level `src/` Organization

The `src/` directory is organized into the following top-level folders:

| Folder | Purpose |
|---|---|
| `App/` | Frontend application (Angular, Blazor, React, etc.) |
| `Aspire/` | .NET Aspire orchestration projects (see ADR 0003) |
| `Core/` | Shared CQRS interfaces and cross-cutting abstractions used by all modules |
| `{ModuleName}/` | One folder per domain module / bounded context (short name, e.g., `Game`, `Conferences`) |
| `IntegrationMessages/` or `Shared/` | Cross-module integration event contracts (optional, if inter-module events exist) |

The `Core/` folder contains the solution-wide shared CQRS interfaces (`ICommandHandler<TCommand, TResult>`, `IQueryHandler<TQuery, TResult>`, `IClock`, etc.) that all modules use. This avoids duplicating the same interfaces in every module.

### Module Folder Structure

Each module folder (e.g., `src/Conferences/`) contains the module's projects:

| Project | Purpose |
|---|---|
| `Company.Product.{ModuleName}` | Domain entities, feature slices, application handlers |
| `Company.Product.{ModuleName}.Abstractions` | Public contracts: service-interface DTOs, cross-module service ports |
| `Company.Product.{ModuleName}.Api` | Minimal API host — **standard for every web-exposed module** |
| `Company.Product.{ModuleName}.Data.{StorageType}` | Persistence adapter (optional, e.g., `Data.Postgres`) |
| `Company.Product.{ModuleName}.Tests` | Optional module-local tests project (legacy/co-located pattern) |
| `Company.Product.{ModuleName}.Integrations` | Integration event handlers (optional, e.g., Dapr pub/sub consumers) |

> Note: Test-project placement topology is defined in testing recommendations. Keep production projects under `src/`; preferred topology places test projects under root `tests/`.

The `.Api` project is the **standard pattern for every web-exposed module**; it is not shared across modules. Each module is an independently deployable service unit with its own API surface.

### Aspire Folder

The `Aspire/` folder under `src/` contains the Aspire orchestration projects (see ADR 0003):

```
src/Aspire/
  Company.Product.Aspire.AppHost/        ← Orchestrates all module APIs and dependencies
  Company.Product.Aspire.ServiceDefaults/ ← Shared observability, health checks, service discovery
```

### Naming Conventions

| Artifact | Pattern | Example |
|---|---|---|
| Module folder | Short module name only | `Conferences/`, `Game/`, `Inventory/` |
| Module core project | `Company.Product.{Module}` | `Company.Product.Conferences` |
| Module abstractions | `Company.Product.{Module}.Abstractions` | `Company.Product.Conferences.Abstractions` |
| Module API host | `Company.Product.{Module}.Api` | `Company.Product.Conferences.Api` |
| Module data project | `Company.Product.{Module}.Data.{Store}` | `Company.Product.Conferences.Data.Postgres` |
| Module unit tests | `Company.Product.{Module}.UnitTests` | `Company.Product.Conferences.UnitTests` |
| Aspire AppHost | `Company.Product.Aspire.AppHost` | `Company.Product.Aspire.AppHost` |
| Aspire ServiceDefaults | `Company.Product.Aspire.ServiceDefaults` | `Company.Product.Aspire.ServiceDefaults` |

Physical Layout Example:

```
Company.Product/
├── src/
│   ├── App/                                          (Angular / Blazor frontend)
│   ├── Aspire/
│   │   ├── Company.Product.Aspire.AppHost/         (Orchestration)
│   │   └── Company.Product.Aspire.ServiceDefaults/ (Shared defaults)
│   ├── Core/
│   │   └── Company.Product.Core/                   (ICommandHandler, IQueryHandler, IClock, etc.)
│   ├── Conferences/
│   │   ├── Company.Product.Conferences/            (Domain + Features)
│   │   ├── Company.Product.Conferences.Abstractions/
│   │   ├── Company.Product.Conferences.Api/        (Minimal API host)
│   │   ├── Company.Product.Conferences.Data.Postgres/
│   │   └── Company.Product.Conferences.Integrations/
│   ├── Profiles/
│   │   ├── Company.Product.Profiles/
│   │   ├── Company.Product.Profiles.Abstractions/
│   │   └── Company.Product.Profiles.Api/
│   └── Shared/
│       └── Company.Product.IntegrationEvents/      (Cross-module event contracts)
├── tests/
│   ├── Company.Product.Conferences.UnitTests/
│   ├── Company.Product.Profiles.UnitTests/
│   ├── Company.Product.IntegrationTests/
│   ├── Company.Product.ArchitectureTests/
│   └── Company.Product.E2ETests/
└── Company.Product.sln
```

### Module Boundary Rules

- No cross-module domain entity sharing — use DTO contracts from the Abstractions project.
- Avoid leaking infrastructure types (e.g., EF `DbContext`) outside the data project.
- Domain projects remain persistence-agnostic (no ORM attributes, no SDK references).
- All inter-module communication via Abstractions interfaces or integration events.
- Repository interfaces (`IOrderRepository`) are defined in the MODULE implementation project (not in Abstractions) — they are internal persistence ports. Only cross-module service contracts belong in Abstractions.
- Each module's `.Api` project wires up all dependencies via the module registration extension method (e.g., `services.AddConferencesModule()`).

### Refactoring Guidance

- Start simple: domain + data in one module directory; introduce Abstractions only when another module requires a stable contract.
- Split Core/Application if use case orchestration logic grows large or distinct from domain invariants.
- Introduce multiple data provider projects only when necessary (e.g., read vs write stores) and tag with an ADR referencing rationale.

### Versioning & Ownership

- Each module can have an OWNER file (future enhancement) for stewardship.
- ADR changes affecting module boundaries must reference impacted modules explicitly.

## Consequences

Positive:

- Clear internal boundaries without distributed system overhead.
- Simplifies future extraction into microservices if needed.
- Improves build ergonomics (selective testing, potential project filtering).
- Encourages explicit contracts and reduces accidental coupling.
- Aligns with Hexagonal & Clean Architecture principles (ports/adapters localized per module).

Negative:

- More projects to manage; initial overhead in solution maintenance.
- Risk of premature abstraction if modules over-split early.
- Requires discipline to prevent a "shared dumping ground" project.

Neutral / Trade-offs:

- Some duplication of simple DTOs until stability proven (preferred over premature centralization).

## References

- ADR 0001 (Adopt .NET 10)
- Chris Richardson: Modular Monolith guidance (external)
- Udi Dahan: Service boundaries (conceptual alignment)
- ThoughtWorks Tech Radar: Evolutionary architecture principles
- Recommendation: Unit testing with xUnit, Moq and Bogus
- Recommendation: Integration Testing
- Recommendation: End-to-End Testing
