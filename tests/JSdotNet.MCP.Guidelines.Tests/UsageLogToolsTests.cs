using System.Text.Json;
using JSdotNet.MCP.Shared.Logging;
using JSdotNet.MCP.Shared.Tools;

namespace JSdotNet.MCP.Guidelines.Tests;

public sealed class UsageLogToolsTests
{
    private static UsageLogEntry MakeEntry(string tool)
        => new(DateTimeOffset.UtcNow, tool, [], [], 0, true, null);

    [Fact]
    public async Task GetUsageLogsAsync_ReturnsSerialisedJsonArray()
    {
        var log = new FakeUsageLog();
        log.Entries.Add(MakeEntry("list_guides"));

        var result = await UsageLogTools.GetUsageLogsAsync(log, 10, CancellationToken.None);

        var entries = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Single(entries);
        Assert.Equal("list_guides", entries[0].GetProperty("toolName").GetString());
    }

    [Fact]
    public async Task GetUsageLogsAsync_EmptyLog_ReturnsEmptyArray()
    {
        var log = new FakeUsageLog();

        var result = await UsageLogTools.GetUsageLogsAsync(log, 10, CancellationToken.None);

        var entries = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Empty(entries);
    }

    [Fact]
    public async Task GetUsageLogsAsync_DefaultCount_Returns20Entries()
    {
        var log = new FakeUsageLog();
        for (var i = 0; i < 30; i++)
            log.Entries.Add(MakeEntry($"tool-{i}"));

        var result = await UsageLogTools.GetUsageLogsAsync(log, ct: TestContext.Current.CancellationToken);

        var entries = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Equal(20, entries.Length);
    }

    [Fact]
    public async Task GetUsageLogsAsync_CountBelowOne_ClampsTo1()
    {
        var log = new FakeUsageLog();
        for (var i = 0; i < 5; i++)
            log.Entries.Add(MakeEntry($"tool-{i}"));

        var result = await UsageLogTools.GetUsageLogsAsync(log, 0, CancellationToken.None);

        var entries = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Single(entries);
    }

    [Fact]
    public async Task GetUsageLogsAsync_CountAbove100_ClampsTo100()
    {
        var log = new FakeUsageLog();
        for (var i = 0; i < 5; i++)
            log.Entries.Add(MakeEntry($"tool-{i}"));

        var result = await UsageLogTools.GetUsageLogsAsync(log, 200, CancellationToken.None);

        var entries = JsonSerializer.Deserialize<JsonElement[]>(result)!;
        Assert.Equal(5, entries.Length);
    }

    [Fact]
    public async Task GetUsageLogsAsync_ReturnsJsonWithCamelCaseProperties()
    {
        var log = new FakeUsageLog();
        log.Entries.Add(MakeEntry("search_guides"));

        var result = await UsageLogTools.GetUsageLogsAsync(log, 10, CancellationToken.None);

        Assert.Contains("toolName", result);
        Assert.Contains("succeeded", result);
    }

    [Fact]
    public async Task GetUsageLogsAsync_ReturnsFormattedJson()
    {
        var log = new FakeUsageLog();
        log.Entries.Add(MakeEntry("get_guide"));

        var result = await UsageLogTools.GetUsageLogsAsync(log, 10, CancellationToken.None);

        // WriteIndented = true produces multi-line output
        Assert.Contains(Environment.NewLine, result);
    }
}
