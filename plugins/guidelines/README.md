# JSdotNet Project Coding Guidelines Plugin

This plugin provides eight skills for working with the **`jsdotnet-coding-guidelines`** MCP server and JSdotNet Copilot setup — the authoritative source of Architecture Decision Records (ADRs), design documents, recommendations, project structure templates, and related Azure Foundry catalog configuration guidance for JSdotNet .NET projects.

1. **coding-guidelines-mcp** — Reference guide for the coding guidelines MCP server: tool reference, categories, tags, and example workflows
2. **gap-analysis** — Analyze project structure against guidelines, identify architectural gaps
3. **code-review** — Review code for architectural and coding standards compliance
4. **decision-validation** — Validate architectural decisions before implementation
5. **migration-planning** — Plan incremental refactoring to align code with guidelines
6. **feedback-loop** — Track MCP usage and propose documentation improvements
7. **copilot-model-selection** — Select recommended Azure Foundry model catalog entries and avoid incompatible reasoning-model parameters
8. **test-guidelines-server** — Smoke-test the coding guidelines MCP server end-to-end

> For UX/design token guidance (color, typography, spacing, motion), use the companion **`jsdotnet-design-ux-guidelines`** plugin (`plugins/design/`).

---

## What the Coding Guidelines MCP Server Serves

The `jsdotnet-coding-guidelines` server serves documents from the `guide/` folder, organized in five categories:

| Category | Contents |
|---|---|
| `adrs` | Architecture Decision Records |
| `designs` | Exploratory design documents and diagrams |
| `recommendations` | Prescriptive best-practice guidance |
| `structures` | Canonical project scaffolds and templates |
| `config` | Configuration file guidelines |

---

## Installation

### Option 1: Copy to User Skills Directory (Global)

Recommended if you want to use these skills across multiple repositories.

```bash
# Windows PowerShell
Copy-Item plugins/guidelines/skills/* $env:USERPROFILE/.copilot/skills/

# macOS/Linux
cp plugins/guidelines/skills/* ~/.copilot/skills/
```

### Option 2: Keep Project-Scoped (This Repository Only)

Skills in `plugins/guidelines/skills/` auto-load as project-scoped skills when you open a Copilot session in this repository.

---

## Quick Start

### 1. Learn the MCP Server

```
Invoke skill: coding-guidelines-mcp
```

Teaches you the available tools, valid categories, common tags, and example workflows for coding/architecture tasks.

### 2. Analyze Your Project

```
Invoke skill: gap-analysis
```

Scans your project structure, identifies architectural gaps, and suggests relevant ADRs.

### 3. Review Code for Compliance

```
Invoke skill: code-review
```

Validates architectural alignment, coding standards, and pattern compliance.

### 4. Validate a Decision

```
Invoke skill: decision-validation
```

Checks guideline alignment before coding, evaluates alternatives with trade-offs.

### 5. Plan Incremental Refactoring

```
Invoke skill: migration-planning
```

Plans phases, manages dependencies, and tracks progress for large-scale alignments.

### 6. Track Improvement Opportunities

```
Invoke skill: feedback-loop
```

Analyzes usage logs, identifies documentation gaps, and drafts improvement issues.

### 7. Select a Copilot Model

```
Invoke skill: copilot-model-selection
```

Recommends Azure Foundry model catalog/deployment entries, token limits, and provider-safe parameter settings for Copilot sessions.

### 8. Test the Server

```
Invoke skill: test-guidelines-server
```

Verifies the server is connected and all tools return expected content.

---

## Skills Reference

| Skill | Best Used When | Pairs With |
|-------|---------------|------------|
| **coding-guidelines-mcp** | Starting out; learning tools and categories | Any other skill |
| **gap-analysis** | Auditing project architecture | coding-guidelines-mcp, code-review, migration-planning |
| **code-review** | Reviewing PRs or new code | coding-guidelines-mcp, decision-validation |
| **decision-validation** | Making significant architectural decisions | coding-guidelines-mcp, migration-planning |
| **migration-planning** | Planning refactoring to align with guidelines | gap-analysis, decision-validation, code-review |
| **feedback-loop** | Analyzing usage, proposing improvements | coding-guidelines-mcp, gap-analysis |
| **copilot-model-selection** | Choosing Azure Foundry catalog entries or provider-safe model settings | coding-guidelines-mcp |
| **test-guidelines-server** | After install/upgrade or when tools return unexpected results | coding-guidelines-mcp |

---

## Example Workflows

### Workflow 1: Gap → Validation → Migration

```
1. Invoke: gap-analysis
   → Identify architectural gaps (e.g., missing application layer)

2. Invoke: decision-validation
   → Validate approach before implementation

3. Invoke: migration-planning
   → Plan phases and timeline

4. Execute plan with code-review for each phase
```

### Workflow 2: Code Review → Validation → Migration

```
1. Invoke: code-review
   → Review code for compliance issues

2. Invoke: decision-validation
   → Validate approach to fix issues

3. Invoke: migration-planning
   → Plan incremental refactoring

4. Execute with coding-guidelines-mcp for reference
```

---

## File Structure

```
plugins/guidelines/
  .github/plugin/plugin.json        <- Canonical plugin metadata
  skills/
    coding-guidelines-mcp.md        <- Skill 1: Server reference
    gap-analysis.md                 <- Skill 2: Architectural gap analysis
    code-review.md                  <- Skill 3: Code review against guidelines
    decision-validation.md          <- Skill 4: Validate decisions pre-implementation
    migration-planning.md           <- Skill 5: Incremental refactoring planner
    feedback-loop.md                <- Skill 6: Usage analysis + issue creation
    copilot-model-selection.md      <- Skill 7: Copilot model selection guidance
    test-guidelines-server.md       <- Skill 8: Smoke-test the coding guidelines server
  README.md                         <- This file
```

---

## Troubleshooting

### Skills are not showing up

- **Project-scoped**: Skills auto-load from `plugins/guidelines/skills/` when in a session for this repository.
- **Global**: Copy skills to `~/.copilot/skills/` and restart your session.
- **Fix**: Run `extensions_reload` in the Copilot session.

### Server not responding

See **Skill: test-guidelines-server** for the full diagnostic workflow.

Short checklist:
1. Check `.mcp.json` — `jsdotnet-coding-guidelines` must be listed.
2. Verify the global tool is installed: `dotnet tool list -g` (look for `jsdotnet-guidelines-mcpserver`)
3. If missing, install: `dotnet tool install -g JSdotNet.MCP.Guidelines`
4. Reload: `extensions_reload`

---

## Version History

- **2.0** (2026-07-28): Split into separate plugins per MCP server
  - Renamed plugin to `jsdotnet-project-guidelines` (was `jsdotnet-guidelines`)
  - Renamed `guidelines-mcp` skill to `coding-guidelines-mcp` (focused on coding server only)
  - Renamed `test-mcp-servers` skill to `test-guidelines-server` (focused on coding server only)
  - Design/UX skills moved to new `plugins/design/` plugin
  - All seven skills now exclusively target `jsdotnet-coding-guidelines`

- **3.0** (2026-07-27): Aligned with two MCP servers; corrected all tool names; added test skill

- **2.1** (2026-06-05): Added decision-validation and migration-planning skills

- **2.0** (2026-06-05): Added code review, ADR creation, error handling, and testing skills

- **1.0** (2026-06-05): Initial release with three core skills
