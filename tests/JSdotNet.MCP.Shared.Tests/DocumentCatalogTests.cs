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
    public void Search_WithEmptyQuery_ReturnsEmpty()
    {
        var catalog = new FileSystemDocumentCatalog();
        Assert.Empty(catalog.Search(""));
    }

    [Fact]
    public void Search_WithWhitespaceQuery_ReturnsEmpty()
    {
        var catalog = new FileSystemDocumentCatalog();
        Assert.Empty(catalog.Search("   "));
    }

    [Fact]
    public void Search_WithNullQuery_ReturnsEmpty()
    {
        var catalog = new FileSystemDocumentCatalog();
        Assert.Empty(catalog.Search(null!));
    }

    // --- Non-existent root ---

    [Fact]
    public void FileSystemCatalog_WithNonExistentRoot_ReturnsEmpty()
    {
        var catalog = new FileSystemDocumentCatalog("C:\\NonExistentPath\\Docs");
        Assert.Empty(catalog.ListDocuments());
    }

    // --- GetContent with unknown id ---

    [Fact]
    public async Task GetContent_WithInvalidId_ThrowsFileNotFoundException()
    {
        var catalog = new FileSystemDocumentCatalog();
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await catalog.GetContentAsync("non-existent-id", TestContext.Current.CancellationToken));
    }

    // --- Cancellation ---

    [Fact]
    public async Task GetContentAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "doc.md"), "# Hello\n\nContent.");

            var catalog = new FileSystemDocumentCatalog(tempDir);
            using var cts = new System.Threading.CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await catalog.GetContentAsync("doc", cts.Token));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
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
    public void FileSystemCatalog_ParsesFrontMatterMultilineTags()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "doc.md"),
                "---\ntitle: \"Multiline Tags Doc\"\ntags:\n- alpha\n- beta\n- gamma\n---\n# Body");

            var catalog = new FileSystemDocumentCatalog(tempDir);
            var docs = catalog.ListDocuments();

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
    public void FileSystemCatalog_FrontMatterNoEndMarker_FallsBackToH1Title()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "doc.md"),
                "---\ntitle: \"Should Be Ignored\"\n\n# Real H1 Title\n\nContent.");

            var catalog = new FileSystemDocumentCatalog(tempDir);
            var docs = catalog.ListDocuments();

            Assert.Single(docs);
            Assert.Equal("Real H1 Title", docs[0].Title);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}

