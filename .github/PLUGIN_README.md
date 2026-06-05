# JSdotNet Project Guidelines Plugin

This plugin provides three complementary skills to help you work with the JSdotNet Project Guidelines MCP server:

1. **guidelines-mcp** — Reference guide for using MCP tools
2. **gap-analysis** — Analyze project structure against guidelines
3. **feedback-loop** — Track MCP usage and propose improvements

Plus one extension that enables gap analysis and feedback loop tools.

---

## What You Get

### Skills (Available to Copilot agents)

- **guidelines-mcp** — How to use the six MCP tools for guided decisions
- **gap-analysis** — Workflow for identifying architectural gaps and solutions
- **feedback-loop** — Process for analyzing usage logs and creating improvement issues

### Extension (Provides tools)

- **guidelines-feedback** — Node.js extension that provides `analyze_guidelines_usage`, `draft_guidelines_issue`, and `submit_guidelines_issue` tools
  - Reads MCP server JSONL logs
  - Aggregates usage statistics
  - Drafts and submits GitHub issues

---

## Installation

### Option 1: Copy to Your User Extensions (Global)

Recommended if you want to use these skills across multiple repositories.

1. Clone or download this plugin folder
2. Copy the skills to your Copilot skills directory:
   ```bash
   cp -r .github/skills/* ~/.copilot/skills/
   ```
3. Copy the extension to your user extensions directory:
   ```bash
   cp -r .github/extensions/guidelines-feedback ~/.copilot/extensions/
   ```
4. Restart your Copilot CLI session

### Option 2: Keep Project-Scoped (This Repository Only)

The plugin is already in `.github/` — no installation needed. Skills and extensions auto-load when you open a session in this repo.

### Option 3: Install via Copilot CLI Tool

If you have the `install_extension` tool available:

```bash
# Install the feedback extension from this repo
install_extension url="https://github.com/JSdotNet/Project-Guidelines-MCP/tree/main/.github/extensions/guidelines-feedback"

# Manually copy skills to ~/.copilot/skills/ (or this repo's .github/skills/)
```

---

## Quick Start

### 1. Learn the MCP Basics

```
Invoke skill: guidelines-mcp

This teaches you:
- Six MCP tools available
- When to use each tool
- Common tags for searching
- Example workflows
```

### 2. Analyze Your Project

```
If start_gap_analysis tool is available (from guidelines-feedback extension):
  Call: start_gap_analysis(projectPath: "/path/to/MyProject")

Then follow the returned workflow to:
- Use search_docs and get_doc to find relevant ADRs
- Identify architectural gaps
- Plan improvements
```

### 3. Track Improvement Opportunities

```
Call: analyze_guidelines_usage()
  → Identify patterns in MCP tool usage

Call: draft_guidelines_issue(title, body, labels)
  → Prepare a GitHub issue with evidence

Call: submit_guidelines_issue(draftId)
  → Create the issue in the guidelines repo
```

---

## Skills Usage

Each skill can be invoked by name from a Copilot session. They work best when combined:

| Skill | Best Used When | Pairs With |
|-------|---|---|
| **guidelines-mcp** | Starting out; learning MCP tools | Any other skill |
| **gap-analysis** | Analyzing project structure | guidelines-mcp, feedback-loop |
| **feedback-loop** | Proposing improvements to guidelines | guidelines-mcp, gap-analysis |

### Example Combined Workflow

```
1. guidelines-mcp skill
   → Learn the tools

2. gap-analysis skill  
   → Run project scan
   → Identify gaps (e.g., missing Domain layer)

3. guidelines-mcp skill
   → search_docs("domain layer")
   → get_doc("adr-NNNN")
   → Understand recommended structure

4. feedback-loop skill
   → analyze_guidelines_usage()
   → If gap is common: draft_guidelines_issue(...)
   → submit_guidelines_issue(...)

5. Back to gap-analysis
   → Plan implementation based on ADRs
   → Reference ADR numbers in code
```

---

## The Three Skills Explained

### Skill 1: guidelines-mcp

**What it teaches**: How to use the MCP server.

**When to invoke it**: 
- You're new to the MCP server
- You need a reference for which tool to use
- You want to understand common tags

