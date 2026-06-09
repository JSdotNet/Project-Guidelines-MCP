namespace JSdotNet.Project.Guidelines.McpServer.Logging;

/// <summary>
/// Records tool invocations so usage can be analysed later to improve the server.
/// </summary>
public interface IUsageLog
{
    /// <summary>Append a single tool invocation record.</summary>
    ValueTask RecordAsync(UsageLogEntry entry, CancellationToken ct = default);

    /// <summary>Return the most recent <paramref name="count"/> entries in chronological order.</summary>
    Task<IReadOnlyList<UsageLogEntry>> GetRecentAsync(int count, CancellationToken ct = default);
}
