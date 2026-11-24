# Moved to JSdotNet.AppDefaults.Mcp

This package is retired. `JSdotNet.MCP.Guidelines` and `JSdotNet.MCP.Design` were merged into
one server, [JSdotNet.AppDefaults.Mcp](https://www.nuget.org/packages/JSdotNet.AppDefaults.Mcp),
which serves the guidelines and the design guidance together.

This final version serves nothing: its tool prints the notice above to stderr and exits.

Replace the old server entry with:

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

Setup for every host: [docs/install.md](https://github.com/JSdotNet/app-defaults/blob/main/docs/install.md).
