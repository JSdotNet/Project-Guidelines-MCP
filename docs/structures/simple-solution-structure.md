---
title: "Simple Solution Structure Design"
date: 2026-06-01
status: Accepted
tags: [design, architecture, aspire, single-module, structure]
---
## Simple Solution Structure Design

**Status**: Accepted  
**Date**: 2026-06-01  
**Author**: Design Guidelines Team

## Overview

This document defines the recommended structure for **simple single-solution** projects.

Use this structure when the solution has one bounded context and no near-term requirement to split into multiple modules. For multi-module solutions, use `structures/modular-solution-structure.md`.

Aspire is optional in this profile. Use Aspire when distributed orchestration is needed; otherwise omit `src/Aspire/`.

## MCP Selection

When querying through the MCP service:

- Use `simple-solution-structure` when asking for single-module layouts.
- Use `modular-solution-structure` when asking for multi-module layouts.

This split avoids returning mixed profiles in one document.

## High-Level Organization

```text
solution-root/
├── src/
│   ├── App/                                       # Optional frontend (Angular, Blazor, React)
│   ├── Aspire/                                    # Optional orchestration for distributed/web-enabled solutions
│   │   ├── Company.Product.Aspire.AppHost/
│   │   └── Company.Product.Aspire.ServiceDefaults/
│   ├── Company.Product/                           # Domain + application + abstractions (single solution core)
│   ├── Company.Product.Api/
│   └── Company.Product.Data.{StorageType}/
├── tests/
│   ├── Company.Product.UnitTests/
│   ├── Company.Product.IntegrationTests/          # Optional
│   └── Company.Product.E2ETests/                  # Optional, typically when UI exists
└── Company.Product.sln
```

Integration tests, E2E tests, and architecture tests are optional for this simple profile and are not required by default.

Testing options for this profile:

- Unit + E2E (common when a UI is part of the solution)
- Unit + Integration (common for API/service solutions without UI)
- Unit + Integration + E2E (when risk justifies both)
- Unit only (very small/internal solutions)

## Example

```text
Contoso.Orders/
├── src/
│   ├── Contoso.Orders/
│   ├── Contoso.Orders.Api/
│   ├── Contoso.Orders.Data.SqlServer/
│   └── README.md
├── tests/
│   ├── Contoso.Orders.UnitTests/
│   ├── Contoso.Orders.IntegrationTests/    # Optional
│   └── Contoso.Orders.E2ETests/            # Optional
└── Contoso.Orders.sln
```

## Related Documents

- ADR 0003: Strong Recommendation to Adopt .NET Aspire for ASP.NET Web Services
- Design: Modular Solution Structure Design
- Recommendation: Unit testing with xUnit, Moq and Bogus
- Recommendation: Integration Testing
- Recommendation: End-to-End Testing
