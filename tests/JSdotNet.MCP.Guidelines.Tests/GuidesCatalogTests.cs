using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JSdotNet.MCP.Shared.Infrastructure;

namespace JSdotNet.MCP.Guidelines.Tests;

/// <summary>
/// Integration tests that exercise the real guide/ document catalog.
/// </summary>
public sealed class GuidesCatalogTests
{
    [Fact]
    public void ListDocuments_ReturnsItems()
    {
        var catalog = new FileSystemDocumentCatalog();
        var docs = catalog.ListDocuments();
        Assert.NotEmpty(docs);
        Assert.Contains(docs, d => d.Id.Contains("adopt-dotnet"));
    }

    [Fact]
    public async Task GetContent_ReturnsMarkdown()
    {
        var catalog = new FileSystemDocumentCatalog();
        var id = catalog.ListDocuments().First().Id;
        var content = await catalog.GetContentAsync(id, TestContext.Current.CancellationToken);
        Assert.False(string.IsNullOrWhiteSpace(content));
        Assert.Contains("#", content);
    }

    [Fact]
    public void Search_FindsExpected()
    {
        var catalog = new FileSystemDocumentCatalog();
        var results = catalog.Search("target framework");
        Assert.Contains(results, r => r.Id.Contains("0001"));
    }

    [Fact]
    public void Search_CaseInsensitive_FindsResults()
    {
        var catalog = new FileSystemDocumentCatalog();
        var results = catalog.Search("TARGET FRAMEWORK");
        Assert.NotEmpty(results);
    }

    [Fact]
    public void Search_ByTag_FindsExpected()
    {
        var catalog = new FileSystemDocumentCatalog();
        var results = catalog.SearchByTag("dotnet");
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Id.Contains("adopt-dotnet"));
    }

    [Fact]
    public void ListDocuments_ReturnsSortedByCategory()
    {
        var catalog = new FileSystemDocumentCatalog();
        var docs = catalog.ListDocuments();
        var categories = docs.Select(d => d.Category).ToList();
        Assert.Equal(categories.OrderBy(c => c).ToList(), categories);
    }

    [Fact]
    public void ListDocuments_AllDocumentsHaveValidIds()
    {
        var catalog = new FileSystemDocumentCatalog();
        foreach (var doc in catalog.ListDocuments())
        {
            Assert.False(string.IsNullOrWhiteSpace(doc.Id));
            Assert.False(string.IsNullOrWhiteSpace(doc.Title));
            Assert.False(string.IsNullOrWhiteSpace(doc.RelativePath));
        }
    }

    [Fact]
    public async Task GetContent_MultipleDocuments_AllReadable()
    {
        var catalog = new FileSystemDocumentCatalog();
        foreach (var doc in catalog.ListDocuments().Take(3))
        {
            var content = await catalog.GetContentAsync(doc.Id, TestContext.Current.CancellationToken);
            Assert.False(string.IsNullOrWhiteSpace(content));
        }
    }

    [Fact]
    public void GuideIndex_ExistsAndIsValid()
    {
        var repoRoot = FindRepoRoot();
        Assert.NotNull(repoRoot);

        var indexPath = Path.Combine(repoRoot!, "guide", "index.json");
        Assert.True(File.Exists(indexPath), "guide/index.json must exist");

        var indexData = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(indexPath));
        Assert.True(indexData.TryGetProperty("version", out _));
        Assert.True(indexData.TryGetProperty("generated", out _));
        Assert.True(indexData.TryGetProperty("documents", out var docs));
        Assert.True(docs.GetArrayLength() > 0);
    }

    private static string? FindRepoRoot()
    {
        var dir = new System.IO.DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "JSdotNet.MCP.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }
}
