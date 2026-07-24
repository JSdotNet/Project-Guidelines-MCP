using System.Text.Json;
using JSdotNet.MCP.Shared.Abstractions;
using JSdotNet.MCP.Shared.Logging;
using JSdotNet.MCP.Shared.Tools;
using Microsoft.Extensions.Logging.Abstractions;

namespace JSdotNet.MCP.Guidelines.Tests;

public sealed class GuidesToolsTests
{
    private static DocumentInfo MakeDoc(string id, string category, string[] tags)
        => new(id, $"Title {id}", $"Description for {id}", category, $"{category}/{id}.md", tags);

    // --- ListGuidesAsync ---

    [Fact]
    public async Task ListGuidesAsync_ReturnsSerialisedJsonArrayWithAllDocs()
    {
        var catalog = new FakeDocumentCatalog([
            MakeDoc("doc-1", "adrs", ["tag-a"]),
            MakeDoc("doc-2", "recommendations", ["tag-b"]),
        ]);
        var sut = new GuidesTools(catalog, new FakeUsageLog(), NullLogger<GuidesTools>.Instance);

        var result = await sut.ListGuidesAsync(CancellationToken.None);

        var docs = JsonSerializer.Deserialize<JsonElement[]>(result);
        Assert.NotNull(docs);
        Assert.Equal(2, docs.Length);
        Assert.Equal("doc-1", docs[0].GetProperty("Id").GetString());
        Assert.Equal("doc-2", docs[1].GetProperty("Id").GetString());
    }

    [Fact]
    public async Task ListGuidesAsync_EmptyCatalog_ReturnsEmptyJsonArray()
    {
        var sut = new GuidesTools(new FakeDocumentCatalog([]), new FakeUsageLog(), NullLogger<GuidesTools>.Instance);

        var result = await sut.ListGuidesAsync(CancellationToken.None);

        Assert.Equal("[]", result);
    }

    [Fact]
    public async Task ListGuidesAsync_RecordsSuccessfulUsage()
    {
        var usageLog = new FakeUsageLog();
        var catalog = new FakeDocumentCatalog([MakeDoc("d", "adrs", [])]);
        var sut = new GuidesTools(catalog, usageLog, NullLogger<GuidesTools>.Instance);

        await sut.ListGuidesAsync(CancellationToken.None);

        Assert.Single(usageLog.Entries);
        Assert.Equal("list_guides", usageLog.Entries[0].ToolName);
        Assert.True(usageLog.Entries[0].Succeeded);
        Assert.Null(usageLog.Entries[0].ErrorMessage);
    }

    [Fact]
    public async Task ListGuidesAsync_ResultDocumentIdsMatchReturnedDocs()
    {
        var usageLog = new FakeUsageLog();
        var catalog = new FakeDocumentCatalog([
            MakeDoc("doc-a", "adrs", []),
            MakeDoc("doc-b", "designs", []),
        ]);
        var sut = new GuidesTools(catalog, usageLog, NullLogger<GuidesTools>.Instance);

        await sut.ListGuidesAsync(CancellationToken.None);

        Assert.Equal(["doc-a", "doc-b"], usageLog.Entries[0].ResultDocumentIds);
    }

    // --- ListGuidesByTypeAsync ---

    [Fact]
    public async Task ListGuidesByTypeAsync_FiltersToMatchingCategory()
    {
        var catalog = new FakeDocumentCatalog([
            MakeDoc("adr-1", "adrs", []),
            MakeDoc("rec-1", "recommendations", []),
        ]);
        var sut = new GuidesTools(catalog, new FakeUsageLog(), NullLogger<GuidesTools>.Instance);

        var result = await sut.ListGuidesByTypeAsync("adrs", CancellationToken.None);

        var docs = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Single(docs);
        Assert.Equal("adr-1", docs[0].GetProperty("Id").GetString());
    }

    [Fact]
    public async Task ListGuidesByTypeAsync_FilterIsCaseInsensitive()
    {
        var catalog = new FakeDocumentCatalog([MakeDoc("d", "ADRs", [])]);
        var sut = new GuidesTools(catalog, new FakeUsageLog(), NullLogger<GuidesTools>.Instance);

        var result = await sut.ListGuidesByTypeAsync("adrs", CancellationToken.None);

        var docs = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Single(docs);
    }

    [Fact]
    public async Task ListGuidesByTypeAsync_SubcategoryPath_MatchesOnFirstSegment()
    {
        var catalog = new FakeDocumentCatalog([MakeDoc("doc", "adrs/0001", [])]);
        var sut = new GuidesTools(catalog, new FakeUsageLog(), NullLogger<GuidesTools>.Instance);

        var result = await sut.ListGuidesByTypeAsync("adrs", CancellationToken.None);

        var docs = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Single(docs);
    }

    [Fact]
    public async Task ListGuidesByTypeAsync_RecordsUsageWithCategory()
    {
        var usageLog = new FakeUsageLog();
        var sut = new GuidesTools(new FakeDocumentCatalog([]), usageLog, NullLogger<GuidesTools>.Instance);

        await sut.ListGuidesByTypeAsync("adrs", CancellationToken.None);

        Assert.Single(usageLog.Entries);
        Assert.Equal("list_guides_by_type", usageLog.Entries[0].ToolName);
        Assert.Equal("adrs", usageLog.Entries[0].Parameters["category"]);
        Assert.True(usageLog.Entries[0].Succeeded);
    }

