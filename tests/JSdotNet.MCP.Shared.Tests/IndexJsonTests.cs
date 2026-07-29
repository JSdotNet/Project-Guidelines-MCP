using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JSdotNet.MCP.Shared.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace JSdotNet.MCP.Shared.Tests;

public class IndexJsonTests
{
    [Fact]
    public async Task FileSystemCatalog_LoadsFromIndexJson_WhenAvailable()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            var indexPath = Path.Combine(tempDir, "index.json");
            var indexContent = @"{
                ""version"": ""1.0"",
                ""generated"": ""2025-11-19T00:00:00Z"",
                ""documents"": [
                    {
                        ""id"": ""test-doc"",
                        ""title"": ""Test Document"",
                        ""description"": ""A document about testing and verification"",
                        ""category"": ""test"",
                        ""relativePath"": ""test/test-doc.md"",
                        ""tags"": [""testing"", ""sample""]
                    }
                ]
            }";
            File.WriteAllText(indexPath, indexContent);

            // Act
            var catalog = new FileSystemDocumentCatalog(tempDir);
            var docs = await catalog.ListDocumentsAsync();

            // Assert
            Assert.Single(docs);
            Assert.Equal("test-doc", docs[0].Id);
            Assert.Equal("Test Document", docs[0].Title);
            Assert.Equal("A document about testing and verification", docs[0].Description);
            Assert.Contains("testing", docs[0].Tags);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task FileSystemCatalog_Search_MatchesDescription()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(Path.Combine(tempDir, "test"));

        try
        {
            var indexPath = Path.Combine(tempDir, "index.json");
            var indexContent = @"{
                ""version"": ""1.0"",
                ""generated"": ""2025-11-19T00:00:00Z"",
                ""documents"": [
                    {
                        ""id"": ""test-doc"",
                        ""title"": ""Some Document"",
                        ""description"": ""Uniqueterm that appears only in the description"",
                        ""category"": ""test"",
                        ""relativePath"": ""test/test-doc.md"",
                        ""tags"": [""testing""]
                    }
                ]
            }";
            File.WriteAllText(indexPath, indexContent);
            File.WriteAllText(Path.Combine(tempDir, "test", "test-doc.md"), "# Some Document\n\nContent without the unique term.");

            // Act
            var catalog = new FileSystemDocumentCatalog(tempDir);
            var results = await catalog.SearchAsync("Uniqueterm");

            // Assert
            Assert.NotEmpty(results);
            Assert.Equal("test-doc", results[0].Id);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task FileSystemCatalog_FallsBackToScanning_WhenIndexMissing()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var testDir = Path.Combine(tempDir, "test");
        Directory.CreateDirectory(testDir);

        try
        {
            var testFile = Path.Combine(testDir, "test-doc.md");
            File.WriteAllText(testFile, @"---
title: ""Fallback Test Document""
tags: [fallback, test]
---
# Fallback Test Document

Content here.
");

            // Act
            var catalog = new FileSystemDocumentCatalog(tempDir);
            var docs = await catalog.ListDocumentsAsync();

            // Assert
            Assert.Single(docs);
            Assert.Equal("test-doc", docs[0].Id);
            Assert.Equal("Fallback Test Document", docs[0].Title);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact(Skip = "Requires GitHub network access")]
    public async Task GitHubCatalog_LoadsFromIndexJson_WhenAvailable()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());

        // Act - This will attempt to fetch from GitHub
        var catalog = new GitHubDocumentCatalog(cache);
        var docs = await catalog.ListDocumentsAsync();

        // Assert - Should have loaded from index.json in the repo
        // ID format doesn't include leading zero padding
        Assert.NotEmpty(docs);
        Assert.Contains(docs, d => d.Id.Contains("use-index-json"));
    }

    [Fact(Skip = "Requires GitHub network access")]
    public async Task GitHubCatalog_CachesResults_WithSlidingExpiration()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var catalog = new GitHubDocumentCatalog(cache);

        // Act - First call
        var docs1 = await catalog.ListDocumentsAsync();

        // Act - Second call (should hit cache)
        var docs2 = await catalog.ListDocumentsAsync();

        // Assert
        Assert.NotEmpty(docs1);
        Assert.NotEmpty(docs2);
        Assert.Equal(docs1.Count, docs2.Count);
    }

    private static string? FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Join(dir.FullName, "JSdotNet.MCP.slnx")))
                return dir.FullName;

            dir = dir.Parent;
        }

        return null;
    }
}
