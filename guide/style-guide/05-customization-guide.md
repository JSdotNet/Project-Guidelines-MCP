---
title: "Style Guide: Customization Guide"
date: 2026-06-05
tags: [style-guide, customization, color, design-tokens, branding]
---
# Customization Guide

## What You May Customize

**Color tokens are the only permitted customization.** All other design tokens — typography, spacing, border radius, shadows, motion — are fixed and shared across all projects.

This constraint exists to:
- Maintain a coherent, accessible UX baseline across the organization's products.
- Reduce design decision overhead for each project team.
- Ensure consistent information density, readability, and interaction patterns.

If you believe a non-color token should be overridable for a legitimate reason, raise it as a proposal against this style guide rather than making local overrides.

---

## Which Tokens May Be Overridden

You may override any token in the **Brand Colors** group and the **Semantic Colors** group from [01 — Color Palette](01-color-palette.md):

| Overridable Token Group | Tokens |
|---|---|
| Brand | `color-primary`, `color-primary-light`, `color-primary-dark`, `color-secondary` |
| Semantic | `color-success`, `color-warning`, `color-error`, `color-info` |

You **may not** override:
- Any `color-text-*` token
- Any `color-background-*` token
- Any `color-border-*` token
- Any typography, spacing, radius, shadow, or motion token

> **Rationale:** Text, background, and border tokens are tightly coupled to accessibility contrast requirements that have been validated for the default palette. Overriding them without re-validating all contrast pairs risks WCAG non-compliance.

---

## Override Contract

When overriding brand or semantic colors, the following constraints **must** be satisfied:

### 1. Both Light and Dark Variants Required

Every overridden token must supply both a light-mode value and a dark-mode value. Partial overrides (e.g., only light mode) are not permitted.

### 2. Contrast Requirements Must Be Maintained

| Token Override | Minimum Contrast Requirement |
|---|---|
| `color-primary` | 4.5:1 against `color-background` (light) and `color-background` (dark) |
| Semantic surface token | 4.5:1 against its intended foreground text/icon color |

Use a contrast checker (e.g., [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)) to validate before merging.

### 3. Semantic Surfaces Must Remain Distinct

Overriding a semantic color (e.g., `color-success`) requires that:
- The semantic surface remains clearly distinguishable from `color-background`.
- The intended foreground text/icon color on that surface maintains at least 4.5:1 contrast.

### 4. Document the Override

Add a `COLOR_TOKENS.md` (or equivalent) to your project repository that lists every overridden token with its justification, the chosen value for both modes, and the measured contrast ratios.

---

## Override Mechanism

Overrides are declared in the project's central token file, as specified in [ADR 0011](../adrs/0011-centralized-frontend-styling-variables.md). The mechanism is technology-agnostic: you replace the default value of only the permitted tokens and leave all others unchanged.

**Conceptually**, the override looks like this:

```
[Default token file — provided by the style guide baseline]
  color-primary (light):  #A16207
  color-primary (dark):   #F2C14E
  ...all other tokens at their fixed values...

[Project override file — only brand/semantic colors]
  color-primary (light):  #8B0000   ← project brand red
  color-primary (dark):   #C0392B   ← accessible dark-mode variant
```

The implementation details (SCSS variable override, CSS custom property override, design-token JSON merge) depend on the frontend technology. See [ADR 0011](../adrs/0011-centralized-frontend-styling-variables.md) for framework-specific patterns.

---

## Step-by-Step Override Process

1. **Identify brand colors.** Obtain the brand color palette from the design team (brand guidelines PDF, Figma file, etc.).
2. **Map brand colors to tokens.** Decide which brand color maps to `color-primary` and which neutral support color maps to `color-secondary`.
3. **Derive dark-mode variants.** For each brand color chosen for light mode, derive an accessible dark-mode variant. A common technique: lighten the hue by 10–20% and verify contrast.
4. **Derive semantic surface colors.** For each overridden semantic color, choose a calm surface tint for light mode and a dark desaturated surface tint for dark mode.
5. **Validate all contrast pairs.** Check every combination listed in the Override Contract above.
6. **Create `COLOR_TOKENS.md`** in the project and document all values and ratios.
7. **Implement via ADR 0011 mechanism.** Declare overrides in the project's token file only; do not touch typography, spacing, or motion files.
8. **Peer review.** Another team member verifies the `COLOR_TOKENS.md` ratios and approves the override PR.

---

## Example: Applying a Red Brand Palette

| Token | Default Light | Project Override (Light) | Project Override (Dark) | Contrast (Light) | Contrast (Dark) |
|---|---|---|---|---|---|
| `color-primary` | `#A16207` | `#C0392B` | `#E74C3C` | 5.1:1 ✅ | 4.6:1 ✅ |
| `color-primary-light` | `#D4A72C` | `#E74C3C` | `#F07060` | — | — |
| `color-primary-dark` | `#7C4A03` | `#922B21` | `#C0392B` | — | — |
| `color-secondary` | `#6C757D` | `#7D3C98` | `#A569BD` | 4.6:1 ✅ | 4.5:1 ✅ |

---

## What Happens If You Override Non-Color Tokens

Non-color overrides are a **style guide violation** and must be caught in code review. If a project introduces a non-color token override:

1. The pull request must be rejected.
2. If the project has a genuine design need that the current scale does not cover, open a proposal to extend the fixed token set for all projects — not just one.
