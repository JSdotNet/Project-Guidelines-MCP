---
title: "02 Constraints"
date: 2026-11-19
status: Accepted
tags: [arc42, constraints, dotnet, aspire, ef-core, azure]
---
# 02 Constraints

The fixed givens every JSdotNet application starts from. A project does not revisit them; it
records only the choices these leave open (database, broker, identity provider, frontend
framework) in its own ADRs.

| Constraint | What it fixes | Source |
|---|---|---|
| .NET | The newest GA .NET and its C# version (today .NET 11, C# 15), pinned in `global.json`; Central Package Management with lock files; warnings as errors | ADR 0001 |
| Aspire | Aspire 17 for every app: a `Microsoft.NET.Sdk` AppHost with `Aspire.Hosting.Dotnet` and `AddDotnetProject`, run with `aspire run`, deployed with `aspire deploy` | ADR 0002 |
| EF Core | The only data-access library, only inside `{Module}.Data.{Provider}` projects; the provider is the app's choice | ADR 0006 |
| Azure | The deployment target: `aspire publish` generates the Bicep, `aspire deploy` applies it, managed identity wherever the target supports it; no second IaC tool and no portal change | ADR 0002 |

## Consequences for a project

- A project's `02-constraints.md` links this page and adds only constraints of its own
  (regulation, an existing system, a team boundary).
- A resource Aspire cannot model is added through the AppHost's customization hooks, not
  outside it.
- Every library choice outside these four is checked against the rated options in `tech/`
  before it lands.
