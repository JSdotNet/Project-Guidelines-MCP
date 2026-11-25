---
name: jsdotnet-app-defaults
description: "Use the jsdotnet-app-defaults MCP server: which tool, category or prompt answers a question about the JSdotNet application defaults (ADRs, building blocks, crosscutting concepts, AI setup, design tokens), and how to cite the answer. Use when: looking up a default before writing code, choosing a tool or prompt, starting a new app, reviewing against the defaults, or proposing an ADR."
---

# jsdotnet-app-defaults

The server's own skill (ADR 0011). Tools, categories, document ids and tags are in
[reference.md](reference.md).

## Workflow

1. **Find the documents.** A known id → `get_guide(id)`. A known topic →
   `search_guides_by_tag(tag)`. Otherwise → `search_guides(query)`. A whole kind (every ADR,
   every building block) → `list_guides_by_type(category)`.
2. **Read before applying.** Call `get_guide` on every document a step relies on; a search
   result is a pointer, not the rule.
3. **Prefer the server's prompt** when the task is one of its workflows:
   - `new-app` — scaffold a new application to the golden setup;
   - `review-against-defaults` — review code, a diff, a pull request or a design and report
     each deviation with its doc id;
   - `propose-adr` — draft an ADR for a decision no document covers.
4. **Cite every default you apply** by doc id, in the answer and in code
   (`// ADR-0003: …`).
5. **A choice the defaults leave to the application** (database, broker, frontend framework)
   is recorded as the application's own ADR, never guessed.
6. **Nothing covers it** → say so and use `propose-adr`; never answer from memory.

For the `design` category, use the `design-token-lookup`, `ui-component-styling` and
`design-customization` skills. `get_usage_logs` is for the `feedback-loop` skill only.
