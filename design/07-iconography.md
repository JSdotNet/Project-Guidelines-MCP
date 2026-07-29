---
title: "Style Guide: Iconography"
date: 2026-07-28
tags: [style-guide, iconography, icons, accessibility, design-tokens]
---
# Iconography

> This document defines the shared icon baseline for web products. Lucide is the default library, but compatible SVG substitutions are allowed when they preserve the same visual and accessibility characteristics.

---

## Icon Library

This style guide uses **Lucide** as the default icon library.

| Property | Value |
|---|---|
| Library | [Lucide](https://lucide.dev) |
| Style | Outline, stroke-based, 2 px stroke width |
| Grid size | 24 x 24 px base |
| License | ISC (open source, suitable for commercial use) |

### Rationale

- A consistent 2 px stroke weight harmonizes visually with the `border-width-2` token and the Inter and Poppins typefaces.
- Lucide's outline style remains legible at small sizes and in both light and dark mode.
- The library provides broad coverage without requiring a custom icon font.

### Compatible Substitutions

If a needed icon is not available in Lucide, a substitute is allowed only when it:

1. Uses the same stroke-based style and roughly the same 2 px visual weight.
2. Is shipped as SVG, not as an icon font or bitmap asset.
3. Is added to a project-local icon registry so the team can discover and reuse it.

---

## Sizing Scale

| Token | Size | Usage |
|---|---|---|
| `icon-xs` | 12 x 12 px | Inline badge indicator, status dot complement |
| `icon-sm` | 16 x 16 px | Inline text icons, dense table actions |
| `icon-md` | 20 x 20 px | Default for most UI contexts (buttons, inputs, nav items) |
| `icon-base` | 24 x 24 px | Standard icon grid; use when the icon stands alone |
| `icon-lg` | 32 x 32 px | Section headers, feature highlights |
| `icon-xl` | 48 x 48 px | Empty-state illustrations, onboarding |
| `icon-2xl` | 64 x 64 px | Hero or splash illustrations only |

### Usage Rules

- Default to `icon-md` (20 px) when placing an icon inside a button, input, or navigation item.
- Never scale SVG icons via `font-size` - use explicit `width` and `height` or CSS dimensions.
- Do not mix icon sizes within the same control group.

---

## Color Rules

Icons inherit color from the surrounding text context using `currentColor`. This ensures automatic adaptation to light mode, dark mode, and disabled states.

| Context | Color Token |
|---|---|
| Default inline icon | `color-text-primary` (via `currentColor`) |
| Secondary or supporting icon | `color-text-secondary` |
| Disabled icon | `color-text-disabled` |
| Icon on brand or primary fill | `color-text-inverse` |
| Semantic icon (success) | Foreground text on `color-success` surface |
| Semantic icon (warning) | Foreground text on `color-warning` surface |
| Semantic icon (error) | Foreground text on `color-error` surface |
| Semantic icon (info) | Foreground text on `color-info` surface |

### Usage Rules

- Never apply a custom `fill` or `stroke` color directly in product code. Use `currentColor` and control color through the parent element.
- Do **not** rely on icon color alone to convey meaning - always pair it with a text label or ARIA annotation.
- Ensure icon color meets the **3:1** contrast ratio against its background.

---

## Accessibility Requirements

### Icon-only elements

When an icon appears without a visible text label, a text alternative is **mandatory**:

| Element type | Required approach |
|---|---|
| `<button>` | `aria-label="Action name"` on the button element |
| `<a>` | `aria-label="Link purpose"` on the anchor element |
| Standalone `<svg>` | `role="img"` plus `<title>` inside the SVG |
| Purely decorative SVG | `aria-hidden="true"` |

### Icon + label elements

When an icon accompanies visible text, hide the icon from assistive technology:

```html
<button>
  <svg aria-hidden="true" focusable="false">...</svg>
  Save changes
</button>
```

- Set `aria-hidden="true"` on the SVG.
- Set `focusable="false"` so the SVG itself does not receive focus.

### Touch targets

- Icon buttons must have a minimum touch target of **44 x 44 px**, even if the icon itself is 20 px.

---

## Usage Patterns

### Icon in Button

```
[icon] Label       <- leading icon (most common)
Label [icon]       <- trailing icon (chevron or expand only)
  [icon]           <- icon-only (requires aria-label)
```

### Icon in Navigation

- Navigation items use `icon-md` (20 px) alongside the label.
- Active navigation items use `color-primary` for the icon color.
- Collapsed or icon-only navigation must surface tooltips for all items.

### Status Icons

Use consistent icons for semantic states across the product:

| State | Default Icon | Color Context |
|---|---|---|
| Success | `check-circle` | `color-success` surface |
| Warning | `alert-triangle` | `color-warning` surface |
| Error | `x-circle` | `color-error` surface |
| Info | `info` | `color-info` surface |
| Loading | `loader-2` (spinning) | `color-text-secondary` |
| Empty | `inbox` or `folder-open` | `color-text-secondary` |

---

## Do / Don't

| Yes | No |
|---|---|
| Use SVG icons with `currentColor` | Use raster icons for UI |
| Provide `aria-label` on icon-only buttons | Omit text alternatives for icon-only controls |
| Use the defined sizing scale | Set arbitrary sizes like 17 px or 22 px |
| Hide decorative icons with `aria-hidden="true"` | Leave decorative icons in the accessibility tree |
| Pair semantic icons with text labels or `aria-label` | Rely on icon color alone to communicate state |
