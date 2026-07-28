# Skill: JSdotNet Project Guidelines MCP

**Description:** Learn how to use the two JSdotNet MCP servers — `jsdotnet-coding-guidelines` (coding standards, ADRs, architecture) and `jsdotnet-design-ux-guidelines` (UX style guide, design tokens) — to retrieve guidance for .NET and frontend projects.

---

## Two MCP Servers

This repository exposes **two separate MCP servers**. Always use the right server for your task:

| Server | MCP name | Serves | Contains |
|--------|----------|--------|----------|
| **Coding Guidelines** | `jsdotnet-coding-guidelines` | `guide/` | ADRs, designs, recommendations, structures, config |
| **Design / UX** | `jsdotnet-design-ux-guidelines` | `design/` | UX style guide, design tokens (color, typography, spacing, motion) |

Both servers expose the **same six tools** — the difference is which content they serve.

---

## Available MCP Tools

| Tool | When to use |
|------|-------------|
| `list_guides` | Browse the full catalog — good when exploring an unfamiliar area |
| `list_guides_by_type(category)` | Get all docs of one type (see categories below) |
| `search_guides(query)` | Free-text keyword search across titles, descriptions, and IDs |
| `search_guides_by_tag(tag)` | Precise tag-based search — more targeted than text search |
| `get_guide(id)` | Fetch the full markdown of a specific document by ID |
| `get_usage_logs(count)` | Retrieve recent tool-invocation records for analysis |

### Coding Guidelines — Valid `list_guides_by_type` categories

`adrs` · `designs` · `recommendations` · `structures` · `config`

### Design / UX — Valid `list_guides_by_type` categories

`style-guide`

---

## Quick Decision Guide

### For coding / architecture questions (use `jsdotnet-coding-guidelines`):

- **"What guidance exists for X?"** → `search_guides("X")`
- **"What are the accepted ADRs?"** → `list_guides_by_type("adrs")`
- **"Show me everything tagged persistence."** → `search_guides_by_tag("persistence")`
- **"I found an ADR ID — read it."** → `get_guide("adr-id")`
- **"Starting a new project — what templates exist?"** → `list_guides_by_type("structures")`

### For UX / frontend questions (use `jsdotnet-design-ux-guidelines`):

- **"What color tokens are defined?"** → `search_guides_by_tag("color")`
- **"What typography rules apply?"** → `get_guide("02-typography")`
- **"What can I customize per project?"** → `get_guide("05-customization-guide")`
- **"Show all style-guide documents."** → `list_guides_by_type("style-guide")`

---

## Common Tags

Use `search_guides_by_tag()` to filter by topic:

### Coding Guidelines tags
- **Architecture**: `hexagonal`, `ddd`, `clean-architecture`, `cqrs`, `ports-adapters`
- **Patterns**: `value-object`, `aggregate`, `domain-event`, `repository`, `factory`
- **Cross-cutting**: `logging`, `error-handling`, `resilience`, `testing`, `security`, `observability`
- **Infrastructure**: `persistence`, `messaging`, `external-api`, `caching`
- **Code Style**: `csharp`, `dotnet`, `naming`, `dependency-injection`

### Design / UX tags
- `style-guide`, `design-tokens`, `color`, `typography`, `spacing`, `layout`, `motion`, `animation`, `accessibility`, `customization`, `branding`, `ux`, `frontend`

---

## Workflow: Aligning Code with Guidelines

When implementing a new feature, ensure alignment with documented decisions:

1. **Identify the domain**: What architectural layer (Domain, Application, Infrastructure)?
2. **Search for relevant guidance**: `search_guides("your feature")` or `search_guides_by_tag("relevant-tag")`
3. **Review accepted decisions**: `list_guides_by_type("adrs")` — filter to status `Accepted`
4. **Read the full ADR**: `get_guide("adr-id")` to understand the rationale
5. **Apply recommendations**: `search_guides_by_tag("your-domain")` for prescriptive guidance
6. **Use templates**: `list_guides_by_type("structures")` to scaffold your code
7. **Reference in comments**: Add `// ADR-NNNN: Brief rationale` to justify decisions inline

---

## Example Workflows

### Implementing a Persistence Adapter

```
[jsdotnet-coding-guidelines]
1. search_guides("persistence adapter")
   → Find ADRs and recommendations on repository pattern and EF Core usage
2. search_guides_by_tag("persistence")
   → Get all infrastructure guidance
3. list_guides_by_type("structures")
   → Find adapter scaffold templates
4. get_guide("adr-NNNN")
   → Read the specific decision on ORM strategy
```

### Styling a Frontend Component

```
[jsdotnet-design-ux-guidelines]
1. list_guides_by_type("style-guide")
   → See all style guide documents
2. get_guide("01-color-palette")
   → Get available color tokens for light and dark mode
3. get_guide("02-typography")
   → Check font and size rules
4. get_guide("05-customization-guide")
   → Understand what color overrides are allowed per project
```

### Error Handling & Resilience

```
[jsdotnet-coding-guidelines]
1. search_guides("error handling")
   → Search for exception strategies
2. search_guides_by_tag("resilience")
   → Find circuit breaker, retry, and bulkhead patterns
3. get_guide("adr-NNNN")
   → Read the retry + resilience policy
```

---

## Tips

- **Always call `get_guide` before implementing** anything that might be governed by an ADR or recommendation.
- **Use tag search for precise lookups** — much faster than free-text search when you know the topic.
- **Reference ADRs in code comments** — this creates traceability between decisions and implementation.
- **Check document status** — look for `Accepted` ADRs; be cautious with `Proposed` or `Deprecated` ones.
- **Use `get_usage_logs`** sparingly — it's for analyzing MCP server usage, not for normal guidance lookups.
- **Know which server to query** — coding/architecture questions go to `jsdotnet-coding-guidelines`; frontend/UX questions go to `jsdotnet-design-ux-guidelines`.