**Output**: Reference guide, decision tree, example workflows.

---

### Skill 2: gap-analysis

**What it teaches**: How to analyze your project structure against guidelines.

**When to invoke it**:
- You want to audit your project architecture
- You're implementing a new layer and need guidance
- You want to identify common structural issues

**Output**: Scan instructions, common gap patterns, solutions with ADR references.

---

### Skill 3: feedback-loop

**What it teaches**: How to use MCP usage logs to improve guidelines.

**When to invoke it**:
- You notice a pattern of "what users are searching for but not finding"
- You want to propose a new ADR or recommendation
- You're collecting evidence for documentation improvements

**Output**: Analysis workflow, issue drafting template, submission process.

---

## File Structure

```
.github/
  skills/
    guidelines-mcp.md          ← Skill 1: MCP reference
    gap-analysis.md            ← Skill 2: Gap analysis workflow
    feedback-loop.md           ← Skill 3: Feedback loop process
  extensions/
    guidelines-feedback/
      extension.mjs            ← Provides tools for skills 2 & 3
  plugin-manifest.json         ← This plugin's metadata (optional)

README.md                       ← This file
```

---

## Troubleshooting

### Skills are not showing up

**Check**: Are you in a Copilot session in this repository or have you installed them globally?

- Project-scoped: Skills auto-load from `.github/skills/` when you start a session
- Global: Skills must be copied to `~/.copilot/skills/` and your session restarted

**Fix**: Restart your Copilot CLI session or run:
```bash
extensions_reload
```

### Extension tools are not available

**Check**: Is the `guidelines-feedback` extension loaded?

**Fix**: Ensure the extension is in `.github/extensions/guidelines-feedback/` (project) or `~/.copilot/extensions/guidelines-feedback/` (global), then restart your session.

**Verify**: Run:
```bash
extensions_manage operation=list
```

Look for `guidelines-feedback` in the list.

### `analyze_guidelines_usage` returns no logs

**Check**: Has the MCP server been used yet? Usage logs are only created when tools are called.

**Fix**: Open the guidelines repository and call MCP tools (e.g., `search_docs("ADR")`), then try the feedback loop again.

**Note**: Logs are stored in `%LOCALAPPDATA%\JSdotNet\MCPServer` (Windows) or `~/.jsdotnet/mcp-server` (macOS/Linux). Set `JSDOTNET_LOG_PATH` to override.

---

## Integration with Projects

### In Your Own .NET Project

1. **Install globally** (option 1 above)
2. Open a Copilot session in your project
3. Invoke: `Skill: gap-analysis` to scan your project
4. Use `search_docs` to find guidance for any gaps
5. Use the feedback loop to propose improvements

### In the Guidelines Repository

1. Skills and extension are project-scoped (already in `.github/`)
2. Use `Skill: guidelines-mcp` to teach new contributors
3. Use `Skill: feedback-loop` to prioritize documentation work
4. Changes to skills are committed to the repo and versioned

---

## Configuration

### Environment Variables

The **guidelines-feedback** extension respects:

- `JSDOTNET_LOG_PATH` — Where to read/write MCP logs (default: `%LOCALAPPDATA%\JSdotNet\MCPServer`)
- `GH_TOKEN` — GitHub token for issue creation (defaults to `gh auth` credentials)

### Log Directory

MCP logs are written to per-process JSONL files:
```
%LOCALAPPDATA%\JSdotNet\MCPServer\
  usage-12345.jsonl
  usage-12346.jsonl
  ...
```

Each entry is a JSON object with: `toolName`, `parameters`, `result`, `timestamp`, `success`.

---

## Support & Feedback

If you have questions or find bugs:

1. Check this README section: **Troubleshooting**
2. Review the individual skill markdown files for detailed workflows
3. Open an issue in the JSdotNet/Project-Guidelines-MCP repository

---

## Version History

- **1.0** (2026-06-05): Initial release with three skills + feedback extension
  - guidelines-mcp: MCP reference guide
  - gap-analysis: Project structure audit workflow
  - feedback-loop: Usage analysis + issue creation workflow
  - guidelines-feedback extension: JSONL parsing + GitHub issue tools
