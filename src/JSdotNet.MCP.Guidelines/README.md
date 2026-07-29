# JSdotNet.MCP.Guidelines

NuGet package for the **Project Guidelines MCP server** used to provide architecture and engineering guidance to AI tools.

## MCP server identity

- Server name: `jsdotnet-project-guidelines`
- Runtime command: `jsdotnet-guidelines-mcpserver`
- Package ID: `JSdotNet.MCP.Guidelines`

## What this server is for

Use this server when you need governance-oriented project context, such as:

- ADRs
- Recommendations
- Designs
- Project structures

In this repository, architecture work is expected to consult this server before making architecture changes.

## Configuration snippets

### `copilot-tools.json`

```json
{
  "mcpServers": [
    {
      "name": "jsdotnet-project-guidelines",
      "packageId": "JSdotNet.MCP.Guidelines",
      "enabled": true
    }
  ]
}
```

### `mcp.json`

```json
{
  "mcpServers": {
    "jsdotnet-project-guidelines": {
      "type": "stdio",
      "command": "jsdotnet-guidelines-mcpserver",
      "args": [],
      "tools": ["*"]
    }
  }
}
```

## Notes

- Keep server name and command consistent across `copilot-tools.json` and `mcp.json`.
- If package updates are available, re-run your install/update script and restart the MCP client.
