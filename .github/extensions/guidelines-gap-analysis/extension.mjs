import { joinSession } from "@github/copilot-sdk/extension";
import { readdir, stat } from "node:fs/promises";
import { join, basename } from "node:path";

const EXCLUDED_DIRS = new Set(["bin", "obj", ".git", "node_modules", ".vs", "packages", "TestResults"]);

const session = await joinSession({
    tools: [
        {
            name: "start_gap_analysis",
            description:
                "Scans a .NET project or solution directory and identifies layer structure, missing conventions, " +
                "and configuration gaps. Returns a structured project summary plus a step-by-step workflow " +
                "instructing the agent to use the guidelines MCP to complete the gap analysis. " +
                "Run this first, then follow the returned workflow using MCP tools.",
            parameters: {
                type: "object",
                properties: {
                    projectPath: {
                        type: "string",
                        description:
                            "Absolute path to the .NET solution root. Defaults to the current working directory.",
                    },
                },
            },
            handler: async (args) => {
                const root = args.projectPath || process.cwd();
                let summary;
                try {
                    summary = await buildProjectSummary(root);
                } catch (err) {
                    summary = `(Could not scan directory: ${err.message})`;
                }

                return `# Gap Analysis — Project Scan Results

**Root:** ${root}

${summary}

---

## How to Complete the Gap Analysis

Follow these steps using the guidelines MCP tools:

### Step 1: Get the full guidelines catalog
Call \`list_docs\` to see all available ADRs, designs, recommendations, and structures.

### Step 2: Review foundational decisions (ADRs)
Call \`list_docs_by_type("adrs")\` and for each Accepted ADR ask:
- Does the project structure above reflect this decision?
- Is there evidence in the project (layer names, project references, patterns) that it is followed?

### Step 3: Review recommendations
Call \`list_docs_by_type("recommendations")\` and for each recommendation:
- Is there evidence of compliance in the scanned structure?
- Are there known anti-patterns present (e.g. logic in controllers, EF attributes in Domain)?

### Step 4: Compare to structural templates
Call \`list_docs_by_type("structures")\` and compare the canonical scaffold against the found layer structure.

### Step 5: Search for topic-specific guidance
For each identified concern in the scan above, run:
- \`search_docs("relevant term")\` — e.g. "error handling", "testing", "logging"
- \`search_docs_by_tag("relevant-tag")\` — e.g. "domain", "persistence", "resilience"

### Step 6: Produce the gap report
For each gap found, report:
| Field | Content |
|-------|---------|
| **Gap** | What is missing or non-compliant |
| **Guideline** | ADR / recommendation ID and title |
| **Evidence** | What in the scan suggests the gap |
| **Priority** | Critical / High / Medium / Low |
| **Action** | What should be done to close the gap |`;
            },
        },
    ],
    hooks: {},
});

// ---------------------------------------------------------------------------
// Project scanning helpers
// ---------------------------------------------------------------------------

async function buildProjectSummary(root) {
    const lines = [];

    // Solution files
    const slnFiles = await findFiles(root, [".slnx", ".sln"], 2);
    if (slnFiles.length > 0) {
        lines.push(`**Solution files:** ${slnFiles.map((f) => basename(f)).join(", ")}`);
    } else {
        lines.push("**Solution files:** ⚠️ None found — may not be a .NET solution root");
    }

    // C# project files, categorised by layer
    const csprojFiles = await findFiles(root, [".csproj"], 6);
    const layers = { domain: [], application: [], infrastructure: [], adapters: [], tests: [], other: [] };

    for (const p of csprojFiles) {
        const name = basename(p, ".csproj").toLowerCase();
        if (name.includes("domain")) layers.domain.push(basename(p, ".csproj"));
        else if (name.includes("application")) layers.application.push(basename(p, ".csproj"));
        else if (name.includes("infrastructure")) layers.infrastructure.push(basename(p, ".csproj"));
        else if (name.includes("adapter") || name.includes(".http") || name.includes(".api") || name.includes(".grpc") || name.includes(".cli") || name.includes(".worker"))
            layers.adapters.push(basename(p, ".csproj"));
        else if (name.includes("test")) layers.tests.push(basename(p, ".csproj"));
        else layers.other.push(basename(p, ".csproj"));
    }

    lines.push(`\n**Projects found (${csprojFiles.length}):**`);
    for (const [layer, projects] of Object.entries(layers)) {
        if (projects.length > 0) {
            lines.push(`- **${layer}** (${projects.length}): ${projects.join(", ")}`);
        }
    }
    if (layers.other.length > 0) {
        lines.push(`  _(Note: "other" projects could not be classified by name — manual review needed)_`);
    }

    // Hexagonal layer compliance indicators
    lines.push("\n**Hexagonal layer indicators:**");
    lines.push(`- ${layers.domain.length > 0 ? "✅" : "❌"} Domain layer project(s) present`);
    lines.push(`- ${layers.application.length > 0 ? "✅" : "❌"} Application layer project(s) present`);
    lines.push(`- ${layers.infrastructure.length > 0 ? "✅" : "❌"} Infrastructure / adapter project(s) present`);
    lines.push(`- ${layers.tests.length > 0 ? "✅" : "⚠️"} Test project(s) present`);

    // Repository-level configuration
    lines.push("\n**Repository configuration:**");
    const checks = [
        { file: "global.json", label: "global.json (SDK version pin)" },
        { file: ".editorconfig", label: ".editorconfig (code style enforcement)" },
        { file: "Directory.Build.props", label: "Directory.Build.props (shared MSBuild props)" },
        { file: "Directory.Packages.props", label: "Directory.Packages.props (central package management)" },
        { file: "coverlet.runsettings", label: "coverlet.runsettings (coverage config)" },
        { file: ".github/copilot-instructions.md", label: "copilot-instructions.md (AI guidance)" },
    ];
    for (const { file, label } of checks) {
        const exists = await fileExists(join(root, file));
        lines.push(`- ${exists ? "✅" : "❌"} ${label}`);
    }

    return lines.join("\n");
}

async function findFiles(dir, extensions, maxDepth, depth = 0) {
    if (depth > maxDepth) return [];
    const results = [];
    let entries;
    try {
        entries = await readdir(dir, { withFileTypes: true });
    } catch {
        return results;
    }
    for (const entry of entries) {
        if (EXCLUDED_DIRS.has(entry.name.toLowerCase())) continue;
        const fullPath = join(dir, entry.name);
        try {
            if (entry.isSymbolicLink()) continue;
            if (entry.isDirectory()) {
                const nested = await findFiles(fullPath, extensions, maxDepth, depth + 1);
                results.push(...nested);
            } else if (extensions.some((ext) => entry.name.endsWith(ext))) {
                results.push(fullPath);
            }
        } catch {
            // skip unreadable entries
        }
    }
    return results;
}

async function fileExists(path) {
    try {
        await stat(path);
        return true;
    } catch {
        return false;
    }
}
