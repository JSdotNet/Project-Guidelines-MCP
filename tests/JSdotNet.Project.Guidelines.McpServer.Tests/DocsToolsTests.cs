using System.Text.Json;
using JSdotNet.Project.Guidelines.Docs.Abstractions;
using JSdotNet.Project.Guidelines.McpServer.Logging;
using JSdotNet.Project.Guidelines.McpServer.Tools;
using Microsoft.Extensions.Logging.Abstractions;

namespace JSdotNet.Project.Guidelines.McpServer.Tests;

public sealed class DocsToolsTests
{
    private static DocumentInfo MakeDoc(string id, string category, string[] tags)
        => new(id, $"Title {id}", $"Description for {id}", category, $"{category}/{id}.md", tags);

    // --- ListDocsAsync ---

    [Fact]
    public async Task ListDocsAsync_ReturnsSerialisedJsonArrayWithAllDocs()
    {
        var catalog = new FakeDocumentCatalog([
            MakeDoc("doc-1", "adrs", ["tag-a"]),
            MakeDoc("doc-2", "recommendations", ["tag-b"]),
        ]);
        var sut = new DocsTools(catalog, new FakeUsageLog(), NullLogger<DocsTools>.Instance);

        var result = await sut.ListDocsAsync(CancellationToken.None);

        var docs = JsonSerializer.Deserialize<JsonElement[]>(result);
        Assert.NotNull(docs);
        Assert.Equal(2, docs.Length);
        Assert.Equal("doc-1", docs[0].GetProperty("Id").GetString());
        Assert.Equal("doc-2", docs[1].GetProperty("Id").GetString());
    }

    [Fact]
    public async Task ListDocsAsync_EmptyCatalog_ReturnsEmptyJsonArray()
    {
        var sut = new DocsTools(new FakeDocumentCatalog([]), new FakeUsageLog(), NullLogger<DocsTools>.Instance);

        var result = await sut.ListDocsAsync(CancellationToken.None);

        Assert.Equal("[]", result);
    }

    [Fact]
    public async Task ListDocsAsync_RecordsSuccessfulUsage()
    {
        var usageLog = new FakeUsageLog();
        var catalog = new FakeDocumentCatalog([MakeDoc("d", "adrs", [])]);
        var sut = new DocsTools(catalog, usageLog, NullLogger<DocsTools>.Instance);

        await sut.ListDocsAsync(CancellationToken.None);

        Assert.Single(usageLog.Entries);
        Assert.Equal("list_docs", usageLog.Entries[0].ToolName);
        Assert.True(usageLog.Entries[0].Succeeded);
        Assert.Null(usageLog.Entries[0].ErrorMessage);
    }

    [Fact]
    public async Task ListDocsAsync_ResultDocumentIdsMatchReturnedDocs()
    {
        var usageLog = new FakeUsageLog();
        var catalog = new FakeDocumentCatalog([
            MakeDoc("doc-a", "adrs", []),
            MakeDoc("doc-b", "designs", []),
        ]);
        var sut = new DocsTools(catalog, usageLog, NullLogger<DocsTools>.Instance);

        await sut.ListDocsAsync(CancellationToken.None);

        Assert.Equal(["doc-a", "doc-b"], usageLog.Entries[0].ResultDocumentIds);
    }

    // --- ListDocsByTypeAsync ---

    [Fact]
    public async Task ListDocsByTypeAsync_FiltersToMatchingCategory()
    {
        var catalog = new FakeDocumentCatalog([
            MakeDoc("adr-1", "adrs", []),
            MakeDoc("rec-1", "recommendations", []),
        ]);
        var sut = new DocsTools(catalog, new FakeUsageLog(), NullLogger<DocsTools>.Instance);

        var result = await sut.ListDocsByTypeAsync("adrs", CancellationToken.None);

        var docs = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Single(docs);
        Assert.Equal("adr-1", docs[0].GetProperty("Id").GetString());
    }

    [Fact]
    public async Task ListDocsByTypeAsync_FilterIsCaseInsensitive()
    {
        var catalog = new FakeDocumentCatalog([MakeDoc("d", "ADRs", [])]);
        var sut = new DocsTools(catalog, new FakeUsageLog(), NullLogger<DocsTools>.Instance);

        var result = await sut.ListDocsByTypeAsync("adrs", CancellationToken.None);

        var docs = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Single(docs);
    }

    [Fact]
    public async Task ListDocsByTypeAsync_SubcategoryPath_MatchesOnFirstSegment()
    {
        var catalog = new FakeDocumentCatalog([MakeDoc("doc", "adrs/0001", [])]);
        var sut = new DocsTools(catalog, new FakeUsageLog(), NullLogger<DocsTools>.Instance);

        var result = await sut.ListDocsByTypeAsync("adrs", CancellationToken.None);

        var docs = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Single(docs);
    }

    [Fact]
    public async Task ListDocsByTypeAsync_RecordsUsageWithCategory()
    {
        var usageLog = new FakeUsageLog();
        var sut = new DocsTools(new FakeDocumentCatalog([]), usageLog, NullLogger<DocsTools>.Instance);

        await sut.ListDocsByTypeAsync("adrs", CancellationToken.None);

        Assert.Single(usageLog.Entries);
        Assert.Equal("list_docs_by_type", usageLog.Entries[0].ToolName);
        Assert.Equal("adrs", usageLog.Entries[0].Parameters["category"]);
        Assert.True(usageLog.Entries[0].Succeeded);
    }

