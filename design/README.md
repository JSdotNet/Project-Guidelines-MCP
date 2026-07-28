---
title: "Style Guide: Overview"
date: 2026-07-28
tags: [style-guide, design-tokens, ux, frontend]
---
# UX Style Guide

## Version and Ownership

| Property | Value |
|---|---|
| Version | 2.0 |
| Last updated | 2026-07-28 |
| Status | Active |
| Owner | JSdotNet UX Guild |
| Change process | Open a PR against this repository; at least one guild member must approve |

---

## Purpose

This style guide is the **canonical design token reference** for all frontend projects in this organization. It defines the visual language — colors, typography, spacing, motion — that ensures a consistent, accessible user experience across applications built with any frontend technology (Angular, React, Vue, Svelte, Blazor, or plain HTML/CSS).

> **Technology-agnostic:** This guide deliberately avoids SCSS, CSS, or framework-specific syntax. For implementation patterns, see [ADR 0011: Centralized Frontend Styling Variables](../adrs/0011-centralized-frontend-styling-variables.md).

---

## Structure

| Document | Contents |
|---|---|
| [01 — Color Palette](01-color-palette.md) | Color tokens for light and dark mode, hex values, semantic names, usage rules, visual swatches |
| [02 — Typography](02-typography.md) | Font families, sizes, weights, line heights |
| [03 — Spacing & Layout](03-spacing-and-layout.md) | Spacing scale, border radius, shadows, z-index |
| [04 — Motion & Interaction](04-motion-and-interaction.md) | Transition durations, easing curves, focus states, loading indicators |
| [05 — Customization Guide](05-customization-guide.md) | How to override the color scheme per project |
| [06 — Component Patterns](06-component-patterns.md) | Button, Form Input, Card, Badge, Toast — states, variants, accessibility |
| [07 — Iconography](07-iconography.md) | Icon library (Lucide), sizing scale, color rules, accessibility |
| [08 — Voice and Tone](08-voice-and-tone.md) | Language personality, button labels, error messages, empty states |
| [09 — Interaction Patterns](09-interaction-patterns.md) | Form validation, loading, error handling, navigation (tabs, breadcrumbs, modals, drawers) |

---

## What Is and Isn't Customizable

| Aspect | Customizable Per Project? |
|---|---|
| **Color scheme** | ✅ Yes — the only permitted customization |
| Typography | ❌ No — fixed for all projects |
| Spacing scale | ❌ No — fixed for all projects |
| Border radius | ❌ No — fixed for all projects |
| Motion / transitions | ❌ No — fixed for all projects |
| Component patterns | ❌ No — fixed for all projects |
| Iconography | ❌ No — fixed for all projects |
| Voice and tone | ❌ No — fixed for all projects |

A project team may only override the **color tokens** defined in [01 — Color Palette](01-color-palette.md). All other tokens are fixed and must not be modified. See [05 — Customization Guide](05-customization-guide.md) for the permitted override mechanism.

---

## Design Principles

1. **Semantic naming** — tokens are named by *purpose* (e.g., `color-primary`, `color-error`), not by raw value (never `blue-500` or `#dc3545`).
2. **Light and dark first** — every color token has both a light-mode and a dark-mode variant. There is no single-mode design.
3. **Accessibility** — all color combinations meet WCAG 2.1 AA contrast requirements (4.5:1 for normal text, 3:1 for large text and UI components).
4. **Consistency over cleverness** — use the predefined scale; do not introduce one-off values.
5. **User-centred copy** — clear, direct, calm language in all UI text; see [08 — Voice and Tone](08-voice-and-tone.md).
6. **Progressive disclosure** — show only what is needed at each step; reveal complexity on demand.

---

## Platform Scope

This style guide targets **web** applications. All responsive breakpoints, touch targets, and layout guidelines apply to browser-rendered UIs:

- Desktop-first layout, fully responsive down to mobile (`≥ 320 px`).
- Touch targets ≥ 44 × 44 px for all interactive elements on mobile.
- Dark mode support is mandatory — every token has both light and dark values.

---

## Relationship to ADR 0011

This style guide defines **what** the tokens are and what they mean.  
[ADR 0011](../adrs/0011-centralized-frontend-styling-variables.md) defines **how** those tokens are declared and used in code (SCSS variables, CSS custom properties, framework-specific integration).

The style guide is the source of truth. ADR 0011 is the implementation contract.

---

## Visual References

Sample SVG files in [`assets/`](assets/) let you verify the style guide visually without running any build:

- [`color-palette-light.svg`](assets/color-palette-light.svg) — all color tokens in light mode
- [`color-palette-dark.svg`](assets/color-palette-dark.svg) — all color tokens in dark mode
- [`foundations-overview.svg`](assets/foundations-overview.svg) — typography, spacing, surfaces, motion, and the "colors-only" customization boundary
