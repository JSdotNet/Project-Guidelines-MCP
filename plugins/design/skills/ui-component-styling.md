# Skill: UI Component Styling

**Description:** Style UI components correctly using the JSdotNet design token system. Covers button states, form inputs, cards, typography hierarchy, semantic feedback (success/warning/error/info), focus management, and accessibility rules.

---

## Before You Style Anything

1. **Retrieve the token catalog**: `get_guide("01-color-palette")` for colors, `get_guide("02-typography")` for fonts and sizes.
2. **Identify the component's purpose**: What state is it communicating? What hierarchy does it occupy?
3. **Pick tokens by purpose**: Use semantic names (`color-primary`, `color-error`) — never raw hex values.
4. **Verify contrast**: All foreground/background pairs must meet WCAG 2.1 AA (4.5:1 normal text).

---

## Component Styling Patterns

### Buttons

#### Primary Button

```
Background:  color-primary
Text:        color-text-inverse
Hover:       color-primary-light (background)
Active/Press: color-primary-dark (background)
Disabled:    color-text-disabled (text), reduced opacity background
Focus ring:  color-border-focus, ≥2px outline
```

```
Verify: color-primary + color-text-inverse must have ≥4.5:1 contrast
Tool: get_guide("01-color-palette") → Brand Colors section
```

#### Secondary Button

```
Background:  transparent or color-background
Text:        color-secondary
Border:      color-border (default), color-secondary (hover)
Disabled:    color-text-disabled
Focus ring:  color-border-focus, ≥2px outline
```

#### Destructive / Danger Button

```
Background:  color-error (surface)
Text:        color-text-primary (on the error surface)
Focus ring:  color-border-focus, ≥2px outline
```

> Note: `color-error` is a soft surface — verify foreground contrast before using it as a solid button background.

---

### Form Inputs

```
Background:   color-background
Border:       color-border (default)
Border hover: color-border-strong
Border focus: color-border-focus, ≥2px (mandatory)
Label text:   color-text-primary
Helper text:  color-text-secondary
Placeholder:  color-text-disabled (visual only, not a contrast-required element)
Error state:  color-error (border), supplemented by icon + error message text
Disabled:     color-text-disabled (label/value), color-background-alt (background)
```

**Accessibility requirement**: Always pair focus with a visible `color-border-focus` ring ≥2px. Never rely on color alone to indicate state — always add an icon, shape change, or label text.

---

### Cards and Panels

```
Default card:       color-background-alt
Elevated/modal:     color-background-raised
Card border:        color-border
Heading in card:    color-text-primary, font-family-heading, font-size-xl or font-size-2xl
Body text in card:  color-text-primary, font-family-base, font-size-base
Metadata/timestamp: color-text-secondary, font-size-sm
```

---

### Typography Hierarchy

Use these combinations for consistent visual hierarchy:

| Level | Font family | Size token | Weight | Color token |
|-------|------------|-----------|--------|------------|
| H1 / Hero | `font-family-heading` | `font-size-5xl` | Bold | `color-text-primary` |
| H2 | `font-family-heading` | `font-size-4xl` | Bold | `color-text-primary` |
| H3 | `font-family-heading` | `font-size-3xl` | SemiBold | `color-text-primary` |
| H4 | `font-family-heading` | `font-size-2xl` | SemiBold | `color-text-primary` |
| Lead paragraph | `font-family-base` | `font-size-lg` | Regular | `color-text-primary` |
| Body | `font-family-base` | `font-size-base` | Regular | `color-text-primary` |
| Secondary body | `font-family-base` | `font-size-sm` | Regular | `color-text-secondary` |
| Helper/hint | `font-family-base` | `font-size-xs` | Regular | `color-text-secondary` |
| Code | `font-family-mono` | `font-size-sm` | Regular | `color-text-primary` |
| Link | `font-family-base` | (inherited) | Regular | `color-text-link` |

---

### Semantic Feedback Components

Semantic colors are **soft surface colors** — use them as background containers, never as strong standalone button colors.

