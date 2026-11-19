using JSdotNet.MCP.Shared.Abstractions;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

namespace JSdotNet.MCP.Shared.Resources;

/// <summary>
/// Serves every catalog document as an MCP resource at
/// <c>defaults://&lt;relative path without .md&gt;</c>, e.g. <c>defaults://arc42/adr/0003-aspire</c>.
/// The resource name is the document id, so every response stays traceable to its source.
/// </summary>
public static class DefaultsResources
{
    /// <summary>The URI scheme prefix of every defaults resource.</summary>
    public const string Scheme = "defaults://";

    /// <summary>The MIME type every document is served as.</summary>
    public const string MarkdownMimeType = "text/markdown";

    /// <summary>The resource URI of a document.</summary>
    public static string ToUri(DocumentInfo document)
    {
        var path = document.RelativePath.Replace('\\', '/').TrimStart('/');
        if (path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            path = path[..^3];
        }

        return Scheme + path;
    }

    /// <summary>Lists every document as a resource, cacheable for <paramref name="timeToLive"/>.</summary>
    public static async ValueTask<ListResourcesResult> ListAsync(
        IDocumentCatalog catalog, TimeSpan timeToLive, CancellationToken cancellationToken = default)
    {
        var docs = await catalog.ListDocumentsAsync(cancellationToken).ConfigureAwait(false);
        return new ListResourcesResult
        {
            Resources = [.. docs.Select(ToResource)],
            TimeToLive = timeToLive,
        };
    }

    /// <summary>Reads the document a <c>defaults://</c> URI names.</summary>
    /// <exception cref="McpProtocolException">No document has that URI.</exception>
    public static async ValueTask<ReadResourceResult> ReadAsync(
        IDocumentCatalog catalog, string? uri, TimeSpan timeToLive, CancellationToken cancellationToken = default)
    {
        var docs = await catalog.ListDocumentsAsync(cancellationToken).ConfigureAwait(false);
        var doc = docs.FirstOrDefault(d => string.Equals(ToUri(d), uri, StringComparison.OrdinalIgnoreCase))
            ?? throw new McpProtocolException($"Resource '{uri}' not found", McpErrorCode.ResourceNotFound);

        var content = await catalog.GetContentAsync(doc.Id, cancellationToken).ConfigureAwait(false);
        return new ReadResourceResult
        {
            Contents = [new TextResourceContents { Uri = ToUri(doc), MimeType = MarkdownMimeType, Text = content }],
            TimeToLive = timeToLive,
        };
    }

    private static Resource ToResource(DocumentInfo doc) => new()
    {
        Uri = ToUri(doc),
        Name = doc.Id,
        Title = doc.Title,
        Description = doc.Description,
        MimeType = MarkdownMimeType,
    };
}
