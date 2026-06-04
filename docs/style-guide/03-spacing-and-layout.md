---
title: "Style Guide: Spacing and Layout"
date: 2026-06-04
tags: [style-guide, spacing, layout, design-tokens]
---
# Spacing and Layout

> Spacing, border radius, and shadow tokens are **fixed** — they are not customizable per project.

---

## Spacing Scale

The spacing system is based on a **4 px (0.25 rem) base unit** using a geometric progression. All values are expressed in `rem`.

| Token | rem Value | px Equivalent | Common Usage |
|---|---|---|---|
| `spacing-0` | `0` | 0 px | Explicit zero — removes inherited spacing |
| `spacing-xs` | `0.25rem` | 4 px | Micro-gaps between icon and label, badge padding |
| `spacing-sm` | `0.5rem` | 8 px | Input padding (vertical), button padding (vertical), inline gaps |
| `spacing-md` | `1rem` | 16 px | Default component padding, card body, form field spacing |
| `spacing-lg` | `1.5rem` | 24 px | Section padding (small), card header/footer |
| `spacing-xl` | `2rem` | 32 px | Section padding (medium), modal padding |
| `spacing-2xl` | `3rem` | 48 px | Section padding (large), page-level vertical rhythm |
| `spacing-3xl` | `4rem` | 64 px | Hero sections, large screen whitespace |
| `spacing-4xl` | `6rem` | 96 px | Page margins on wide viewports, splash sections |

### Usage Rules

- **Always** use a token from the scale. Never introduce one-off values such as `13px` or `1.3rem`.
- Prefer `spacing-md` (16 px) as the default padding unit for component internals.
- Use `spacing-lg` or `spacing-xl` for outer container / section padding.
- Horizontal and vertical spacing should both come from the same scale for visual harmony.

---

## Layout Grid

| Concept | Value |
|---|---|
| Column count | 12 |
| Default column gutter | `spacing-md` (16 px) |
| Narrow container max-width | `640px` |
| Default container max-width | `1280px` |
| Wide container max-width | `1536px` |
| Responsive breakpoints | `sm` ≥ 640 px · `md` ≥ 768 px · `lg` ≥ 1024 px · `xl` ≥ 1280 px · `2xl` ≥ 1536 px |

---

## Border Radius

| Token | Value | Usage |
|---|---|---|
| `border-radius-none` | `0` | Square components — data tables, code blocks |
| `border-radius-sm` | `0.25rem` | Small controls — badges, chips, tags (4 px) |
| `border-radius-md` | `0.5rem` | Default — inputs, buttons, cards (8 px) |
| `border-radius-lg` | `1rem` | Large cards, panels, modals (16 px) |
| `border-radius-xl` | `1.5rem` | Feature cards, hero images (24 px) |
| `border-radius-full` | `9999px` | Fully rounded — pill buttons, avatar circles |

### Usage Rules

- Interactive controls (buttons, inputs, selects) use `border-radius-md` by default.
- Cards and panels use `border-radius-md` or `border-radius-lg` depending on visual weight.
- Never mix `border-radius-none` and `border-radius-full` on adjacent controls in the same group.

---

## Border Width

| Token | Value | Usage |
|---|---|---|
| `border-width` | `1px` | Default border — inputs, cards, dividers |
| `border-width-2` | `2px` | Emphasis borders — focus rings, selected states |
| `border-width-4` | `4px` | Alert accent stripes, progress bars |

---

## Shadows

Shadows use `rgba` values so they compose correctly over any background.

| Token | Value | Usage |
|---|---|---|
| `shadow-none` | `none` | Flat surfaces, disabled cards |
| `shadow-sm` | `0 1px 2px rgba(0,0,0,0.05)` | Subtle lift — inputs on dark backgrounds, inline chips |
| `shadow-md` | `0 4px 6px rgba(0,0,0,0.10)` | Default card elevation |
| `shadow-lg` | `0 10px 15px rgba(0,0,0,0.15)` | Dropdown menus, popovers |
| `shadow-xl` | `0 20px 25px rgba(0,0,0,0.20)` | Modals, drawers, toasts |
| `shadow-inner` | `inset 0 2px 4px rgba(0,0,0,0.06)` | Pressed / active states, inset inputs |

### Usage Rules

- Use `shadow-md` for all interactive cards; upgrade to `shadow-lg` on hover.
- Use `shadow-xl` exclusively for top-layer elements: modals, toast notifications, command palettes.
- `shadow-inner` is appropriate for text inputs and "active/pressed" button states.
- In dark mode, reduce shadow opacity by ~30% or prefer `color-background-raised` instead of shadow for elevation.

---

## Z-Index Scale

| Token | Value | Layer |
|---|---|---|
| `z-index-base` | `0` | Normal document flow |
| `z-index-raised` | `10` | Sticky headers, floating action buttons |
| `z-index-dropdown` | `100` | Dropdown menus, date pickers |
| `z-index-overlay` | `200` | Side drawers, slide-over panels |
| `z-index-modal` | `300` | Modal dialogs |
| `z-index-toast` | `400` | Toast / snackbar notifications |
| `z-index-tooltip` | `500` | Tooltips and popovers (always on top) |

### Usage Rules

- Never use arbitrary z-index values (e.g., `z-index: 9999`). Use only the defined scale.
- Ensure focus-trap logic respects the z-index layer for modals and overlays.
