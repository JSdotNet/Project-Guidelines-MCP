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

    public FakeDocumentCatalog(
        IReadOnlyList<DocumentInfo> documents,
        IReadOnlyList<DocumentInfo>? searchResults = null,
        IReadOnlyList<DocumentInfo>? tagResults = null,
        string? content = null,
        bool throwOnGetContent = false)
    {
        _documents = documents;
        _searchResults = searchResults;
        _tagResults = tagResults;
        _content = content;
        _throwOnGetContent = throwOnGetContent;
    }

    public IReadOnlyList<DocumentInfo> ListDocuments() => _documents;

    public Task<string> GetContentAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_throwOnGetContent)
            throw new FileNotFoundException($"Document '{id}' not found.");
        return Task.FromResult(_content ?? $"# {id}");
    }

    public IReadOnlyList<DocumentInfo> Search(string query) => _searchResults ?? [];

    public IReadOnlyList<DocumentInfo> SearchByTag(string tag) => _tagResults ?? [];
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
