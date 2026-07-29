using JSdotNet.MCP.Shared.Abstractions;
using JSdotNet.MCP.Shared.Logging;

namespace JSdotNet.MCP.Shared.Tests;

internal sealed class FakeDocumentCatalog : IDocumentCatalog
{
    private readonly IReadOnlyList<DocumentInfo> _documents;
    private readonly IReadOnlyList<DocumentInfo>? _searchResults;
    private readonly IReadOnlyList<DocumentInfo>? _tagResults;
    private readonly string? _content;
    private readonly bool _throwOnGetContent;
    private readonly bool _throwOnList;
    private readonly bool _throwOnSearch;

    public FakeDocumentCatalog(
        IReadOnlyList<DocumentInfo> documents,
        IReadOnlyList<DocumentInfo>? searchResults = null,
        IReadOnlyList<DocumentInfo>? tagResults = null,
        string? content = null,
        bool throwOnGetContent = false,
        bool throwOnList = false,
        bool throwOnSearch = false)
    {
        _documents = documents;
        _searchResults = searchResults;
        _tagResults = tagResults;
        _content = content;
        _throwOnGetContent = throwOnGetContent;
        _throwOnList = throwOnList;
        _throwOnSearch = throwOnSearch;
    }

    public Task<IReadOnlyList<DocumentInfo>> ListDocumentsAsync(CancellationToken cancellationToken = default)
    {
        if (_throwOnList) throw new InvalidOperationException("Catalog unavailable.");
        return Task.FromResult(_documents);
    }

    public Task<string> GetContentAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_throwOnGetContent)
            throw new FileNotFoundException($"Document '{id}' not found.");
        return Task.FromResult(_content ?? $"# {id}");
    }

    public Task<IReadOnlyList<DocumentInfo>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (_throwOnSearch) throw new InvalidOperationException("Search unavailable.");
        return Task.FromResult(_searchResults ?? (IReadOnlyList<DocumentInfo>)[]);
    }

    public Task<IReadOnlyList<DocumentInfo>> SearchByTagAsync(string tag, CancellationToken cancellationToken = default)
        => Task.FromResult(_tagResults ?? (IReadOnlyList<DocumentInfo>)[]);
}

internal sealed class FakeUsageLog : IUsageLog
{
    public List<UsageLogEntry> Entries { get; } = [];

    public ValueTask RecordAsync(UsageLogEntry entry, CancellationToken ct = default)
    {
        Entries.Add(entry);
        return ValueTask.CompletedTask;
    }

    public Task<IReadOnlyList<UsageLogEntry>> GetRecentAsync(int count, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<UsageLogEntry>>(Entries.TakeLast(count).ToList());
}