#### Success Banner / Confirmation

```
Background:  color-success
Text:        color-text-primary (standard body text)
Icon:        ✓ Checkmark, color-text-primary
Border:      color-border (optional subtle outline)
```

#### Warning Notice

```
Background:  color-warning
Text:        color-text-primary
Icon:        ⚠ Warning symbol, color-text-primary
```

#### Error / Validation Summary

```
Background:  color-error
Text:        color-text-primary
Icon:        ✕ Error symbol, color-text-primary
```

#### Info Notice

```
Background:  color-info
Text:        color-text-primary
Icon:        ℹ Info symbol, color-text-primary
```

> **Rule:** Semantic surfaces alone are not sufficient. Always supplement with an icon and/or text label — never convey state through color alone.

---

### Navigation and Links

```
Link text:           color-text-link
Link hover:          color-primary (underline recommended)
Active/current item: color-primary (text or left-border accent)
Visited link:        color-text-secondary (optional; check design intent)
Focus ring:          color-border-focus, ≥2px
```

---

### Modal / Dialog Overlay

```
Backdrop:         color-background-overlay (rgba token)
Dialog surface:   color-background-raised
Dialog border:    color-border (optional)
Focus trap:       Ensure first interactive element inside modal has focus-ring visible
Close button:     color-text-secondary (icon), focus ring required
```

---

## Accessibility Checklist

Before finalizing component styles:

- [ ] All text/background pairs meet ≥4.5:1 contrast (WCAG 2.1 AA)
- [ ] Large text (≥18pt / ≥14pt bold) and UI components meet ≥3:1 contrast
- [ ] Focus ring uses `color-border-focus` and is ≥2px wide
- [ ] No information conveyed by color alone (always add icon, shape, or text)
- [ ] Motion respects `prefers-reduced-motion` (see `get_guide("04-motion-and-interaction")`)
- [ ] Disabled states use `color-text-disabled` — exempt from contrast requirements
- [ ] Interactive elements have a hover state distinguishable from the default state

---

## Styling Workflow

```
Step 1: Identify the component type
  → Button, input, card, banner, navigation?

Step 2: Retrieve relevant token docs
  → get_guide("01-color-palette")     (colors)
  → get_guide("02-typography")         (fonts/sizes)
  → get_guide("03-spacing-and-layout") (spacing/radius)
  → get_guide("04-motion-and-interaction") (transitions)

Step 3: Map each visual property to a token
  → Background → color-background-* or color-*
  → Text → color-text-*
  → Border → color-border-*
  → Size → font-size-*
  → Spacing → spacing-*

Step 4: Verify accessibility
  → Check foreground/background contrast pairs
  → Confirm focus ring is present and visible

Step 5: Implement using token variables
  → var(--token-name) in CSS
  → $token-name in SCSS
  → Follow ADR 0011 for your framework
```

---

## Common Mistakes

| ❌ Problem | ✅ Fix |
|-----------|--------|
| Using `color-success` as a button color | Use `color-primary` for actions; `color-success` is a surface |
| No focus ring on interactive elements | Add `outline: 2px solid var(--color-border-focus)` |
| Semantic state conveyed by color only | Add icon + text label alongside color |
| Custom `font-size: 13px` | Use `font-size-sm` (14px) from the scale |
| Disabled element meets 4.5:1 contrast | `color-text-disabled` is intentionally exempt — don't "fix" it |
| Only implementing light mode | Every token has a dark variant — implement both |

---

## Tips

- **Use `get_guide` before styling** — the quick tables in this skill are summaries; the full docs have additional rules and edge cases.
- **Semantic tokens adapt automatically** — `color-text-primary` is dark in light mode, light in dark mode. You get both with one token.
- **Elevation is expressed by surface** — in dark mode, elevation is communicated via `color-background-raised` (lighter surface), not just shadows.
- **Only brand/semantic colors are customizable** — do not attempt to customize typography, spacing, or motion tokens.
