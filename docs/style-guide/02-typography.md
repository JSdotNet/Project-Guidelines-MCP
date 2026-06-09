---
title: "Style Guide: Typography"
date: 2026-06-04
tags: [style-guide, typography, fonts, design-tokens]
---
# Typography

> Typography tokens are **fixed** — they are not customizable per project.

---

## Font Families

| Token | Value | Usage |
|---|---|---|
| `font-family-base` | `'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif` | Body text, labels, inputs, all default prose |
| `font-family-heading` | `'Poppins', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif` | H1–H4 headings, display text |
| `font-family-mono` | `'Fira Code', 'Courier New', monospace` | Code blocks, inline code, terminal output |

### Rationale

- **Inter** is designed for screen legibility at all sizes and renders well across Windows, macOS, and Linux.
- **Poppins** provides a geometric, approachable heading style that pairs cleanly with Inter.
- **Fira Code** includes ligatures for code readability without sacrificing monospace alignment.
- All families include system-font fallbacks to ensure legible rendering even if web fonts fail to load.

---

## Font Sizes

Sizes follow a modular scale based on a **16px (1 rem) base**. All values are expressed in `rem` to respect the user's browser font size preference.

| Token | rem Value | px Equivalent | Usage |
|---|---|---|---|
| `font-size-xs` | `0.75rem` | 12 px | Helper text, legal fine print, badges |
| `font-size-sm` | `0.875rem` | 14 px | Secondary body text, table cells, form hints |
| `font-size-base` | `1rem` | 16 px | Default body text |
| `font-size-lg` | `1.125rem` | 18 px | Lead paragraphs, emphasized body text |
| `font-size-xl` | `1.25rem` | 20 px | Card titles, section sub-labels |
| `font-size-2xl` | `1.5rem` | 24 px | H4 headings |
| `font-size-3xl` | `1.875rem` | 30 px | H3 headings |
| `font-size-4xl` | `2.25rem` | 36 px | H2 headings |
| `font-size-5xl` | `3rem` | 48 px | H1 headings, display/hero text |

### Usage Rules

- Never set body font sizes in `px` — always use the scale tokens to preserve user accessibility settings.
- Do not introduce intermediate sizes outside the scale.
- Use `font-size-base` as the default; deviate only for clear hierarchy or emphasis.

---

## Font Weights

| Token | Value | Usage |
|---|---|---|
| `font-weight-light` | `300` | Decorative display text only; use sparingly |
| `font-weight-normal` | `400` | All body text |
| `font-weight-medium` | `500` | Emphasized body text, button labels |
| `font-weight-semibold` | `600` | Sub-headings, card titles, nav labels |
| `font-weight-bold` | `700` | Primary headings, strong emphasis |

### Usage Rules

- `font-weight-light` (300) is permitted only at `font-size-3xl` and above.
- Do not use `font-weight-bold` (700) for body text — use `font-weight-semibold` (600) for inline emphasis.

---

## Line Heights

| Token | Value | Usage |
|---|---|---|
| `line-height-none` | `1` | Single-line UI elements (buttons, badges) |
| `line-height-tight` | `1.25` | Headings and display text |
| `line-height-normal` | `1.5` | Default body text |
| `line-height-relaxed` | `1.75` | Long-form content, help text, tooltips |

### Usage Rules

- Body text (`font-size-base` and below) must use `line-height-normal` or `line-height-relaxed`.
- Headings must use `line-height-tight`.
- Single-line interactive controls (buttons, chips, tags) use `line-height-none` and rely on padding for height.

---

## Letter Spacing

| Token | Value | Usage |
|---|---|---|
| `letter-spacing-tight` | `-0.025em` | Large display headings (≥ `font-size-4xl`) |
| `letter-spacing-normal` | `0` | All body text and headings up to `font-size-3xl` |
| `letter-spacing-wide` | `0.05em` | All-caps labels, badges, overline text |
| `letter-spacing-widest` | `0.1em` | Decorative all-caps only |

---

## Heading Defaults

Apply these token combinations for standard HTML headings:

| Element | Size | Weight | Line Height | Family |
|---|---|---|---|---|
| H1 | `font-size-5xl` | `font-weight-bold` | `line-height-tight` | `font-family-heading` |
| H2 | `font-size-4xl` | `font-weight-bold` | `line-height-tight` | `font-family-heading` |
| H3 | `font-size-3xl` | `font-weight-semibold` | `line-height-tight` | `font-family-heading` |
| H4 | `font-size-2xl` | `font-weight-semibold` | `line-height-tight` | `font-family-heading` |
| H5 | `font-size-xl` | `font-weight-semibold` | `line-height-normal` | `font-family-base` |
| H6 | `font-size-lg` | `font-weight-semibold` | `line-height-normal` | `font-family-base` |
| Body | `font-size-base` | `font-weight-normal` | `line-height-normal` | `font-family-base` |
| Small | `font-size-sm` | `font-weight-normal` | `line-height-normal` | `font-family-base` |
| Code | `font-size-sm` | `font-weight-normal` | `line-height-relaxed` | `font-family-mono` |

---

## Accessibility Notes

- Minimum body text size: `font-size-sm` (14 px). Do not go below this for any readable content.
- Do not rely on font weight or style alone to convey meaning — pair with color and/or an icon.
- `font-weight-light` (300) may fail WCAG legibility for low-vision users — always validate with a screen reader.
