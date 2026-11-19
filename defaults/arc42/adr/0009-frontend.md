---
title: "ADR 0009: Frontend"
date: 2026-11-19
status: Accepted
tags: [frontend, ui, blazor, react, vite, tailwind, design-tokens, css-variables, openapi, component-catalog, aspire, adr]
---
# ADR 0009: Frontend

## Context

The guidance listed five frontend frameworks with a styling sample for each, so every app
re-decided the stack and its wiring. What has to be the same across apps is not the framework
but how the UI is built, run, styled and connected to the API. Hard-coded colours and sizes
drift from the style guide in `defaults/design/`.

## Decision

**Rules, not a framework.** Every UI follows these rules, whichever framework it uses:

- The UI is its own app under `src/App/`, not part of a module's Api project.
- It runs as an Aspire resource in the AppHost (ADR 0002): `AddDotnetProject` for Blazor,
  `AddViteApp` for React, and `.WithBrowserLogs()` on both.
- It calls the API only through a client generated from the API's OpenAPI document (ADR 0004).
- It takes colours, typography, spacing and motion from the `defaults/design/` tokens as CSS custom
  properties. Components never hard-code a colour, font or size; a theme or dark mode swaps
  the property values, not the components.
- It has a component catalog that shows every shared component in isolation, styled by the
  tokens.
- It is covered end to end by the Playwright suite (ADR 0010).

**Two options.** An app picks one and records the pick in its own ADR:

| Option | Tokens arrive as |
|---|---|
| Blazor | CSS custom properties in a shared stylesheet, used from CSS isolation files |
| React 19 + Vite + Tailwind 4 | CSS custom properties mapped into Tailwind's `@theme` |

```css
/* Tailwind 4: the theme reads the design tokens instead of its own values */
@theme {
  --color-primary: var(--jsd-color-primary);
  --spacing-md: var(--jsd-space-md);
}
```

Angular, Vue and Svelte are not options.

## Consequences

### Positive

- Every UI starts, logs and is tested the same way, whichever framework it uses.
- A contract change breaks the UI build instead of the running app.
- A rebrand or dark mode is a token change, not a component sweep.

### Negative

- Two framework options still mean two sets of samples, catalogs and skills to maintain.
- Teams with Angular, Vue or Svelte experience start on an unfamiliar stack.
- A component catalog is extra code to keep running.

## References

- [defaults/design/ style guide](../../design/README.md)
- [Tailwind CSS theme variables](https://tailwindcss.com/docs/theme)
- [ASP.NET Core Blazor CSS isolation](https://learn.microsoft.com/aspnet/core/blazor/components/css-isolation)
- [Aspire JavaScript hosting](https://aspire.dev/integrations/frameworks/javascript/)
- ADR 0002: Aspire, Configuration and Infrastructure
- ADR 0004: HTTP API
- ADR 0010: Testing

Replaces: ADR 0011 (Centralized Frontend Styling Variables).
