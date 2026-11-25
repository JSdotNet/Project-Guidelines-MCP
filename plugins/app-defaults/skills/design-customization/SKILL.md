---
name: design-customization
description: "Customize the project color scheme by overriding brand and semantic color tokens. Covers which tokens may be overridden, contrast requirements, and the override contract."
---

# Design customization

Which tokens may change, the override contract and the contrast table:
[reference.md](reference.md). Only brand and semantic colors are overridable; typography,
spacing, radius and motion are fixed.

## Workflow

1. **Read the contract**: `get_guide("05-customization-guide")`.
2. **Note the defaults you replace**: `get_guide("01-color-palette")`, brand and semantic
   sections.
3. **Supply both values**: a light and a dark value for every token you override.
4. **Validate contrast** in both modes: `color-primary` on `color-background` ≥ 4.5:1, and
   each semantic surface with its foreground text ≥ 4.5:1.
5. **Keep the semantic surfaces distinct** from each other and from `color-background`.
6. **Wire the overrides** as CSS custom property overrides, the mechanism ADR 0009 defines:
   `get_guide("0009-frontend")`.
7. **Check both modes**: focus rings visible, semantic banners distinguishable.

An override that breaks the contract is refused: name the rule and the value that meets it.
