# Diagnosing a failed smoke test

## Tools not available

1. The host's MCP configuration lists `jsdotnet-app-defaults`, spelled exactly.
2. The entry runs the package: `"command": "dnx", "args": ["JSdotNet.AppDefaults.Mcp", "--yes"]`.
   Setup per host: [docs/install.md](../../../../docs/install.md).
3. From source: `dotnet run --project src/JSdotNet.AppDefaults.Mcp`.
4. Restart or reload the host after changing its configuration.

## Empty or short lists and searches

1. `defaults/index.json` exists and lists every markdown file under `defaults/`.
2. A category that comes back empty has no entry with that `category` in the index.
3. A tag search with no results means the documents lack that tag in the index.

## `get_guide` returns nothing

1. The id is in `defaults/index.json`.
2. The file its `path` names exists.
3. `JSDOTNET_DOCS_PATH`, when set, overrides where the server reads `defaults/` from.

## `get_usage_logs` returns nothing

1. Each server process writes its own `usage-<pid>.jsonl`, so only this session's calls show.
2. The folder is `JSDOTNET_LOG_PATH` when set, else
   `%LOCALAPPDATA%\JSdotNet\AppDefaultsMcpServer\`.
