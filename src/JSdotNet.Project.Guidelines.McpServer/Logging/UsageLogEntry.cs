namespace JSdotNet.Project.Guidelines.McpServer.Logging;

/// <summary>
/// A single recorded tool invocation with its input parameters and result metadata.
/// </summary>
public sealed record UsageLogEntry(
    DateTimeOffset Timestamp,
    string ToolName,
    Dictionary<string, string> Parameters,
    string[] ResultDocumentIds,
    int ResultCount,
    bool Succeeded,
    string? ErrorMessage
);
