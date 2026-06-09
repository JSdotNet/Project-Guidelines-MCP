using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JSdotNet.Project.Guidelines.McpServer.Logging;
using Xunit;

namespace JSdotNet.Project.Guidelines.McpServer.Tests;

public sealed class UsageLogTests : IDisposable
{
    private readonly string _tempDir;

    public UsageLogTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"usage-log-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string TempFile(string name = "usage.jsonl") => Path.Combine(_tempDir, name);

    private static UsageLogEntry MakeEntry(string tool, bool succeeded = true, string? error = null)
        => new(DateTimeOffset.UtcNow, tool, new Dictionary<string, string> { ["q"] = "test" },
               ["doc-1", "doc-2"], 2, succeeded, error);

    // --- Construction ---

    [Fact]
    public void Constructor_CreatesDirectoryIfMissing()
    {
        var nested = Path.Combine(_tempDir, "sub", "dir", "usage.jsonl");
        _ = new JsonFileUsageLog(nested);
        Assert.True(Directory.Exists(Path.GetDirectoryName(nested)));
    }

    // --- RecordAsync ---

    [Fact]
    public async Task RecordAsync_WritesEntryToFile()
    {
        var sut = new JsonFileUsageLog(TempFile());
        await sut.RecordAsync(MakeEntry("list_docs"), TestContext.Current.CancellationToken);

        Assert.True(File.Exists(TempFile()));
        var lines = await File.ReadAllLinesAsync(TempFile(), TestContext.Current.CancellationToken);
        Assert.Single(lines.Where(l => !string.IsNullOrWhiteSpace(l)));
    }

    [Fact]
    public async Task RecordAsync_MultipleEntries_AppendsEachOnOwnLine()
    {
        var sut = new JsonFileUsageLog(TempFile());
        await sut.RecordAsync(MakeEntry("list_docs"), TestContext.Current.CancellationToken);
        await sut.RecordAsync(MakeEntry("search_docs"), TestContext.Current.CancellationToken);
        await sut.RecordAsync(MakeEntry("get_doc"), TestContext.Current.CancellationToken);

        var lines = (await File.ReadAllLinesAsync(TempFile(), TestContext.Current.CancellationToken))
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();
        Assert.Equal(3, lines.Count);
    }

    [Fact]
    public async Task RecordAsync_CapturesFailedEntry()
    {
        var sut = new JsonFileUsageLog(TempFile());
        var entry = MakeEntry("get_doc", succeeded: false, error: "Document 'x' not found");
        await sut.RecordAsync(entry, TestContext.Current.CancellationToken);

        var lines = await File.ReadAllLinesAsync(TempFile(), TestContext.Current.CancellationToken);
        Assert.Contains("not found", lines.First(l => !string.IsNullOrWhiteSpace(l)));
    }

    // --- GetRecentAsync ---

    [Fact]
    public async Task GetRecentAsync_FileNotExist_ReturnsEmpty()
    {
        var sut = new JsonFileUsageLog(TempFile("nonexistent.jsonl"));
        var result = await sut.GetRecentAsync(10, TestContext.Current.CancellationToken);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsRoundTrippedEntries()
    {
        var sut = new JsonFileUsageLog(TempFile());
        var original = MakeEntry("search_docs");
        await sut.RecordAsync(original, TestContext.Current.CancellationToken);

        var results = await sut.GetRecentAsync(10, TestContext.Current.CancellationToken);

        Assert.Single(results);
        var r = results[0];
        Assert.Equal("search_docs", r.ToolName);
        Assert.Equal(2, r.ResultCount);
        Assert.True(r.Succeeded);
        Assert.Equal(original.Parameters["q"], r.Parameters["q"]);
    }

    [Fact]
    public async Task GetRecentAsync_RespectsCountLimit()
    {
        var sut = new JsonFileUsageLog(TempFile());
        for (var i = 0; i < 10; i++)
            await sut.RecordAsync(MakeEntry($"tool-{i}"), TestContext.Current.CancellationToken);

        var results = await sut.GetRecentAsync(3, TestContext.Current.CancellationToken);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsInChronologicalOrder()
    {
        var sut = new JsonFileUsageLog(TempFile());
        await sut.RecordAsync(MakeEntry("first"), TestContext.Current.CancellationToken);
        await sut.RecordAsync(MakeEntry("second"), TestContext.Current.CancellationToken);
        await sut.RecordAsync(MakeEntry("third"), TestContext.Current.CancellationToken);

        var results = await sut.GetRecentAsync(10, TestContext.Current.CancellationToken);
        Assert.Equal(["first", "second", "third"], results.Select(r => r.ToolName).ToArray());
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsLastN_WhenMoreExist()
    {
        var sut = new JsonFileUsageLog(TempFile());
        for (var i = 0; i < 5; i++)
            await sut.RecordAsync(MakeEntry($"tool-{i}"), TestContext.Current.CancellationToken);

        var results = await sut.GetRecentAsync(2, TestContext.Current.CancellationToken);
        Assert.Equal(["tool-3", "tool-4"], results.Select(r => r.ToolName).ToArray());
    }

    [Fact]
    public async Task GetRecentAsync_SkipsMalformedLines()
    {
        var path = TempFile();
        await File.WriteAllTextAsync(path,
            "{\"toolName\":\"valid\",\"timestamp\":\"2025-01-01T00:00:00+00:00\",\"parameters\":{},\"resultDocumentIds\":[],\"resultCount\":0,\"succeeded\":true,\"errorMessage\":null}\n" +
            "INVALID JSON LINE\n" +
            "{\"toolName\":\"also-valid\",\"timestamp\":\"2025-01-01T00:01:00+00:00\",\"parameters\":{},\"resultDocumentIds\":[],\"resultCount\":0,\"succeeded\":true,\"errorMessage\":null}\n",
            TestContext.Current.CancellationToken);

        var sut = new JsonFileUsageLog(path);
        var results = await sut.GetRecentAsync(10, TestContext.Current.CancellationToken);

        Assert.Equal(2, results.Count);
        Assert.Equal(["valid", "also-valid"], results.Select(r => r.ToolName).ToArray());
    }

    // --- Concurrency ---

    [Fact]
    public async Task RecordAsync_ConcurrentWrites_AllEntriesRecorded()
    {
        var sut = new JsonFileUsageLog(TempFile());
        const int taskCount = 20;
        var tasks = Enumerable.Range(0, taskCount)
            .Select(i => sut.RecordAsync(MakeEntry($"tool-{i}"), TestContext.Current.CancellationToken).AsTask());
        await Task.WhenAll(tasks);

        var results = await sut.GetRecentAsync(taskCount, TestContext.Current.CancellationToken);
        Assert.Equal(taskCount, results.Count);
    }

    // --- FilePath ---

    [Fact]
    public void FilePath_ReturnsConfiguredPath()
    {
        var path = TempFile("my-log.jsonl");
        var sut = new JsonFileUsageLog(path);
        Assert.Equal(path, sut.FilePath);
    }
}
