using System.IO;
using System.Linq;
using System.Text.Json;
using JSdotNet.MCP.Shared.Abstractions;
using JSdotNet.MCP.Shared.Infrastructure;
using Xunit;

namespace JSdotNet.MCP.Design.Tests;

public sealed class DesignServerTests
{
    [Fact]
    public async Task DesignCatalog_LoadsDesignDocs()
    {
        var catalog = DesignCatalog();
        var docs = await catalog.ListDocumentsAsync();

        Assert.NotEmpty(docs);
        Assert.Contains(docs, d => d.Id.Contains("color-palette") || d.Id.Contains("typography") || d.Id.Contains("spacing"));
    }

    [Fact]
    public async Task DesignCatalog_SearchReturnsResults()
    {
        var catalog = DesignCatalog();
        var results = await catalog.SearchAsync("color");

        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task DesignCatalog_GetContent_ReturnsMarkdown()
    {
        var catalog = DesignCatalog();
        var doc = (await catalog.ListDocumentsAsync()).First();
        var content = await catalog.GetContentAsync(doc.Id, TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrWhiteSpace(content));
        Assert.Contains("#", content);
    }

    [Fact]
    public void DesignIndex_ExistsAndIsValid()
    {
        var defaultsRoot = FindFolder("defaults");
        Assert.NotNull(defaultsRoot);

        var indexPath = Path.Combine(defaultsRoot!, "index.json");
        Assert.True(File.Exists(indexPath), "defaults/index.json must exist");

        var indexData = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(indexPath));
        Assert.True(indexData.TryGetProperty("version", out _));
        Assert.True(indexData.TryGetProperty("documents", out var docs));
        var design = docs.EnumerateArray()
            .Where(d => d.GetProperty("category").GetString() == "design")
            .ToList();
        Assert.NotEmpty(design);
        Assert.All(design, d => Assert.StartsWith("design/", d.GetProperty("relativePath").GetString()));
    }

    [Fact]
    public async Task DesignCatalog_ServesOnlyDesignDocuments()
    {
        var docs = await DesignCatalog().ListDocumentsAsync();

        Assert.All(docs, d => Assert.Equal("design", d.Category));
        Assert.DoesNotContain(docs, d => d.Id == "0001-platform-and-build");
    }

    private static IDocumentCatalog DesignCatalog()
    {
        var defaultsRoot = FindFolder("defaults");
        Assert.NotNull(defaultsRoot);
        return CategoryFilteredDocumentCatalog.DesignOnly(new FileSystemDocumentCatalog(defaultsRoot!));
    }

    private static string? FindFolder(string name)
    {
        var current = AppContext.BaseDirectory;
        while (current != null)
        {
            var candidate = Path.Combine(current, name);
            if (Directory.Exists(candidate))
                return candidate;
            current = Directory.GetParent(current)?.FullName;
        }
        return null;
    }
}
