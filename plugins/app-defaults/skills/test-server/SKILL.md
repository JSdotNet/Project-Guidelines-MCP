---
name: test-server
description: "Smoke-test the jsdotnet-app-defaults MCP server after install, upgrade or a content change: each tool, each category, the design category and the usage log, with a pass or fail per check. Use when: the server was just installed or upgraded, tools return nothing, or a document seems missing."
---

# Test the jsdotnet-app-defaults server

Run each check in order and report pass or fail with what came back. Failures:
[reference.md](reference.md).

| # | Call | Pass when |
|---|---|---|
| 1 | `list_guides()` | Entries in every category: `adrs`, `designs`, `structures`, `recommendations`, `config`, `design` |
| 2 | `list_guides_by_type(c)` for each category | Each returns ≥1 entry, all of category `c` |
| 3 | `list_guides_by_type("design")` | Only style-guide documents, `readme` and `01-color-palette` among them |
| 4 | `search_guides("persistence")`, `search_guides("color palette")` | ≥1 result each, matching the term |
| 5 | `search_guides_by_tag("cqrs")`, `search_guides_by_tag("design-tokens")` | ≥1 result each, every one carrying the tag |
| 6 | `get_guide("0003-solution-structure")`, `get_guide("01-color-palette")` | Full markdown with headings; the palette has light and dark values |
| 7 | `get_usage_logs(5)` | The calls made above, each with `toolName`, `timestamp`, `succeeded` |

Close with one line: all passed, or the failing check numbers and the likely cause.
