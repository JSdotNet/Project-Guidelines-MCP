# JSdotNet.AppDefaults.Mcp

MCP server that serves the **JSdotNet application defaults** — every document under `defaults/`:
ADRs, building blocks, structures, AI setup, configuration and design (one category of
the same catalog).

## Identity

- Server id: `jsdotnet-app-defaults`
- Tool command: `jsdotnet-app-defaults`
- Package id: `JSdotNet.AppDefaults.Mcp` (package type `McpServer`, with `.mcp/server.json`)

## Run it

No install needed on .NET 11:

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

## What it serves

| Primitive | Names |
|---|---|
| Tools | `list_guides`, `list_guides_by_type` (`adrs`, `designs`, `structures`, `ai`, `config`, `tech`, `design`), `search_guides`, `search_guides_by_tag`, `get_guide`, `get_usage_logs` |
| Resources | every document at `defaults://<relative path without .md>`, e.g. `defaults://arc42/adr/0001-platform-and-build` |
| Prompts | `new-app`, `review-against-defaults`, `propose-adr` |

List results carry `ttlMs` (five minutes). `ServerInfo.Version` is the package version.

## Configuration

| Variable | Effect |
|---|---|
| `JSDOTNET_DOCS_PATH` | Serve a local `defaults/` folder (or a repository root holding one) instead of GitHub |
| `JSDOTNET_LOG_PATH` | Folder for the usage log |
| `GITHUB_TOKEN` | Raises the GitHub rate limit when documents are fetched from GitHub |
