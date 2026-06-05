import { joinSession } from "@github/copilot-sdk/extension";
import { readdir, readFile, writeFile, unlink } from "node:fs/promises";
import { join } from "node:path";
import { execFile } from "node:child_process";
import { promisify } from "node:util";
import { tmpdir } from "node:os";

const execFileAsync = promisify(execFile);

// Drafts staged for confirmation before being submitted as GitHub issues.
const draftStore = new Map();

const session = await joinSession({
    tools: [
        // ------------------------------------------------------------------
        // Tool 1: Analyze usage logs
        // ------------------------------------------------------------------
        {
            name: "analyze_guidelines_usage",
            description:
                "Reads the MCP server usage logs (JSONL files written by JsonFileUsageLog) and returns a " +
                "structured analysis of usage patterns: tool call frequencies, zero-result searches, " +
                "most accessed documents, and failed calls. This is global server telemetry — it aggregates " +
                "across all sessions and repos that share the same log directory. " +
                "Use this as the first step of the feedback loop before creating improvement issues.",
            parameters: {
                type: "object",
                properties: {
                    maxEntries: {
                        type: "number",
                        description: "Maximum number of recent log entries to analyse (default 200, max 1000).",
                    },
                    maxAgeDays: {
                        type: "number",
                        description: "Only include entries from the last N days (default 30).",
                    },
                },
            },
            handler: async (args) => {
                const maxEntries = Math.min(args.maxEntries || 200, 1000);
                const maxAgeDays = args.maxAgeDays || 30;
                const cutoff = Date.now() - maxAgeDays * 24 * 60 * 60 * 1000;

                const logDir = resolveLogDir();
                let allEntries = [];

                try {
                    const files = (await readdir(logDir)).filter(
                        (f) => f.startsWith("usage-") && f.endsWith(".jsonl")
                    );

                    if (files.length === 0) {
                        return `No usage log files found in ${logDir}.\n\nThe MCP server must have been used at least once to generate logs.`;
                    }

                    for (const file of files) {
                        let content;
                        try {
                            content = await readFile(join(logDir, file), "utf-8");
                        } catch {
                            continue; // file locked or deleted — skip
                        }
                        for (const line of content.split("\n")) {
                            if (!line.trim()) continue;
                            try {
                                const entry = JSON.parse(line);
                                const ts = new Date(entry.timestamp).getTime();
                                if (!isNaN(ts) && ts >= cutoff) {
                                    allEntries.push(entry);
                                }
                            } catch {
                                // malformed line — skip
                            }
                        }
                    }
                } catch (err) {
                    return `Could not read log directory ${logDir}: ${err.message}\n\nSet JSDOTNET_LOG_PATH to override the log location.`;
                }

                if (allEntries.length === 0) {
                    return `No log entries found within the last ${maxAgeDays} days in ${logDir}.`;
                }

                // Take last N entries (chronological)
                allEntries.sort((a, b) => new Date(a.timestamp) - new Date(b.timestamp));
                const entries = allEntries.slice(-maxEntries);

                // Aggregate statistics
                const toolCounts = {};
                const queryFreq = {};
                const docFreq = {};
                const zeroResults = [];
                const failures = [];

                for (const e of entries) {
                    toolCounts[e.toolName] = (toolCounts[e.toolName] || 0) + 1;

                    const param = e.parameters?.query || e.parameters?.tag || e.parameters?.category || e.parameters?.id;
                    if (param) queryFreq[param] = (queryFreq[param] || 0) + 1;

                    for (const id of e.resultDocumentIds || []) {
                        docFreq[id] = (docFreq[id] || 0) + 1;
                    }

                    if (e.resultCount === 0 && ["search_docs", "search_docs_by_tag", "list_docs_by_type"].includes(e.toolName)) {
                        zeroResults.push({ tool: e.toolName, params: e.parameters });
                    }

                    if (!e.succeeded && e.errorMessage) {
                        failures.push({ tool: e.toolName, params: e.parameters, error: e.errorMessage });
                    }
                }

                const topDocs = Object.entries(docFreq).sort((a, b) => b[1] - a[1]).slice(0, 10);
                const topQueries = Object.entries(queryFreq).sort((a, b) => b[1] - a[1]).slice(0, 10);
                const uniqueZero = [...new Map(zeroResults.map((r) => [JSON.stringify(r.params), r])).values()];

                return `# Guidelines MCP Usage Analysis

> **Scope:** Global telemetry across all sessions. Covers ${entries.length} entries from the last ${maxAgeDays} days.
> **Log directory:** ${logDir}
> **Log files:** ${(await readdir(logDir).catch(() => [])).filter((f) => f.startsWith("usage-") && f.endsWith(".jsonl")).length} file(s)

## Tool Call Frequency
${Object.entries(toolCounts).map(([k, v]) => `- \`${k}\`: ${v} calls`).join("\n")}

## Top Search Queries / Parameters (${topQueries.length})
${topQueries.length > 0 ? topQueries.map(([q, c]) => `- "${q}" — ${c}×`).join("\n") : "_(none)_"}

## Most Accessed Documents (${topDocs.length})
${topDocs.length > 0 ? topDocs.map(([id, c]) => `- \`${id}\` — ${c}×`).join("\n") : "_(none)_"}

## Zero-Result Searches (${uniqueZero.length} distinct)
${uniqueZero.length > 0
    ? uniqueZero.map((r) => `- \`${r.tool}\`(${JSON.stringify(r.params)})`).join("\n")
    : "None — all searches returned results ✅"}

## Failed Tool Calls (${failures.length})
${failures.length > 0
    ? failures.map((f) => `- \`${f.tool}\`: ${f.error}`).join("\n")
    : "None ✅"}

## Suggested Next Steps

Use the guidelines MCP to cross-check before creating issues:
1. For each zero-result search, call \`search_docs("term")\` to confirm the gap is real.
2. For frequently accessed docs, call \`get_doc("id")\` to assess quality and completeness.
3. For repeated queries without a matching document, consider drafting a new ADR or recommendation.

Then call \`draft_guidelines_issue\` for each improvement, and \`submit_guidelines_issue\` to file it.`;
            },
        },

        // ------------------------------------------------------------------
        // Tool 2: Draft an issue (store locally, don't submit yet)
        // ------------------------------------------------------------------
        {
            name: "draft_guidelines_issue",
            description:
                "Drafts a GitHub issue for a guidelines improvement and stores it locally for review. " +
                "Does NOT submit to GitHub yet — call submit_guidelines_issue to do that. " +
                "Use this after analyze_guidelines_usage and MCP research to prepare improvement tickets. " +
                "Returns a draft ID to reference when submitting.",
            parameters: {
                type: "object",
                properties: {
                    title: {
                        type: "string",
                        description: "Clear, actionable issue title (e.g. 'Add ADR for Redis caching strategy').",
                    },
                    body: {
                        type: "string",
                        description:
                            "Full issue body in markdown. Include: motivation/evidence from usage analysis, " +
                            "what the document should contain, and acceptance criteria.",
                    },
                    labels: {
                        type: "array",
                        items: { type: "string" },
                        description: "Labels to apply. Common: documentation, enhancement, missing-adr, feedback-loop.",
                    },
                },
                required: ["title", "body"],
            },
            handler: async (args) => {
                const id = `draft-${Date.now()}`;
                draftStore.set(id, {
                    title: args.title,
                    body: args.body,
                    labels: args.labels || ["documentation", "feedback-loop"],
                });

                return `## Issue Drafted

**Draft ID:** \`${id}\`
**Title:** ${args.title}
**Labels:** ${(args.labels || ["documentation", "feedback-loop"]).join(", ")}

**Body preview:**
${args.body.slice(0, 400)}${args.body.length > 400 ? "\n_(truncated — full body stored)_" : ""}

---
Review the draft above. When ready, call \`submit_guidelines_issue\` with draft ID \`${id}\` to create the GitHub issue.
To discard this draft, simply do not submit it.`;
            },
        },

        // ------------------------------------------------------------------
        // Tool 3: Submit a drafted issue to GitHub
        // ------------------------------------------------------------------
        {
            name: "submit_guidelines_issue",
            description:
                "Submits a previously drafted issue (created by draft_guidelines_issue) to GitHub " +
                "using the gh CLI. The issue is created in the current repository. " +
                "This does NOT use the MCP server — it uses the gh CLI directly.",
            parameters: {
                type: "object",
                properties: {
                    draftId: {
                        type: "string",
                        description: "The draft ID returned by draft_guidelines_issue.",
                    },
                },
                required: ["draftId"],
            },
            handler: async (args) => {
                const draft = draftStore.get(args.draftId);
                if (!draft) {
                    return `Draft "${args.draftId}" not found. Available drafts: ${[...draftStore.keys()].join(", ") || "none"}`;
                }

                const tmpFile = join(tmpdir(), `guidelines-issue-${Date.now()}.md`);
                try {
                    await writeFile(tmpFile, draft.body, "utf-8");

                    const ghArgs = [
                        "issue",
                        "create",
                        "--title", draft.title,
                        "--body-file", tmpFile,
                        ...draft.labels.flatMap((l) => ["--label", l]),
                    ];

                    const { stdout } = await execFileAsync("gh", ghArgs, { cwd: process.cwd() });
                    draftStore.delete(args.draftId);
                    return `✅ Issue created: ${stdout.trim()}`;
                } catch (err) {
                    return `❌ Failed to create issue: ${err.message}\n\nEnsure \`gh\` is authenticated (\`gh auth status\`) and you are in the repository directory.`;
                } finally {
                    await unlink(tmpFile).catch(() => {});
                }
            },
        },

        // ------------------------------------------------------------------
        // Tool 4: Feedback session workflow guide
        // ------------------------------------------------------------------
        {
            name: "prepare_feedback_session",
            description:
                "Returns a structured step-by-step guide for running a complete guidelines feedback loop: " +
                "analyze usage logs, research gaps with the MCP, draft improvement issues, and submit them to GitHub. " +
                "Start here at the beginning of a feedback session.",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Guidelines Feedback Loop — Session Workflow

## Overview

The feedback loop turns MCP server usage data into actionable GitHub improvement issues.
**Important:** Gather evidence with the MCP first; create issues only when the gap is confirmed.

---

## Step 1: Analyze Usage Logs

Call \`analyze_guidelines_usage\` to get a summary of recent patterns.

Look for:
- **Zero-result searches** — a search returned nothing, suggesting a documentation gap
- **Frequently searched terms** — high interest topics that may need better coverage or discoverability
- **Most accessed documents** — validate these are high quality and not outdated
- **Failed calls** — infrastructure or data quality problems
- **Imbalanced tool usage** — e.g. many \`search_docs\` but few \`get_doc\` may mean results aren't useful

---

## Step 2: Verify Gaps with the MCP

For each zero-result search or suspicious pattern, use the guidelines MCP to confirm the gap is real:

- \`search_docs("term")\` — confirm no relevant doc exists
- \`list_docs_by_type("adrs")\` — check if a decision should be documented
- \`get_doc("id")\` — read an existing doc to check if it needs expanding
- \`search_docs_by_tag("tag")\` — check discoverability via tags

---

## Step 3: Categorize Improvements

Common issue types for this repository:

| Type | When to use | Suggested label |
|------|------------|----------------|
| Missing ADR | Decision was made in practice but not documented | missing-adr |
| Missing recommendation | Pattern used but not prescribed | documentation |
| Improve discoverability | Doc exists but searches don't find it (tags/description) | enhancement |
| Outdated content | An ADR/recommendation is stale | maintenance |
| New structure template | A project pattern lacks a scaffold | documentation |
| Search quality | Frequently searched terms return irrelevant results | enhancement |

---

## Step 4: Draft Issues

For each confirmed gap, call \`draft_guidelines_issue\`:

**Good issue body includes:**
- Evidence: "Searched for X 12 times, no results returned"
- Context: "We use Redis for caching but have no ADR documenting the decision"
- What's needed: "An ADR covering: when to use Redis vs in-memory, TTL strategy, serialization"
- Acceptance criteria: "ADR is Accepted, tagged 'caching', search_docs('redis') returns it"

**Batch related issues** — don't create 1 issue per zero-result query; group by topic.

---

## Step 5: Review Drafts and Submit

Review each drafted issue (shown when you call \`draft_guidelines_issue\`).
When satisfied, call \`submit_guidelines_issue\` with the draft ID.

Issues are created in the current repository using \`gh issue create\` — NOT through the MCP server.`,
        },
    ],

    hooks: {},
});

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function resolveLogDir() {
    if (process.env.JSDOTNET_LOG_PATH) return process.env.JSDOTNET_LOG_PATH;
    const localAppData =
        process.env.LOCALAPPDATA || join(process.env.HOME || process.env.USERPROFILE || ".", ".local", "share");
    return join(localAppData, "JSdotNet", "MCPServer");
}
