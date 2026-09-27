---
name: mcp-server
description: Maintainer invariants for the MCP server code and its tests.
paths:
  - "src/**"
  - "tests/**"
---

# MCP server

- Serve guidance only; keep business logic out of the server.
- Include source doc ids in every response for traceability.
- `FileSystemDocumentCatalog` serves the local `guide/` (preferring its `index.json`) when
  running locally; `GitHubDocumentCatalog` fetches from GitHub when installed as a global tool.
  A change to how documents are found or read applies to both.
- Cache results; make no redundant GitHub API calls.
- Keep merged line coverage over `src/` at 80% or more; CI fails the build below it. `Program`
  classes and `GitHubDocumentCatalog` are excluded, so today the gate measures
  `JSdotNet.MCP.Shared` and `JSdotNet.MCP.Publish`. New code ships with the tests that keep
  it there.
