# Skill: Test Design & UX Server

**Description:** Verify that the `jsdotnet-design-ux-guidelines` MCP server is responding correctly, returning expected style-guide content, and that all tools work end-to-end. Use after installation, upgrades, or when troubleshooting unexpected tool behavior.

---

## Server Under Test

| Server | MCP name | Serves |
|--------|----------|--------|
| **Design / UX** | `jsdotnet-design-ux-guidelines` | `design/` — UX style guide (color, typography, spacing, motion) |

> To test the coding guidelines server, use the **test-guidelines-server** skill from the `jsdotnet-project-guidelines` plugin.

---

## Smoke Tests

Run each call in order and verify the expected outcome.

### Test 1 — List all guides

```
list_guides()
```

**Expected:** JSON array with 6 entries (5 style-guide docs + README).  
**Pass criteria:** All entries have `category` = `style-guide`; IDs include `01-color-palette`, `02-typography`, `03-spacing-and-layout`, `04-motion-and-interaction`, `05-customization-guide`, `readme`.  
**Fail:** Empty array or wrong categories → server is not connected or `design/` folder is missing.

---

### Test 2 — List by category

```
list_guides_by_type("style-guide")
```

**Expected:** Same 6 documents as Test 1.  
**Pass criteria:** All entries have `category` = `style-guide`; count = 6.  
**Fail:** Empty array → category filter broken or no docs; wrong count → index out of sync with disk.

---

### Test 3 — Tag search for design tokens

```
search_guides_by_tag("design-tokens")
search_guides_by_tag("color")
search_guides_by_tag("typography")
```

**Expected:** Each returns at least one document.  
**Pass criteria:**
- `design-tokens` returns ≥4 docs (most docs share this tag)
- `color` returns ≥2 docs (color palette + customization guide)
- `typography` returns ≥1 doc (typography doc)

**Fail:** Zero results for any tag → tag metadata missing from `design/index.json`.

---

### Test 4 — Free-text search

```
search_guides("color palette")
search_guides("typography")
search_guides("customization")
```

**Expected:** At least one result per query.  
**Pass criteria:** Results contain documents whose titles or descriptions match the search term.  
**Fail:** Zero results → `design/index.json` is stale or missing. Regenerate it.

---

### Test 5 — Fetch specific guides

```
get_guide("01-color-palette")
get_guide("02-typography")
get_guide("05-customization-guide")
```

**Expected:** Full markdown content for each document (hundreds of characters minimum).  
**Pass criteria:** Content includes token tables with light/dark values and usage rules.  
**Fail:** Empty string or error → `design/` directory not found or file is missing.

---

### Test 6 — Usage logs

```
get_usage_logs(5)
```

**Expected:** JSON array with up to 5 recent log entries, each with `toolName`, `timestamp`, `succeeded`.  
**Pass criteria:** Array contains entries from Tests 1–5 above.  
**Fail:** Empty array → usage log is not being written; check default log path.

---

## Cross-Server Isolation Check

Both the design server and the coding guidelines server expose the same tool names. Verify they return **different** content:

```
# On jsdotnet-design-ux-guidelines:
search_guides_by_tag("ddd")
→ Expected: zero results (DDD is a coding concept, not a design token)

# On jsdotnet-coding-guidelines:
search_guides_by_tag("color")
→ Expected: zero results (color is a design concept, not in the coding guide)
```

**Pass criteria:** Each tag returns results only on the server that owns its content.  
**Fail:** Both return the same results → servers may be pointing at the same content directory.

---

## Full Test Checklist

| # | Test | Pass condition |
|---|------|----------------|
| 1 | `list_guides()` | 6 entries, all `style-guide` category |
| 2 | `list_guides_by_type("style-guide")` | 6 entries |
| 3 | `search_guides_by_tag("design-tokens")` | ≥4 results |
| 4 | `search_guides_by_tag("color")` | ≥2 results |
| 5 | `search_guides_by_tag("typography")` | ≥1 result |
| 6 | `search_guides("color palette")` | ≥1 result |
| 7 | `get_guide("01-color-palette")` | Non-empty markdown with token tables |
| 8 | `get_guide("05-customization-guide")` | Non-empty markdown with override contract |
| 9 | `get_usage_logs(5)` | Logs from current session |
| 10 | Cross-server: `search_guides_by_tag("ddd")` on design server | Zero results |

---

## Diagnosing Failures

### Server not responding / tools not available

1. Check `.mcp.json` in the project root — `jsdotnet-design-ux-guidelines` must be listed.
2. Verify the Design MCP project builds: `dotnet build src/JSdotNet.MCP.Design`
3. If the build fails, check for compilation errors in `src/JSdotNet.MCP.Design/`.
4. Reload extensions: run `extensions_reload` in the Copilot session.

### Empty results from `list_guides` or `search_guides`

1. Check that `design/index.json` exists and is current.
2. The index must match the actual markdown files in `design/`.
3. If stale: regenerate the index by scanning all markdown files in `design/` and updating `design/index.json`.
4. Expected documents: `README.md`, `01-color-palette.md`, `02-typography.md`, `03-spacing-and-layout.md`, `04-motion-and-interaction.md`, `05-customization-guide.md`

### `get_guide` returns empty

1. Verify the `id` exists in `design/index.json`.
2. Confirm the markdown file referenced by `relativePath` exists on disk in `design/`.
3. Check environment variable `JSDOTNET_DOCS_PATH` — if set, it overrides the default path.

### `get_usage_logs` returns empty

1. Usage logs are written per process. Each server start creates a new log file.
2. Logs only contain entries from tool calls made in the **current session**.
3. Default log path: `%LOCALAPPDATA%\JSdotNet\DesignMcpServer\`

---

## Tips

- **Run smoke tests after every install or upgrade** to catch regressions early.
- **Run the cross-server isolation check** if you suspect the design and coding servers are returning mixed content.
- **Use `get_usage_logs` to confirm a tool was called** — it proves the server received the call.
- **A healthy design server should always return exactly 6 docs** from `list_guides` — use this as a quick sanity check.
