# Skill: Test MCP Servers

**Description:** Verify that both JSdotNet MCP servers are responding correctly, returning expected content, and that their tools work end-to-end. Use after installation, upgrades, or when troubleshooting unexpected tool behavior.

---

## Two Servers to Test

| Server | MCP name | Serves |
|--------|----------|--------|
| **Coding Guidelines** | `jsdotnet-coding-guidelines` | `guide/` — ADRs, designs, recommendations, structures, config |
| **Design / UX** | `jsdotnet-design-ux-guidelines` | `design/` — UX style guide (color, typography, spacing, motion) |

Run the smoke tests below against each server. If a server is not connected, the tool calls will fail — that itself is a test result.

---

## Smoke Tests: Coding Guidelines Server (`jsdotnet-coding-guidelines`)

Run each call and verify the expected outcome.

### Test 1 — List all guides

```
list_guides()
```

**Expected:** JSON array with multiple entries. Each entry has `id`, `title`, `category`, `relativePath`, `tags`.  
**Pass criteria:** At least one entry with `category` of `adrs`, `recommendations`, or `structures`.  
**Fail:** Empty array or error → server is not connected or `guide/` content is missing.

---

### Test 2 — List by type

```
list_guides_by_type("adrs")
```

**Expected:** Array of ADR documents only.  
**Pass criteria:** All returned entries have `category` equal to `adrs`.  
**Fail:** Empty array → no ADRs exist; non-ADR entries → category filter broken.

```
list_guides_by_type("recommendations")
list_guides_by_type("structures")
```

Repeat for each category. All three should return non-empty arrays.

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

**Expected:** Full markdown content of the ADR (hundreds of characters minimum).  
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

## Smoke Tests: Design / UX Server (`jsdotnet-design-ux-guidelines`)

### Test 7 — List all design guides

```
list_guides()
```

**Expected:** JSON array with 6 entries (5 style-guide docs + README).  
**Pass criteria:** All entries have `category` equal to `style-guide`.  
**Fail:** Empty array or wrong categories → server is not connected or `design/` folder is missing.

---

### Test 8 — List by category

```
list_guides_by_type("style-guide")
```

**Expected:** Same 6 documents as above.  
**Pass criteria:** All entries have `category` = `style-guide`.

---

### Test 9 — Tag search for design tokens

```
search_guides_by_tag("design-tokens")
search_guides_by_tag("color")
search_guides_by_tag("typography")
```

**Expected:** Each returns at least one document.  
**Pass criteria:** `design-tokens` returns 4+ docs; `color` returns at least 2; `typography` returns at least 1.

---

### Test 10 — Fetch a design guide

```
get_guide("01-color-palette")
get_guide("02-typography")
```

**Expected:** Full markdown for the respective style-guide document.  
**Pass criteria:** Content includes token definitions and usage rules.

---

## Cross-Server Isolation Test

Both servers use the same tool names. Verify they return **different** content:

```
# On jsdotnet-coding-guidelines:
search_guides_by_tag("ddd")
→ Should return ADRs or recommendations about Domain-Driven Design

# On jsdotnet-design-ux-guidelines:
search_guides_by_tag("ddd")
→ Should return zero results (DDD is a coding concept, not a design token)
```

**Pass criteria:** Results differ between servers.  
**Fail:** Both return the same results → servers may be pointing at the same content directory.

---

## Full Test Checklist

| # | Test | Server | Pass condition |
|---|------|--------|----------------|
| 1 | `list_guides()` returns entries | Guidelines | ≥1 entry with known category |
| 2 | `list_guides_by_type("adrs")` | Guidelines | All entries are ADRs |
| 3 | `search_guides("hexagonal")` | Guidelines | ≥1 result |
| 4 | `search_guides_by_tag("testing")` | Guidelines | ≥1 result |
| 5 | `get_guide(<adr-id>)` | Guidelines | Non-empty markdown |
| 6 | `get_usage_logs(5)` | Guidelines | Logs from current session |
| 7 | `list_guides()` returns 6 entries | Design | All `style-guide` category |
| 8 | `list_guides_by_type("style-guide")` | Design | 6 entries |
| 9 | `search_guides_by_tag("color")` | Design | ≥2 results |
| 10 | `get_guide("01-color-palette")` | Design | Non-empty markdown |
| 11 | Cross-server isolation | Both | Different results for `ddd` |

---

## Diagnosing Failures

### Server not responding / tools not available

1. Check `.mcp.json` in the project root — both servers must be listed.
2. For `jsdotnet-coding-guidelines`: verify `jsdotnet-guidelines-mcpserver` is installed globally (`dotnet tool list -g`).
3. For `jsdotnet-design-ux-guidelines`: verify the project builds (`dotnet build src/JSdotNet.MCP.Design`).
4. Reload extensions: run `extensions_reload` in the Copilot session.

### Empty results from `list_guides` or `search_guides`

1. Check that `guide/index.json` (or `design/index.json`) exists and is current.
2. The index must match the actual markdown files in the folder.
3. If stale: regenerate the index by scanning all markdown files and updating `index.json`.

### `get_guide` returns empty

1. Verify the `id` exists in `index.json`.
2. Confirm the markdown file referenced by `relativePath` exists on disk.
3. Check environment variable `JSDOTNET_DOCS_PATH` — if set, it overrides the default path.

### `get_usage_logs` returns empty

1. Usage logs are written per process. Each server start creates a new log file.
2. Logs only contain entries from tool calls made in the **current session**.
3. Default log path: `%LOCALAPPDATA%\JSdotNet\GuidelinesMcpServer\` (Guidelines) or `DesignMcpServer\` (Design).

---

## Tips

- **Run smoke tests after every install or upgrade** to catch regressions early.
- **Run the cross-server isolation test** if you suspect content is mixed up.
- **Use `get_usage_logs` to confirm a tool was called** — it proves the server received the call.
- **Test both servers independently** — a healthy Guidelines server doesn't mean the Design server is working.
