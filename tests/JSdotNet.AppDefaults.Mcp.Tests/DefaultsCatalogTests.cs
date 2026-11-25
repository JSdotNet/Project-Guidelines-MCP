using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JSdotNet.MCP.Shared.Abstractions;
using JSdotNet.MCP.Shared.Infrastructure;

namespace JSdotNet.AppDefaults.Mcp.Tests;

/// <summary>
/// Integration tests that exercise the real defaults/ document catalog the server serves.
/// </summary>
public sealed class DefaultsCatalogTests
{
    [Fact]
    public async Task ListDocuments_ReturnsItems()
    {
        IDocumentCatalog catalog = new FileSystemDocumentCatalog();
        var docs = await catalog.ListDocumentsAsync(TestContext.Current.CancellationToken);
        Assert.NotEmpty(docs);
        Assert.Contains(docs, d => d.Id == "0001-platform-and-build");
    }

    [Fact]
    public async Task GetContent_ReturnsMarkdown()
    {
        IDocumentCatalog catalog = new FileSystemDocumentCatalog();
        var id = (await catalog.ListDocumentsAsync(TestContext.Current.CancellationToken)).First().Id;
        var content = await catalog.GetContentAsync(id, TestContext.Current.CancellationToken);
        Assert.False(string.IsNullOrWhiteSpace(content));
        Assert.Contains("#", content);
    }

    [Fact]
    public async Task Search_FindsExpected()
    {
        IDocumentCatalog catalog = new FileSystemDocumentCatalog();
        var results = await catalog.SearchAsync("central package management", TestContext.Current.CancellationToken);
        Assert.Contains(results, r => r.Id == "0001-platform-and-build");
    }

    [Fact]
    public async Task Search_CaseInsensitive_FindsResults()
    {
        IDocumentCatalog catalog = new FileSystemDocumentCatalog();
        var results = await catalog.SearchAsync("CENTRAL PACKAGE MANAGEMENT", TestContext.Current.CancellationToken);
        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task Search_ByTag_FindsExpected()
    {
        IDocumentCatalog catalog = new FileSystemDocumentCatalog();
        var results = await catalog.SearchByTagAsync("dotnet", TestContext.Current.CancellationToken);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Id == "0001-platform-and-build");
    }

    [Fact]
    public async Task ListDocuments_ReturnsSortedByCategory()
    {
        IDocumentCatalog catalog = new FileSystemDocumentCatalog();
        var docs = await catalog.ListDocumentsAsync(TestContext.Current.CancellationToken);
        var categories = docs.Select(d => d.Category).ToList();
        Assert.Equal(categories.OrderBy(c => c).ToList(), categories);
    }

    [Fact]
    public async Task ListDocuments_AllDocumentsHaveValidIds()
    {
        IDocumentCatalog catalog = new FileSystemDocumentCatalog();
        foreach (var doc in await catalog.ListDocumentsAsync(TestContext.Current.CancellationToken))
        {
            Assert.False(string.IsNullOrWhiteSpace(doc.Id));
            Assert.False(string.IsNullOrWhiteSpace(doc.Title));
            Assert.False(string.IsNullOrWhiteSpace(doc.RelativePath));
        }
    }

    [Fact]
    public async Task GetContent_MultipleDocuments_AllReadable()
    {
        IDocumentCatalog catalog = new FileSystemDocumentCatalog();
        foreach (var doc in (await catalog.ListDocumentsAsync(TestContext.Current.CancellationToken)).Take(3))
        {
            var content = await catalog.GetContentAsync(doc.Id, TestContext.Current.CancellationToken);
            Assert.False(string.IsNullOrWhiteSpace(content));
        }
    }

    [Fact]
    public async Task CopilotInstructionGuide_IsDiscoverableAndReadable()
    {
        IDocumentCatalog catalog = new FileSystemDocumentCatalog();

        var doc = Assert.Single(await catalog.ListDocumentsAsync(TestContext.Current.CancellationToken), d => d.Id == "copilot-instruction-file-setup");
        Assert.Equal("ai/copilot-instruction-file-setup.md", doc.RelativePath.Replace('\\', '/'));

        var content = await catalog.GetContentAsync(doc.Id, TestContext.Current.CancellationToken);
        Assert.Contains("## AGENTS.md: the One Root File", content);
        Assert.Contains("## Host Wrappers", content);
        Assert.Contains("## Path-Scoped Rules", content);
        Assert.Contains("## Tool and MCP Selection Policy", content);
        Assert.Contains("@AGENTS.md", content);
        Assert.Contains(".agents/rules/<topic>.md", content);
        Assert.Contains("jsdotnet-app-defaults", content);
        Assert.DoesNotContain("jsdotnet-project-guidelines", content);
    }

    [Fact]
    public async Task CrosscuttingConcepts_IsDiscoverableAndHoldsEveryConcern()
    {
        IDocumentCatalog catalog = new FileSystemDocumentCatalog();

        var doc = Assert.Single(await catalog.ListDocumentsAsync(TestContext.Current.CancellationToken), d => d.Id == "08-crosscutting-concepts");
        Assert.Equal("arc42/08-crosscutting-concepts.md", doc.RelativePath.Replace('\\', '/'));

        var content = await catalog.GetContentAsync(doc.Id, TestContext.Current.CancellationToken);
        foreach (var section in new[]
        {
            "## Caching", "## Feature flags", "## Idempotency", "## API versioning",
            "## Logging and audit", "## Background jobs", "## Testing",
            "## Code style and domain code", "## Time", "## Concurrency",
            "## Real-time updates", "## Secrets at rest",
        })
        {
            Assert.Contains(section, content);
        }
    }

    [Fact]
    public async Task Recommendations_FoldedIntoChapters_AreNoLongerListed()
    {
        IDocumentCatalog catalog = new FileSystemDocumentCatalog();

        var docs = await catalog.ListDocumentsAsync(TestContext.Current.CancellationToken);

        Assert.DoesNotContain(docs, d => d.RelativePath.Replace('\\', '/').StartsWith("arc42/concepts/", StringComparison.Ordinal));
        Assert.Single(docs, d => d.Id == "07-deployment-view");
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
    public async Task ListDocuments_IncludesDesignCategory()
    {
        IDocumentCatalog catalog = new FileSystemDocumentCatalog();
        var docs = await catalog.ListDocumentsAsync(TestContext.Current.CancellationToken);
        Assert.Contains(docs, d => d.Category == "design");
        Assert.Contains(docs, d => d.Category == "adrs");
    }

    [Fact]
    public async Task IndexEntries_AllPointAtExistingFiles()
    {
        var catalog = new FileSystemDocumentCatalog(Path.Combine(FindRepoRoot()!, "defaults"));
        foreach (var doc in await catalog.ListDocumentsAsync(TestContext.Current.CancellationToken))
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
            if (File.Exists(Path.Combine(dir.FullName, "JSdotNet.AppDefaults.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }
}
