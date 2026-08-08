namespace JSdotNet.MCP.Publish.Publishing;

/// <summary>Metadata describing a published result file.</summary>
public sealed record PublishedResult(string RelativePath, string FullPath, long SizeBytes, DateTimeOffset LastModifiedUtc);

/// <summary>Writes, reads and removes result files inside a single configured root folder.</summary>
public interface IResultPublisher
{
    /// <summary>Absolute path of the configured publish root.</summary>
    string RootPath { get; }

    /// <summary>Writes <paramref name="content"/> to <paramref name="relativePath"/> beneath the root.</summary>
    Task<PublishedResult> PublishAsync(string relativePath, string content, bool overwrite, CancellationToken ct = default);

    /// <summary>Appends <paramref name="content"/> to <paramref name="relativePath"/>, creating the file when needed.</summary>
    Task<PublishedResult> AppendAsync(string relativePath, string content, CancellationToken ct = default);

    /// <summary>Lists published files matching an optional glob pattern (defaults to all files).</summary>
    Task<IReadOnlyList<PublishedResult>> ListAsync(string? pattern = null, CancellationToken ct = default);

    /// <summary>Reads the text content of a published file.</summary>
    Task<string> ReadAsync(string relativePath, CancellationToken ct = default);

    /// <summary>Deletes a published file. Returns false when it did not exist.</summary>
    Task<bool> DeleteAsync(string relativePath, CancellationToken ct = default);
}
