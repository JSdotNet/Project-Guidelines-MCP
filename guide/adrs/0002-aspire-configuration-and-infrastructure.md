---
title: "ADR 0002: Aspire, Configuration and Infrastructure"
date: 2026-11-19
status: Accepted
tags: [aspire, apphost, configuration, options, secrets, managed-identity, data-protection, infrastructure-as-code, bicep, deployment, adr]
---
# ADR 0002: Aspire, Configuration and Infrastructure

## Context

Apps that run differently on a laptop, in CI and in Azure drift apart, and every second tool
(a start script, hand-written Bicep, a portal change) is another place that drifts. Aspire 17
makes the AppHost the one model of an app's resources from local run to deployment. It also
deprecates the project SDK and `AddProject` that earlier AppHosts used, and lets a team share
templates, integrations and agent skills from one Git repository.

## Decision

**Aspire 17 is mandatory for every app**, single-service ones included.

- The AppHost is a `Microsoft.NET.Sdk` project that references `Aspire.Hosting.AppHost` and
  `Aspire.Hosting.Dotnet`, and adds projects by path with `AddDotnetProject`.
  `Aspire.AppHost.Sdk`, `AddProject` and `AddCSharpApp` are obsolete; their diagnostics are
  never suppressed (ADR 0001).
- Hot reload uses `dotnet watch` until `aspire run --watch` supports `AddDotnetProject`.
- Every frontend resource calls `.WithBrowserLogs()`.
- Routine development actions (reset or seed a database, clear a cache) are AppHost resource
  commands (`WithCommand`), not ad-hoc scripts.
- MCP servers that an integration adds for a resource are development-only and read-only, and
  are never deployed.
- Shared templates, hosting integrations and agent skills come from this repository as the
  team's Aspire config source; there is no per-solution `Aspire.Extensions` project.

```csharp
var builder = DistributedApplication.CreateBuilder(args);
builder.AddDotnetProject("orders-api", "../Orders/Contoso.Orders.Api/Contoso.Orders.Api.csproj");
builder.Build().Run();
```

**Configuration and secrets.**

- Configuration binds to options classes with validation and `ValidateOnStart()`; feature code
  never reads raw `IConfiguration` keys.
- Aspire parameters are the source of settings and secrets; secrets are set per environment
  with `aspire secret set --environment`. Nothing secret is committed.
- Managed identity wherever the target supports it, instead of a key or connection secret.
- Tokens the app must keep are protected with Data Protection; API tokens are stored only as
  hashes.

**Infrastructure.** Aspire is the infrastructure code. `aspire publish` generates the Bicep
and `aspire deploy` applies it with per-environment parameters. There is no second IaC tool
and no portal change; a resource Aspire cannot model yet is added through the AppHost's
customization hooks. Alert rules are modelled in the AppHost and deploy with it.

## Consequences

### Positive

- One model runs the app locally, in tests and in Azure; agents and people inspect it with the
  same `aspire` commands.
- Invalid configuration fails at startup, and no secret lives in source or in plain storage.
- New apps get the shared templates, integrations and skills from one place.

### Negative

- Every app, however small, carries an AppHost.
- Existing AppHosts migrate `AddProject` calls by hand; `aspire update` does not rewrite them.
- Infrastructure limited to what Aspire models or its hooks allow.

## References

- [What's new in Aspire 17](https://aspire.dev/whats-new/aspire-17/)
- [Upgrade Aspire](https://aspire.dev/whats-new/upgrade-aspire/)
- [Options pattern](https://learn.microsoft.com/dotnet/core/extensions/options)
- ADR 0001: Platform and Build
- ADR 0007: Observability and Resilience

Replaces: ADR 0003 (.NET Aspire Recommendation for ASP.NET Services), ADR 0018 (Configuration and Options Binding).
