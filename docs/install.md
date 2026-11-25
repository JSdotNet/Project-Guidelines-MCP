# Install the app defaults server

How to register the `jsdotnet-app-defaults` MCP server, and the `app-defaults` agent plugin, in
each host. What the server is for and what it serves is in the [README](../README.md).

## Requirements

- The .NET 11 SDK, which carries `dnx`. `dnx` downloads and runs the `JSdotNet.AppDefaults.Mcp`
  package on demand, so nothing is installed globally.
- Network access to nuget.org (the package) and GitHub (the documents, read from `main`).
  Set `GITHUB_TOKEN` to raise the GitHub rate limit.

Keep the server id exactly `jsdotnet-app-defaults`: the plugin's skills and `AGENTS.md` files
refer to it by name.

## Claude Code

Per project, commit a `.mcp.json` at the repository root:

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

For every project on the machine, register it at user level instead:

```powershell
claude mcp add -s user jsdotnet-app-defaults -- dnx JSdotNet.AppDefaults.Mcp --yes
```

A server in a project's `.mcp.json` overrides a user-level one with the same id, and Claude Code
asks to approve it the first time `claude` starts in that project. Verify with
`claude mcp get jsdotnet-app-defaults` or `/mcp` in a new session.

**Plugin.** Add this repository as a marketplace and install the plugin:

```text
/plugin marketplace add JSdotNet/app-defaults
/plugin install app-defaults@jsdotnet-app-defaults
```

## GitHub Copilot CLI

Copilot CLI reads the same project `.mcp.json` shown above once the project folder is trusted:
start `copilot` in the folder and trust it when asked. To add the server for every project:

```powershell
copilot mcp add jsdotnet-app-defaults -- dnx JSdotNet.AppDefaults.Mcp --yes
```

Verify with `copilot mcp get jsdotnet-app-defaults`, or `/mcp show jsdotnet-app-defaults` in a
session.

**Plugin.** Copilot reads the plugin's `.github/plugin/plugin.json` manifest from the same
marketplace:

```powershell
copilot plugin marketplace add JSdotNet/app-defaults
```

```powershell
copilot plugin install app-defaults@jsdotnet-app-defaults
```

## VS Code

Add the server to `.vscode/mcp.json` in the workspace, or to the user-level MCP configuration
(**MCP: Open User Configuration**):

```json
{
  "servers": {
    "jsdotnet-app-defaults": {
      "type": "stdio",
      "command": "dnx",
      "args": ["JSdotNet.AppDefaults.Mcp", "--yes"]
    }
  }
}
```

Start it from the **MCP: List Servers** view; its output shows in the **Output** panel under
the server's name. Ask Copilot Chat "which ADRs do the app defaults have?" to verify.

## Visual Studio

Visual Studio reads a `.mcp.json` in the solution folder, so the project file above works
as is. To add it by hand, open Copilot Chat in agent mode, select the tools button, add a
server named `jsdotnet-app-defaults` of type stdio with the command
`dnx JSdotNet.AppDefaults.Mcp --yes`, and restart the chat.

## GitHub Copilot coding agent

In the repository's **Settings → Copilot → Coding agent → MCP configuration**, add:

```json
{
  "mcpServers": {
    "jsdotnet-app-defaults": {
      "type": "local",
      "command": "dnx",
      "args": ["JSdotNet.AppDefaults.Mcp", "--yes"],
      "tools": ["*"]
    }
  }
}
```

The agent's environment needs the .NET 11 SDK; install it in
`.github/workflows/copilot-setup-steps.yml` with `actions/setup-dotnet`.

## Run from source

To serve a working tree's `defaults/` while changing them, clone the repository and run the
server project; this repository's own [.mcp.json](../.mcp.json) does exactly that:

```powershell
$env:JSDOTNET_DOCS_PATH = "."; dotnet run --project .\src\JSdotNet.AppDefaults.Mcp\JSdotNet.AppDefaults.Mcp.csproj
```

`JSDOTNET_DOCS_PATH` points at a `defaults/` folder or a repository root holding one. Build the
solution once first (`dotnet build JSdotNet.AppDefaults.slnx`): the first `dotnet run` in a
clean clone can exceed a host's MCP startup timeout.

To try a package before it is published:

```powershell
dotnet pack src/JSdotNet.AppDefaults.Mcp/JSdotNet.AppDefaults.Mcp.csproj -o ./local-packages
```

```powershell
dnx JSdotNet.AppDefaults.Mcp --yes --add-source ./local-packages
```

## Troubleshooting

| Symptom | Check |
|---|---|
| The server does not start | `dotnet --version` reports an 11.x SDK, and `dnx JSdotNet.AppDefaults.Mcp --yes` runs in a terminal |
| No documents load | GitHub is reachable; on a rate-limit error, set `GITHUB_TOKEN` |
| The skills cannot find the server | The server id is exactly `jsdotnet-app-defaults` |
| An old server id still answers | Remove `jsdotnet-coding-guidelines`, `jsdotnet-project-guidelines` and `jsdotnet-design-ux-guidelines` entries; their packages are retired |
| The server prints "Moved to JSdotNet.AppDefaults.Mcp" and exits | It runs a retired package; replace the entry with the one above |

## Retired packages

`JSdotNet.MCP.Guidelines` and `JSdotNet.MCP.Design` were merged into `JSdotNet.AppDefaults.Mcp`.
Their last version, 1.0.13, only prints that notice to stderr and exits; earlier versions are
unlisted. Their sources live in `retired/`, which publishes nothing but that notice version.
`JSdotNet.MCP.Publish` is not retired: it lives on in `JSdotNet/mcp-publish`.
