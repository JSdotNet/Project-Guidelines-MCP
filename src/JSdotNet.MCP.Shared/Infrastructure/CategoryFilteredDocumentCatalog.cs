using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JSdotNet.MCP.Shared.Abstractions;

namespace JSdotNet.MCP.Shared.Infrastructure;

/// <summary>
/// Serves the part of a catalog whose category passes a filter. Both servers read the one
/// <c>defaults/index.json</c>: the design server keeps the <c>design</c> category, the
/// guidelines server keeps everything else.
/// </summary>
public sealed class CategoryFilteredDocumentCatalog(IDocumentCatalog inner, Func<string, bool> includeCategory) : IDocumentCatalog
{
    /// <summary>The category of the UX style guide under <c>defaults/design/</c>.</summary>
    public const string DesignCategory = "design";

    /// <summary>Keeps only the design documents.</summary>
    public static CategoryFilteredDocumentCatalog DesignOnly(IDocumentCatalog inner)
        => new(inner, IsDesign);

    /// <summary>Keeps every document except the design documents.</summary>
    public static CategoryFilteredDocumentCatalog WithoutDesign(IDocumentCatalog inner)
        => new(inner, category => !IsDesign(category));

    private static bool IsDesign(string category)
        => string.Equals(category.Split('/', '\\')[0], DesignCategory, StringComparison.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<DocumentInfo>> ListDocumentsAsync(CancellationToken cancellationToken = default)
        => Filter(await inner.ListDocumentsAsync(cancellationToken).ConfigureAwait(false));

    public async Task<IReadOnlyList<DocumentInfo>> SearchAsync(string query, CancellationToken cancellationToken = default)
        => Filter(await inner.SearchAsync(query, cancellationToken).ConfigureAwait(false));

    public async Task<IReadOnlyList<DocumentInfo>> SearchByTagAsync(string tag, CancellationToken cancellationToken = default)
        => Filter(await inner.SearchByTagAsync(tag, cancellationToken).ConfigureAwait(false));

    public async Task<string> GetContentAsync(string id, CancellationToken cancellationToken = default)
    {
        var all = await inner.ListDocumentsAsync(cancellationToken).ConfigureAwait(false);
        var doc = all.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (doc is null || !includeCategory(doc.Category))
        {
            throw new FileNotFoundException($"Document '{id}' not found");
        }

        return await inner.GetContentAsync(id, cancellationToken).ConfigureAwait(false);
    }

    private IReadOnlyList<DocumentInfo> Filter(IReadOnlyList<DocumentInfo> docs)
        => docs.Where(d => includeCategory(d.Category)).ToList();
}
