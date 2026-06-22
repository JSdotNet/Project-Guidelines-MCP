---
title: "ADR 0018: Configuration and Options Binding"
date: 2026-06-04
status: Accepted
tags: [configuration, options, secrets, validation, aspnet, adr]
---
# ADR 0018: Configuration and Options Binding

## Context

The repository recommends environment-driven configuration and secret externalization, but it does not yet define how configuration should be structured in code. Teams need consistent guidance for:

1. `IOptions` usage.
2. Startup validation.
3. Section naming and ownership.
4. Secret handling across environments.

## Decision

We STANDARDIZE strongly typed options with **bind + validate + fail fast** semantics.

### 1. Use strongly typed options objects

Configuration consumed by application code is represented through dedicated options classes:

- `DatabaseOptions`
- `JwtOptions`
- `ExternalApiOptions`

Avoid scattering raw `IConfiguration["Some:Key"]` access through feature code.

### 2. Validate options at startup

Options used for critical runtime behavior must be validated during startup.

Required practices:

- Use data annotations or custom validators where appropriate.
- Use `ValidateOnStart()` for required runtime configuration.
- Fail startup when critical configuration is invalid or missing.

### 3. Ownership follows the same module boundaries as code

Each module or shared technical capability owns its own configuration section. Configuration section names must be stable and explicit.

Examples:

- `Modules:Orders:Persistence`
- `Security:Jwt`
- `Integrations:Payments`

### 4. Secrets never live in source control

Secrets are provided through secure external configuration sources:

- Environment variables
- Secret managers / vaults
- `dotnet user-secrets` for local development

Checked-in configuration files may contain structure and non-secret defaults only.

### 5. Prefer options injection at boundaries

Inject `IOptions<T>`, `IOptionsSnapshot<T>`, or `IOptionsMonitor<T>` according to runtime needs:

- `IOptions<T>` for singleton/static configuration
- `IOptionsSnapshot<T>` for scoped request-time refresh in ASP.NET
- `IOptionsMonitor<T>` for long-lived services that must observe changes

Feature code should depend on an already-bound options object or a thin technical adapter, not raw configuration plumbing.

## Consequences

### Positive

1. **Safer startup**: Invalid configuration fails early instead of producing runtime surprises.
2. **Cleaner code**: Configuration access is explicit, typed, and testable.
3. **Boundary alignment**: Modules own their own configuration contracts.
4. **Better secret hygiene**: Secret handling remains externalized and auditable.

### Negative

1. **More upfront classes**: Each capability needs a dedicated options type and validation.
2. **Intentional naming required**: Poorly chosen section names can still create confusion if not reviewed.

## References

- ADR 0003: Strong Recommendation to Adopt .NET Aspire for ASP.NET Web Services
- ADR 0012: Authentication with External Identity Providers
- ADR 0013: Authorization & Zero Trust Security Model
