using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JSdotNet.MCP.Shared.Abstractions;
using JSdotNet.MCP.Shared.Infrastructure;

namespace JSdotNet.MCP.Guidelines.Tests;

/// <summary>
/// Integration tests that exercise the real defaults/ document catalog, filtered the way the
/// guidelines server filters it.
/// </summary>
public sealed class GuidesCatalogTests
{
    [Fact]
    public async Task ListDocuments_ReturnsItems()
    {
        IDocumentCatalog catalog = CategoryFilteredDocumentCatalog.WithoutDesign(new FileSystemDocumentCatalog());
        var docs = await catalog.ListDocumentsAsync();
        Assert.NotEmpty(docs);
        Assert.Contains(docs, d => d.Id == "0001-platform-and-build");
    }

    [Fact]
    public async Task GetContent_ReturnsMarkdown()
    {
        IDocumentCatalog catalog = CategoryFilteredDocumentCatalog.WithoutDesign(new FileSystemDocumentCatalog());
        var id = (await catalog.ListDocumentsAsync()).First().Id;
        var content = await catalog.GetContentAsync(id, TestContext.Current.CancellationToken);
        Assert.False(string.IsNullOrWhiteSpace(content));
        Assert.Contains("#", content);
    }

    [Fact]
    public async Task Search_FindsExpected()
    {
        IDocumentCatalog catalog = CategoryFilteredDocumentCatalog.WithoutDesign(new FileSystemDocumentCatalog());
        var results = await catalog.SearchAsync("central package management");
        Assert.Contains(results, r => r.Id == "0001-platform-and-build");
    }

    [Fact]
    public async Task Search_CaseInsensitive_FindsResults()
    {
        IDocumentCatalog catalog = CategoryFilteredDocumentCatalog.WithoutDesign(new FileSystemDocumentCatalog());
        var results = await catalog.SearchAsync("CENTRAL PACKAGE MANAGEMENT");
        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task Search_ByTag_FindsExpected()
    {
        IDocumentCatalog catalog = CategoryFilteredDocumentCatalog.WithoutDesign(new FileSystemDocumentCatalog());
        var results = await catalog.SearchByTagAsync("dotnet");
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Id == "0001-platform-and-build");
    }

    [Fact]
    public async Task ListDocuments_ReturnsSortedByCategory()
    {
        IDocumentCatalog catalog = CategoryFilteredDocumentCatalog.WithoutDesign(new FileSystemDocumentCatalog());
        var docs = await catalog.ListDocumentsAsync();
        var categories = docs.Select(d => d.Category).ToList();
        Assert.Equal(categories.OrderBy(c => c).ToList(), categories);
    }

    [Fact]
    public async Task ListDocuments_AllDocumentsHaveValidIds()
    {
        IDocumentCatalog catalog = CategoryFilteredDocumentCatalog.WithoutDesign(new FileSystemDocumentCatalog());
        foreach (var doc in await catalog.ListDocumentsAsync())
        {
            Assert.False(string.IsNullOrWhiteSpace(doc.Id));
            Assert.False(string.IsNullOrWhiteSpace(doc.Title));
            Assert.False(string.IsNullOrWhiteSpace(doc.RelativePath));
        }
    }

    [Fact]
    public async Task GetContent_MultipleDocuments_AllReadable()
    {
        IDocumentCatalog catalog = CategoryFilteredDocumentCatalog.WithoutDesign(new FileSystemDocumentCatalog());
        foreach (var doc in (await catalog.ListDocumentsAsync()).Take(3))
        {
            var content = await catalog.GetContentAsync(doc.Id, TestContext.Current.CancellationToken);
            Assert.False(string.IsNullOrWhiteSpace(content));
        }
    }

    [Fact]
    public async Task CopilotInstructionGuide_IsDiscoverableAndReadable()
    {
        IDocumentCatalog catalog = CategoryFilteredDocumentCatalog.WithoutDesign(new FileSystemDocumentCatalog());

        var doc = Assert.Single(await catalog.ListDocumentsAsync(), d => d.Id == "copilot-instruction-file-setup");
        Assert.Equal("ai/copilot-instruction-file-setup.md", doc.RelativePath.Replace('\\', '/'));

        var content = await catalog.GetContentAsync(doc.Id, TestContext.Current.CancellationToken);
        Assert.Contains("## AGENTS.md: the One Root File", content);
        Assert.Contains("## Host Wrappers", content);
        Assert.Contains("## Path-Scoped Rules", content);
        Assert.Contains("## Tool and MCP Selection Policy", content);
        Assert.Contains("@AGENTS.md", content);
        Assert.Contains(".agents/rules/<topic>.md", content);
        Assert.Contains("jsdotnet-project-guidelines-mcpserver", content);
    }

    [Fact]
    public async Task DevbookAdoptionGuide_IsDiscoverableAndReadable()
    {
        IDocumentCatalog catalog = CategoryFilteredDocumentCatalog.WithoutDesign(new FileSystemDocumentCatalog());

        var doc = Assert.Single(await catalog.ListDocumentsAsync(), d => d.Id == "devbook-adoption");
        Assert.Equal("arc42/concepts/devbook-adoption.md", doc.RelativePath.Replace('\\', '/'));

        var content = await catalog.GetContentAsync(doc.Id, TestContext.Current.CancellationToken);
        Assert.Contains("claude plugin marketplace add JSdotNet/devbook", content);
        Assert.Contains("devbook:init", content);
        Assert.Contains("devbook:update", content);
        Assert.Contains("devbook:validate", content);
        Assert.Contains("## The Optional Step Up", content);
    }

    [Fact]
    public void GuideIndex_ExistsAndIsValid()
    {
        var repoRoot = FindRepoRoot();
        Assert.NotNull(repoRoot);

        var indexPath = Path.Combine(repoRoot!, "defaults", "index.json");
        Assert.True(File.Exists(indexPath), "defaults/index.json must exist");

        var indexData = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(indexPath));
        Assert.True(indexData.TryGetProperty("version", out _));
        Assert.True(indexData.TryGetProperty("generated", out _));
        Assert.True(indexData.TryGetProperty("documents", out var docs));
        Assert.True(docs.GetArrayLength() > 0);
    }

    [Fact]
    public async Task ListDocuments_ExcludesDesignDocuments()
    {
        IDocumentCatalog catalog = CategoryFilteredDocumentCatalog.WithoutDesign(new FileSystemDocumentCatalog());
        var docs = await catalog.ListDocumentsAsync();
        Assert.DoesNotContain(docs, d => d.Category == "design");
    }

    [Fact]
    public async Task IndexEntries_AllPointAtExistingFiles()
    {
        var catalog = new FileSystemDocumentCatalog(Path.Combine(FindRepoRoot()!, "defaults"));
        foreach (var doc in await catalog.ListDocumentsAsync())
        {
            var content = await catalog.GetContentAsync(doc.Id, TestContext.Current.CancellationToken);
            Assert.False(string.IsNullOrWhiteSpace(content), doc.RelativePath);
        }
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
