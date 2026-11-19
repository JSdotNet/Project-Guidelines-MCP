---
title: "Style Guide: Color Palette"
date: 2026-06-05
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
| `color-primary` | `#A16207` | `#F2C14E` | Primary buttons, active links, key actions |
| `color-primary-light` | `#D4A72C` | `#FFD166` | Hover states, tinted backgrounds |
| `color-primary-dark` | `#7C4A03` | `#D4A72C` | Pressed states, high-contrast variant |
| `color-secondary` | `#6C757D` | `#ADB5BD` | Secondary buttons, less prominent labels |

### Usage Rules

- `color-primary` must always appear on a background that provides at least **4.5:1** contrast.
- Never use brand colors directly for semantic states (e.g., do not use `color-primary` to signal success).

---

## Semantic Colors

Semantic colors are intentionally **soft surface colors**. This guide does not define a separate high-intensity semantic layer.

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `color-success` | `#D4EDDA` | `#1A3A22` | Success banners, confirmation panels, positive status surfaces |
| `color-warning` | `#FFF3CD` | `#3D2E00` | Warning banners, caution panels, non-blocking alert surfaces |
| `color-error` | `#F8D7DA` | `#3D0A0D` | Error banners, validation summaries, destructive-status surfaces |
| `color-info` | `#D1ECF1` | `#0A2C31` | Informational notices, neutral announcement surfaces |

### Usage Rules

- Semantic colors are **surface/background tokens first** — use readable foreground text and icons on top of them rather than treating them as strong standalone UI colors.
- Use semantic surfaces for banners, notices, inline validation summaries, and status containers.
- Do not invent a second, stronger semantic palette in product code.

---

## Neutral / Text Colors

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `color-text-primary` | `#212529` | `#F8F9FA` | Body text, headings, primary labels |
| `color-text-secondary` | `#6C757D` | `#CED4DA` | Supporting text, metadata, placeholders |
| `color-text-disabled` | `#ADB5BD` | `#6C757D` | Disabled controls, non-interactive text |
| `color-text-inverse` | `#FFFFFF` | `#212529` | Text on dark/colored surfaces |
| `color-text-link` | `#A16207` | `#F2C14E` | Hyperlinks (same as `color-primary`) |

### Usage Rules

- `color-text-primary` on `color-background` must maintain **7:1** contrast (AAA target).
- `color-text-secondary` on `color-background` must maintain at least **4.5:1** (AA).
- `color-text-disabled` is exempt from contrast requirements — it intentionally signals unavailability.
- `color-text-inverse` is for text placed on colored or dark backgrounds only.

---

## Background Colors

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `color-background` | `#FFFFFF` | `#0F172A` | Primary page / panel background |
| `color-background-alt` | `#F8F9FA` | `#1E293B` | Subtle alternating row, sidebar, card surface |
| `color-background-raised` | `#FFFFFF` | `#334155` | Elevated surface (dialog, popover, dropdown) |
| `color-background-overlay` | `rgba(0,0,0,0.40)` | `rgba(0,0,0,0.60)` | Modal backdrop / scrim |

### Usage Rules

- `color-background-raised` uses an elevated surface color (not a shadow alone) in dark mode.
- Always pair a background token with the appropriate `color-text-*` token to guarantee contrast.

---

## Border Colors

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `color-border` | `#DEE2E6` | `#475569` | Default dividers, input outlines, card edges |
| `color-border-strong` | `#ADB5BD` | `#64748B` | Emphasized borders, focus rings (fallback) |
| `color-border-focus` | `#A16207` | `#F2C14E` | Keyboard focus ring (same as `color-primary`) |

### Usage Rules

- Focus rings **must** use `color-border-focus` and have a visible width of at least **2px**.
- Never rely on border color alone to convey state — always pair with shape, icon, or text.

---

## Full Token Reference Table

| Token | Light | Dark |
|---|---|---|
| `color-primary` | `#A16207` | `#F2C14E` |
| `color-primary-light` | `#D4A72C` | `#FFD166` |
| `color-primary-dark` | `#7C4A03` | `#D4A72C` |
| `color-secondary` | `#6C757D` | `#ADB5BD` |
| `color-success` | `#D4EDDA` | `#1A3A22` |
| `color-warning` | `#FFF3CD` | `#3D2E00` |
| `color-error` | `#F8D7DA` | `#3D0A0D` |
| `color-info` | `#D1ECF1` | `#0A2C31` |
| `color-text-primary` | `#212529` | `#F8F9FA` |
| `color-text-secondary` | `#6C757D` | `#CED4DA` |
| `color-text-disabled` | `#ADB5BD` | `#6C757D` |
| `color-text-inverse` | `#FFFFFF` | `#212529` |
| `color-text-link` | `#A16207` | `#F2C14E` |
| `color-background` | `#FFFFFF` | `#0F172A` |
| `color-background-alt` | `#F8F9FA` | `#1E293B` |
| `color-background-raised` | `#FFFFFF` | `#334155` |
| `color-background-overlay` | `rgba(0,0,0,0.40)` | `rgba(0,0,0,0.60)` |
| `color-border` | `#DEE2E6` | `#475569` |
| `color-border-strong` | `#ADB5BD` | `#64748B` |
| `color-border-focus` | `#A16207` | `#F2C14E` |
