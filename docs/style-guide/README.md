---
title: "Style Guide: Overview"
date: 2026-06-04
tags: [style-guide, design-tokens, ux, frontend]
---
# UX Style Guide

## Purpose

This style guide is the **canonical design token reference** for all frontend projects in this organization. It defines the visual language — colors, typography, spacing, motion — that ensures a consistent, accessible user experience across applications built with any frontend technology (Angular, React, Vue, Svelte, Blazor, or plain HTML/CSS).

> **Technology-agnostic:** This guide deliberately avoids SCSS, CSS, or framework-specific syntax. For implementation patterns, see [ADR 0011: Centralized Frontend Styling Variables](../adrs/0011-centralized-frontend-styling-variables.md).

---

## Structure

| Document | Contents |
|---|---|
| [01 — Color Palette](01-color-palette.md) | Color tokens for light and dark mode, hex values, semantic names, usage rules, visual swatches |
| [02 — Typography](02-typography.md) | Font families, sizes, weights, line heights |
| [03 — Spacing & Layout](03-spacing-and-layout.md) | Spacing scale, border radius, shadows |
| [04 — Motion & Interaction](04-motion-and-interaction.md) | Transition durations, easing curves, focus states |
| [05 — Customization Guide](05-customization-guide.md) | How to override the color scheme per project |

---

## What Is and Isn't Customizable

| Aspect | Customizable Per Project? |
|---|---|
| **Color scheme** | ✅ Yes — the only permitted customization |
| Typography | ❌ No — fixed for all projects |
| Spacing scale | ❌ No — fixed for all projects |
| Border radius | ❌ No — fixed for all projects |
| Motion / transitions | ❌ No — fixed for all projects |

A project team may only override the **color tokens** defined in [01 — Color Palette](01-color-palette.md). All other tokens are fixed and must not be modified. See [05 — Customization Guide](05-customization-guide.md) for the permitted override mechanism.

---

## Design Principles

1. **Semantic naming** — tokens are named by *purpose* (e.g., `color-primary`, `color-error`), not by raw value (never `blue-500` or `#dc3545`).
2. **Light and dark first** — every color token has both a light-mode and a dark-mode variant. There is no single-mode design.
3. **Accessibility** — all color combinations meet WCAG 2.1 AA contrast requirements (4.5:1 for normal text, 3:1 for large text and UI components).
4. **Consistency over cleverness** — use the predefined scale; do not introduce one-off values.

---

## Relationship to ADR 0011

This style guide defines **what** the tokens are and what they mean.  
[ADR 0011](../adrs/0011-centralized-frontend-styling-variables.md) defines **how** those tokens are declared and used in code (SCSS variables, CSS custom properties, framework-specific integration).

The style guide is the source of truth. ADR 0011 is the implementation contract.

---

## Visual References

Sample SVG files in [`assets/`](assets/) let you verify the color scheme at a glance without running any build:

- [`color-palette-light.svg`](assets/color-palette-light.svg) — all color tokens in light mode
- [`color-palette-dark.svg`](assets/color-palette-dark.svg) — all color tokens in dark mode
