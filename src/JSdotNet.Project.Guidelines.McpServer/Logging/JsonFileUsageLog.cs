using System.Text.Json;

namespace JSdotNet.Project.Guidelines.McpServer.Logging;

/// <summary>
/// Persists usage log entries as JSON Lines (one JSON object per line) in a per-process
/// file so concurrent MCP server instances never write to the same file.
/// </summary>
public sealed class JsonFileUsageLog : IUsageLog
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public JsonFileUsageLog(string filePath)
    {
        _filePath = filePath;
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }

    public async ValueTask RecordAsync(UsageLogEntry entry, CancellationToken ct = default)
    {
        var line = JsonSerializer.Serialize(entry, SerializerOptions);
        await _lock.WaitAsync(ct);
        try
        {
            await File.AppendAllTextAsync(_filePath, line + Environment.NewLine, ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<UsageLogEntry>> GetRecentAsync(int count, CancellationToken ct = default)
    {
        if (!File.Exists(_filePath))
            return Array.Empty<UsageLogEntry>();

        await _lock.WaitAsync(ct);
        try
        {
            var lines = await File.ReadAllLinesAsync(_filePath, ct);
            return lines
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .TakeLast(count)
                .Select(l => TryDeserialize(l))
                .Where(e => e is not null)
                .Cast<UsageLogEntry>()
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>Returns the path of the log file for this process.</summary>
    public string FilePath => _filePath;

    private static UsageLogEntry? TryDeserialize(string line)
    {
        try
        {
            return JsonSerializer.Deserialize<UsageLogEntry>(line, SerializerOptions);
        }
        catch
        {
            return null;
        }
    }
}
