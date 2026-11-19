---
title: "ADR 0013: Agent-Ready Repository"
date: 2026-11-19
status: Accepted
tags: [agents, agents-md, instructions, rules, skills, aspire-cli, mcp, dnx, claude-code, copilot, adr]
---
# ADR 0013: Agent-Ready Repository

## Context

Coding agents read different instruction files per host, so rules copied into each drift
apart. Aspire 17's `aspire agent init` installs Aspire skills through the host's own plugin
system, and agents with a shell read live state faster through the Aspire CLI than through
the dashboard MCP server. Agents that answer from memory instead of from these defaults give
answers nobody agreed on.

## Decision

- **One `AGENTS.md`** at the repository root holds the standing rules. Each host gets a thin
  wrapper that imports it (`CLAUDE.md`, `.github/copilot-instructions.md`); no rule is copied
  into a wrapper.
- **Path-scoped rules** for one kind of file live in `.agents/rules/<topic>.md`, wrapped per
  host (`.claude/rules/`, `.github/instructions/`).
- **Aspire skills, CLI first.** `aspire agent init --skills aspire` installs the Aspire skills.
  Agents use the CLI (`aspire describe`, `aspire logs`, `aspire otel`, resource commands);
  the dashboard MCP server is configured only for a host without a shell.
- **.NET skills.** The `dotnet/skills` skills are installed alongside.
- **The defaults server.** `.mcp.json` points at this repository's guidelines server through
  `dnx`, so no global tool install is needed:

```json
{
  "mcpServers": {
    "jsdotnet-coding-guidelines": {
      "command": "dnx",
      "args": ["JSdotNet.MCP.Guidelines", "--yes"]
    }
  }
}
```

## Consequences

### Positive

- One set of rules for every host, reviewed in one file.
- Agents see the running app's real state and the agreed defaults instead of guessing.
- Skill updates arrive through the host's plugin system, not hand-copied files.

### Negative

- Each new host needs its own wrapper and rule wrappers.
- Old local skill files can override the installed ones and must be removed.
- `dnx` needs the .NET 10 SDK or later on every machine that runs an agent.

## References

- [AGENTS.md](https://agents.md/)
- [Aspire agent skills](https://aspire.dev/get-started/ai-coding-agents/)
- [dotnet/skills](https://github.com/dotnet/skills)
- [defaults/ai/config/.mcp.json.md](../../ai/config/.mcp.json.md)
- ADR 0002: Aspire, Configuration and Infrastructure

Replaces: none; new in this set.
