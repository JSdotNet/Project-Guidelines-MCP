using System.IO;
using System.Threading.Tasks;
using JSdotNet.MCP.Shared.Abstractions;
using JSdotNet.MCP.Shared.Infrastructure;
using Xunit;

namespace JSdotNet.MCP.Shared.Tests;

public sealed class CategoryFilteredDocumentCatalogTests
{
    private static readonly DocumentInfo Adr = new("0001-adr", "ADR", "", "adrs", "arc42/adr/0001-adr.md", ["dotnet"]);
    private static readonly DocumentInfo Color = new("01-color", "Color", "", "design", "design/01-color.md", ["dotnet"]);

    private static FakeDocumentCatalog Inner() => new([Adr, Color], searchResults: [Adr, Color], tagResults: [Adr, Color]);

    [Fact]
    public async Task DesignOnly_KeepsOnlyDesignCategory()
    {
        var catalog = CategoryFilteredDocumentCatalog.DesignOnly(Inner());

        Assert.Equal([Color], await catalog.ListDocumentsAsync(TestContext.Current.CancellationToken));
        Assert.Equal([Color], await catalog.SearchAsync("x", TestContext.Current.CancellationToken));
        Assert.Equal([Color], await catalog.SearchByTagAsync("dotnet", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WithoutDesign_DropsDesignCategory()
    {
        var catalog = CategoryFilteredDocumentCatalog.WithoutDesign(Inner());

        Assert.Equal([Adr], await catalog.ListDocumentsAsync(TestContext.Current.CancellationToken));
        Assert.Equal([Adr], await catalog.SearchAsync("x", TestContext.Current.CancellationToken));
        Assert.Equal([Adr], await catalog.SearchByTagAsync("dotnet", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetContent_ForIncludedDocument_ReturnsInnerContent()
    {
        var catalog = CategoryFilteredDocumentCatalog.WithoutDesign(Inner());

        Assert.Equal("# 0001-ADR", await catalog.GetContentAsync("0001-ADR", TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("01-color")]
    [InlineData("unknown")]
    public async Task GetContent_ForExcludedOrUnknownDocument_Throws(string id)
    {
        var catalog = CategoryFilteredDocumentCatalog.WithoutDesign(Inner());

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => catalog.GetContentAsync(id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DesignOnly_MatchesNestedDesignCategory()
    {
        var nested = new DocumentInfo("icon", "Icon", "", "design/assets", "design/assets/icon.md", []);
        var catalog = CategoryFilteredDocumentCatalog.DesignOnly(new FakeDocumentCatalog([nested, Adr]));

        Assert.Equal([nested], await catalog.ListDocumentsAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ResolveDocumentsPath_WithoutValue_ReturnsNull(string? configured)
        => Assert.Null(FileSystemDocumentCatalog.ResolveDocumentsPath(configured));

    [Fact]
    public void ResolveDocumentsPath_WithMissingFolder_ReturnsNull()
        => Assert.Null(FileSystemDocumentCatalog.ResolveDocumentsPath(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())));

    [Fact]
    public void ResolveDocumentsPath_WithRepositoryRoot_ReturnsNestedDefaults()
    {
        var root = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var defaults = Directory.CreateDirectory(Path.Combine(root, "defaults")).FullName;

            Assert.Equal(defaults, FileSystemDocumentCatalog.ResolveDocumentsPath(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ResolveDocumentsPath_WithDefaultsFolder_ReturnsItUnchanged()
    {
        var root = Directory.CreateTempSubdirectory().FullName;
        try
        {
            File.WriteAllText(Path.Combine(root, "index.json"), "{}");
            Directory.CreateDirectory(Path.Combine(root, "defaults"));

            Assert.Equal(root, FileSystemDocumentCatalog.ResolveDocumentsPath(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
