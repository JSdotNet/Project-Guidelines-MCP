# 01. Introduction and Goals

```meta
status: draft
related: [.devbook/domain/context-map.md]
```

Project-Guidelines-MCP is the single source of truth for JSdotNet .NET C# project guidance,
and the set of MCP servers that serve it to coding agents. The guidance is Markdown in
`guide/` and `design/`; the servers are built from `src/` and published to NuGet as .NET
global tools.

## Requirements overview

```meta
status: draft
```

| Server | Project | Serves |
|---|---|---|
| `jsdotnet-coding-guidelines` | `src/JSdotNet.MCP.Guidelines` | The ADRs, designs, recommendations, structures and config docs under `guide/` |
| `jsdotnet-design-ux-guidelines` | `src/JSdotNet.MCP.Design` | The UX style guide under `design/` |
| `jsdotnet-publish-results` | `src/JSdotNet.MCP.Publish` | A configurable file location agents publish results to |

`src/JSdotNet.MCP.Shared` holds what the servers have in common, including the document
catalogs: `FileSystemDocumentCatalog` serves a local checkout, `GitHubDocumentCatalog` fetches
from GitHub when the server runs as an installed global tool.

## Quality goals

```meta
status: draft
```

| Priority | Goal | What it means here |
|---|---|---|
| 1 | Traceability | Every response names the source doc ids it came from. |
| 2 | Serve, never decide | The servers carry guidance only; no business logic lives in them. |
| 3 | Frugal remote access | Results are cached and no GitHub API call is made twice. |
| 4 | Tested core | Merged line coverage over the measured `src/` projects stays at 80% or more. |
