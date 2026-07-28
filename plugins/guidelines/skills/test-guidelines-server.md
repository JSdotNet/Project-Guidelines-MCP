# Skill: Test Coding Guidelines Server

**Description:** Verify that the `jsdotnet-coding-guidelines` MCP server is responding correctly, returning expected content, and that all tools work end-to-end. Use after installation, upgrades, or when troubleshooting unexpected tool behavior.

---

## Server Under Test

| Server | MCP name | Serves |
|--------|----------|--------|
| **Coding Guidelines** | `jsdotnet-coding-guidelines` | `guide/` — ADRs, designs, recommendations, structures, config |

> To test the design/UX server, use the **test-design-server** skill from the `jsdotnet-design-ux-guidelines` plugin.

---

## Smoke Tests

Run each call in order and verify the expected outcome.

### Test 1 — List all guides

```
list_guides()
```

**Expected:** JSON array with multiple entries. Each entry has `id`, `title`, `category`, `relativePath`, `tags`.  
**Pass criteria:** At least one entry with `category` of `adrs`, `recommendations`, or `structures`.  
**Fail:** Empty array or error → server is not connected or `guide/` content is missing.

---

### Test 2 — List by type (all categories)

```
list_guides_by_type("adrs")
list_guides_by_type("recommendations")
list_guides_by_type("structures")
```

**Expected:** Non-empty array for each call. All entries in each call share the same `category`.  
**Pass criteria:** Each call returns ≥1 result; no cross-category contamination.  
**Fail:** Empty array → no documents of that type exist; mixed categories → category filter is broken.

---

### Test 3 — Free-text search

```
search_guides("hexagonal")
search_guides("error handling")
search_guides("persistence")
```

**Expected:** At least one result per query.  
**Pass criteria:** Results contain documents whose titles or descriptions mention the search term.  
**Fail:** Zero results → index is stale or content is missing. Regenerate `guide/index.json`.

---

### Test 4 — Tag search

```
search_guides_by_tag("cqrs")
search_guides_by_tag("testing")
search_guides_by_tag("persistence")
```

**Expected:** One or more matching documents per tag.  
**Pass criteria:** All returned documents have the queried tag in their `tags` array.  
**Fail:** Zero results for a common tag → documents may be missing tag metadata.

---

### Test 5 — Fetch a specific guide

```
list_guides_by_type("adrs")
```

Pick any `id` from the result, then:

```
get_guide("<id-from-step-above>")
```

**Expected:** Full markdown content of the document (hundreds of characters minimum).  
**Pass criteria:** Content includes readable headings and text; not an empty string.  
**Fail:** Empty string or error → `guide/` directory not found or file is missing.

---

### Test 6 — Usage logs

```
get_usage_logs(5)
```

**Expected:** JSON array with up to 5 recent log entries, each with `toolName`, `timestamp`, `succeeded`.  
**Pass criteria:** Array contains entries from Tests 1–5 above.  
**Fail:** Empty array → usage log is not being written; check `JSDOTNET_LOG_PATH` or default log path.

---

## Full Test Checklist

| # | Test | Pass condition |
|---|------|----------------|
| 1 | `list_guides()` | ≥1 entry with a known category |
| 2 | `list_guides_by_type("adrs")` | All entries are ADRs |
| 3 | `list_guides_by_type("recommendations")` | All entries are recommendations |
| 4 | `list_guides_by_type("structures")` | All entries are structures |
| 5 | `search_guides("hexagonal")` | ≥1 result |
| 6 | `search_guides_by_tag("testing")` | ≥1 result |
| 7 | `get_guide(<adr-id>)` | Non-empty markdown |
| 8 | `get_usage_logs(5)` | Logs from current session |

---

## Diagnosing Failures

### Server not responding / tools not available

1. Check `.mcp.json` in the project root — `jsdotnet-coding-guidelines` must be listed.
2. Verify `jsdotnet-guidelines-mcpserver` is installed globally: `dotnet tool list -g`
3. If missing, install: `dotnet tool install -g JSdotNet.MCP.Guidelines`
4. Reload extensions: run `extensions_reload` in the Copilot session.

### Empty results from `list_guides` or `search_guides`

1. Check that `guide/index.json` exists and is current.
2. The index must match the actual markdown files in `guide/`.
3. If stale: regenerate the index by scanning all markdown files and updating `guide/index.json`.

### `get_guide` returns empty

1. Verify the `id` exists in `guide/index.json`.
2. Confirm the markdown file referenced by `relativePath` exists on disk.
3. Check environment variable `JSDOTNET_DOCS_PATH` — if set, it overrides the default path.

### `get_usage_logs` returns empty

1. Usage logs are written per process. Each server start creates a new log file.
2. Logs only contain entries from tool calls made in the **current session**.
3. Default log path: `%LOCALAPPDATA%\JSdotNet\GuidelinesMcpServer\`

---

## Tips

- **Run smoke tests after every install or upgrade** to catch regressions early.
- **Use `get_usage_logs` to confirm a tool was called** — it proves the server received the call.
- **Check `list_guides_by_type` for all five categories** — a healthy index should have docs in each.

