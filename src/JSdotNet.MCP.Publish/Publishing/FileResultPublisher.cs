using Microsoft.Extensions.Options;

namespace JSdotNet.MCP.Publish.Publishing;

/// <summary>
/// File-system backed publisher. All paths are confined to the configured root folder so a
/// caller can never escape the publish location with an absolute path or <c>..</c> segments.
/// </summary>
public sealed class FileResultPublisher : IResultPublisher
{
    private readonly PublishOptions _options;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FileResultPublisher(IOptions<PublishOptions> options)
        : this(options.Value)
    {
    }

    public FileResultPublisher(PublishOptions options)
    {
        _options = options;
        RootPath = PublishOptions.ResolveRootPath(options.RootPath);
        Directory.CreateDirectory(RootPath);
    }

    public string RootPath { get; }

    public async Task<PublishedResult> PublishAsync(string relativePath, string content, bool overwrite, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(relativePath);
        var allowOverwrite = overwrite || _options.AllowOverwriteByDefault;

        await _lock.WaitAsync(ct);
        try
        {
            if (File.Exists(fullPath) && !allowOverwrite)
                throw new IOException($"'{Normalize(relativePath)}' already exists. Pass overwrite=true to replace it.");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            await File.WriteAllTextAsync(fullPath, content, ct);
            return Describe(fullPath);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<PublishedResult> AppendAsync(string relativePath, string content, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(relativePath);

        await _lock.WaitAsync(ct);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            await File.AppendAllTextAsync(fullPath, content, ct);
            return Describe(fullPath);
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task<IReadOnlyList<PublishedResult>> ListAsync(string? pattern = null, CancellationToken ct = default)
    {
        if (!Directory.Exists(RootPath))
            return Task.FromResult<IReadOnlyList<PublishedResult>>([]);

        var searchPattern = string.IsNullOrWhiteSpace(pattern) ? "*" : pattern.Trim().Replace('\\', '/');
        if (Path.IsPathRooted(searchPattern) || searchPattern.Split('/').Any(s => s == ".."))
            throw new ArgumentException("Pattern must be relative and may not contain '..'.", nameof(pattern));

        ct.ThrowIfCancellationRequested();

        IReadOnlyList<PublishedResult> results = Directory
            .EnumerateFiles(RootPath, searchPattern, SearchOption.AllDirectories)
            .Select(Describe)
            .OrderByDescending(r => r.LastModifiedUtc)
            .ToList();

        return Task.FromResult(results);
    }

    public async Task<string> ReadAsync(string relativePath, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(relativePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"'{Normalize(relativePath)}' was not found in the publish location.", fullPath);

        return await File.ReadAllTextAsync(fullPath, ct);
    }

    public Task<bool> DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(relativePath);
        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    private PublishedResult Describe(string fullPath)
    {
        var info = new FileInfo(fullPath);
        return new PublishedResult(
            Path.GetRelativePath(RootPath, fullPath).Replace('\\', '/'),
            fullPath,
            info.Exists ? info.Length : 0,
            info.Exists ? info.LastWriteTimeUtc : DateTimeOffset.UtcNow);
    }

    private static string Normalize(string relativePath) => relativePath.Replace('\\', '/').Trim();

    /// <summary>Resolves a caller-supplied relative path to an absolute path inside the root.</summary>
    internal string ResolvePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("A relative file path is required.", nameof(relativePath));

        var candidate = Normalize(relativePath);

        if (candidate.StartsWith('/') || Path.IsPathRooted(candidate) || Path.IsPathFullyQualified(candidate))
            throw new ArgumentException("Path must be relative to the configured publish location.", nameof(relativePath));

        if (candidate.Split('/').Any(segment => segment == ".."))
            throw new ArgumentException("Path may not contain '..' segments.", nameof(relativePath));

        var fullPath = Path.GetFullPath(Path.Join(RootPath, candidate));
        var rootPrefix = RootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootPrefix, StringComparison.Ordinal))
            throw new ArgumentException("Path resolves outside the configured publish location.", nameof(relativePath));

        return fullPath;
    }
}