    // --- SearchDocsAsync ---

    [Fact]
    public async Task SearchDocsAsync_ReturnsCatalogSearchResults()
    {
        var doc = MakeDoc("found-doc", "adrs", []);
        var catalog = new FakeDocumentCatalog([], searchResults: [doc]);
        var sut = new DocsTools(catalog, new FakeUsageLog(), NullLogger<DocsTools>.Instance);

        var result = await sut.SearchDocsAsync("logging", CancellationToken.None);

        var docs = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Single(docs);
        Assert.Equal("found-doc", docs[0].GetProperty("Id").GetString());
    }

    [Fact]
    public async Task SearchDocsAsync_NoResults_ReturnsEmptyArray()
    {
        var sut = new DocsTools(new FakeDocumentCatalog([]), new FakeUsageLog(), NullLogger<DocsTools>.Instance);

        var result = await sut.SearchDocsAsync("unknown-query", CancellationToken.None);

        Assert.Equal("[]", result);
    }

    [Fact]
    public async Task SearchDocsAsync_RecordsUsageWithQuery()
    {
        var usageLog = new FakeUsageLog();
        var sut = new DocsTools(new FakeDocumentCatalog([]), usageLog, NullLogger<DocsTools>.Instance);

        await sut.SearchDocsAsync("my query", CancellationToken.None);

        Assert.Equal("search_docs", usageLog.Entries[0].ToolName);
        Assert.Equal("my query", usageLog.Entries[0].Parameters["query"]);
        Assert.True(usageLog.Entries[0].Succeeded);
    }

    // --- SearchDocsByTagAsync ---

    [Fact]
    public async Task SearchDocsByTagAsync_ReturnsCatalogTagResults()
    {
        var doc = MakeDoc("tagged-doc", "adrs", ["persistence"]);
        var catalog = new FakeDocumentCatalog([], tagResults: [doc]);
        var sut = new DocsTools(catalog, new FakeUsageLog(), NullLogger<DocsTools>.Instance);

        var result = await sut.SearchDocsByTagAsync("persistence", CancellationToken.None);

        var docs = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Single(docs);
        Assert.Equal("tagged-doc", docs[0].GetProperty("Id").GetString());
    }

    [Fact]
    public async Task SearchDocsByTagAsync_NoMatches_ReturnsEmptyArray()
    {
        var sut = new DocsTools(new FakeDocumentCatalog([]), new FakeUsageLog(), NullLogger<DocsTools>.Instance);

        var result = await sut.SearchDocsByTagAsync("nonexistent-tag", CancellationToken.None);

        Assert.Equal("[]", result);
    }

    [Fact]
    public async Task SearchDocsByTagAsync_RecordsUsageWithTag()
    {
        var usageLog = new FakeUsageLog();
        var sut = new DocsTools(new FakeDocumentCatalog([]), usageLog, NullLogger<DocsTools>.Instance);

        await sut.SearchDocsByTagAsync("cqrs", CancellationToken.None);

        Assert.Equal("search_docs_by_tag", usageLog.Entries[0].ToolName);
        Assert.Equal("cqrs", usageLog.Entries[0].Parameters["tag"]);
        Assert.True(usageLog.Entries[0].Succeeded);
    }

    // --- GetDocAsync ---

    [Fact]
    public async Task GetDocAsync_ReturnsDocumentContent()
    {
        var catalog = new FakeDocumentCatalog([], content: "# My Doc\nContent here.");
        var sut = new DocsTools(catalog, new FakeUsageLog(), NullLogger<DocsTools>.Instance);

        var result = await sut.GetDocAsync("doc-1", CancellationToken.None);

        Assert.Equal("# My Doc\nContent here.", result);
    }

    [Fact]
    public async Task GetDocAsync_RecordsSuccessWithDocId()
    {
        var usageLog = new FakeUsageLog();
        var catalog = new FakeDocumentCatalog([], content: "# Content");
        var sut = new DocsTools(catalog, usageLog, NullLogger<DocsTools>.Instance);

        await sut.GetDocAsync("doc-1", CancellationToken.None);

        Assert.Equal("get_doc", usageLog.Entries[0].ToolName);
        Assert.Equal("doc-1", usageLog.Entries[0].Parameters["id"]);
        Assert.True(usageLog.Entries[0].Succeeded);
        Assert.Equal(["doc-1"], usageLog.Entries[0].ResultDocumentIds);
    }

    [Fact]
    public async Task GetDocAsync_WhenCatalogThrows_PropagatesExceptionAndRecordsFailure()
    {
        var usageLog = new FakeUsageLog();
        var catalog = new FakeDocumentCatalog([], throwOnGetContent: true);
        var sut = new DocsTools(catalog, usageLog, NullLogger<DocsTools>.Instance);

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => sut.GetDocAsync("missing-doc", CancellationToken.None));

        Assert.Single(usageLog.Entries);
        Assert.False(usageLog.Entries[0].Succeeded);
        Assert.NotNull(usageLog.Entries[0].ErrorMessage);
        Assert.Contains("missing-doc", usageLog.Entries[0].ErrorMessage);
    }
}