    // --- SearchGuidesAsync ---

    [Fact]
    public async Task SearchGuidesAsync_ReturnsCatalogSearchResults()
    {
        var doc = MakeDoc("found-doc", "adrs", []);
        var catalog = new FakeDocumentCatalog([], searchResults: [doc]);
        var sut = new GuidesTools(catalog, new FakeUsageLog(), NullLogger<GuidesTools>.Instance);

        var result = await sut.SearchGuidesAsync("logging", CancellationToken.None);

        var docs = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Single(docs);
        Assert.Equal("found-doc", docs[0].GetProperty("Id").GetString());
    }

    [Fact]
    public async Task SearchGuidesAsync_NoResults_ReturnsEmptyArray()
    {
        var sut = new GuidesTools(new FakeDocumentCatalog([]), new FakeUsageLog(), NullLogger<GuidesTools>.Instance);

        var result = await sut.SearchGuidesAsync("unknown-query", CancellationToken.None);

        Assert.Equal("[]", result);
    }

    [Fact]
    public async Task SearchGuidesAsync_RecordsUsageWithQuery()
    {
        var usageLog = new FakeUsageLog();
        var sut = new GuidesTools(new FakeDocumentCatalog([]), usageLog, NullLogger<GuidesTools>.Instance);

        await sut.SearchGuidesAsync("my query", CancellationToken.None);

        Assert.Equal("search_guides", usageLog.Entries[0].ToolName);
        Assert.Equal("my query", usageLog.Entries[0].Parameters["query"]);
        Assert.True(usageLog.Entries[0].Succeeded);
    }

    // --- SearchGuidesByTagAsync ---

    [Fact]
    public async Task SearchGuidesByTagAsync_ReturnsCatalogTagResults()
    {
        var doc = MakeDoc("tagged-doc", "adrs", ["persistence"]);
        var catalog = new FakeDocumentCatalog([], tagResults: [doc]);
        var sut = new GuidesTools(catalog, new FakeUsageLog(), NullLogger<GuidesTools>.Instance);

        var result = await sut.SearchGuidesByTagAsync("persistence", CancellationToken.None);

        var docs = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Single(docs);
        Assert.Equal("tagged-doc", docs[0].GetProperty("Id").GetString());
    }

    [Fact]
    public async Task SearchGuidesByTagAsync_NoMatches_ReturnsEmptyArray()
    {
        var sut = new GuidesTools(new FakeDocumentCatalog([]), new FakeUsageLog(), NullLogger<GuidesTools>.Instance);

        var result = await sut.SearchGuidesByTagAsync("nonexistent-tag", CancellationToken.None);

        Assert.Equal("[]", result);
    }

    [Fact]
    public async Task SearchGuidesByTagAsync_RecordsUsageWithTag()
    {
        var usageLog = new FakeUsageLog();
        var sut = new GuidesTools(new FakeDocumentCatalog([]), usageLog, NullLogger<GuidesTools>.Instance);

        await sut.SearchGuidesByTagAsync("cqrs", CancellationToken.None);

        Assert.Equal("search_guides_by_tag", usageLog.Entries[0].ToolName);
        Assert.Equal("cqrs", usageLog.Entries[0].Parameters["tag"]);
        Assert.True(usageLog.Entries[0].Succeeded);
    }

    // --- GetGuideAsync ---

    [Fact]
    public async Task GetGuideAsync_ReturnsDocumentContent()
    {
        var catalog = new FakeDocumentCatalog([], content: "# My Doc\nContent here.");
        var sut = new GuidesTools(catalog, new FakeUsageLog(), NullLogger<GuidesTools>.Instance);

        var result = await sut.GetGuideAsync("doc-1", CancellationToken.None);

        Assert.Equal("# My Doc\nContent here.", result);
    }

    [Fact]
    public async Task GetGuideAsync_RecordsSuccessWithDocId()
    {
        var usageLog = new FakeUsageLog();
        var catalog = new FakeDocumentCatalog([], content: "# Content");
        var sut = new GuidesTools(catalog, usageLog, NullLogger<GuidesTools>.Instance);

        await sut.GetGuideAsync("doc-1", CancellationToken.None);

        Assert.Equal("get_guide", usageLog.Entries[0].ToolName);
        Assert.Equal("doc-1", usageLog.Entries[0].Parameters["id"]);
        Assert.True(usageLog.Entries[0].Succeeded);
        Assert.Equal(["doc-1"], usageLog.Entries[0].ResultDocumentIds);
    }

    [Fact]
    public async Task GetGuideAsync_WhenCatalogThrows_PropagatesExceptionAndRecordsFailure()
    {
        var usageLog = new FakeUsageLog();
        var catalog = new FakeDocumentCatalog([], throwOnGetContent: true);
        var sut = new GuidesTools(catalog, usageLog, NullLogger<GuidesTools>.Instance);

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => sut.GetGuideAsync("missing-doc", CancellationToken.None));

        Assert.Single(usageLog.Entries);
        Assert.False(usageLog.Entries[0].Succeeded);
        Assert.NotNull(usageLog.Entries[0].ErrorMessage);
        Assert.Contains("missing-doc", usageLog.Entries[0].ErrorMessage);
    }
}
