import { joinSession } from "@github/copilot-sdk/extension";

// Tracks whether we have already injected the MCP reminder this session so
// the onUserPromptSubmitted hook fires at most once and doesn't get noisy.
let mcpReminderSent = false;

const ARCHITECTURE_KEYWORDS =
    /\b(architecture|pattern|adr|design|structure|hexagonal|ddd|domain-driven|domain\s+model|persistence|repository|adapter|cqrs|mediator|resilience|logging|error.handling|testing|dependency.inversion|clean\s+arch|value\s+object|aggregate|use.case|port|interface|infrastructure)\b/i;

const session = await joinSession({
    hooks: {
        onSessionStart: async () => {
            mcpReminderSent = false;
            return {
                additionalContext: `
## JSdotNet Project Guidelines MCP Server

Six MCP tools are available to retrieve coding guidelines, ADRs, recommendations, and templates:

| Tool | When to use |
|------|-------------|
| \`list_docs\` | Browse the full catalog — good when exploring an unfamiliar area |
| \`list_docs_by_type(category)\` | Get all docs of one type: \`adrs\`, \`designs\`, \`recommendations\`, \`structures\` |
| \`search_docs(query)\` | Free-text keyword search across titles, descriptions, and content |
| \`search_docs_by_tag(tag)\` | Precise tag-based search — more targeted than text search |
| \`get_doc(id)\` | Fetch the full markdown of a specific document by ID |
| \`get_usage_logs(count)\` | Retrieve recent tool-invocation records for analysis |

**Decision guide — choose the right tool:**
- *"What guidance exists for X?"* → \`search_docs("X")\`
- *"What are the accepted ADRs?"* → \`list_docs_by_type("adrs")\`
- *"Show me everything tagged persistence."* → \`search_docs_by_tag("persistence")\`
- *"I found an ADR id — read it."* → \`get_doc("adr-id")\`
- *"Starting a new project — what templates exist?"* → \`list_docs_by_type("structures")\`

**Always call \`get_doc\` before implementing anything that may be governed by an ADR or recommendation.**
Call \`guidelines_mcp_help\` for common tags and further guidance.
`.trim(),
            };
        },

        onUserPromptSubmitted: async (input) => {
            if (mcpReminderSent) return;
            if (!ARCHITECTURE_KEYWORDS.test(input.prompt)) return;

            mcpReminderSent = true;
            return {
                additionalContext:
                    "Architecture or design topic detected. Before answering, search the guidelines MCP: " +
                    "use search_docs or search_docs_by_tag to find relevant ADRs and recommendations, " +
                    "then get_doc to read them. This ensures alignment with documented decisions.",
            };
        },
    },

    tools: [
        {
            name: "guidelines_mcp_help",
            description:
                "Returns a reference guide on how to use the JSdotNet Project Guidelines MCP server: " +
                "all available tools, common tags, the consultation decision guide, and workflow examples. " +
                "Call this when unsure which MCP tool to use or what tags are available.",
            parameters: { type: "object", properties: {} },
            handler: async () => `# JSdotNet Project Guidelines MCP — Reference Guide

## Available Tools

| Tool | Parameters | Purpose |
|------|-----------|---------|
| list_docs | — | Full document catalog with metadata |
| list_docs_by_type | category | Docs in one category |
| search_docs | query | Free-text search across titles, descriptions, content |
| search_docs_by_tag | tag | Exact-match tag filter |
| get_doc | id | Full markdown of a single document |
| get_usage_logs | count | Recent invocation records (default 20, max 100) |

## Categories

| Category | Contents |
|----------|---------|
| adrs | Architecture Decision Records — status, context, decision, consequences |
| designs | Exploratory design documents and diagrams |
| recommendations | Prescriptive best-practice guidance |
| structures | Canonical project scaffolds and templates |

## Common Tags

dotnet, architecture, domain, persistence, resilience, testing, logging,
cqrs, ddd, hexagonal, security, error-handling, configuration, observability

## Decision Guide — Which Tool to Use?

1. "What guidance exists for X?" -> search_docs("X")
2. "Which tags does topic X fall under?" -> search_docs("X") -> look at tags -> search_docs_by_tag("tag")
3. "Show me all accepted architectural decisions." -> list_docs_by_type("adrs")
4. "What project templates are available?" -> list_docs_by_type("structures")
5. "I have a doc ID — read it." -> get_doc("the-id")
6. "What is the team actually consulting?" -> get_usage_logs(50)

## Workflow Example: Implementing a Persistence Adapter

  1. search_docs_by_tag("persistence")   -> discover related docs
  2. list_docs_by_type("adrs")           -> find any persistence ADRs
  3. get_doc("adr-id")                   -> read decision + consequences
  4. list_docs_by_type("structures")     -> find adapter scaffold template
  5. get_doc("structure-id")             -> read the template

## Tips

- ADR status matters: Accepted = must follow; Deprecated/Superseded = don't use.
- Cite decisions in code: // ADR-0003: repository returns domain entities, not EF entities
- Recommendation documents are prescriptive; designs are exploratory.
- Superseded ADRs reference the replacing ADR — always read the successor.`,
        },
    ],
});
