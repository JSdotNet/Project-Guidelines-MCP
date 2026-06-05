# Skill: JSdotNet Project Guidelines MCP

**Description:** Learn how to use the JSdotNet Project Guidelines MCP server to retrieve coding standards, ADRs, recommendations, and architectural templates for modern .NET projects.

---

## Available MCP Tools

Six MCP tools are available to retrieve coding guidelines, ADRs, recommendations, and templates:

| Tool | When to use |
|------|-------------|
| `list_docs` | Browse the full catalog — good when exploring an unfamiliar area |
| `list_docs_by_type(category)` | Get all docs of one type: `adrs`, `designs`, `recommendations`, `structures` |
| `search_docs(query)` | Free-text keyword search across titles, descriptions, and content |
| `search_docs_by_tag(tag)` | Precise tag-based search — more targeted than text search |
| `get_doc(id)` | Fetch the full markdown of a specific document by ID |
| `get_usage_logs(count)` | Retrieve recent tool-invocation records for analysis |

---

## Quick Decision Guide

Choose the right tool for your task:

- **"What guidance exists for X?"** → `search_docs("X")`
- **"What are the accepted ADRs?"** → `list_docs_by_type("adrs")`
- **"Show me everything tagged persistence."** → `search_docs_by_tag("persistence")`
- **"I found an ADR ID — read it."** → `get_doc("adr-id")`
- **"Starting a new project — what templates exist?"** → `list_docs_by_type("structures")`

---

## Common Tags

These tags are used to organize documents. Use `search_docs_by_tag()` to filter by topic:

- **Architecture**: `hexagonal`, `ddd`, `clean-architecture`, `cqrs`, `ports-adapters`
- **Patterns**: `value-object`, `aggregate`, `domain-event`, `repository`, `factory`
- **Cross-cutting**: `logging`, `error-handling`, `resilience`, `testing`, `security`, `observability`
- **Infrastructure**: `persistence`, `messaging`, `external-api`, `caching`
- **Code Style**: `csharp`, `dotnet`, `naming`, `dependency-injection`

---

## Workflow: Aligning Code with Guidelines

When you're implementing a new feature, use this workflow to ensure alignment with documented decisions:

1. **Identify the domain**: What architectural layer will this live in (Domain, Application, Infrastructure)?
2. **Search for relevant guidance**: `search_docs("your feature")` or `search_docs_by_tag("relevant-tag")`
3. **Review accepted decisions**: `list_docs_by_type("adrs")` — filter to status `Accepted`
4. **Read the full ADR**: `get_doc("adr-id")` to understand the rationale
5. **Apply recommendations**: `search_docs_by_tag("your-domain")` for prescriptive guidance
6. **Use templates**: `list_docs_by_type("structures")` to scaffold your code
7. **Reference in comments**: Add `// ADR-NNNN: Brief rationale` to justify decisions inline

---

## Example Workflows

### Implementing a Persistence Adapter

```
1. search_docs("persistence adapter")
   → Find ADRs and recommendations on repository pattern and EF Core usage
2. search_docs_by_tag("persistence")
   → Get all infrastructure guidance
3. list_docs_by_type("structures")
   → Find adapter scaffold templates
4. get_doc("adr-NNNN")
   → Read the specific decision on ORM strategy
```

### Structuring Domain Validation

```
1. search_docs_by_tag("value-object")
   → Learn immutable validation patterns
2. search_docs_by_tag("aggregate")
   → Understand aggregate invariants
3. get_doc("adr-NNNN")
   → Review the domain exception strategy
```

### Error Handling & Resilience

```
1. search_docs("error handling")
   → Search for exception strategies
2. search_docs_by_tag("resilience")
   → Find circuit breaker, retry, and bulkhead patterns
3. get_doc("adr-NNNN")
   → Read the retry + resilience policy
```

---

## Tips

- **Always call `get_doc` before implementing** anything that might be governed by an ADR or recommendation.
- **Use tag search for precise lookups** — much faster than free-text search when you know the topic.
- **Reference ADRs in code comments** — this creates traceability between decisions and implementation.
- **Check document status** — look for `Accepted` ADRs; be cautious with `Proposed` or `Deprecated` ones.
- **Use `get_usage_logs`** sparingly — it's for analyzing MCP server usage, not for normal guidance lookups.

---

## Getting Help

If you need further guidance on how these tools work, call `guidelines_mcp_help` for an interactive reference.
