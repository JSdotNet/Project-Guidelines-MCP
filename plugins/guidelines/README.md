# JSdotNet Project Guidelines Plugin

This plugin provides six complementary skills to help you work with the two JSdotNet MCP servers, plus one skill to test them:

1. **guidelines-mcp** — Reference guide for using both MCP servers and their tools
2. **gap-analysis** — Analyze project structure against guidelines
3. **code-review** — Review code for architectural and guideline compliance
4. **decision-validation** — Validate decisions before implementation
5. **migration-planning** — Plan incremental refactoring to align with guidelines
6. **feedback-loop** — Track MCP usage and propose improvements
7. **test-mcp-servers** — Smoke-test both MCP servers end-to-end

---

## What You Get

### Seven Skills

- **guidelines-mcp** — Reference guide for both MCP servers, tool reference, quick decision guide, common tags
- **gap-analysis** — Analyze project structure against guidelines, identify gaps
- **code-review** — Validate code for architectural and style compliance
- **decision-validation** — Validate decisions before implementation, ensure alignment
- **migration-planning** — Plan and execute incremental refactoring to align with guidelines
- **feedback-loop** — Analyze usage logs and propose improvements
- **test-mcp-servers** — Smoke-test both MCP servers after install, upgrade, or when troubleshooting

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
- Two MCP servers and when to use each
- Six MCP tools available on both servers
- Valid categories and common tags
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

### 4. Validate a Decision

```
Invoke skill: decision-validation

This helps you:
- Check guideline alignment before coding
- Evaluate alternatives with trade-offs
- Get team consensus
- Document the decision
```

### 5. Plan Incremental Refactoring

```
Invoke skill: migration-planning

This guides you on:
- Assessing migration scope
- Breaking work into safe phases
- Managing dependencies
- Tracking and communicating progress
```

### 6. Track Improvement Opportunities

```
Invoke skill: feedback-loop

This helps you:
- Analyze usage logs
- Identify doc gaps
- Draft improvement issues
- Propose enhancements
```

### 7. Test the MCP Servers

```
Invoke skill: test-mcp-servers

This helps you:
- Verify both servers are connected
- Confirm all six tools work
- Diagnose empty results or missing content
- Run cross-server isolation checks
```

---

## Skills Usage

Each skill can be invoked by name from a Copilot session. They work best when combined:

| Skill | Best Used When | Pairs With |
|-------|---|---|
| **guidelines-mcp** | Starting out; learning MCP tools or servers | Any other skill |
| **gap-analysis** | Analyzing project structure | guidelines-mcp, code-review, migration-planning |
| **code-review** | Reviewing PRs or new code | guidelines-mcp, decision-validation |
| **decision-validation** | Making significant technical decisions | guidelines-mcp, migration-planning |
| **migration-planning** | Planning refactoring to align with guidelines | gap-analysis, decision-validation, code-review |
| **feedback-loop** | Analyzing usage, proposing improvements | guidelines-mcp, gap-analysis |
| **test-mcp-servers** | After install/upgrade or when tools return unexpected results | guidelines-mcp |

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

## The Seven Skills Explained

### Skill 1: guidelines-mcp

**What it teaches**: How to use both MCP servers and their tools.

**When to invoke it**: 
- You're new to the MCP servers
- You need a reference for which server and tool to use
- You want to understand categories and common tags

**Output**: Server overview, tool reference, decision tree, example workflows.

---

### Skill 2: gap-analysis

**What it teaches**: How to analyze your project structure against guidelines.

**When to invoke it**:
- You want to audit your project architecture
- You're implementing a new layer and need guidance
- You want to identify common structural issues

**Output**: Scan instructions, common gap patterns, solutions with ADR references.

---

### Skill 3: code-review

**What it teaches**: How to review code against guidelines.

**When to invoke it**:
- Reviewing a PR for architectural compliance
- Checking a design before implementation
- Validating a code snippet against patterns

**Output**: Review checklist, common findings with guidance, workflow examples.

---

### Skill 4: decision-validation

**What it teaches**: How to validate technical decisions before implementing.

**When to invoke it**:
- Making a significant architecture or pattern choice
- Checking if a decision aligns with existing ADRs
- Evaluating alternatives with trade-offs

**Output**: Validation framework, checklist, example scenarios.

---

### Skill 5: migration-planning

**What it teaches**: How to plan incremental refactoring.

**When to invoke it**:
- Code significantly violates guidelines
- Multiple projects need the same change
- You want a structured, low-risk migration approach

**Output**: Phase templates, effort estimation, risk mitigation, communication templates.

---

### Skill 6: feedback-loop

**What it teaches**: How to use MCP usage logs to improve guidelines.

**When to invoke it**:
- You notice patterns in what users search for but can't find
- You want to propose a new ADR or recommendation
- You're collecting evidence for documentation improvements

**Output**: Analysis workflow, issue drafting template, submission process.

---

### Skill 7: test-mcp-servers

**What it teaches**: How to verify both MCP servers are working correctly.

**When to invoke it**:
- After installing or upgrading the MCP servers
- When tool calls return unexpected empty results
- When you want to confirm both servers are connected and serving the right content

**Output**: Numbered smoke tests, pass/fail criteria, cross-server isolation test, diagnostic guidance.

---

## File Structure

```
plugins/guidelines/
  .github/plugin/plugin.json   ← Canonical plugin metadata
  skills/
    guidelines-mcp.md          ← Skill 1: Both MCP servers reference
    gap-analysis.md            ← Skill 2: Gap analysis workflow
    code-review.md             ← Skill 3: Code review against guidelines
    decision-validation.md     ← Skill 4: Validate decisions pre-implementation
    migration-planning.md      ← Skill 5: Incremental refactoring planner
    feedback-loop.md           ← Skill 6: Usage analysis + issue creation workflow
    test-mcp-servers.md        ← Skill 7: Smoke-test both MCP servers
  .github/
  README.md                    ← This file
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

- **3.0** (2026-07-27): Aligned with two MCP servers; corrected all tool names; added test skill
  - Fixed all tool names (`search_docs` → `search_guides`, `get_doc` → `get_guide`, etc.)
  - Updated `guidelines-mcp` to document both servers (`jsdotnet-coding-guidelines` and `jsdotnet-design-ux-guidelines`)
  - Added `test-mcp-servers`: smoke-test both MCP servers end-to-end
  - Removed stale `adr-creation`, `error-handling`, `testing-strategy` references from README

- **2.1** (2026-06-05): Replaced ADR/error/testing skills with decision/migration skills
  - Added decision-validation: Validate decisions before implementation
  - Added migration-planning: Plan and execute incremental refactoring
  - Removed adr-creation, error-handling, testing-strategy

- **2.0** (2026-06-05): Added code review, ADR creation, error handling, and testing skills

- **1.0** (2026-06-05): Initial release with three core skills
