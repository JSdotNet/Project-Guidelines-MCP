---
title: "Style Guide: Iconography"
date: 2026-07-28
tags: [style-guide, iconography, icons, accessibility, design-tokens]
---
# Iconography

> Icon tokens are **fixed** — icon library, sizing rules, and usage principles are not customizable per project.

---

## Icon Library

This style guide adopts **Lucide** as the standard icon library.

| Property | Value |
|---|---|
| Library | [Lucide](https://lucide.dev) |
| Style | Outline / stroke-based, 2 px stroke width |
| Grid size | 24 × 24 px base |
| License | ISC (open source, suitable for commercial use) |

### Rationale

- Consistent stroke weight (2 px) harmonizes visually with the `border-width-2` token and the Inter/Poppins type faces.
- Lucide's outline style remains legible at small sizes and in both light and dark mode.
- The library provides broad coverage (1 500+ icons) without requiring a custom icon font.

### Alternatives

If a needed icon is not available in Lucide, the following criteria must be met for any substitute:

1. Same stroke-based style and 2 px stroke width.
2. Available as an SVG (not a font icon).
3. Added to a project-local icon registry so it is discoverable by the team.

---

## Sizing Scale

| Token | Size | Usage |
|---|---|---|
| `icon-xs` | 12 × 12 px | Inline badge indicator, status dot complement |
| `icon-sm` | 16 × 16 px | Inline text icons, dense table actions |
| `icon-md` | 20 × 20 px | Default for most UI contexts (buttons, inputs, nav items) |
| `icon-base` | 24 × 24 px | Standard Lucide grid; use when icon stands alone |
| `icon-lg` | 32 × 32 px | Section headers, feature highlights |
| `icon-xl` | 48 × 48 px | Empty state illustrations, onboarding |
| `icon-2xl` | 64 × 64 px | Hero / splash illustrations only |

### Usage Rules

- Default to `icon-md` (20 px) when placing an icon inside a button, input, or navigation item.
- Never scale SVG icons via `font-size` — use explicit `width` and `height` attributes or CSS dimensions.
- Do not mix icon sizes within the same control group (e.g., a toolbar with some 16 px and some 24 px icons).

---

## Color Rules

Icons inherit color from the surrounding text context using `currentColor`. This ensures automatic adaptation to light/dark mode and disabled states.

| Context | Color Token |
|---|---|
| Default inline icon | `color-text-primary` (via `currentColor`) |
| Secondary / supporting icon | `color-text-secondary` |
| Disabled icon | `color-text-disabled` |
| Icon on brand / primary fill | `color-text-inverse` |
| Semantic icon (success) | Foreground text on `color-success` surface |
| Semantic icon (warning) | Foreground text on `color-warning` surface |
| Semantic icon (error) | Foreground text on `color-error` surface |
| Semantic icon (info) | Foreground text on `color-info` surface |

### Usage Rules

- Never apply a custom `fill` or `stroke` color directly to an icon in product code. Use `currentColor` and control color through the parent element's text color.
- Do **not** rely on icon color alone to convey meaning — always pair with a text label or ARIA annotation.
- Ensure the icon color meets the **3:1** contrast ratio against its background (WCAG 2.1 AA for non-text UI elements).

---

## Accessibility Requirements

### Icon-only elements

When an icon appears without a visible text label, a text alternative is **mandatory**:

| Element type | Required approach |
|---|---|
| `<button>` | `aria-label="Action name"` on the button element |
| `<a>` | `aria-label="Link purpose"` on the anchor element |
| Standalone `<svg>` | `role="img"` + `<title>` element inside the SVG |
| Purely decorative SVG | `aria-hidden="true"` — removes from accessibility tree |

### Icon + label elements

When an icon accompanies visible text, hide the icon from assistive technology:

```html
<button>
  <svg aria-hidden="true" focusable="false">...</svg>
  Save changes
</button>
```

- Set `aria-hidden="true"` on the SVG.
- Set `focusable="false"` to prevent IE/Edge from placing focus on the SVG itself.

### Touch targets

- Icon buttons must have a minimum touch target of **44 × 44 px**, even if the icon itself is 20 px. Add padding or use a wrapping element to achieve this.

---

## Usage Patterns

### Icon in Button

```
[icon] Label       ← leading icon (most common)
Label [icon]       ← trailing icon (chevron / expand only)
  [icon]           ← icon-only (requires aria-label)
```

### Icon in Navigation

- Navigation items use `icon-md` (20 px) alongside the label.
- Active navigation items use `color-primary` for the icon color.
- Collapsed/icon-only navigation must surface tooltips for all items.

### Status Icons

Use consistent icons for semantic states across the product:

| State | Lucide Icon | Color Context |
|---|---|---|
| Success | `check-circle` | `color-success` surface |
| Warning | `alert-triangle` | `color-warning` surface |
| Error | `x-circle` | `color-error` surface |
| Info | `info` | `color-info` surface |
| Loading | `loader-2` (spinning) | `color-text-secondary` |
| Empty | `inbox` or `folder-open` | `color-text-secondary` |

---

## Do / Don't

| ✅ Do | ❌ Don't |
|---|---|
| Use SVG icons with `currentColor` | Use raster (PNG/JPG) icons for UI |
| Provide `aria-label` on icon-only buttons | Omit text alternatives for icon-only controls |
| Use the defined sizing scale | Set arbitrary sizes like 17 px or 22 px |
| Hide decorative icons with `aria-hidden="true"` | Leave decorative icons in the accessibility tree |
| Pair semantic icons with text labels or `aria-label` | Rely on icon color alone to communicate state |
