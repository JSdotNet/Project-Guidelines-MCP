using System.ComponentModel;
using ModelContextProtocol.Server;

namespace JSdotNet.MCP.Shared.Prompts;

/// <summary>
/// Workflow prompts over the defaults catalog. Each one tells the agent which tools to call and
/// which documents to cite, so the answer stays traceable to doc ids.
/// </summary>
[McpServerPromptType]
public static class DefaultsPrompts
{
    [McpServerPrompt(Name = "new-app"), Description("Scaffold a new application to the JSdotNet golden setup described by the defaults.")]
    public static string NewApp(
        [Description("Name of the application, e.g. Contoso.Orders")] string name,
        [Description("Optional notes on what the application does")] string? purpose = null)
        => $"""
            Scaffold a new application named `{name}` to the JSdotNet application defaults.
            {(string.IsNullOrWhiteSpace(purpose) ? string.Empty : $"Purpose: {purpose}\n")}
            1. Call `list_guides_by_type` with `adrs` and read every accepted ADR with `get_guide`.
            2. Call `list_guides_by_type` with `structures` and follow the building-block view for
               the solution, module and test layout.
            3. Apply the `design` category for any UI, and the `config` category for `.mcp.json`
               and instruction files.
            4. Where the defaults leave a choice to the application (database, broker, frontend
               framework), record it as the application's own ADR instead of guessing.
            Cite the doc id of every default you apply (e.g. `// ADR-0003`).
            """;

    [McpServerPrompt(Name = "review-against-defaults"), Description("Review code, a change or a design against the JSdotNet application defaults and report every deviation with its source doc id.")]
    public static string ReviewAgainstDefaults(
        [Description("What to review: a path, a diff, a pull request or a pasted design")] string target)
        => $"""
            Review `{target}` against the JSdotNet application defaults.

            1. Identify the concerns the target touches (architecture, API, persistence, testing,
               observability, security, UI, configuration).
            2. For each concern, call `search_guides` or `search_guides_by_tag` and read the
               governing documents with `get_guide`.
            3. Report every deviation as: the doc id it breaks, what the target does, what the
               default requires, and the compliant alternative.
            4. Say explicitly which concerns no document covers; propose an ADR for those instead
               of judging from first principles.
            """;

    [McpServerPrompt(Name = "propose-adr"), Description("Draft a new ADR for a decision the defaults do not cover yet, numbered after the existing ADRs.")]
    public static string ProposeAdr(
        [Description("The decision to record, as a short title")] string title,
        [Description("Optional context: the problem and the forces behind it")] string? context = null)
        => $"""
            Draft an ADR titled "{title}" for the JSdotNet application defaults.
            {(string.IsNullOrWhiteSpace(context) ? string.Empty : $"Context: {context}\n")}
            1. Call `list_guides_by_type` with `adrs`; the new ADR takes the next free number and
               never reuses a retired one.
            2. Call `search_guides` for the topic and read each related document with `get_guide`.
               When an existing ADR already covers the decision, propose rewriting it instead.
            3. Write `defaults/arc42/adr/NNNN-<kebab-title>.md` with Status (Proposed), Context,
               Decision, Consequences and the doc ids it relates to, and add its entry to
               `defaults/index.json`.
            """;
}
