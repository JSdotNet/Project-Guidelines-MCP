using JSdotNet.MCP.Publish.Publishing;

namespace JSdotNet.MCP.Publish.Tests;

public sealed class FileResultPublisherTests
{
    [Fact]
    public async Task Publish_CreatesFileAndNestedFolders()
    {
        using var temp = new TempPublishRoot();

        var result = await temp.Publisher.PublishAsync("reports/2026/summary.md", "# Hello", overwrite: false, TestContext.Current.CancellationToken);

        Assert.Equal("reports/2026/summary.md", result.RelativePath);
        Assert.True(File.Exists(result.FullPath));
        Assert.Equal("# Hello", await File.ReadAllTextAsync(result.FullPath, TestContext.Current.CancellationToken));
        Assert.True(result.SizeBytes > 0);
    }

    [Fact]
    public async Task Publish_WithoutOverwrite_FailsWhenFileExists()
    {
        using var temp = new TempPublishRoot();
        await temp.Publisher.PublishAsync("result.txt", "first", overwrite: false, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<IOException>(() =>
            temp.Publisher.PublishAsync("result.txt", "second", overwrite: false, TestContext.Current.CancellationToken));

        Assert.Equal("first", await temp.Publisher.ReadAsync("result.txt", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Publish_WithOverwrite_ReplacesContent()
    {
        using var temp = new TempPublishRoot();
        await temp.Publisher.PublishAsync("result.txt", "first", overwrite: false, TestContext.Current.CancellationToken);

        await temp.Publisher.PublishAsync("result.txt", "second", overwrite: true, TestContext.Current.CancellationToken);

        Assert.Equal("second", await temp.Publisher.ReadAsync("result.txt", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Publish_WithAllowOverwriteByDefault_ReplacesContent()
    {
        using var temp = new TempPublishRoot(allowOverwriteByDefault: true);
        await temp.Publisher.PublishAsync("result.txt", "first", overwrite: false, TestContext.Current.CancellationToken);

        await temp.Publisher.PublishAsync("result.txt", "second", overwrite: false, TestContext.Current.CancellationToken);

        Assert.Equal("second", await temp.Publisher.ReadAsync("result.txt", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Append_CreatesThenAppends()
    {
        using var temp = new TempPublishRoot();

        await temp.Publisher.AppendAsync("log/run.md", "line1\n", TestContext.Current.CancellationToken);
        await temp.Publisher.AppendAsync("log/run.md", "line2\n", TestContext.Current.CancellationToken);

        Assert.Equal("line1\nline2\n", await temp.Publisher.ReadAsync("log/run.md", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task List_ReturnsFilesRecursivelyAndFiltersByPattern()
    {
        using var temp = new TempPublishRoot();
        await temp.Publisher.PublishAsync("a.md", "a", overwrite: false, TestContext.Current.CancellationToken);
        await temp.Publisher.PublishAsync("nested/b.md", "b", overwrite: false, TestContext.Current.CancellationToken);
        await temp.Publisher.PublishAsync("nested/c.json", "{}", overwrite: false, TestContext.Current.CancellationToken);

        var all = await temp.Publisher.ListAsync(null, TestContext.Current.CancellationToken);
        var markdown = await temp.Publisher.ListAsync("*.md", TestContext.Current.CancellationToken);

        Assert.Equal(3, all.Count);
        Assert.Equal(2, markdown.Count);
        Assert.Contains(markdown, r => r.RelativePath == "nested/b.md");
    }

    [Fact]
    public async Task List_OnEmptyRoot_ReturnsEmpty()
    {
        using var temp = new TempPublishRoot();

        var results = await temp.Publisher.ListAsync(null, TestContext.Current.CancellationToken);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Delete_RemovesFileAndReportsMissingFile()
    {
        using var temp = new TempPublishRoot();
        await temp.Publisher.PublishAsync("gone.txt", "x", overwrite: false, TestContext.Current.CancellationToken);

        Assert.True(await temp.Publisher.DeleteAsync("gone.txt", TestContext.Current.CancellationToken));
        Assert.False(await temp.Publisher.DeleteAsync("gone.txt", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Read_MissingFile_Throws()
    {
        using var temp = new TempPublishRoot();

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            temp.Publisher.ReadAsync("nope.txt", TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("../escape.txt")]
    [InlineData("nested/../../escape.txt")]
    [InlineData("/etc/passwd")]
    [InlineData(@"C:\Windows\system.ini")]
    [InlineData(@"\Windows\system.ini")]
    public async Task Publish_RejectsUnsafePaths(string relativePath)
    {
        using var temp = new TempPublishRoot();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            temp.Publisher.PublishAsync(relativePath, "x", overwrite: true, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task List_RejectsUnsafePattern()
    {
        using var temp = new TempPublishRoot();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            temp.Publisher.ListAsync("../*.md", TestContext.Current.CancellationToken));
    }

    [Fact]
    public void RootPath_IsCreatedOnConstruction()
    {
        using var temp = new TempPublishRoot();

        Assert.True(Directory.Exists(temp.Publisher.RootPath));
        Assert.Equal(Path.GetFullPath(temp.RootPath), temp.Publisher.RootPath);
    }
}
