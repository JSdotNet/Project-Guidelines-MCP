# JSdotNet App Defaults Plugin

Skills for working with the **`jsdotnet-app-defaults`** MCP server, the one server that serves
every document under `defaults/`: ADRs, building blocks, crosscutting concepts, AI setup, and the
UX style guide (the `design` category).

## Skills

| Skill | Use it when |
|---|---|
| `jsdotnet-app-defaults` | The server's own skill: which tool, category or prompt answers a question, and how to cite it |
| `gap-analysis` | Auditing a whole repository against the defaults |
| `migration-planning` | Planning incremental refactoring toward the defaults |
| `feedback-loop` | Turning the server's usage logs into improvement issues |
| `design-token-lookup` | Finding the right color, typography, spacing or motion token |
| `ui-component-styling` | Styling a component with the design tokens |
| `design-customization` | Overriding the project's brand and semantic colors |
| `test-server` | Smoke-testing the server, the `design` category included, after install or upgrade |

The server's prompts cover the workflows that only wrap tool calls: `new-app` scaffolds a new
application, `review-against-defaults` reviews code, a change or a design, and `propose-adr`
drafts an ADR. They replace the former `code-review` and `decision-validation` skills.

## Layout

```
plugins/app-defaults/
  .claude-plugin/plugin.json   Claude Code manifest (skills found in skills/)
  .github/plugin/plugin.json   GitHub Copilot manifest
  skills/<name>/SKILL.md       the workflow, at most 40 lines, with name and description front matter
  skills/<name>/reference.md   the tables and templates SKILL.md points at
```

Both manifests are hand-authored: a change to one is owed to the other. The repository root's
`.claude-plugin/marketplace.json` lists the plugin for both hosts.

## Server

The skills expect an MCP server registered as `jsdotnet-app-defaults`:

```json
{
  "mcpServers": {
    "jsdotnet-app-defaults": {
      "type": "stdio",
      "command": "dnx",
      "args": ["JSdotNet.AppDefaults.Mcp", "--yes"]
    }
  }
}
```

The publish-results server moved to its own repository,
[JSdotNet/mcp-publish](https://github.com/JSdotNet/mcp-publish).
