# Skill: JSdotNet Coding Guidelines MCP

**Description:** Learn how to use the `jsdotnet-coding-guidelines` MCP server to retrieve coding standards, Architecture Decision Records (ADRs), design documents, recommendations, and project structure templates for .NET projects.

---

## The MCP Server

| Server | MCP name | Serves | Contains |
|--------|----------|--------|----------|
| **Coding Guidelines** | `jsdotnet-coding-guidelines` | `guide/` | ADRs, designs, recommendations, structures, config |

> For UX/design token guidance, use the separate `jsdotnet-design-ux-guidelines` server and its plugin.

---

## Available Tools

| Tool | When to use |
|------|-------------|
| `list_guides` | Browse the full catalog — good when exploring an unfamiliar area |
| `list_guides_by_type(category)` | Get all docs of one type (see categories below) |
| `search_guides(query)` | Free-text keyword search across titles, descriptions, and IDs |
| `search_guides_by_tag(tag)` | Precise tag-based search — more targeted than text search |
| `get_guide(id)` | Fetch the full markdown of a specific document by ID |
| `get_usage_logs(count)` | Retrieve recent tool-invocation records for analysis |

### Valid `list_guides_by_type` categories

`adrs` · `designs` · `recommendations` · `structures` · `config`

---

## Quick Decision Guide

- **"What guidance exists for X?"** → `search_guides("X")`
- **"What are the accepted ADRs?"** → `list_guides_by_type("adrs")`
- **"Show me everything tagged persistence."** → `search_guides_by_tag("persistence")`
- **"I found an ADR ID — read it."** → `get_guide("adr-id")`
- **"Starting a new project — what templates exist?"** → `list_guides_by_type("structures")`
- **"What recommendations exist?"** → `list_guides_by_type("recommendations")`
- **"Any config guidelines?"** → `list_guides_by_type("config")`

---

## Common Tags

Use `search_guides_by_tag()` to filter by topic:

- **Architecture**: `hexagonal`, `ddd`, `clean-architecture`, `cqrs`, `ports-adapters`
- **Patterns**: `value-object`, `aggregate`, `domain-event`, `repository`, `factory`
- **Cross-cutting**: `logging`, `error-handling`, `resilience`, `testing`, `security`, `observability`
- **Infrastructure**: `persistence`, `messaging`, `external-api`, `caching`
- **Code Style**: `csharp`, `dotnet`, `naming`, `dependency-injection`

---

## Workflow: Aligning Code with Guidelines

When implementing a new feature:

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
1. search_guides("persistence adapter")
   → Find ADRs and recommendations on repository pattern and EF Core usage
2. search_guides_by_tag("persistence")
   → Get all infrastructure guidance
3. list_guides_by_type("structures")
   → Find adapter scaffold templates
4. get_guide("adr-NNNN")
   → Read the specific decision on ORM strategy
```

### Implementing Error Handling & Resilience

```
1. search_guides("error handling")
   → Search for exception strategies
2. search_guides_by_tag("resilience")
   → Find circuit breaker, retry, and bulkhead patterns
3. get_guide("adr-NNNN")
   → Read the retry + resilience policy
```

### Reviewing Domain Model Design

```
1. search_guides_by_tag("ddd")
   → Find all DDD-related guidance
2. search_guides_by_tag("value-object")
   → Immutability and validation rules
3. search_guides_by_tag("aggregate")
   → Aggregate root conventions
4. list_guides_by_type("structures")
   → Find domain model scaffold templates
```

---

## Tips

- **Always call `get_guide` before implementing** anything that might be governed by an ADR or recommendation.
- **Use tag search for precise lookups** — much faster than free-text when you know the topic.
- **Reference ADRs in code comments** — creates traceability between decisions and implementation.
- **Check document status** — look for `Accepted` ADRs; be cautious with `Proposed` or `Deprecated` ones.
- **Use `get_usage_logs`** sparingly — it's for analyzing MCP server usage, not for normal guidance lookups.

