# Skill: Design Token Lookup

**Description:** Find the right design token for any visual property — color, typography, spacing, border, shadow, or motion — before writing CSS or style code. Always look up the token name and verify its light/dark values rather than hardcoding raw values.

---

## Why Use Token Lookup?

Using token names instead of raw values:
- Ensures correct light **and** dark mode support automatically.
- Keeps the codebase aligned with the central style guide.
- Prevents WCAG contrast failures caused by ad-hoc color choices.
- Makes future palette updates automatic (one token change, all components update).

**Rule:** Never use a hex value, rem value, or named color directly in component styles. Always use a token.

---

## Token Categories

### 1. Color Tokens

**Document:** `get_guide("01-color-palette")`

#### Brand Colors

| Token | Light | Dark | Use for |
|-------|-------|------|---------|
| `color-primary` | `#A16207` | `#F2C14E` | Primary buttons, active links, key actions |
| `color-primary-light` | `#D4A72C` | `#FFD166` | Hover states, tinted backgrounds |
| `color-primary-dark` | `#7C4A03` | `#D4A72C` | Pressed states, high-contrast variant |
| `color-secondary` | `#6C757D` | `#ADB5BD` | Secondary buttons, less prominent labels |

#### Semantic Colors (Surface Colors)

| Token | Light | Dark | Use for |
|-------|-------|------|---------|
| `color-success` | `#D4EDDA` | `#1A3A22` | Success banners, confirmation panels |
| `color-warning` | `#FFF3CD` | `#3D2E00` | Warning banners, caution panels |
| `color-error` | `#F8D7DA` | `#3D0A0D` | Error banners, validation summaries |
| `color-info` | `#D1ECF1` | `#0A2C31` | Informational notices |

> Semantic colors are **surface/background tokens** — always place readable text on top.

#### Text Colors

| Token | Light | Dark | Use for |
|-------|-------|------|---------|
| `color-text-primary` | `#212529` | `#F8F9FA` | Body text, headings |
| `color-text-secondary` | `#6C757D` | `#CED4DA` | Supporting text, metadata |
| `color-text-disabled` | `#ADB5BD` | `#6C757D` | Disabled controls |
| `color-text-inverse` | `#FFFFFF` | `#212529` | Text on dark/colored surfaces |
| `color-text-link` | `#A16207` | `#F2C14E` | Hyperlinks |

#### Background Colors

| Token | Light | Dark | Use for |
|-------|-------|------|---------|
| `color-background` | `#FFFFFF` | `#0F172A` | Primary page/panel background |
| `color-background-alt` | `#F8F9FA` | `#1E293B` | Alternating rows, sidebar, card surface |
| `color-background-raised` | `#FFFFFF` | `#334155` | Dialogs, popovers, dropdowns |
| `color-background-overlay` | `rgba(0,0,0,0.40)` | `rgba(0,0,0,0.60)` | Modal backdrop/scrim |

#### Border Colors

| Token | Light | Dark | Use for |
|-------|-------|------|---------|
| `color-border` | `#DEE2E6` | `#475569` | Default dividers, input outlines |
| `color-border-strong` | `#ADB5BD` | `#64748B` | Emphasized borders |
| `color-border-focus` | `#A16207` | `#F2C14E` | Keyboard focus rings (≥2px width required) |

---

### 2. Typography Tokens

**Document:** `get_guide("02-typography")`

#### Font Families

| Token | Value | Use for |
|-------|-------|---------|
| `font-family-base` | `'Inter', sans-serif` fallback chain | Body text, labels, inputs |
| `font-family-heading` | `'Poppins', sans-serif` fallback chain | H1–H4 headings |
| `font-family-mono` | `'Fira Code', monospace` fallback chain | Code blocks, inline code |

#### Font Sizes

