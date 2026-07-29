using System;
using System.IO;
using System.Threading.Tasks;
using JSdotNet.MCP.Shared.Abstractions;
using JSdotNet.MCP.Shared.Infrastructure;
using Xunit;

namespace JSdotNet.MCP.Shared.Tests;

/// <summary>
/// Pure unit tests for FileSystemDocumentCatalog and DocumentInfo.
/// Uses temp directories or non-real paths — no dependency on guide/ or design/.
/// Integration tests against real doc folders live in Guidelines.Tests / Design.Tests.
/// </summary>
public class DocumentCatalogTests
{
    // --- Search guard-clause tests ---

    [Fact]
    public async Task Search_WithEmptyQuery_ReturnsEmpty()
    {
        var catalog = new FileSystemDocumentCatalog();
        Assert.Empty(await catalog.SearchAsync(""));
    }

    [Fact]
    public async Task Search_WithWhitespaceQuery_ReturnsEmpty()
    {
        var catalog = new FileSystemDocumentCatalog();
        Assert.Empty(await catalog.SearchAsync("   "));
    }

    [Fact]
    public async Task Search_WithNullQuery_ReturnsEmpty()
    {
        var catalog = new FileSystemDocumentCatalog();
        Assert.Empty(await catalog.SearchAsync(null!));
    }

    // --- Non-existent root ---

    [Fact]
    public async Task FileSystemCatalog_WithNonExistentRoot_ReturnsEmpty()
    {
        var catalog = new FileSystemDocumentCatalog("C:\\NonExistentPath\\Docs");
        Assert.Empty(await catalog.ListDocumentsAsync());
    }

    // --- GetContent with unknown id ---

    [Fact]
    public async Task GetContent_WithInvalidId_ThrowsFileNotFoundException()
    {
        var catalog = new FileSystemDocumentCatalog();
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await catalog.GetContentAsync("non-existent-id", TestContext.Current.CancellationToken));
    }

    // --- DocumentInfo record ---

    [Fact]
    public void DocumentInfo_RecordEquality_Works()
    {
        var doc1 = new DocumentInfo("test-id", "Test Title", string.Empty, "category", "path/to/file.md", Array.Empty<string>());
        var doc2 = new DocumentInfo("test-id", "Test Title", string.Empty, "category", "path/to/file.md", Array.Empty<string>());
        var doc3 = new DocumentInfo("other-id", "Test Title", string.Empty, "category", "path/to/file.md", Array.Empty<string>());

        Assert.Equal(doc1, doc2);
        Assert.NotEqual(doc1, doc3);
    }

    [Fact]
    public void DocumentInfo_ToString_ContainsId()
    {
        var doc = new DocumentInfo("test-id", "Test Title", string.Empty, "category", "path/to/file.md", new[] { "tag-a" });
        Assert.Contains("test-id", doc.ToString());
    }

    // --- Front-matter parsing (isolated via temp dirs) ---

    [Fact]
    public async Task FileSystemCatalog_ParsesFrontMatterMultilineTags()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "doc.md"),
                "---\ntitle: \"Multiline Tags Doc\"\ntags:\n- alpha\n- beta\n- gamma\n---\n# Body");

            var catalog = new FileSystemDocumentCatalog(tempDir);
            var docs = await catalog.ListDocumentsAsync();

            Assert.Single(docs);
            Assert.Equal("Multiline Tags Doc", docs[0].Title);
            Assert.Contains("alpha", docs[0].Tags);
            Assert.Contains("beta", docs[0].Tags);
            Assert.Contains("gamma", docs[0].Tags);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task FileSystemCatalog_FrontMatterNoEndMarker_FallsBackToH1Title()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "doc.md"),
                "---\ntitle: \"Should Be Ignored\"\n\n# Real H1 Title\n\nContent.");

            var catalog = new FileSystemDocumentCatalog(tempDir);
            var docs = await catalog.ListDocumentsAsync();

            Assert.Single(docs);
            Assert.Equal("Real H1 Title", docs[0].Title);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}

