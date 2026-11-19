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
- `FileSystemDocumentCatalog` serves the local `defaults/` (preferring its `index.json`) when
  running locally; `GitHubDocumentCatalog` fetches from GitHub when installed as a global tool.
  A change to how documents are found or read applies to both. The one server,
  `JSdotNet.AppDefaults.Mcp`, serves all of `defaults/`; design is the `design` category.
- Every document is also a resource at `defaults://<relative path without .md>`; list and read
  results carry `ttlMs`. Keep primitives in `JSdotNet.MCP.Shared` (`Hosting/DefaultsServer`) so
  the coverage gate measures them.
- Cache results; make no redundant GitHub API calls.
- Keep merged line coverage over `src/` at 80% or more; CI fails the build below it. `Program`
  classes and `GitHubDocumentCatalog` are excluded, so the gate measures `JSdotNet.MCP.Shared`. New code ships with the tests that keep it there.
