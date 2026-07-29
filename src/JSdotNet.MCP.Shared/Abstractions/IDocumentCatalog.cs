using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JSdotNet.MCP.Shared.Abstractions;

/// <summary>
/// Provides access to coding guidelines, standards, recommendations and ADRs.
/// </summary>
public interface IDocumentCatalog
{
    /// <summary>
    /// List all available documents with metadata.
    /// </summary>
    Task<IReadOnlyList<DocumentInfo>> ListDocumentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the raw Markdown content of a document by id.
    /// </summary>
    Task<string> GetContentAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Simple case-insensitive search over title and content.
    /// </summary>
    Task<IReadOnlyList<DocumentInfo>> SearchAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search documents by tag.
    /// </summary>
    Task<IReadOnlyList<DocumentInfo>> SearchByTagAsync(string tag, CancellationToken cancellationToken = default);
}

/// <summary>
/// Lightweight document metadata.
/// </summary>
public sealed record DocumentInfo(
    string Id,
    string Title,
    string Description,
    string Category,
    string RelativePath,
    IReadOnlyList<string> Tags
);
