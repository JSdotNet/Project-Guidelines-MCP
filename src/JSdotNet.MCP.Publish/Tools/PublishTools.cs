using System.ComponentModel;
using System.Text.Json;
using JSdotNet.MCP.Publish.Publishing;
using JSdotNet.MCP.Shared.Logging;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace JSdotNet.MCP.Publish.Tools;

/// <summary>MCP tools that publish results to the configured file location.</summary>
[McpServerToolType]
public sealed class PublishTools(IResultPublisher publisher, IUsageLog usageLog, ILogger<PublishTools> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    [McpServerTool(Name = "publish_result"), Description("Writes a result to a file inside the configured publish location. Supply a path relative to that location (subfolders are created automatically, e.g. 'reports/2026-08/summary.md') and the full text content. Fails when the file already exists unless overwrite is true. Returns the relative path, absolute path, size and last-modified timestamp of the written file.")]
    public async Task<string> PublishResultAsync(
        [Description("File path relative to the configured publish location, e.g. 'reports/summary.md'. Absolute paths and '..' segments are rejected.")] string relativePath,
        [Description("Full text content to write to the file")] string content,
        [Description("Replace the file when it already exists (default false)")] bool overwrite = false,
        CancellationToken ct = default)
    {
        var succeeded = false;
        string? errorMessage = null;
        try
        {
            var result = await publisher.PublishAsync(relativePath, content, overwrite, ct);
            succeeded = true;
            return JsonSerializer.Serialize(result, JsonOptions);
        }
        catch (Exception ex) when (Capture(ex, out errorMessage))
        {
            logger.LogError(ex, "Error in publish_result for {RelativePath}", relativePath);
            throw;
        }
        finally
        {
            await TryRecordAsync(
                "publish_result",
                new() { ["relativePath"] = relativePath, ["overwrite"] = overwrite.ToString(), ["contentLength"] = content.Length.ToString() },
                succeeded ? [relativePath] : [],
                succeeded,
                errorMessage,
                ct);
        }
    }

    [McpServerTool(Name = "append_result"), Description("Appends text to a file inside the configured publish location, creating the file and any missing folders when needed. Use this to accumulate incremental output such as run logs or streaming reports. Returns the relative path, absolute path, size and last-modified timestamp of the file.")]
    public async Task<string> AppendResultAsync(
        [Description("File path relative to the configured publish location, e.g. 'reports/run-log.md'. Absolute paths and '..' segments are rejected.")] string relativePath,
        [Description("Text to append to the file")] string content,
        CancellationToken ct = default)
    {
        var succeeded = false;
        string? errorMessage = null;
        try
        {
            var result = await publisher.AppendAsync(relativePath, content, ct);
            succeeded = true;
            return JsonSerializer.Serialize(result, JsonOptions);
        }
        catch (Exception ex) when (Capture(ex, out errorMessage))
        {
            logger.LogError(ex, "Error in append_result for {RelativePath}", relativePath);
            throw;
        }
        finally
        {
            await TryRecordAsync(
                "append_result",
                new() { ["relativePath"] = relativePath, ["contentLength"] = content.Length.ToString() },
                succeeded ? [relativePath] : [],
                succeeded,
                errorMessage,
                ct);
        }
    }

    [McpServerTool(Name = "list_published"), Description("Lists the files that were published to the configured location, newest first. Optionally filter with a file-name glob such as '*.md' or 'summary-*.json'; the search always recurses into subfolders. Also reports the configured publish root so you can tell the user where results were written.")]
    public async Task<string> ListPublishedAsync(
        [Description("Optional file-name glob filter, e.g. '*.md'. Defaults to all files.")] string? pattern = null,
        CancellationToken ct = default)
    {
        string[] ids = [];
        var succeeded = false;
        string? errorMessage = null;
        try
        {
            var results = await publisher.ListAsync(pattern, ct);
            ids = results.Select(r => r.RelativePath).ToArray();
            succeeded = true;
            return JsonSerializer.Serialize(new { rootPath = publisher.RootPath, files = results }, JsonOptions);
        }
        catch (Exception ex) when (Capture(ex, out errorMessage))
        {
            logger.LogError(ex, "Error in list_published for pattern {Pattern}", pattern);
            throw;
        }
        finally
        {
            await TryRecordAsync("list_published", new() { ["pattern"] = pattern ?? string.Empty }, ids, succeeded, errorMessage, ct);
        }
    }

    [McpServerTool(Name = "read_published"), Description("Reads back the full text content of a previously published file. Use this to verify what was written or to update a result based on its current content.")]
    public async Task<string> ReadPublishedAsync(
        [Description("File path relative to the configured publish location")] string relativePath,
        CancellationToken ct = default)
    {
        var succeeded = false;
        string? errorMessage = null;
        try
        {
            var content = await publisher.ReadAsync(relativePath, ct);
            succeeded = true;
            return content;
        }
        catch (Exception ex) when (Capture(ex, out errorMessage))
        {
            logger.LogError(ex, "Error in read_published for {RelativePath}", relativePath);
            throw;
        }
        finally
        {
            await TryRecordAsync("read_published", new() { ["relativePath"] = relativePath }, succeeded ? [relativePath] : [], succeeded, errorMessage, ct);
        }
    }

    [McpServerTool(Name = "delete_published"), Description("Deletes a previously published file from the configured publish location. Returns whether a file was actually removed; deleting a non-existent file is not an error.")]
    public async Task<string> DeletePublishedAsync(
        [Description("File path relative to the configured publish location")] string relativePath,
        CancellationToken ct = default)
    {
        var succeeded = false;
        string? errorMessage = null;
        try
        {
            var deleted = await publisher.DeleteAsync(relativePath, ct);
            succeeded = true;
            return JsonSerializer.Serialize(new { relativePath, deleted }, JsonOptions);
        }
        catch (Exception ex) when (Capture(ex, out errorMessage))
        {
            logger.LogError(ex, "Error in delete_published for {RelativePath}", relativePath);
            throw;
        }
        finally
        {
            await TryRecordAsync("delete_published", new() { ["relativePath"] = relativePath }, succeeded ? [relativePath] : [], succeeded, errorMessage, ct);
        }
    }

    [McpServerTool(Name = "get_publish_location"), Description("Returns the absolute directory this server publishes results to, as resolved from configuration (the 'Publish:RootPath' setting, the JSDOTNET_PUBLISH_PATH environment variable, or the per-user default). Call this when you need to tell the user exactly where results will be stored.")]
    public async Task<string> GetPublishLocationAsync(CancellationToken ct = default)
    {
        await TryRecordAsync("get_publish_location", [], [], true, null, ct);
        return JsonSerializer.Serialize(new { rootPath = publisher.RootPath }, JsonOptions);
    }

    private async ValueTask TryRecordAsync(
        string toolName,
        Dictionary<string, string> parameters,
        string[] resultIds,
        bool succeeded,
        string? errorMessage,
        CancellationToken ct)
    {
        try
        {
            await usageLog.RecordAsync(new UsageLogEntry(
                DateTimeOffset.UtcNow,
                toolName,
                parameters,
                resultIds,
                resultIds.Length,
                succeeded,
                errorMessage), ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Failed to write usage log entry for {ToolName}", toolName);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError(ex, "Failed to write usage log entry for {ToolName}", toolName);
        }
    }

    // Exception filter that always returns false: captures the message without swallowing the exception.
    private static bool Capture(Exception ex, out string? message)
    {
        message = ex.Message;
        return false;
    }
}
