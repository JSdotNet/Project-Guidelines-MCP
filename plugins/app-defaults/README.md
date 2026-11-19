# JSdotNet App Defaults Plugin

Skills for working with the **`jsdotnet-app-defaults`** MCP server, the one server that serves
every document under `defaults/`: ADRs, building blocks, crosscutting concepts, AI setup, and the
UX style guide (the `design` category). It replaces the former `guidelines` and `design` plugins.

## Skills

| Skill | Use it when |
|---|---|
| `coding-guidelines-mcp` | Learning the server's tools, categories and tags for architecture and code |
| `design-mcp` | Learning the `design` category: style-guide documents and token categories |
| `gap-analysis` | Auditing a project's structure against the defaults |
| `code-review` | Reviewing a change against the ADRs and coding concepts |
| `decision-validation` | Checking a decision against the defaults before implementing it |
| `migration-planning` | Planning incremental refactoring toward the defaults |
| `feedback-loop` | Turning the server's usage logs into improvement issues |
| `design-token-lookup` | Finding the right color, typography, spacing or motion token |
| `ui-component-styling` | Styling a component with the design tokens |
| `design-customization` | Overriding the project's brand and semantic colors |
| `test-guidelines-server` | Smoke-testing the coding documents after install or upgrade |
| `test-design-server` | Smoke-testing the `design` category after install or upgrade |

## Layout

```
plugins/app-defaults/
  .claude-plugin/plugin.json   Claude Code manifest (skills found in skills/)
  .github/plugin/plugin.json   GitHub Copilot manifest
  skills/<name>/SKILL.md       one folder per skill, with name and description front matter
```

Both manifests are hand-authored: a change to one is owed to the other.

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
