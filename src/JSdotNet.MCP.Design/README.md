# JSdotNet.MCP.Design

NuGet package for the **Project Design MCP server** used to provide design and UX guidance to AI tools.

## MCP server identity

- Server name: `jsdotnet-project-design`
- Runtime command: `jsdotnet-design-mcpserver`
- Package ID: `JSdotNet.MCP.Design`

## What this server is for

Use this server when you need design-oriented context, such as:

- UX and interaction guidance
- Design-focused architecture decisions
- Product design recommendations

## Configuration snippets

### `copilot-tools.json`

```json
{
  "mcpServers": [
    {
      "name": "jsdotnet-project-design",
      "packageId": "JSdotNet.MCP.Design",
      "enabled": true
    }
  ]
}
```

### `mcp.json`

```json
{
  "mcpServers": {
    "jsdotnet-project-design": {
      "type": "stdio",
      "command": "jsdotnet-design-mcpserver",
      "args": [],
      "tools": ["*"]
    }
  }
}
```

## Notes

- Use alongside `JSdotNet.MCP.Guidelines` for balanced architecture and design context.
- If package updates are available, re-run your install/update script and restart the MCP client.
