using System.IO;
using System.Linq;
using System.Text.Json;
using JSdotNet.MCP.Shared.Infrastructure;
using Xunit;

namespace JSdotNet.MCP.Design.Tests;

public sealed class DesignServerTests
{
    [Fact]
    public async Task DesignCatalog_LoadsDesignDocs()
    {
        var designRoot = FindFolder("design");
        Assert.NotNull(designRoot);

        var catalog = new FileSystemDocumentCatalog(designRoot!);
        var docs = await catalog.ListDocumentsAsync();

        Assert.NotEmpty(docs);
        Assert.Contains(docs, d => d.Id.Contains("color-palette") || d.Id.Contains("typography") || d.Id.Contains("spacing"));
    }

    [Fact]
    public async Task DesignCatalog_SearchReturnsResults()
    {
        var designRoot = FindFolder("design");
        Assert.NotNull(designRoot);

        var catalog = new FileSystemDocumentCatalog(designRoot!);
        var results = await catalog.SearchAsync("color");

        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task DesignCatalog_GetContent_ReturnsMarkdown()
    {
        var designRoot = FindFolder("design");
        Assert.NotNull(designRoot);

        var catalog = new FileSystemDocumentCatalog(designRoot!);
        var doc = (await catalog.ListDocumentsAsync()).First();
        var content = await catalog.GetContentAsync(doc.Id, TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrWhiteSpace(content));
        Assert.Contains("#", content);
    }

    [Fact]
    public void DesignIndex_ExistsAndIsValid()
    {
        var repoRoot = FindFolder("design", returnParent: true);
        Assert.NotNull(repoRoot);

        var indexPath = Path.Combine(repoRoot!, "design", "index.json");
        Assert.True(File.Exists(indexPath), "design/index.json must exist");

        var indexData = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(indexPath));
        Assert.True(indexData.TryGetProperty("version", out _));
        Assert.True(indexData.TryGetProperty("documents", out var docs));
        Assert.True(docs.GetArrayLength() > 0);
    }

    private static string? FindFolder(string name, bool returnParent = false)
    {
        var current = AppContext.BaseDirectory;
        while (current != null)
        {
            var candidate = Path.Combine(current, name);
            if (Directory.Exists(candidate))
                return returnParent ? current : candidate;
            current = Directory.GetParent(current)?.FullName;
        }
        return null;
    }
}