| Token | rem | px | Use for |
|-------|-----|-----|---------|
| `font-size-xs` | `0.75rem` | 12 px | Helper text, badges |
| `font-size-sm` | `0.875rem` | 14 px | Secondary body, table cells, hints |
| `font-size-base` | `1rem` | 16 px | Default body text |
| `font-size-lg` | `1.125rem` | 18 px | Lead paragraphs, emphasized body |
| `font-size-xl` | `1.25rem` | 20 px | Card titles, section sub-labels |
| `font-size-2xl` | `1.5rem` | 24 px | H4 headings |
| `font-size-3xl` | `1.875rem` | 30 px | H3 headings |
| `font-size-4xl` | `2.25rem` | 36 px | H2 headings |
| `font-size-5xl` | `3rem` | 48 px | H1 headings, display/hero text |

> Always use `rem` tokens — never hardcode `px` font sizes.

---

### 3. Spacing and Layout Tokens

**Document:** `get_guide("03-spacing-and-layout")`

Use `get_guide("03-spacing-and-layout")` to retrieve the full spacing scale, border radius values, and box-shadow tokens.

Key points:
- The spacing scale is fixed and token-based (e.g., `spacing-1`, `spacing-2`, ..., `spacing-16`).
- Border radius tokens cover `none`, `sm`, `md`, `lg`, and `full`.
- Shadow tokens cover `shadow-sm`, `shadow-md`, `shadow-lg`, `shadow-xl`.

---

### 4. Motion and Interaction Tokens

**Document:** `get_guide("04-motion-and-interaction")`

Use `get_guide("04-motion-and-interaction")` to retrieve:
- Transition duration tokens (`duration-fast`, `duration-normal`, `duration-slow`)
- Easing curve tokens (`ease-in`, `ease-out`, `ease-in-out`)
- Focus state requirements and `prefers-reduced-motion` guidance

---

## Token Lookup Workflow

```
Step 1: Identify what you need
  → Color? → get_guide("01-color-palette")
  → Typography? → get_guide("02-typography")
  → Spacing/radius/shadow? → get_guide("03-spacing-and-layout")
  → Motion/transitions? → get_guide("04-motion-and-interaction")

Step 2: Find the correct token
  → Scan the token table for the semantic group that fits
  → Pick the token by purpose, not by value

Step 3: Verify the token
  → Does it have both light and dark values?
  → Does the contrast meet the stated requirement?

Step 4: Use the token in your code
  → CSS: var(--color-primary)
  → SCSS: $color-primary
  → Framework variable: as documented in ADR 0011
```

---

## Quick Lookup by Scenario

| Scenario | Token to use |
|----------|-------------|
| Primary action button | `color-primary` (bg), `color-text-inverse` (text) |
| Secondary/ghost button | `color-secondary` (border/text) |
| Page background | `color-background` |
| Card or panel surface | `color-background-alt` |
| Modal or popover | `color-background-raised` |
| Body text | `color-text-primary` |
| Hint or metadata | `color-text-secondary` |
| Disabled element | `color-text-disabled` |
| Hyperlink | `color-text-link` |
| Form input border | `color-border` |
| Focus ring | `color-border-focus` (≥2px) |
| Success state banner | `color-success` (bg) |
| Error/validation banner | `color-error` (bg) |
| Warning notice | `color-warning` (bg) |
| Code block | `font-family-mono` |
| Page heading (H1) | `font-size-5xl`, `font-family-heading` |

---

## Common Mistakes

| ❌ Wrong | ✅ Correct |
|---------|-----------|
| `color: #A16207` | `color: var(--color-primary)` |
| `background: #FFFFFF` | `background: var(--color-background)` |
| `font-size: 16px` | `font-size: var(--font-size-base)` |
| `border: 1px solid #DEE2E6` | `border: 1px solid var(--color-border)` |
| Inventing a new shade | Use the nearest defined token |

---

## Tips

- **Start with purpose, not value** — ask "what is this element communicating?" then pick the semantic token.
- **Pair background and text tokens** — each background token has defined companion text tokens for contrast.
- **Check the full doc** — the quick tables above are summaries; use `get_guide()` to get full usage rules.
- **Spacing and motion are fixed** — do not introduce custom values outside the defined scale.
