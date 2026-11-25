---
name: design-token-lookup
description: "Look up design tokens by category (color, typography, spacing, motion). Find the right token name, its light/dark values, and usage rules before writing any CSS or style code."
---

# Design token lookup

Token tables and common mistakes: [reference.md](reference.md). They summarise; the
`design` documents on `jsdotnet-app-defaults` are authoritative.

## Workflow

1. **Pick the document** for the property:
   - color → `get_guide("01-color-palette")`;
   - font family, size, weight → `get_guide("02-typography")`;
   - spacing, radius, shadow → `get_guide("03-spacing-and-layout")`;
   - duration, easing, focus → `get_guide("04-motion-and-interaction")`.
2. **Choose by purpose, not by value**: take the token whose semantic group fits
   (`color-error`, not the hex value that looks red).
3. **Check it**: it has a light and a dark value, and the foreground/background pair meets
   WCAG 2.1 AA (4.5:1 text, 3:1 large text and UI parts).
4. **Use the name, never the value**: `var(--color-primary)` in CSS. How a framework
   declares the variables is ADR 0009: `get_guide("0009-frontend")`.
5. **No token fits** → say so and use the nearest one; never introduce a one-off value.
