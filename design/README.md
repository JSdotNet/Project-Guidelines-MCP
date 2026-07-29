---
title: "Style Guide: Overview"
date: 2026-07-28
tags: [style-guide, design-tokens, ux, frontend]
---
# UX Style Guide

## Version and Ownership

| Property | Value |
|---|---|
| Version | 2.1 |
| Last updated | 2026-07-28 |
| Status | Active |
| Owner | JSdotNet UX Guild |
| Change process | Open a PR against this repository; at least one guild member must approve |

---

## Purpose

This style guide is the **canonical UX foundation, component, content, and interaction reference** for web products in this organization. It defines the shared visual language, reusable UI patterns, copy guidance, and recurring interaction behaviors that keep products consistent and accessible across frameworks.

> **Framework-agnostic web guidance:** These documents avoid framework-specific implementation details. For token implementation patterns in code, see [ADR 0011: Centralized Frontend Styling Variables](../guide/adrs/0011-centralized-frontend-styling-variables.md).

---

## Structure

| Area | Documents | Purpose |
|---|---|---|
| **Foundations** | [01 - Color Palette](01-color-palette.md), [02 - Typography](02-typography.md), [03 - Spacing & Layout](03-spacing-and-layout.md), [04 - Motion & Interaction](04-motion-and-interaction.md), [05 - Customization Guide](05-customization-guide.md) | Design tokens, scales, motion rules, and the allowed customization boundary |
| **Components** | [06 - Component Patterns](06-component-patterns.md), [07 - Iconography](07-iconography.md) | Anatomy, states, variants, and accessibility requirements for reusable UI building blocks |
| **Content** | [08 - Voice and Tone](08-voice-and-tone.md) | Product language, button labels, message structure, and UI writing conventions |
| **Interactions** | [09 - Interaction Patterns](09-interaction-patterns.md) | Canonical behavior for validation, empty states, loading, errors, and navigation patterns |

---

## How to Use This Guide

1. Start with the foundation documents before defining component-level behavior.
2. Use [04 - Motion & Interaction](04-motion-and-interaction.md) for timing, easing, and reduced-motion constraints.
3. Use [06 - Component Patterns](06-component-patterns.md) for component anatomy, states, and accessibility.
4. Use [08 - Voice and Tone](08-voice-and-tone.md) for copy rules and message examples.
5. Use [09 - Interaction Patterns](09-interaction-patterns.md) as the canonical source for recurring UI behavior and layout patterns.

When a topic appears in more than one document, the more specific document is authoritative for that concern.

---

## What Is and Isn't Customizable

| Aspect | Customizable Per Project? |
|---|---|
| **Color scheme** | Yes - the only permitted customization |
| Typography | No - fixed for all projects |
| Spacing scale | No - fixed for all projects |
| Border radius | No - fixed for all projects |
| Motion / transitions | No - fixed for all projects |
| Component patterns | No - shared baseline |
| Iconography | No - shared baseline with limited substitution rules |
| Voice and tone | No - shared baseline |
| Interaction patterns | No - shared baseline |

A project team may only override the **color tokens** defined in [01 - Color Palette](01-color-palette.md). All other tokens and guidance are fixed unless the style guide itself is updated through the repository change process. See [05 - Customization Guide](05-customization-guide.md) for the permitted override mechanism.

---

## Design Principles

1. **Semantic naming** - tokens are named by purpose (for example, `color-primary`, `color-error`), not by raw value.
2. **Light and dark first** - every color token has both a light-mode and a dark-mode variant.
3. **Accessibility by default** - all color, motion, focus, and copy guidance must support WCAG 2.1 AA as a baseline.
4. **Consistency over cleverness** - use the predefined scale and shared patterns before introducing local exceptions.
5. **Clear language is part of UX** - UI text should be direct, calm, and useful; see [08 - Voice and Tone](08-voice-and-tone.md).
6. **Behavior belongs to patterns** - recurring flows such as validation, loading, and error recovery are defined once in [09 - Interaction Patterns](09-interaction-patterns.md) and reused consistently.

---

## Platform Scope

This style guide targets **web applications**:

- Responsive layouts from mobile (`>= 320 px`) through large desktop screens.
- Keyboard-accessible interactions as a default, not an enhancement.
- Touch targets >= 44 x 44 px for interactive controls on mobile and touch devices.
- Mandatory support for light mode, dark mode, and reduced-motion user preferences.

---

## Relationship to ADR 0011

This style guide defines **what** the shared tokens and UX rules are.
[ADR 0011](../guide/adrs/0011-centralized-frontend-styling-variables.md) defines **how** the design tokens are represented and consumed in code.

The style guide is the source of truth for UX decisions. ADR 0011 is the implementation contract for frontend projects.

---

## Visual References

Sample SVG files in [`assets/`](assets/) let you verify the style guide visually without running any build:

- [`color-palette-light.svg`](assets/color-palette-light.svg) - all color tokens in light mode
- [`color-palette-dark.svg`](assets/color-palette-dark.svg) - all color tokens in dark mode
- [`foundations-overview.svg`](assets/foundations-overview.svg) - typography, spacing, surfaces, motion, and the colors-only customization boundary
