---
title: "Style Guide: Color Palette"
date: 2026-06-04
tags: [style-guide, color, design-tokens, dark-mode, light-mode]
---
# Color Palette

> This is the **only** aspect of the style guide that projects may customize. See [05 — Customization Guide](05-customization-guide.md) for the override mechanism.

Visual swatches: [`assets/color-palette-light.svg`](assets/color-palette-light.svg) · [`assets/color-palette-dark.svg`](assets/color-palette-dark.svg)

---

## Token Groups

Colors are organized into five semantic groups:

| Group | Purpose |
|---|---|
| **Brand** | Primary identity colors — actions, links, highlights |
| **Semantic** | Communicative states — success, warning, error, info |
| **Neutral / Text** | Body copy, headings, disabled states |
| **Background** | Page and surface backgrounds |
| **Border** | Dividers, outlines, input frames |

---

## Brand Colors

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `color-primary` | `#0066CC` | `#3385DB` | Primary buttons, active links, key actions |
| `color-primary-light` | `#3385DB` | `#5599E0` | Hover states, tinted backgrounds |
| `color-primary-dark` | `#004999` | `#0066CC` | Pressed states, high-contrast variant |
| `color-secondary` | `#6C757D` | `#ADB5BD` | Secondary buttons, less prominent labels |
| `color-accent` | `#FF6B35` | `#FF8C5A` | Accent highlights, call-to-action badges |

### Usage Rules

- `color-primary` must always appear on a background that provides at least **4.5:1** contrast.
- `color-accent` is for **emphasis only** — do not use it as a primary action color.
- Never use brand colors directly for semantic states (e.g., do not use `color-primary` to signal success).

---

## Semantic Colors

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `color-success` | `#28A745` | `#48C664` | Confirmation messages, success toasts, valid input |
| `color-success-subtle` | `#D4EDDA` | `#1A3A22` | Success banner backgrounds |
| `color-warning` | `#FFC107` | `#FFD454` | Warnings, non-blocking alerts, caution indicators |
| `color-warning-subtle` | `#FFF3CD` | `#3D2E00` | Warning banner backgrounds |
| `color-error` | `#DC3545` | `#F06070` | Errors, destructive actions, invalid input |
| `color-error-subtle` | `#F8D7DA` | `#3D0A0D` | Error banner backgrounds |
| `color-info` | `#17A2B8` | `#3FC4D8` | Informational messages, neutral notifications |
| `color-info-subtle` | `#D1ECF1` | `#0A2C31` | Info banner backgrounds |

### Usage Rules

- `color-*-subtle` tokens are **background-only** — never use them for text or icons.
- The non-subtle semantic tokens must pass **3:1** contrast against `color-background` at minimum.
- Pair each semantic color with its `color-*-subtle` counterpart for banners and alerts.

---

## Neutral / Text Colors

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `color-text-primary` | `#212529` | `#F8F9FA` | Body text, headings, primary labels |
| `color-text-secondary` | `#6C757D` | `#CED4DA` | Supporting text, metadata, placeholders |
| `color-text-disabled` | `#ADB5BD` | `#6C757D` | Disabled controls, non-interactive text |
| `color-text-inverse` | `#FFFFFF` | `#212529` | Text on dark/colored surfaces |
| `color-text-link` | `#0066CC` | `#3385DB` | Hyperlinks (same as `color-primary`) |

### Usage Rules

- `color-text-primary` on `color-background` must maintain **7:1** contrast (AAA target).
- `color-text-secondary` on `color-background` must maintain at least **4.5:1** (AA).
- `color-text-disabled` is exempt from contrast requirements — it intentionally signals unavailability.
- `color-text-inverse` is for text placed on colored or dark backgrounds only.

---

## Background Colors

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `color-background` | `#FFFFFF` | `#1A1A2E` | Primary page / panel background |
| `color-background-alt` | `#F8F9FA` | `#16213E` | Subtle alternating row, sidebar, card surface |
| `color-background-raised` | `#FFFFFF` | `#0F3460` | Elevated surface (dialog, popover, dropdown) |
| `color-background-overlay` | `rgba(0,0,0,0.40)` | `rgba(0,0,0,0.60)` | Modal backdrop / scrim |

### Usage Rules

- `color-background-raised` uses an elevated surface color (not a shadow alone) in dark mode.
- Always pair a background token with the appropriate `color-text-*` token to guarantee contrast.

---

## Border Colors

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `color-border` | `#DEE2E6` | `#343A40` | Default dividers, input outlines, card edges |
| `color-border-strong` | `#ADB5BD` | `#6C757D` | Emphasized borders, focus rings (fallback) |
| `color-border-focus` | `#0066CC` | `#3385DB` | Keyboard focus ring (same as `color-primary`) |

### Usage Rules

- Focus rings **must** use `color-border-focus` and have a visible width of at least **2px**.
- Never rely on border color alone to convey state — always pair with shape, icon, or text.

---

## Full Token Reference Table

| Token | Light | Dark |
|---|---|---|
| `color-primary` | `#0066CC` | `#3385DB` |
| `color-primary-light` | `#3385DB` | `#5599E0` |
| `color-primary-dark` | `#004999` | `#0066CC` |
| `color-secondary` | `#6C757D` | `#ADB5BD` |
| `color-accent` | `#FF6B35` | `#FF8C5A` |
| `color-success` | `#28A745` | `#48C664` |
| `color-success-subtle` | `#D4EDDA` | `#1A3A22` |
| `color-warning` | `#FFC107` | `#FFD454` |
| `color-warning-subtle` | `#FFF3CD` | `#3D2E00` |
| `color-error` | `#DC3545` | `#F06070` |
| `color-error-subtle` | `#F8D7DA` | `#3D0A0D` |
| `color-info` | `#17A2B8` | `#3FC4D8` |
| `color-info-subtle` | `#D1ECF1` | `#0A2C31` |
| `color-text-primary` | `#212529` | `#F8F9FA` |
| `color-text-secondary` | `#6C757D` | `#CED4DA` |
| `color-text-disabled` | `#ADB5BD` | `#6C757D` |
| `color-text-inverse` | `#FFFFFF` | `#212529` |
| `color-text-link` | `#0066CC` | `#3385DB` |
| `color-background` | `#FFFFFF` | `#1A1A2E` |
| `color-background-alt` | `#F8F9FA` | `#16213E` |
| `color-background-raised` | `#FFFFFF` | `#0F3460` |
| `color-background-overlay` | `rgba(0,0,0,0.40)` | `rgba(0,0,0,0.60)` |
| `color-border` | `#DEE2E6` | `#343A40` |
| `color-border-strong` | `#ADB5BD` | `#6C757D` |
| `color-border-focus` | `#0066CC` | `#3385DB` |
