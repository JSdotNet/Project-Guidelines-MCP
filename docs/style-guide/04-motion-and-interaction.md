---
title: "Style Guide: Motion and Interaction"
date: 2026-06-05
tags: [style-guide, motion, animation, interaction, accessibility, design-tokens]
---
# Motion and Interaction

> Motion and interaction tokens are **fixed** — they are not customizable per project.

---

## Design Principles

1. **Motion serves purpose** — animate only to communicate state changes, direct attention, or provide feedback. Never animate for decoration alone.
2. **Respect user preferences** — honor the `prefers-reduced-motion` media query. All animations must degrade gracefully when motion is reduced.
3. **Speed signals intent** — fast transitions (≤ 150 ms) feel like direct manipulation; slower transitions (250–350 ms) signal a structural navigation change.
4. **Consistent easing** — use the defined easing curves; mixing curves creates visual noise.

---

## Transition Durations

| Token | Value | Usage |
|---|---|---|
| `transition-instant` | `0ms` | Immediate state changes that need no animation (e.g., visibility toggles for screen readers) |
| `transition-fast` | `150ms` | Hover states, focus rings, button presses, color changes on interactive elements |
| `transition-base` | `250ms` | Default for most UI transitions — dropdowns opening, card hover elevation |
| `transition-slow` | `350ms` | Panels, drawers, toasts entering/exiting |
| `transition-page` | `500ms` | Full-page route transitions |

### Usage Rules

- Do not use values outside this scale.
- Entrance animations (`transition-slow`) should feel slightly slower than exit animations (`transition-base`) to avoid abruptness.
- Never animate layout-triggering properties (`width`, `height`, `top`, `left`) on the critical path — prefer `transform` and `opacity`.

---

## Easing Curves

| Token | Value | Usage |
|---|---|---|
| `ease-linear` | `linear` | Progress bars, loading indicators — continuous motion |
| `ease-in` | `cubic-bezier(0.4, 0, 1, 1)` | Elements exiting the screen — start slow, end fast |
| `ease-out` | `cubic-bezier(0, 0, 0.2, 1)` | Elements entering the screen — start fast, end gently |
| `ease-in-out` | `cubic-bezier(0.4, 0, 0.2, 1)` | Default — state changes that do not involve screen entry/exit |
| `ease-bounce` | `cubic-bezier(0.34, 1.56, 0.64, 1)` | Confirmations, success indicators — slight overshoot for delight |

### Usage Rules

- Default to `ease-in-out` for most transitions.
- Use `ease-out` for elements *entering* the viewport (dropdown opens, modal slides in).
- Use `ease-in` for elements *leaving* the viewport (dropdown closes, modal slides out).
- `ease-bounce` is reserved for positive confirmations (checkmark animations, saved indicators) — never for error states.

---

## Standard Transitions

Apply these token combinations consistently for common patterns:

| Interaction | Duration | Easing |
|---|---|---|
| Button hover color/shadow | `transition-fast` | `ease-in-out` |
| Input focus ring | `transition-fast` | `ease-in-out` |
| Card hover elevation | `transition-base` | `ease-in-out` |
| Dropdown / menu open | `transition-base` | `ease-out` |
| Dropdown / menu close | `transition-fast` | `ease-in` |
| Modal / dialog enter | `transition-slow` | `ease-out` |
| Modal / dialog exit | `transition-base` | `ease-in` |
| Side drawer enter | `transition-slow` | `ease-out` |
| Side drawer exit | `transition-base` | `ease-in` |
| Toast / snackbar enter | `transition-slow` | `ease-out` |
| Toast / snackbar exit | `transition-base` | `ease-in` |
| Page / route transition | `transition-page` | `ease-in-out` |
| Success confirmation | `transition-base` | `ease-bounce` |

---

## Focus States

Keyboard focus must always be **clearly visible** and must never be removed without replacement.

| Property | Value |
|---|---|
| Focus ring color | `color-border-focus` (`#A16207` / `#F2C14E` dark) |
| Focus ring width | `2px` |
| Focus ring offset | `2px` (outside the element boundary) |
| Focus ring style | `outline` (not `box-shadow` — `outline` is visible in Windows High Contrast mode) |

### Usage Rules

- Never set `outline: none` without providing a fully compliant replacement.
- Focus styles must pass **3:1** contrast against the adjacent background.
- Interactive elements that are not natively focusable must receive `tabindex="0"` and explicit focus styles.
- Focus order must follow a logical reading order (top-to-bottom, left-to-right for LTR layouts).

---

## Reduced Motion

Every animation or transition in the codebase must degrade gracefully:

```
@media (prefers-reduced-motion: reduce) {
  /* Replace motion with instant state changes or opacity-only fades */
}
```

Permitted in reduced-motion mode:
- Instant state changes (no transition)
- Opacity fade-only transitions at `transition-fast` or slower

Not permitted in reduced-motion mode:
- Translate, scale, rotate transforms
- Slide-in/slide-out effects
- Parallax scrolling

---

## Loading & Progress Indicators

| Pattern | Guidance |
|---|---|
| **Spinner** | Use for indeterminate waits < 10 s. Animate with `ease-linear` and a constant rotation. Pause animation when `prefers-reduced-motion` is active. |
| **Skeleton loader** | Use for content-shaped placeholders. Pulse animation at `2s` cycle; disable pulse under reduced motion. |
| **Progress bar** | Use for determinate progress. Transition `width` with `ease-linear`. |
| **Skeleton shimmer** | Subtle `background-position` animation. Use `animation-duration: 1.5s`. Disable under reduced motion. |

---

## Hover and Active States

| State | Visual Change |
|---|---|
| Hover | Slightly elevated `shadow` or `background-color` shift by ~5–10% lightness |
| Active / Pressed | `shadow-inner` + scale `0.98` (transform only — skip under reduced motion) |
| Disabled | `color-text-disabled`, `cursor: not-allowed`, no hover/focus response |
| Loading | Replace label with spinner; disable pointer events |
