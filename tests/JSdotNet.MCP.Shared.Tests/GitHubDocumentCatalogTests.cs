using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JSdotNet.MCP.Shared.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace JSdotNet.MCP.Shared.Tests;

/// <summary>
/// Unit tests for GitHubDocumentCatalog verifying fault-recovery and cache-hit behaviour.
/// Uses a hand-rolled fake HttpMessageHandler — no mocking library required.
/// </summary>
public class GitHubDocumentCatalogTests
{
    // Minimal valid index.json served by the fake handler on success.
    private const string ValidIndexJson = """
        {
          "version": "1.0",
          "generated": "2025-01-01T00:00:00Z",
          "documents": [
            {
              "id": "sample-doc",
              "title": "Sample Doc",
              "description": "A sample document.",
              "category": "recommendations",
              "relativePath": "recommendations/sample-doc.md",
              "tags": ["sample"]
            }
          ]
        }
        """;

    private static IMemoryCache CreateCache() =>
        new MemoryCache(Options.Create(new MemoryCacheOptions()));

    private static GitHubDocumentCatalog CreateCatalog(HttpMessageHandler handler, IMemoryCache cache) =>
        new(cache, httpClient: new HttpClient(handler));

    [Fact]
    public async Task ListDocuments_AfterTransientFailure_RecoversOnNextCall()
    {
        // First ListDocuments() makes 2 HTTP calls:
        //   1. index.json fetch → caught internally, falls through to traversal
        //   2. traversal GET    → throws HttpRequestException that escapes to caller
        // Second ListDocuments() retries:
        //   3. index.json fetch → returns valid index, documents cached
        var handler = new SequentialFakeHandler(
        [
            _ => throw new HttpRequestException("fail (index.json)"),
            _ => throw new HttpRequestException("fail (traversal)"),
            _ => new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(ValidIndexJson) }
        ]);

        using var cache = CreateCache();
        await using var catalog = CreateCatalog(handler, cache);

        // Act: first call fails with HttpRequestException from the traversal path.
        Assert.Throws<HttpRequestException>(() => catalog.ListDocuments());

        // Act: second call succeeds — no permanent faulted state.
        var docs = catalog.ListDocuments();

        // Assert
        Assert.Single(docs);
        Assert.Equal("Sample Doc", docs[0].Title);
    }

    [Fact]
    public async Task ListDocuments_OnSecondCall_ReturnsCachedResultWithoutExtraHttp()
    {
        // Arrange: only one HTTP response; second call must come from cache.
        var callCount = 0;
        var handler = new CountingFakeHandler(
            _ =>
            {
                callCount++;
                return new HttpResponseMessage(HttpStatusCode.OK)
                    { Content = new StringContent(ValidIndexJson) };
            });

        using var cache = CreateCache();
        await using var catalog = CreateCatalog(handler, cache);

        // Act
        var first = catalog.ListDocuments();
        var second = catalog.ListDocuments();

        // Assert: HTTP was called exactly once; both results are the same list.
        Assert.Equal(1, callCount);
        Assert.Single(first);
        Assert.Same(first, second);
    }

    [Fact]
    public async Task SearchByTag_AfterTransientFailure_RecoversOnNextCall()
    {
        // Same 2-failure + 1-success pattern as ListDocuments recovery test.
        var handler = new SequentialFakeHandler(
        [
            _ => throw new HttpRequestException("fail (index.json)"),
            _ => throw new HttpRequestException("fail (traversal)"),
            _ => new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(ValidIndexJson) }
        ]);

        using var cache = CreateCache();
        await using var catalog = CreateCatalog(handler, cache);

        Assert.Throws<HttpRequestException>(() => catalog.SearchByTag("sample"));

        var results = catalog.SearchByTag("sample");
        Assert.Single(results);
    }

    [Fact]
    public async Task GetContentAsync_AfterTransientFailure_RecoversOnNextCall()
    {
        // First GetContentAsync() makes 2 HTTP calls (same as ListDocuments recovery):
        //   1. index.json fetch → caught
        //   2. traversal GET   → throws (escapes)
        // Second GetContentAsync() retries:
        //   3. index.json fetch → success
        //   4. content GET      → returns document body
        var handler = new SequentialFakeHandler(
        [
            _ => throw new HttpRequestException("fail (index.json)"),
            _ => throw new HttpRequestException("fail (traversal)"),
            _ => new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(ValidIndexJson) },
            _ => new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("# Sample Doc content") }
        ]);

        using var cache = CreateCache();
        await using var catalog = CreateCatalog(handler, cache);

        // First catalog load fails.
        await Assert.ThrowsAsync<HttpRequestException>(
            async () => await catalog.GetContentAsync("sample-doc", TestContext.Current.CancellationToken));

        // Second call: catalog loads successfully and content is fetched.
        var content = await catalog.GetContentAsync("sample-doc", TestContext.Current.CancellationToken);
        Assert.Equal("# Sample Doc content", content);
    }

    // ── Fake handlers ──────────────────────────────────────────────────────────

    /// <summary>Executes each delegate in sequence; throws if exhausted.</summary>
    private sealed class SequentialFakeHandler(Func<HttpRequestMessage, HttpResponseMessage>[] responses)
        : HttpMessageHandler
    {
        private int _index;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var i = _index++;
            if (i >= responses.Length) throw new InvalidOperationException("No more fake responses.");
            return Task.FromResult(responses[i](request));
        }
    }

    /// <summary>Delegates every call to a single factory; tracks invocation count.</summary>
    private sealed class CountingFakeHandler(Func<HttpRequestMessage, HttpResponseMessage> factory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(factory(request));
    }
}
