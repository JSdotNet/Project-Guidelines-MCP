---
title: "Web Technologies"
date: 2026-11-25
status: Accepted
tags: [tech, ratings, frontend, ui, blazor, react, vite, tailwind]
---
# Web Technologies

The UI app under `src/App/`. An app picks Blazor or the React stack and records the pick in
its own ADR; both follow the same frontend rules
([ADR 0009](../arc42/adr/0009-frontend.md)). Ratings and layers:
[technology-graph.md](technology-graph.md).

## Blazor

- **Rating:** adopted · **Version:** 11.0
- **Depends on:** [ASP.NET Core Minimal APIs](backend.md#aspnet-core-minimal-apis)
- **Used for:** a .NET UI, added to the AppHost with `AddDotnetProject`; design tokens arrive
  as CSS custom properties used from CSS isolation files.
- **Why:** [ADR 0009](../arc42/adr/0009-frontend.md).

## React

- **Rating:** adopted · **Version:** 19
- **Depends on:** [Vite](#vite)
- **Used for:** a JavaScript UI, calling the API through a client generated from OpenAPI.
- **Why:** [ADR 0009](../arc42/adr/0009-frontend.md).

## Vite

- **Rating:** adopted
- **Used for:** building and serving the React UI, added to the AppHost with `AddViteApp`.
- **Why:** [ADR 0009](../arc42/adr/0009-frontend.md).

## Tailwind CSS

- **Rating:** adopted · **Version:** 4
- **Depends on:** [Vite](#vite)
- **Used for:** styling the React UI, with the design tokens mapped into `@theme`.
- **Why:** [ADR 0009](../arc42/adr/0009-frontend.md), tokens from [../design/](../design/README.md).

## Angular, Vue and Svelte

- **Rating:** hold
- **Used for:** nothing in a new app.
- **Why:** they are not frontend options ([ADR 0009](../arc42/adr/0009-frontend.md)).
