# Skill: JSdotNet Design & UX MCP

**Description:** Learn how to use the `jsdotnet-design-ux-guidelines` MCP server to retrieve design tokens, style-guide documents, and UX standards for frontend projects built with Angular, React, Vue, Svelte, Blazor, or plain HTML/CSS.

---

## The MCP Server

| Server | MCP name | Serves | Contains |
|--------|----------|--------|----------|
| **Design / UX** | `jsdotnet-design-ux-guidelines` | `design/` | UX style guide, design tokens (color, typography, spacing, motion) |

> For coding/architecture guidance (ADRs, patterns, C# standards), use the separate `jsdotnet-coding-guidelines` server and its plugin.

---

## Available Tools

| Tool | When to use |
|------|-------------|
| `list_guides` | Browse the full style-guide catalog |
| `list_guides_by_type(category)` | Get all docs in a category (only `style-guide` exists) |
| `search_guides(query)` | Free-text search across titles and descriptions |
| `search_guides_by_tag(tag)` | Precise tag-based search — fastest for known topics |
| `get_guide(id)` | Fetch the full markdown of a specific style-guide document |
| `get_usage_logs(count)` | Retrieve recent tool-invocation records for analysis |

### Valid `list_guides_by_type` categories

`style-guide`

---

## Style-Guide Catalog

| Document ID | Title | Key Content |
|-------------|-------|-------------|
| `readme` | Style Guide: Overview | Purpose, structure, what is/isn't customizable, design principles |
| `01-color-palette` | Style Guide: Color Palette | All color tokens (brand, semantic, text, background, border) with light/dark values |
| `02-typography` | Style Guide: Typography | Font families, size scale, weights, line heights |
| `03-spacing-and-layout` | Style Guide: Spacing and Layout | Spacing scale, border radius, shadows |
| `04-motion-and-interaction` | Style Guide: Motion and Interaction | Transition durations, easing curves, focus states |
| `05-customization-guide` | Style Guide: Customization Guide | Override contract for brand/semantic colors |

---

## Quick Decision Guide

- **"What color tokens exist?"** → `get_guide("01-color-palette")`
- **"Which colors can I customize?"** → `get_guide("05-customization-guide")`
- **"What fonts are used?"** → `get_guide("02-typography")`
- **"What spacing values should I use?"** → `get_guide("03-spacing-and-layout")`
- **"How should I animate a transition?"** → `get_guide("04-motion-and-interaction")`
- **"Give me the full catalog."** → `list_guides_by_type("style-guide")`
- **"Find all color-related docs."** → `search_guides_by_tag("color")`
- **"Find docs about accessibility."** → `search_guides_by_tag("accessibility")`

---

## Common Tags

Use `search_guides_by_tag()` to filter by topic:

- **Visual**: `color`, `typography`, `spacing`, `layout`, `motion`, `animation`
- **System**: `design-tokens`, `style-guide`, `branding`, `customization`
- **UX**: `accessibility`, `ux`, `interaction`, `frontend`
- **Mode**: `dark-mode`, `light-mode`
- **Type**: `fonts`

---

## Design Principles

The style guide is built on four principles — always verify your implementation against these:

1. **Semantic naming** — tokens are named by purpose (`color-primary`, `color-error`), never by raw value (`blue-500` or `#dc3545`). Always use the token name, never the hex value.
2. **Light and dark first** — every color token has both a light-mode and a dark-mode variant. There is no single-mode design.
3. **Accessibility** — all color combinations meet WCAG 2.1 AA (4.5:1 normal text, 3:1 large text/UI components).
4. **Consistency over cleverness** — use the predefined scale; do not introduce one-off values.

---

## Relationship to Coding Guidelines

The design server and the coding guidelines server are complementary:

- **`jsdotnet-design-ux-guidelines`** defines *what* the tokens are and what they mean (this server).
- **`jsdotnet-coding-guidelines`** defines *how* to declare and use tokens in code (SCSS variables, CSS custom properties, framework-specific integration) via ADR 0011.

When implementing a UI feature, consult **both servers**: the design server for which tokens to use, the coding guidelines server for how to declare them.

---

## Example Workflows

### Styling a New UI Component

```
1. get_guide("01-color-palette")
   → Identify relevant color tokens (background, text, border, states)
2. get_guide("02-typography")
   → Select the appropriate font size and weight tokens
3. get_guide("03-spacing-and-layout")
   → Pick spacing scale values for padding and margin
4. get_guide("04-motion-and-interaction")
   → Add hover/focus transition using the defined easing and duration tokens
```

### Checking Accessibility for a Component

```
1. get_guide("01-color-palette")
   → Check foreground/background token pairs for contrast rules
   → Verify focus ring uses color-border-focus at ≥2px
2. get_guide("04-motion-and-interaction")
   → Verify motion respects prefers-reduced-motion (see motion docs)
3. search_guides_by_tag("accessibility")
   → Find all accessibility-related guidance
```

### Branding a New Project

```
1. get_guide("readme")
   → Understand what is and isn't customizable
2. get_guide("05-customization-guide")
   → Read the override contract (which tokens, contrast requirements)
3. get_guide("01-color-palette")
   → Get the default brand token names you'll override
```

---

## Tips

- **Always use token names, never raw hex values** — hardcoded values break dark mode and future palette updates.
- **Both light and dark values are required** — every token has both; your implementation must handle both modes.
- **`get_guide` is faster than search** — since the catalog is small (6 documents), call `get_guide` directly with a known ID rather than searching.
- **Only colors are customizable** — typography, spacing, border radius, and motion tokens are fixed across all projects.
- **Use `get_usage_logs`** sparingly — it's for analyzing MCP server usage, not for normal token lookups.
