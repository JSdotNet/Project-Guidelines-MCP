---
name: ui-component-styling
description: "Style UI components using the design token system. Covers button states, form inputs, cards, typography hierarchy, semantic feedback (success/warning/error), and accessibility rules."
---

# UI component styling

Per-component token maps, the accessibility checklist and common mistakes:
[reference.md](reference.md).

## Workflow

1. **Name the component type**: button, input, card, banner, navigation, dialog.
2. **Read its pattern**: `get_guide("06-component-patterns")`, and
   `get_guide("09-interaction-patterns")` for forms and navigation.
3. **Map every visual property to a token** with the `design-token-lookup` skill:
   background `color-background-*`, text `color-text-*`, border `color-border-*`, size
   `font-size-*`, spacing `spacing-*`, transitions from the motion document.
4. **Style every state**: default, hover, focus, active, disabled, and error where it applies.
5. **Check accessibility** against the checklist in [reference.md](reference.md): contrast
   pairs, a visible focus ring, `prefers-reduced-motion`, a label on every input.
6. **Write it with token variables** (`var(--token-name)`), wired as ADR 0009 says:
   `get_guide("0009-frontend")`.
7. **Check both modes**: render it in light and in dark mode before calling it done.
