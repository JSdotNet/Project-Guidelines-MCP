---
title: "Building Block: AppHost"
date: 2026-11-19
status: Accepted
tags: [building-blocks, aspire, apphost, orchestration, deployment, structure]
---
# Building Block: AppHost

`Company.Product.AppHost` is the one model of every resource the app needs, from `aspire run`
on a laptop to `aspire deploy` in Azure (ADR 0002). Part of the
[building-block view](../05-building-block-view.md).

## Project

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.AppHost" />
    <PackageReference Include="Aspire.Hosting.Dotnet" />
    <PackageReference Include="Aspire.Hosting.PostgreSQL" />
    <PackageReference Include="Aspire.Hosting.JavaScript" />
  </ItemGroup>
</Project>
```

Versions come from `Directory.Packages.props` (ADR 0001). `Aspire.AppHost.Sdk`, `AddProject`
and `AddCSharpApp` are obsolete, and their diagnostics are never suppressed.

## Program.cs

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("postgres").AddDatabase("orders-db");

var migrations = builder
    .AddDotnetProject("orders-migrations", "../Contoso.Orders.MigrationRunner/Contoso.Orders.MigrationRunner.csproj")
    .WithReference(db).WaitFor(db);

var api = builder
    .AddDotnetProject("orders-api", "../../Orders/Contoso.Orders.Api/Contoso.Orders.Api.csproj")
    .WithReference(db).WaitForCompletion(migrations)
    .WithCommand("reset-db", "Reset database", ResetDatabaseAsync);

builder.AddViteApp("app", "../../App")
    .WithReference(api).WaitFor(api)
    .WithBrowserLogs();

builder.Build().Run();
```

## Rules

- Projects are added by path with `AddDotnetProject`; frontends with `AddViteApp` (React) or
  `AddDotnetProject` (Blazor), each with `.WithBrowserLogs()` (ADR 0009).
- Migrations run in `Company.Product.MigrationRunner`, a worker that applies every module's
  migrations and exits; each Api waits for its completion (ADR 0006).
- Settings and secrets are Aspire parameters; nothing secret is committed.
- Routine development actions are resource commands (`WithCommand`), not scripts.
- A [harness](test-harness.md) service can be started in place of an external one for local
  runs.
- Resources Aspire cannot model are added through its customization hooks, never a second IaC
  tool; alert rules are modelled here too.
- Hot reload uses `dotnet watch` until `aspire run --watch` supports `AddDotnetProject`.
- The E2E suite starts this host with `aspire start`, detached and isolated (ADR 0010).
