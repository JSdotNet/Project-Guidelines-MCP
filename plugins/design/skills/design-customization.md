# Skill: Design Customization

**Description:** Customize the project-level color scheme by overriding brand and semantic color tokens. Covers which tokens may be overridden, the override contract (contrast requirements, both light/dark variants), and what must never be changed.

---

## What You May Customize

**Color tokens are the only permitted customization.** All other design tokens — typography, spacing, border radius, shadows, motion — are fixed and shared across all projects.

```
get_guide("05-customization-guide")   ← full override contract
get_guide("01-color-palette")         ← default token values to override
```

---

## Overridable Tokens

You may override tokens in two groups:

### Brand Colors (overridable)

| Token | Default Light | Default Dark |
|-------|--------------|-------------|
| `color-primary` | `#A16207` | `#F2C14E` |
| `color-primary-light` | `#D4A72C` | `#FFD166` |
| `color-primary-dark` | `#7C4A03` | `#D4A72C` |
| `color-secondary` | `#6C757D` | `#ADB5BD` |

### Semantic Colors (overridable)

| Token | Default Light | Default Dark |
|-------|--------------|-------------|
| `color-success` | `#D4EDDA` | `#1A3A22` |
| `color-warning` | `#FFF3CD` | `#3D2E00` |
| `color-error` | `#F8D7DA` | `#3D0A0D` |
| `color-info` | `#D1ECF1` | `#0A2C31` |

---

## What You May NOT Override

| Token group | Reason |
|-------------|--------|
| `color-text-*` | Contrast-validated against the default palette; overriding risks WCAG failures |
| `color-background-*` | Same — tightly coupled to text contrast requirements |
| `color-border-*` | Same |
| All typography tokens | Fixed for consistency across the organization |
| All spacing tokens | Fixed for consistent information density |
| All border-radius tokens | Fixed |
| All motion/transition tokens | Fixed |

---

## The Override Contract

Before overriding any token, you must satisfy all three constraints:

### Constraint 1: Both Light and Dark Variants Are Required

Every override must provide a value for **both** light mode and dark mode.

```
✅ Correct:
  --color-primary-light-mode: #0D6EFD;
  --color-primary-dark-mode:  #6EA8FE;

❌ Wrong (partial override — only light mode):
  --color-primary: #0D6EFD;  /* missing dark mode value */
```

### Constraint 2: Contrast Requirements Must Be Met

| Token overridden | Minimum contrast | Against |
|-----------------|-----------------|---------|
| `color-primary` | 4.5:1 | `color-background` (light) AND `color-background` (dark) |
| `color-primary` | 4.5:1 | `color-text-inverse` (when used as button background) |
| Semantic surface (`color-success`, etc.) | 4.5:1 | Intended foreground text/icon color on top of it |

Use a contrast checker (e.g., [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)) before committing.

### Constraint 3: Semantic Surfaces Must Stay Distinct

When overriding semantic tokens, each surface must remain clearly distinguishable:
- `color-success` ≠ `color-warning` ≠ `color-error` ≠ `color-info`
- Each must differ clearly from `color-background` (users rely on semantic distinction)

---

## Customization Workflow

```
Step 1: Read the full override contract
  → get_guide("05-customization-guide")

Step 2: Note the default values you're replacing
  → get_guide("01-color-palette") → Brand Colors and Semantic Colors sections

Step 3: Choose your replacement values
  → Supply both light-mode and dark-mode values for every token

Step 4: Validate contrast
  → color-primary (light) on color-background (light):  ≥4.5:1
  → color-primary (dark)  on color-background (dark):   ≥4.5:1
  → Each semantic surface with its foreground text:       ≥4.5:1

Step 5: Verify semantic distinctness
  → All four semantic surfaces differ from each other and from color-background

Step 6: Implement using the override mechanism defined in ADR 0011
  → Check jsdotnet-coding-guidelines: get_guide("adr-0011-...")
  → Apply as SCSS variable overrides or CSS custom property overrides per ADR 0011

Step 7: Test both modes
  → Render the application in light mode and dark mode
  → Confirm all interactive elements have visible focus rings
  → Confirm semantic banners are distinct
```

---

## Contrast Validation Reference

Run these checks with a contrast-checking tool:

| Pair to validate | Minimum ratio |
|-----------------|--------------|
| New `color-primary` (light) on `#FFFFFF` (`color-background` light) | 4.5:1 |
| New `color-primary` (dark) on `#0F172A` (`color-background` dark) | 4.5:1 |
| New `color-primary` on `color-text-inverse` (for button text) | 4.5:1 |
| New `color-success` surface + intended foreground text | 4.5:1 |
| New `color-error` surface + intended foreground text | 4.5:1 |
| New `color-warning` surface + intended foreground text | 4.5:1 |
| New `color-info` surface + intended foreground text | 4.5:1 |

---

## Common Customization Mistakes

| ❌ Problem | ✅ Fix |
|-----------|--------|
| Overriding `color-text-primary` | This is not permitted — use brand/semantic tokens only |
| Providing only the light-mode value | Always provide both light and dark values |
| Skipping contrast check | Use a contrast checker before merging |
| Using the new brand color for semantic states | Semantic colors must be independently set to communicate distinct meaning |
| Making `color-success` and `color-info` too similar | Each semantic surface must be clearly distinguishable |

---

## Relationship to ADR 0011

The *override mechanism* (how to declare SCSS overrides or CSS custom property overrides in your project) is defined in ADR 0011 in the coding guidelines:

```
jsdotnet-coding-guidelines → get_guide("adr-0011-...")
```

This skill (design customization) governs **which tokens** may be overridden and **what values** are valid. ADR 0011 governs **how** to wire those overrides into your project's build system.

---

## Tips

- **Read `05-customization-guide` before starting** — it contains the full contract and additional edge cases.
- **Test in both modes** — dark-mode contrast is often overlooked and fails differently from light mode.
- **Do not override to "fix" a text or background color** — if the default text or background contrast is insufficient, raise a proposal against the style guide instead of overriding.
- **Semantic distinctness matters** — users have learned that green = success, yellow = warning, red = error. Do not subvert these conventions.
