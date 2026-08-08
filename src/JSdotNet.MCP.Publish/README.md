# JSdotNet Publish Results MCP Server

Model Context Protocol (MCP) server that lets an AI agent publish its results to a **configurable file location**.

## Installation

```bash
dotnet tool install --global JSdotNet.MCP.Publish
```

The tool command is `jsdotnet-publish-mcpserver`.

## Configuring the publish location

The publish root is resolved in this order:

1. Configuration key `Publish:RootPath` — supplied on the command line (`--Publish:RootPath=D:\results`) or as the environment variable `Publish__RootPath`.
2. Environment variable `JSDOTNET_PUBLISH_PATH`.
3. Default: `%LOCALAPPDATA%\JSdotNet\PublishedResults` (Linux/macOS: `~/.local/share/JSdotNet/PublishedResults`).

Additional settings:

| Key | Default | Description |
|-----|---------|-------------|
| `Publish:RootPath` | per-user folder | Directory results are written to. Created if missing. Environment variables in the value are expanded. |
| `Publish:AllowOverwriteByDefault` | `false` | When `true`, `publish_result` may replace existing files without `overwrite=true`. |

All tool paths are relative to the root; absolute paths and `..` segments are rejected so the server can never write outside the configured location.

### Example `.mcp.json`

```json
{
  "mcpServers": {
    "jsdotnet-publish": {
      "type": "stdio",
      "command": "jsdotnet-publish-mcpserver",
      "args": ["--Publish:RootPath=D:\\reports"],
      "tools": ["*"]
    }
  }
}
```

Or with an environment variable:

```json
{
  "mcpServers": {
    "jsdotnet-publish": {
      "type": "stdio",
      "command": "jsdotnet-publish-mcpserver",
      "env": { "JSDOTNET_PUBLISH_PATH": "D:\\reports" },
      "tools": ["*"]
    }
  }
}
```

## Tools

| Tool | Purpose |
|------|---------|
| `publish_result` | Write a result to a file (fails on existing file unless `overwrite=true`). |
| `append_result` | Append text to a file, creating it and its folders when needed. |
| `list_published` | List published files (newest first), optionally filtered by a glob such as `*.md`. |
| `read_published` | Read back the content of a published file. |
| `delete_published` | Delete a published file. |
| `get_publish_location` | Report the resolved absolute publish root. |
| `get_usage_logs` | Recent tool invocations for this server process. |

## License

MIT
