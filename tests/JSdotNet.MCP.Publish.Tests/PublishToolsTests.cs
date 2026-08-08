using System.Text.Json;
using JSdotNet.MCP.Publish.Tools;
using JSdotNet.MCP.Shared.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace JSdotNet.MCP.Publish.Tests;

public sealed class PublishToolsTests
{
    private sealed class RecordingUsageLog : IUsageLog
    {
        public List<UsageLogEntry> Entries { get; } = [];

        public ValueTask RecordAsync(UsageLogEntry entry, CancellationToken ct = default)
        {
            Entries.Add(entry);
            return ValueTask.CompletedTask;
        }

        public Task<IReadOnlyList<UsageLogEntry>> GetRecentAsync(int count, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<UsageLogEntry>>(Entries.TakeLast(count).ToList());
    }

    private static (PublishTools Tools, RecordingUsageLog Log) CreateTools(TempPublishRoot temp)
    {
        var log = new RecordingUsageLog();
        return (new PublishTools(temp.Publisher, log, NullLogger<PublishTools>.Instance), log);
    }

    [Fact]
    public async Task PublishResult_WritesFileAndRecordsUsage()
    {
        using var temp = new TempPublishRoot();
        var (tools, log) = CreateTools(temp);

        var json = await tools.PublishResultAsync("reports/summary.md", "# Result", overwrite: false, TestContext.Current.CancellationToken);

        var element = JsonSerializer.Deserialize<JsonElement>(json);
        Assert.Equal("reports/summary.md", element.GetProperty("relativePath").GetString());
        Assert.Equal("# Result", await temp.Publisher.ReadAsync("reports/summary.md", TestContext.Current.CancellationToken));
        Assert.Contains(log.Entries, e => e.ToolName == "publish_result" && e.Succeeded);
    }

    [Fact]
    public async Task PublishResult_OnFailure_RecordsFailedUsage()
    {
        using var temp = new TempPublishRoot();
        var (tools, log) = CreateTools(temp);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            tools.PublishResultAsync("../escape.md", "x", overwrite: true, TestContext.Current.CancellationToken));

        Assert.Contains(log.Entries, e => e.ToolName == "publish_result" && !e.Succeeded && e.ErrorMessage is not null);
    }

    [Fact]
    public async Task AppendResult_AppendsContent()
    {
        using var temp = new TempPublishRoot();
        var (tools, _) = CreateTools(temp);

        await tools.AppendResultAsync("log.txt", "a", TestContext.Current.CancellationToken);
        await tools.AppendResultAsync("log.txt", "b", TestContext.Current.CancellationToken);

        Assert.Equal("ab", await temp.Publisher.ReadAsync("log.txt", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListPublished_ReturnsRootPathAndFiles()
    {
        using var temp = new TempPublishRoot();
        var (tools, _) = CreateTools(temp);
        await tools.PublishResultAsync("one.md", "1", overwrite: false, TestContext.Current.CancellationToken);

        var json = await tools.ListPublishedAsync("*.md", TestContext.Current.CancellationToken);

        var element = JsonSerializer.Deserialize<JsonElement>(json);
        Assert.Equal(temp.Publisher.RootPath, element.GetProperty("rootPath").GetString());
        Assert.Equal(1, element.GetProperty("files").GetArrayLength());
    }

    [Fact]
    public async Task ReadPublished_ReturnsContent()
    {
        using var temp = new TempPublishRoot();
        var (tools, _) = CreateTools(temp);
        await tools.PublishResultAsync("one.md", "content", overwrite: false, TestContext.Current.CancellationToken);

        var content = await tools.ReadPublishedAsync("one.md", TestContext.Current.CancellationToken);

        Assert.Equal("content", content);
    }

    [Fact]
    public async Task DeletePublished_ReportsDeletion()
    {
        using var temp = new TempPublishRoot();
        var (tools, _) = CreateTools(temp);
        await tools.PublishResultAsync("one.md", "content", overwrite: false, TestContext.Current.CancellationToken);

        var deleted = JsonSerializer.Deserialize<JsonElement>(
            await tools.DeletePublishedAsync("one.md", TestContext.Current.CancellationToken));
        var again = JsonSerializer.Deserialize<JsonElement>(
            await tools.DeletePublishedAsync("one.md", TestContext.Current.CancellationToken));

        Assert.True(deleted.GetProperty("deleted").GetBoolean());
        Assert.False(again.GetProperty("deleted").GetBoolean());
    }

    [Fact]
    public async Task GetPublishLocation_ReturnsConfiguredRoot()
    {
        using var temp = new TempPublishRoot();
        var (tools, _) = CreateTools(temp);

        var json = await tools.GetPublishLocationAsync(TestContext.Current.CancellationToken);

        var element = JsonSerializer.Deserialize<JsonElement>(json);
        Assert.Equal(temp.Publisher.RootPath, element.GetProperty("rootPath").GetString());
    }
}
