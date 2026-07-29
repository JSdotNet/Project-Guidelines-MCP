using System.ComponentModel;
using System.Text.Json;
using JSdotNet.MCP.Shared.Abstractions;
using JSdotNet.MCP.Shared.Logging;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace JSdotNet.MCP.Shared.Tools;

[McpServerToolType]
public sealed class GuidesTools(IDocumentCatalog catalog, IUsageLog usageLog, ILogger<GuidesTools> logger)
{
    [McpServerTool(Name = "list_guides"), Description("Retrieves the complete catalog of all available documentation, including ADRs, designs, recommendations, structures, and config guidelines. Returns id, title, category, description, relative path, and tags for each document. Use this to get a full overview of available guidance before deciding which documents to read in detail.")]
    public async Task<string> ListGuidesAsync(CancellationToken ct)
    {
        string[] ids = [];
        var succeeded = false;
        string? errorMessage = null;
        try
        {
            var docs = (await catalog.ListDocumentsAsync(ct))
                .Select(d => new { d.Id, d.Title, d.Description, d.Category, d.RelativePath, d.Tags })
                .ToList();
            ids = docs.Select(d => d.Id).ToArray();
            succeeded = true;
            return JsonSerializer.Serialize(docs);
        }
        catch (Exception ex) when (Capture(ex, out errorMessage))
        {
            logger.LogError(ex, "Error in list_guides");
            throw;
        }
        finally
        {
            await TryRecordAsync("list_guides", [], ids, succeeded, errorMessage, ct);
        }
    }

    [McpServerTool(Name = "list_guides_by_type"), Description("Retrieves all documents belonging to a specific category. Valid categories are: 'adrs' (Architecture Decision Records with status, context, and consequences), 'designs' (exploratory design documents and diagrams), 'recommendations' (prescriptive best-practice guidance), 'structures' (canonical project scaffolds and templates), and 'config' (configuration file guidelines, e.g. github-app.yml). Use this when you know the type of guidance you need rather than searching by keyword.")]
    public async Task<string> ListGuidesByTypeAsync([Description("Category to filter by")] string category, CancellationToken ct)
    {
        string[] ids = [];
        var succeeded = false;
        string? errorMessage = null;
        try
        {
            var docs = (await catalog.ListDocumentsAsync(ct))
                .Where(d => string.Equals(d.Category.Split('/')?.FirstOrDefault() ?? string.Empty, category, StringComparison.OrdinalIgnoreCase))
                .Select(d => new { d.Id, d.Title, d.Description, d.Category, d.RelativePath, d.Tags })
                .ToList();
            ids = docs.Select(d => d.Id).ToArray();
            succeeded = true;
            return JsonSerializer.Serialize(docs);
        }
        catch (Exception ex) when (Capture(ex, out errorMessage))
        {
            logger.LogError(ex, "Error in list_guides_by_type for category {Category}", category);
            throw;
        }
        finally
        {
            await TryRecordAsync("list_guides_by_type", new() { ["category"] = category }, ids, succeeded, errorMessage, ct);
        }
    }

    [McpServerTool(Name = "search_guides"), Description("Searches the document catalog by matching a query against document titles, IDs, paths, and descriptions (case-insensitive). Returns all matching documents with full metadata. Use this when you have a keyword or concept in mind (e.g. 'logging', 'persistence', 'value object') and want to discover all relevant guidance without knowing exact document IDs or categories.")]
    public async Task<string> SearchGuidesAsync([Description("Search query")] string query, CancellationToken ct)
    {
        string[] ids = [];
        var succeeded = false;
        string? errorMessage = null;
        try
        {
            var docs = (await catalog.SearchAsync(query, ct))
                .Select(d => new { d.Id, d.Title, d.Description, d.Category, d.RelativePath, d.Tags })
                .ToList();
            ids = docs.Select(d => d.Id).ToArray();
            succeeded = true;
            return JsonSerializer.Serialize(docs);
        }
        catch (Exception ex) when (Capture(ex, out errorMessage))
        {
            logger.LogError(ex, "Error in search_guides for query {Query}", query);
            throw;
        }
        finally
        {
            await TryRecordAsync("search_guides", new() { ["query"] = query }, ids, succeeded, errorMessage, ct);
        }
    }

    [McpServerTool(Name = "search_guides_by_tag"), Description("Finds all documents annotated with a specific tag (e.g. 'persistence', 'resilience', 'domain', 'testing', 'cqrs'). Tag-based search is more precise than text search and maps directly to architectural concerns and cross-cutting topics. Use this when you need all guidance related to a specific architectural area or concern rather than a free-text keyword.")]
    public async Task<string> SearchGuidesByTagAsync([Description("Tag to filter by")] string tag, CancellationToken ct)
    {
        string[] ids = [];
        var succeeded = false;
        string? errorMessage = null;
        try
        {
            var docs = (await catalog.SearchByTagAsync(tag, ct))
                .Select(d => new { d.Id, d.Title, d.Description, d.Category, d.RelativePath, d.Tags })
                .ToList();
            ids = docs.Select(d => d.Id).ToArray();
            succeeded = true;
            return JsonSerializer.Serialize(docs);
        }
        catch (Exception ex) when (Capture(ex, out errorMessage))
        {
            logger.LogError(ex, "Error in search_guides_by_tag for tag {Tag}", tag);
            throw;
        }
        finally
        {
            await TryRecordAsync("search_guides_by_tag", new() { ["tag"] = tag }, ids, succeeded, errorMessage, ct);
        }
    }

    [McpServerTool(Name = "get_guide"), Description("Fetches the complete Markdown content of a specific document by its ID. Use this after listing or searching to read the full text of an ADR (including decision rationale and consequences), a recommendation, a design document, or a project structure template. Always call this before implementing anything that may be governed by an existing ADR or recommendation.")]
    public async Task<string> GetGuideAsync([Description("Document id")] string id, CancellationToken ct)
    {
        var succeeded = false;
        string? errorMessage = null;
        try
        {
            var content = await catalog.GetContentAsync(id, ct);
            succeeded = true;
            return content;
        }
        catch (Exception ex) when (Capture(ex, out errorMessage))
        {
            logger.LogError(ex, "Error in get_guide for id {Id}", id);
            throw;
        }
        finally
        {
            await TryRecordAsync("get_guide", new() { ["id"] = id }, succeeded ? [id] : [], succeeded, errorMessage, ct);
        }
    }

    private async ValueTask TryRecordAsync(
        string toolName,
        Dictionary<string, string> parameters,
        string[] resultDocumentIds,
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
                resultDocumentIds,
                resultDocumentIds.Length,
                succeeded,
                errorMessage), ct);
        }
        catch (Exception ex)
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
