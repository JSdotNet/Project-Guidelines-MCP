using System.ComponentModel;
using System.Text.Json;
using JSdotNet.MCP.Shared.Logging;
using ModelContextProtocol.Server;

namespace JSdotNet.MCP.Shared.Tools;

[McpServerToolType]
public static class UsageLogTools
{
    [McpServerTool(Name = "get_usage_logs"), Description("Returns recent tool-invocation records from this MCP server session. Each entry includes the tool name, input parameters, the document IDs that were returned, whether the call succeeded, and a timestamp. Use this to understand how the server is being used and which documents are consulted most. Intended for development and analysis; not recommended for production flows.")]
    public static async Task<string> GetUsageLogsAsync(
        IUsageLog usageLog,
        [Description("Number of most-recent entries to return (1–100, default 20)")] int count = 20,
        CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 100);
        var entries = await usageLog.GetRecentAsync(count, ct);
        return JsonSerializer.Serialize(entries, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        });
    }
}
