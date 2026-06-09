# JSdotNet Project Guidelines Plugin

This plugin provides three complementary skills to help you work with the JSdotNet Project Guidelines MCP server:

1. **guidelines-mcp** — Reference guide for using MCP tools
2. **gap-analysis** — Analyze project structure against guidelines
3. **feedback-loop** — Track MCP usage and propose improvements

Plus one extension that enables gap analysis and feedback loop tools.

---

## What You Get

### Six Skills

- **guidelines-mcp** — Reference guide for MCP tools, decision guide, common tags
- **gap-analysis** — Analyze project structure against guidelines, identify gaps
- **code-review** — Validate code for architectural and style compliance
- **decision-validation** — Validate decisions before implementation, ensure alignment
- **migration-planning** — Plan and execute incremental refactoring to align with guidelines
- **feedback-loop** — Analyze usage logs and propose improvements

All skills are pure markdown with workflows, examples, and integration patterns. No dependencies or setup required.

---

## Installation

### Option 1: Copy to Your User Skills Directory (Global)

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
Invoke skill: gap-analysis

This guides you to:
- Scan project structure
- Identify architectural gaps
- Find relevant ADRs
- Plan improvements
```

### 3. Review Code for Compliance

```
Invoke skill: code-review

This helps you:
- Validate architectural alignment
- Check coding standards
- Identify pattern violations
- Find relevant guidance
```

### 4. Create Architecture Decisions

```
Invoke skill: adr-creation

This helps you:
- Structure ADRs properly
- Capture trade-offs
- Link related decisions
- Get team consensus
```

### 5. Design Error Handling

```
Invoke skill: error-handling

This guides you on:
- Choosing error strategies
- Translating at boundaries
- Logging effectively
- Testing error paths
```

### 6. Organize Tests

```
Invoke skill: testing-strategy

This teaches you:
- Organizing by layer
- Unit vs. integration tests
- Meaningful coverage
- Error path testing
```

### 7. Track Improvement Opportunities

```
Invoke skill: feedback-loop

This helps you:
- Analyze usage logs
- Identify doc gaps
- Draft improvement issues
- Propose enhancements
```

---

## Skills Usage

Each skill can be invoked by name from a Copilot session. They work best when combined:

| Skill | Best Used When | Pairs With |
|-------|---|---|
| **guidelines-mcp** | Starting out; learning MCP tools | Any other skill |
| **gap-analysis** | Analyzing project structure | guidelines-mcp, code-review, migration-planning |
| **code-review** | Reviewing PRs or new code | guidelines-mcp, decision-validation |
| **decision-validation** | Making significant technical decisions | guidelines-mcp, migration-planning |
| **migration-planning** | Planning refactoring to align with guidelines | gap-analysis, decision-validation, code-review |
| **feedback-loop** | Analyzing usage, proposing improvements | guidelines-mcp, gap-analysis |

### Example Combined Workflows

#### Workflow 1: Gap → Validation → Migration

```
1. Invoke: gap-analysis
   → Identify architectural gaps (e.g., missing application layer)

2. Invoke: decision-validation
   → Validate approach before implementation
   → Check guidelines alignment

3. Invoke: migration-planning
   → Plan phases and timeline
   → Break into manageable chunks

4. Execute plan with code-review for each phase
```

#### Workflow 2: Code Review → Validation → Migration

```
1. Invoke: code-review
   → Review code for compliance issues

2. Invoke: decision-validation
   → Validate approach to fix issues

3. Invoke: migration-planning
   → Plan incremental refactoring

4. Execute with guidelines-mcp for reference
```

#### Workflow 3: Decision → Validation → Review

```
1. Have a design idea
2. Invoke: decision-validation
   → Validate against guidelines
   → Check alternatives

3. Invoke: code-review
   → Validate approach before coding

4. Implement with guidelines-mcp for reference
```

---

## The Three Skills Explained

### The Skill 1: guidelines-mcp

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
plugins/guidelines/
  skills/
    guidelines-mcp.md          ← Skill 1: MCP reference
    gap-analysis.md            ← Skill 2: Gap analysis workflow
    feedback-loop.md           ← Skill 3: Feedback loop process
  .github/
  plugin-manifest.json         ← Plugin metadata
  README.md                     ← This file
```

---

## Troubleshooting

### Skills are not showing up

**Check**: Are you in a Copilot session in this repository or have you installed them globally?

- Project-scoped: Skills auto-load from `plugins/guidelines/skills/` when you start a session
- Global: Skills must be copied to `~/.copilot/skills/` and your session restarted

**Fix**: Restart your Copilot CLI session or run:
```bash
extensions_reload
```

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

### Configuration

The skills only use MCP tools that are already available. No environment setup required beyond having the MCP server running.

---

## Support & Feedback

If you have questions or find bugs:

1. Check this README section: **Troubleshooting**
2. Review the individual skill markdown files for detailed workflows
3. Open an issue in the JSdotNet/Project-Guidelines-MCP repository

---

## Version History

- **2.1** (2026-06-05): Replaced ADR/error/testing skills with decision/migration skills
  - Added decision-validation: Validate decisions before implementation
  - Added migration-planning: Plan and execute incremental refactoring
  - Removed adr-creation, error-handling, testing-strategy

- **2.0** (2026-06-05): Added code review, ADR creation, error handling, and testing skills
  - code-review: Validate code against guidelines
  - adr-creation: Create Architecture Decision Records
  - error-handling: Design consistent error handling
  - testing-strategy: Organize tests and coverage

- **1.0** (2026-06-05): Initial release with three core skills
  - guidelines-mcp: MCP reference guide
  - gap-analysis: Project structure audit workflow
  - feedback-loop: Usage analysis + issue creation workflow
