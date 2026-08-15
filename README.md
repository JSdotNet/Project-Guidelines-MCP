# JSdotNet Project Guidelines

Design, architecture, style and structure guidelines for modern .NET (C#) projects, organized as ADRs, designs, recommendations and structures under `guide/`. An MCP Server in `src/` exposes these documents for tools/agents.

> Credits: This project builds on the original HexMaster Design Guidelines work by Eduard Keilholz (GitHub: nikneem) and contributors.
> Original repository: https://github.com/nikneem/hexmaster-design-guidelines

## MCP Server (C#, .NET 10)

An MCP (Model Context Protocol) server implementing the official Microsoft MCP SDK. Exposes design guideline documents as tools that AI assistants can call.

### Requirements

- .NET 10 SDK

### MCP Protocol

The server implements the Model Context Protocol using the official `ModelContextProtocol` NuGet package. It exposes tools for:

1. **ListDocuments** - Lists all available design guideline documents (ADRs, designs, recommendations, structures)
2. **GetDocument** - Retrieves the content of a specific document by its ID
3. **SearchDocuments** - Searches documents by keyword or phrase

Documents are served from the local filesystem when available, with automatic fallback to GitHub repository content.

### Run Standalone (for testing)

From the repository root:

```powershell
dotnet run --project .\src\JSdotNet.MCP.Guidelines\JSdotNet.MCP.Guidelines.csproj
```

The server uses stdio transport for MCP communication. Logs are written to stderr, JSON-RPC messages to stdout.

### Design / UX Optimized Server

A second MCP server is included for the scoped design and UX guidance set under the repository's `design/` folder. It exposes the same tools (`list_guides`, `list_guides_by_type`, `search_guides`, `search_guides_by_tag`, `get_guide`) but reads only that documentation set.

```powershell
dotnet run --project .\src\JSdotNet.MCP.Design\JSdotNet.MCP.Design.csproj
```

When run from the repository root, the server automatically discovers `design/` locally. For GitHub-backed usage, it resolves documents from the repository's `design/` path on GitHub.

### Publish Results Server

A third MCP server (`src/JSdotNet.MCP.Publish`) lets an agent publish its results to a **configurable file location** instead of serving documentation.

```powershell
dotnet run --project .\src\JSdotNet.MCP.Publish\JSdotNet.MCP.Publish.csproj -- --Publish:RootPath=D:\reports
```

Tools: `publish_result`, `append_result`, `list_published`, `read_published`, `delete_published`, `get_publish_location`, `get_usage_logs`.

The publish root is resolved from `Publish:RootPath` (command line or `Publish__RootPath` environment variable), then `JSDOTNET_PUBLISH_PATH`, and finally a per-user default folder (`%LOCALAPPDATA%\JSdotNet\PublishedResults`). All tool paths are relative to that root — absolute paths and `..` segments are rejected. See [src/JSdotNet.MCP.Publish/README.md](src/JSdotNet.MCP.Publish/README.md) for details.

### Install as GitHub Copilot MCP Tool

The MCP Server can be integrated with GitHub Copilot to provide AI agents with access to design guidelines during code generation.

There are two usage scenarios:

1. **Standard Installation** (Recommended) - Install from NuGet, documents fetched from GitHub
2. **Local Development** - Run from source with local documents for testing changes

---

#### Scenario 1: Standard Installation (NuGet Global Tool)

This is the recommended approach for general use. Documents are automatically fetched from the GitHub repository, so no local clone is needed.

**VS Code Setup**

1. **Install the package**:

   ```bash
   dotnet tool install --global JSdotNet.MCP.Guidelines
   ```

2. **Configure VS Code MCP settings**:

   Create or edit `.vscode/mcp.json` in your user profile or workspace:

   ```json
   {
     "inputs": [],
     "servers": {
       "jsdotnet-coding-guidelines": {
         "type": "stdio",
         "command": "jsdotnet-guidelines-mcpserver",
         "args": []
       }
     }
   }
   ```

   **Location options:**
   - **User-level** (all workspaces): `%USERPROFILE%\.vscode\mcp.json` (Windows) or `~/.vscode/mcp.json` (Mac/Linux)
   - **Workspace-level** (specific project): `.vscode/mcp.json` in your project root

3. **Restart VS Code** to apply changes

4. **Verify the connection**:
   - Open the Output panel: View → Output
   - Select "MCP" from the dropdown
   - You should see server startup logs
   - Open GitHub Copilot Chat
   - Ask Copilot: "What ADRs are available in the design guidelines?"

**Visual Studio Setup**

1. **Install the package**:

   ```powershell
   dotnet tool install --global JSdotNet.MCP.Guidelines
   ```

2. **Configure Copilot MCP settings**:
   - Go to `Tools` → `Options`
   - Navigate to `GitHub` → `Copilot` → `MCP Servers`
   - Click "Add Server"
   - Configure:
     - **Name:** `jsdotnet-coding-guidelines`
     - **Command:** `jsdotnet-guidelines-mcpserver`

3. **Restart Visual Studio** to apply changes

4. **Verify the connection**:
   - Open GitHub Copilot Chat window
   - The MCP server should be listed as an active tool
   - Ask Copilot: "Show me the ADR for .NET version adoption"

**How it works**: When installed as a global tool, the server automatically fetches documents from the GitHub repository (`https://github.com/JSdotNet/Project-Guidelines-MCP`). No local clone is required, and you'll always get the latest published content from the `main` branch.

**Update**:

```bash
dotnet tool update --global JSdotNet.MCP.Guidelines
```

**Uninstall**:

```bash
dotnet tool uninstall --global JSdotNet.MCP.Guidelines
```

---

#### Scenario 1b: GitHub Copilot CLI Setup

Copilot CLI reads `.mcp.json` from your project root automatically. This repo ships a ready-to-use `.mcp.json` — you only need to install the global tool first.

1. **Install the package** (if not already installed):

   ```bash
   dotnet tool install --global JSdotNet.MCP.Guidelines
   ```

2. **Copy `.mcp.json` to your project root** (or add the server entry to an existing `.mcp.json`):

   ```json
   {
     "mcpServers": {
       "jsdotnet-coding-guidelines": {
         "type": "stdio",
         "command": "jsdotnet-guidelines-mcpserver",
         "args": [],
         "tools": ["*"]
       }
     }
   }
   ```

3. **Start Copilot CLI** in your project folder:

   ```bash
   copilot
   ```

   The MCP server is picked up automatically — no restart needed.

4. **Verify** by running `/mcp show jsdotnet-coding-guidelines` inside the CLI session.

**Alternatively**, add the server at the user level (available in all projects):

```bash
# Inside a Copilot CLI session
/mcp add
```

Follow the prompts: type `stdio`, command `jsdotnet-guidelines-mcpserver`, tools `*`.

**Update**:

```bash
dotnet tool update --global JSdotNet.MCP.Guidelines
```

Then inside an active Copilot CLI session, run `/mcp show` to confirm the updated server version is loaded.

**Uninstall**:

```bash
dotnet tool uninstall --global JSdotNet.MCP.Guidelines
```

Then remove the entry from `.mcp.json` or run `/mcp delete jsdotnet-coding-guidelines` inside a Copilot CLI session to remove the user-level entry.

---

#### Scenario 1c: Claude Code Setup

Claude Code reads MCP servers from two places: the project's `.mcp.json` (this repo ships one) and a
user-level `mcpServers` block in `~/.claude.json` (`%USERPROFILE%\.claude.json` on Windows). Use the
user level to make the servers available in **every** project without copying config around.

1. **Install the global tools**:

   ```bash
   dotnet tool install --global JSdotNet.MCP.Guidelines
   ```

   ```bash
   dotnet tool install --global JSdotNet.MCP.Design
   ```

   ```bash
   dotnet tool install --global JSdotNet.MCP.Publish
   ```

2. **Register them at user level**. With the `claude` CLI on your PATH:

   ```bash
   claude mcp add -s user jsdotnet-coding-guidelines -- jsdotnet-guidelines-mcpserver
   ```

   Otherwise add a top-level `mcpServers` block to `%USERPROFILE%\.claude.json` directly:

   ```json
   {
     "mcpServers": {
       "jsdotnet-coding-guidelines": {
         "type": "stdio",
         "command": "jsdotnet-guidelines-mcpserver",
         "args": [],
         "tools": ["*"]
       },
       "jsdotnet-design-ux-guidelines": {
         "type": "stdio",
         "command": "jsdotnet-design-mcpserver",
         "args": [],
         "tools": ["*"]
       },
       "jsdotnet-publish-results": {
         "type": "stdio",
         "command": "jsdotnet-publish-mcpserver",
         "args": [],
         "tools": ["*"]
       }
     }
   }
   ```

   Keep the server IDs exactly as shown — the skills in `plugins/` refer to them by name.

3. **Restart Claude Code** (or start a new session) to pick up the change.

4. **Verify** with `/mcp` in a session, or ask "list the available guides".

**Publish root**: with no `args`, `jsdotnet-publish-mcpserver` writes to the per-user default
(`%LOCALAPPDATA%\JSdotNet\PublishedResults`). Add `"args": ["--Publish:RootPath=D:\\reports"]` to
point it elsewhere — use an absolute path at user level, since a relative one resolves against
whatever directory the session was started in.

**Scope precedence**: a server defined in a project's `.mcp.json` overrides a user-level server with
the same ID. In this repository, `.mcp.json` deliberately runs the design and publish servers from
source via `dotnet run`, so those two use the working-tree build here and the installed global tools
everywhere else. Note that the first `dotnet run` startup in a clean clone has to restore and build,
which can exceed the MCP startup timeout — build the solution once (`dotnet build JSdotNet.MCP.slnx`)
before starting a session.

---

#### Scenario 2: Local Development (Run from Source)

For contributors testing local changes before publishing to NuGet. This allows you to work with unpublished ADRs, recommendations, or structural changes.

**VS Code Setup**

1. **Clone the repository**:

   ```bash
   git clone https://github.com/JSdotNet/Project-Guidelines-MCP.git
   cd Project-Guidelines-MCP
   ```

2. **Create or edit `.vscode/mcp.json`** in the repository root with your actual path:

   ```json
   {
     "inputs": [],
     "servers": {
       "jsdotnet-project-guidelines-local": {
         "type": "stdio",
         "command": "dotnet",
         "args": [
           "run",
           "--project",
           "D:/projects/github.com/JSdotNet/Project-Guidelines-MCP/src/JSdotNet.MCP.Guidelines/JSdotNet.MCP.Guidelines.csproj"
         ]
       }
     }
   }
   ```

3. **Restart VS Code** - The MCP server will run directly from your local source code

**How it works**: When running from source with `dotnet run`, the server automatically discovers and reads documents from your local `guide/` folder. This allows you to test changes immediately without publishing.

**Testing Local NuGet Packages (Advanced)**

If you want to test the packaged tool locally before publishing to NuGet.org:

```powershell
# Pack the project
dotnet pack src/JSdotNet.MCP.Guidelines/JSdotNet.MCP.Guidelines.csproj -o ./local-packages

# Install from local package
dotnet tool install --global --add-source ./local-packages JSdotNet.MCP.Guidelines
```

---

#### Troubleshooting

**Server doesn't appear in Copilot**

- Check Output panel (View → Output) and select "MCP" from dropdown
- Verify the command path is correct (use full path if needed)
- Ensure .NET 10 SDK is installed: `dotnet --version`
- Try restarting VS Code

**Documents not loading**

- For NuGet installation: Check internet connectivity (docs fetched from GitHub)
- For local development: Verify `JSDOTNET_REPO_ROOT` points to repository root
- Check server logs in MCP Output panel

**Global tool not found**

- Verify installation: `dotnet tool list --global`
- Check PATH includes .NET tools directory
  - Windows: `%USERPROFILE%\.dotnet\tools`
  - Mac/Linux: `~/.dotnet/tools`

---

### Registering new documents

This repository uses `guide/index.json` as the canonical registry for MCP document discovery.

When you add, rename, edit, or remove markdown files in `guide/`, also update `guide/index.json` so metadata and paths stay in sync.

## Repo structure

```
guide/
 adrs/
 designs/
 recommendations/
 structures/
src/
 Project.Guidelines.guide/
 Project.Guidelines.McpServer/
tests/
 Project.Guidelines.McpServer.Tests/
JSdotNet.MCP.Guidelines.slnx
.github/
 copilot-instructions.md
```

## ADRs

- [0001: Adopt .NET 10 as Target Framework](guide/adrs/0001-adopt-dotnet-10.md) (Accepted)
- [0002: Central Package Management for .NET Solutions](guide/adrs/0002-central-package-management.md) (Accepted)
- [0003: .NET Aspire Recommendation for ASP.NET Services](guide/adrs/0003-recommend-aspire-for-aspnet-projects.md) (Accepted)
- [0004: Standardize Result Objects for Expected Application Outcomes](guide/adrs/0004-standardize-result-objects-for-expected-failures.md) (Accepted)
- [0005: Modular Monolith Project Structure](guide/adrs/0005-modular-monolith-structure.md) (Accepted)
- [0006: CQRS Recommendation for ASP.NET API](guide/adrs/0006-cqrs-recommendation-for-aspnet-api.md) (Accepted)
- [0007: Minimal APIs Over Controller-Based APIs](guide/adrs/0007-minimal-apis-over-controllers.md) (Accepted)
- [0008: Adopt Vertical Slice Architecture for Feature Organization](guide/adrs/0008-vertical-slice-architecture.md) (Accepted)
- [0009: Feature Slices Within Module Projects](guide/adrs/0009-feature-slices-module-structure.md) (Accepted)
- [0010: Adopt OpenTelemetry for Comprehensive Observability](guide/adrs/0010-adopt-opentelemetry-for-observability.md) (Accepted)
- [0011: Centralized Frontend Styling Variables](guide/adrs/0011-centralized-frontend-styling-variables.md) (Accepted)

## Designs

- [Modular Monolith Architecture Design](guide/designs/modular-monolith-architecture-design.md)
- [Pragmatic Domain-Driven Design Approach](guide/designs/pragmatic-domain-driven-design.md)

## Recommendations

- [Blazor Frontend Framework Guidance](guide/recommendations/blazor-frontend-framework-guidance.md)
- [C# Coding Style](guide/recommendations/csharp-coding-style.md)
- [Object Calisthenics for Domain Code](guide/recommendations/object-calisthenics-for-domain.md)
- [Specification Pattern for Business Rules](guide/recommendations/specification-pattern-usage.md)
- [Architecture Testing for Layer and Module Boundaries](guide/recommendations/testing-architecture.md)
- [End-to-End Testing](guide/recommendations/testing-end-to-end.md)
- [Integration Testing](guide/recommendations/testing-integration.md)
- [Testing Shared Instructions](guide/recommendations/testing-shared.md)
- [Unit Testing with xUnit, Moq, and Bogus](guide/recommendations/testing-unit.md)

## Structures

- [Feature Slices Module Structure](guide/structures/feature-slices-module-structure.md)
- [Folder Structure Reference](guide/structures/folder-structure-reference.md)
- [Minimal API Endpoint Organization](guide/structures/minimal-api-endpoint-organization.md)
- [Modular Solution Structure Template](guide/structures/modular-solution-structure.md)
- [Simple Solution Structure Design](guide/structures/simple-solution-structure.md)

---

## Development

### Building and Testing

```bash
# Build the solution
dotnet build JSdotNet.MCP.slnx

# Run all tests
dotnet test JSdotNet.MCP.slnx

# Run tests with coverage
dotnet test JSdotNet.MCP.slnx --collect:"XPlat Code Coverage" --results-directory ./coverage --settings coverlet.runsettings

# Generate coverage report
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:"Html"
```

### Code Coverage Requirements

- **Core Library** (`JSdotNet.MCP.Shared`): ≥80% line coverage
- **Tests**: All tests must pass
- Coverage reports are automatically generated in CI/CD

---

## CI/CD Workflows

### Build and Publish Workflow

**Workflow**: `.github/workflows/publish-nuget.yml`

Triggers on push to `main` branch when files in `src/` change.

Steps:

1. **Versioning** – GitVersion generates semantic version
2. **Build** – Compiles solution in Release configuration with version info
3. **Test** – Runs all unit tests with 80% coverage enforcement
4. **Coverage Report** – Generates coverage summary
5. **Package** – Creates NuGet package for the MCP Server
6. **Publish** – Pushes package to NuGet.org
7. **Release** – Creates GitHub release with version tag and artifacts

Semantic Versioning Strategy (GitHubFlow):

- **Main branch**: 1.0.0, 1.0.1, 1.0.2... (patch increments)
- **Feature branches** (`feature/*`): 1.1.0-alpha.1, 1.1.0-alpha.2... (minor with alpha pre-release)
- **Release branches** (`release/*`): 1.0.0-beta.1, 1.0.0-beta.2... (beta pre-release)

Configuration: `GitVersion.yml` at repository root.

### NuGet Packages

Published to NuGet.org:

- **JSdotNet.MCP.Guidelines** – coding guidelines MCP server (`jsdotnet-guidelines-mcpserver`)
- **JSdotNet.MCP.Design** – design/UX guidance MCP server (`jsdotnet-design-mcpserver`)
- **JSdotNet.MCP.Publish** – publish-results MCP server (`jsdotnet-publish-mcpserver`)

Each package is published independently: the release workflow only pushes a package when its own
source (or the shared `JSdotNet.MCP.Shared` project) changed on `main`.

Package features:

- .NET 10 global tool
- Automatic document discovery from filesystem or GitHub
- ModelContextProtocol SDK integration
- MIT license

### Setup Requirements

To enable automated publishing, add the following GitHub secret:

- `NUGET_API_KEY` – NuGet.org API key with push permissions

Navigate to: Repository Settings → Secrets and variables → Actions → New repository secret

### Local Version Testing

To check what version GitVersion would generate locally:

```bash
# Install GitVersion tool
dotnet tool install --global GitVersion.Tool

# Run in repository root
dotnet-gitversion
```

## Notes

- All code and examples target `.NET 10`.
- The MCP Server uses the `FileSystemDocumentCatalog` for local development and `GitHubDocumentCatalog` for published scenarios.
- Coverage threshold is enforced at 80% for core library code.
- CI/CD pipeline only triggers on changes to `src/` folder when pushed to `main` branch.

