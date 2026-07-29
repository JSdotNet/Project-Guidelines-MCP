# JSdotNet Design & UX Guidelines Plugin

This plugin provides five skills for working with the **`jsdotnet-design-ux-guidelines`** MCP server — the canonical source of design tokens and UX style guidance for all JSdotNet frontend projects.

1. **design-mcp** — Reference guide for the design MCP server: catalog, tool reference, tags, and workflows
2. **design-token-lookup** — Look up color, typography, spacing, and motion tokens before writing CSS
3. **ui-component-styling** — Style buttons, forms, cards, banners, and navigation using design tokens
4. **design-customization** — Override the project color scheme within the allowed contract
5. **test-design-server** — Smoke-test the design MCP server end-to-end

> For coding/architecture guidance (ADRs, patterns, C# standards), use the companion **`jsdotnet-project-guidelines`** plugin (`plugins/guidelines/`).

---

## What the Design MCP Server Serves

The `jsdotnet-design-ux-guidelines` server serves **six style-guide documents** from the `design/` folder:

| Document | Contents |
|---|---|
| `readme` | Purpose, structure, design principles, what is/isn't customizable |
| `01-color-palette` | All color tokens — brand, semantic, text, background, border |
| `02-typography` | Font families, size scale, weights, line heights |
| `03-spacing-and-layout` | Spacing scale, border radius, shadows |
| `04-motion-and-interaction` | Transitions, easing curves, focus states |
| `05-customization-guide` | Brand/semantic color override contract |

**Only color tokens are customizable.** Typography, spacing, border radius, and motion are fixed.

---

## Installation

### Option 1: Copy to User Skills Directory (Global)

```bash
# Windows PowerShell
Copy-Item plugins/design/skills/* $env:USERPROFILE/.copilot/skills/

# macOS/Linux
cp plugins/design/skills/* ~/.copilot/skills/
```

### Option 2: Project-Scoped (This Repository Only)

Skills in `plugins/design/skills/` auto-load as project-scoped skills when you open a Copilot session in this repository.

---

## Quick Start

### 1. Learn the Server

```
Invoke skill: design-mcp
```

Teaches you: the six style-guide documents, available tools, valid categories and tags, and example workflows.

### 2. Look Up a Token

```
Invoke skill: design-token-lookup
```

Quickly find the right token name, its light/dark values, and usage rules before writing CSS.

### 3. Style a Component

```
Invoke skill: ui-component-styling
```

Get concrete token assignments for buttons, inputs, cards, banners, and navigation. Includes an accessibility checklist.

### 4. Customize the Color Scheme

```
Invoke skill: design-customization
```

Understand the override contract, which tokens may be overridden, contrast requirements, and the step-by-step workflow.

### 5. Test the Server

```
Invoke skill: test-design-server
```

Verify all tools return correct content and diagnose issues after install or upgrade.

---

## Skills Reference

| Skill | Best Used When | Pairs With |
|-------|---------------|------------|
| **design-mcp** | Starting out; learning the server | Any other skill |
| **design-token-lookup** | Before writing any CSS/style code | ui-component-styling |
| **ui-component-styling** | Styling a new or existing component | design-token-lookup, design-customization |
| **design-customization** | Setting up project brand colors | design-token-lookup, ui-component-styling |
| **test-design-server** | After install/upgrade or when tools misbehave | design-mcp |

---

## Common Workflows

### Workflow 1: Style a New Component

```
1. Invoke: design-token-lookup
   → Identify which tokens to use (color, typography, spacing)

2. Invoke: ui-component-styling
   → Get the exact token assignment pattern for your component type

3. Implement using var(--token-name) or $token-name
4. Verify accessibility (contrast, focus rings)
```

### Workflow 2: Set Up a New Project's Brand Colors

```
1. Invoke: design-mcp
   → Understand what is and isn't customizable

2. Invoke: design-customization
   → Get the full override contract and workflow

3. Choose replacement brand/semantic color tokens
4. Validate contrast in both light and dark modes
5. Implement overrides per ADR 0011 (coding guidelines)
```

### Workflow 3: Debug a Design Token Issue

```
1. Invoke: test-design-server
   → Verify server is responding and docs are current

2. Invoke: design-token-lookup
   → Confirm you're using the correct token name

3. get_guide("01-color-palette")
   → Check if the token exists and has the expected values
```

---

## File Structure

```
plugins/design/
  .github/plugin/plugin.json       ← Canonical plugin metadata
  skills/
    design-mcp.md                  ← Skill 1: Server reference
    design-token-lookup.md         ← Skill 2: Find tokens by category
    ui-component-styling.md        ← Skill 3: Style components correctly
    design-customization.md        ← Skill 4: Project color overrides
    test-design-server.md          ← Skill 5: Smoke-test the server
  README.md                        ← This file
```

---

## Troubleshooting

### Skills are not showing up

- **Project-scoped**: Skills auto-load from `plugins/design/skills/` when in a session for this repository.
- **Global**: Copy skills to `~/.copilot/skills/` and restart your session.
- **Fix**: Run `extensions_reload` in the Copilot session.

### Server not responding

See **Skill: test-design-server** for the full diagnostic workflow.

Short checklist:
1. Check `.mcp.json` — `jsdotnet-design-ux-guidelines` must be listed.
2. Build the project: `dotnet build src/JSdotNet.MCP.Design`
3. Reload: `extensions_reload`

---

## Version History

- **1.0** (2026-07-28): Initial release — split from the combined `jsdotnet-guidelines` plugin to provide a focused, design-server-specific skill set.
