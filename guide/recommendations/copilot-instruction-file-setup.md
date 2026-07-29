---
title: "Copilot Instruction-File Setup"
date: 2026-07-29
status: Accepted
tags: [copilot, instructions, mcp, agents, plugins, orchestration, recommendations]
---
# Recommendation: Copilot Instruction-File Setup

## Purpose

Define a concise, maintainable setup for repository Copilot instruction files that focuses on tool selection, agent routing, and repository-specific orchestration without duplicating guidance already available elsewhere.

## Recommendation

- Keep repository instruction files short and policy-focused.
- Use instruction files to define **selection and routing**, not to restate architecture, coding, testing, or operational guidance that already exists in repository MCP documents or installed plugin/skill/agent instructions.
- Treat repository MCP documents as the authoritative source for durable project guidance and installed plugins, skills, or agents as the authoritative source for workflow-specific execution behavior.
- Base repository-specific orchestration routing on the installed JSdotNet Copilot skill catalog published at `https://github.com/JSdotNet/Copilot/tree/main/plugins/copilot-app/skills`.

## What Belongs in Repository Instruction Files

Repository instruction files should define only the minimum policy needed for reliable tool use:

- Which MCP servers are authoritative for which topics.
- Which installed plugins, skills, or agents should be selected for specific task categories.
- Which workflows must be routed through repository-specific orchestrators before implementation or release actions.
- What fallback path to use when an MCP server, plugin, skill, or agent is unavailable.

Keep everything else in the repository's MCP-served documents or in the installed tool's own instruction surface.

## Tool and MCP Selection Policy

Use a stable selection order:

1. Repository MCP guidance for repository-specific architecture, design, coding, testing, structure, and governance decisions.
2. Repository-specific orchestration skills and specialist agents selected from the JSdotNet Copilot skill catalog.
3. External or platform MCP servers for vendor, framework, or product documentation.
4. Direct repository inspection and built-in tools for workspace state, code search, diffs, and local validation.

For repositories using the JSdotNet Copilot stack, name the expected MCP servers explicitly in the instruction file:

- `jsdotnet-coding-guidelines` via `jsdotnet-project-guidelines-mcpserver` for repository guidance under `guide/`.
- `jsdotnet-design-mcpserver` for design and UX guidance under `design/`.
- Additional external servers only when needed, such as `microsoft-learn` or `aspire`.

Also name the preferred orchestration and specialist surfaces explicitly so routing stays stable:

- Skills: `orch-architecture`, `orch-adr`, `orch-feature`, `orch-bug`, `orch-create-module`, `orch-create-service`, `orch-update-packages`, `orch-setup`, `create-github-issue`, `update-github-issue`, `pr-jsdotnet`.
- Agents: `architecture:architect`, `csharp-coding:coding`, `documentation:documentation`, `product-owner:product-owner`, `domain-design:domain-architect`, `ux-design:ux-designer`.

Rules:

- Query the authoritative repository MCP server before answering repository-policy questions from memory.
- Cite the relevant document ID, ADR number, or relative path when the instruction file requires grounded guidance.
- Do not copy large sections of MCP guidance into the instruction file; link or route to it instead.
- Keep tool selection rules at the policy level. Do not duplicate detailed usage instructions that are already maintained by the server, plugin, or skill.

### MCP Fallback

If the repository MCP server is unavailable:

1. Read the checked-in document index and referenced markdown files directly if they are present in the repository.
2. If the local documents are also unavailable, state that the guidance could not be verified.
3. Do not invent repository policy from memory when the authoritative source cannot be reached.

## Agent Usage Policy

- Prefer an installed repository-specific plugin, skill, or agent when the task matches its declared specialty.
- Use general-purpose agents only when no specialized option applies or when the specialized option is unavailable.
- Route to one specialist per scope; do not duplicate the same responsibility across multiple agents or instruction files.
- Keep agent instructions in the repository file limited to **when to select** the agent, not **how the agent internally performs** its workflow.

For JSdotNet Copilot repositories, prefer orchestration skills from `plugins/copilot-app/skills` before dropping to specialist agents directly. For example:

- Use `orch-architecture` or `orch-adr` before architecture or decision-record changes.
- Use `orch-feature` or `orch-bug` before implementation work that spans planning, coding, and validation.
- Use `orch-create-module` or `orch-create-service` for new bounded scopes.
- Use `orch-update-packages` for dependency updates.
- Use `pr-jsdotnet`, `create-github-issue`, or `update-github-issue` for repository workflow automation when those skills are installed.

Only route directly to agents such as `architecture:architect` or `csharp-coding:coding` when no orchestrator skill is the better entry point for the task.

### Agent/Plugin/Skill Fallback

If a preferred plugin, skill, or agent is unavailable:

1. Use the closest lower-level repository-approved option, such as a more general agent or direct built-in tools.
2. Preserve the same governance checkpoints called out by the repository, such as consulting MCP guidance first or keeping required review steps.
3. State any reduced assurance explicitly when the fallback removes specialist validation or automation.

## Repo-Specific Orchestration Routing

Repository instruction files should explicitly route governed workflows to the repository's approved orchestration entry points. Typical examples include:

- Architecture documentation and decision records.
- Feature, bug, and module/service creation workflows.
- Dependency or package update workflows.
- Release, packaging, or distribution workflows.
- Pull request, issue, and review-comment automation workflows.

Rules:

- Prefer naming the approved orchestrator, plugin, skill, or agent for each workflow category.
- Describe the routing trigger and required preconditions, such as consulting repository guidance before changing governed assets.
- Do not duplicate the orchestrator's internal checklist in the instruction file when that behavior is already maintained by the orchestrator itself.
- Keep routing repository-specific: only include orchestration paths that are actually installed or supported for that repository.
- For JSdotNet Copilot-based repositories, treat `https://github.com/JSdotNet/Copilot/tree/main/plugins/copilot-app/skills` as the baseline source for orchestration routing names and responsibilities.

### Orchestration Fallback

If the preferred orchestrator is unavailable:

1. Fall back to the nearest named specialist agent, such as `architecture:architect`, `csharp-coding:coding`, `documentation:documentation`, or `product-owner:product-owner`, that can still honor the repository's guidance and checkpoints.
2. If no suitable specialist exists, perform the work with direct tools while following the same documented repository guidance from `jsdotnet-coding-guidelines` or local checked-in docs.
3. Record that orchestration routing from the JSdotNet Copilot skill catalog was unavailable so the user understands why a lower-assurance path was used.

## Anti-Patterns to Avoid

- Turning the instruction file into a duplicate of ADRs, recommendations, or design documents.
- Embedding full plugin, skill, or agent playbooks in the repository instruction file.
- Listing tools or agents without a selection policy for when they should be used.
- Defining fallback behavior that silently changes policy or hides the loss of an authoritative source.

## References

- Config Guideline: .mcp.json
- Config Guideline: github-app.yml
