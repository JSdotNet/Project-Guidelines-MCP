---
title: "ADR Number Map"
date: 2026-11-19
status: Accepted
tags: [adr, index, numbering, retired]
---
# ADR Number Map

The ADRs were consolidated into the set of 13 in this folder. The old numbers 0001 to 0019
are retired and never reused; this table maps each old ADR to the one that replaces it. Git
history keeps the old text.

| Old ADR | Old title | Replaced by |
|---|---|---|
| 0001 | Adopt .NET 10 as Target Framework | [0001: Platform and Build](0001-platform-and-build.md) |
| 0002 | Central Package Management for .NET Solutions | [0001: Platform and Build](0001-platform-and-build.md) |
| 0003 | Strong Recommendation to Adopt .NET Aspire for ASP.NET Web Services | [0002: Aspire, Configuration and Infrastructure](0002-aspire-configuration-and-infrastructure.md) |
| 0004 | Standardize Result Objects for Expected Application Outcomes | [0005: Application Layer](0005-application-layer.md) |
| 0005 | Modular Monolith Project Structure | [0003: Solution Structure](0003-solution-structure.md) |
| 0006 | Recommendation to Implement CQRS for ASP.NET API Projects | [0005: Application Layer](0005-application-layer.md) |
| 0007 | Recommend Minimal APIs Over Controller-Based APIs | [0004: HTTP API](0004-http-api.md) |
| 0008 | Adopt Vertical Slice Architecture for Feature Organization | [0003: Solution Structure](0003-solution-structure.md) |
| 0009 | Feature Slices Within Module Projects | [0003: Solution Structure](0003-solution-structure.md) |
| 0010 | Adopt OpenTelemetry for Comprehensive Observability | [0007: Observability and Resilience](0007-observability-and-resilience.md) |
| 0011 | Centralized Frontend Styling Variables | [0009: Frontend](0009-frontend.md) |
| 0012 | Authentication with External Identity Providers | [0008: Security](0008-security.md) |
| 0013 | Authorization & Zero Trust Security Model | [0008: Security](0008-security.md) |
| 0014 | Persistence Strategy and Repository Boundaries | [0006: Persistence and Messaging](0006-persistence-and-messaging.md) |
| 0015 | Resilience Strategy for Outbound Dependencies | [0007: Observability and Resilience](0007-observability-and-resilience.md) |
| 0016 | Messaging and Integration-Event Delivery | [0006: Persistence and Messaging](0006-persistence-and-messaging.md) |
| 0017 | HTTP Error Contract and Problem Details | [0004: HTTP API](0004-http-api.md) |
| 0018 | Configuration and Options Binding | [0002: Aspire, Configuration and Infrastructure](0002-aspire-configuration-and-infrastructure.md) |
| 0019 | C# Standalone Script Files | [0001: Platform and Build](0001-platform-and-build.md) |

ADRs 0010 (Testing), 0011 (Setting Up MCP), 0012 (Agents in Applications) and 0013
(Agent-Ready Repository) replace no old ADR.
