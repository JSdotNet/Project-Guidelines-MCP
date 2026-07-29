using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JSdotNet.MCP.Shared.Infrastructure;
using Microsoft.Extensions.Caching.Memory;

namespace JSdotNet.MCP.Shared.Tests;

/// <summary>
/// Unit tests for GitHubDocumentCatalog verifying that the MemoryCache 30-minute TTL
/// actually controls refresh behaviour now that the Lazy&lt;Task&lt;&gt;&gt; wrapper is removed.
/// </summary>
public sealed class GitHubDocumentCatalogTests
{
    // Minimal valid index.json that the catalog can parse
    private const string ValidIndexJson = """
        {
          "version": "1.0",
          "generated": "2025-01-01T00:00:00Z",
          "documents": [
            {
              "id": "test-doc",
              "title": "Test Document",
              "description": "A test doc",
              "category": "adrs",
              "relativePath": "adrs/test-doc.md",
              "tags": ["testing"]
            }
          ]
        }
        """;

    private static GitHubDocumentCatalog BuildCatalog(
        CountingHandler handler,
        IMemoryCache cache,
        TimeSpan? cacheDuration = null)
    {
        var httpClient = new HttpClient(handler);
        return new GitHubDocumentCatalog(
            cache,
            owner: "TestOwner",
            repo: "TestRepo",
            branch: "main",
            httpClient: httpClient,
            documentsPath: "guide",
            cacheDuration: cacheDuration);
    }

    [Fact]
    public async Task ListDocuments_SecondCall_HitsCacheNotGitHub()
    {
        // Each call to LoadDocumentsAsync fetches index.json from GitHub.
        // The second call must be served from the MemoryCache — no extra HTTP request.

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new CountingHandler(_ => HttpOk(ValidIndexJson));
        await using var catalog = BuildCatalog(handler, cache);

        var first = catalog.ListDocuments();
        var second = catalog.ListDocuments();

        Assert.Single(first);
        Assert.Single(second);
        Assert.Equal(1, handler.CallCount); // Only one HTTP request
    }

    [Fact]
    public async Task ListDocuments_AfterCacheExpiry_RefetchesFromGitHub()
    {
        // After the TTL expires the catalog must fetch GitHub again.
        // Use a 50ms cache duration so the test can wait just 100ms.

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new CountingHandler(_ => HttpOk(ValidIndexJson));
        await using var catalog = BuildCatalog(handler, cache, cacheDuration: TimeSpan.FromMilliseconds(50));

        catalog.ListDocuments(); // warm the cache — 1 HTTP call

        await Task.Delay(100, TestContext.Current.CancellationToken); // let the entry expire

        catalog.ListDocuments(); // cache miss — should trigger 2nd HTTP call

        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task ListDocuments_AfterFailedFirstFetch_RetriesToGitHub()
    {
        // Issue #32 regression guard: when the first fetch fails, the failure must
        // NOT be permanently cached. The next call should attempt GitHub again.
        //
        // The catalog's LoadDocumentsAsync: catches index.json failure and falls back
        // to directory traversal, which also throws on 503 — so the first call raises.
        // Without Lazy<> the MemoryCache holds no entry, so the second call retries.

        using var cache = new MemoryCache(new MemoryCacheOptions());
        int invocation = 0;
        var handler = new CountingHandler(_ =>
        {
            invocation++;
            // First two calls fail (index.json + directory traversal GET)
            if (invocation <= 2)
                return HttpError(HttpStatusCode.ServiceUnavailable);
            // Third call (index.json retry) succeeds
            return HttpOk(ValidIndexJson);
        });
        await using var catalog = BuildCatalog(handler, cache);

        // First call: both index.json (503, caught) and directory traversal (503) throw → ListDocuments raises
        var ex = Record.Exception(() => catalog.ListDocuments());
        Assert.NotNull(ex); // expected to throw — failure is NOT cached

        // Second call: GitHub is healthy; catalog retries (no permanent lock from Lazy)
        var docs = catalog.ListDocuments();
        Assert.NotEmpty(docs);
    }

    [Fact]
    public async Task SearchByTag_AfterTransientFailure_RecoversOnNextCall()
    {
        // Same 2-failure + 1-success pattern: SearchByTag also calls LoadDocumentsAsync.
        using var cache = new MemoryCache(new MemoryCacheOptions());
        int invocation = 0;
        var handler = new CountingHandler(_ =>
        {
            invocation++;
            return invocation <= 2
                ? HttpError(HttpStatusCode.ServiceUnavailable)
                : HttpOk(ValidIndexJson);
        });
        await using var catalog = BuildCatalog(handler, cache);

        var ex = Record.Exception(() => catalog.SearchByTag("testing"));
        Assert.NotNull(ex);

        var results = catalog.SearchByTag("testing");
        Assert.Single(results);
    }

    [Fact]
    public async Task GetContentAsync_AfterTransientFailure_RecoversOnNextCall()
    {
        // GetContentAsync calls LoadDocumentsAsync then fetches the raw content.
        // First two calls (index.json + traversal) fail; third (index.json retry)
        // and fourth (content fetch) succeed.
        using var cache = new MemoryCache(new MemoryCacheOptions());
        int invocation = 0;
        var handler = new CountingHandler(_ =>
        {
            invocation++;
            return invocation switch
            {
                <= 2 => HttpError(HttpStatusCode.ServiceUnavailable),
                3    => HttpOk(ValidIndexJson),
                _    => HttpOk("# Test Document content")
            };
        });
        await using var catalog = BuildCatalog(handler, cache);

        var ex = await Record.ExceptionAsync(
            async () => await catalog.GetContentAsync("test-doc", TestContext.Current.CancellationToken));
        Assert.NotNull(ex);

        var content = await catalog.GetContentAsync("test-doc", TestContext.Current.CancellationToken);
        Assert.Contains("Test Document", content);
    }

    // ---- Helpers ----

    private static Task<HttpResponseMessage> HttpOk(string json)
        => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

    private static Task<HttpResponseMessage> HttpError(HttpStatusCode statusCode)
        => Task.FromResult(new HttpResponseMessage(statusCode));

    /// <summary>Counts how many times SendAsync is called and delegates to a factory.</summary>
    private sealed class CountingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _factory;
        public int CallCount;

        public CountingHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> factory)
            => _factory = factory;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref CallCount);
            return await _factory(request).ConfigureAwait(false);
        }
    }
}
