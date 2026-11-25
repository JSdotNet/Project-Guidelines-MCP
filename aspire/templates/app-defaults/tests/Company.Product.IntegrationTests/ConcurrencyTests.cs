using System.Net;
using System.Net.Http.Json;
using Company.Product.Abstractions.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Company.Product.IntegrationTests;

/// <summary>The concurrency token of ADR 0006 against a real database: a stale write is a 409.</summary>
public sealed class ConcurrencyTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Rename_WithTheVersionRead_SucceedsAndAStaleRetryIsA409ProblemDetails()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        var created = await CreateAsync(client, cancellationToken);

        using var renamed = await client.PutAsJsonAsync(
            new Uri($"/modulename/items/{created.Id}", UriKind.Relative),
            new RenameItemRequest("First rename", created.Version),
            cancellationToken);
        using var stale = await client.PutAsJsonAsync(
            new Uri($"/modulename/items/{created.Id}", UriKind.Relative),
            new RenameItemRequest("Second rename", created.Version),
            cancellationToken);

        Assert.Equal(HttpStatusCode.OK, renamed.StatusCode);
        Assert.Equal(created.Version + 1, (await renamed.Content.ReadFromJsonAsync<ItemDto>(cancellationToken))!.Version);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("application/problem+json", stale.Content.Headers.ContentType?.MediaType);
        Assert.Equal(409, (await stale.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken))!.Status);
    }

    [Fact]
    public async Task Save_AfterAnotherWriteChangedTheRow_ReportsTheConflict()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        var created = await CreateAsync(client, cancellationToken);
        using var first = factory.Services.CreateScope();
        using var second = factory.Services.CreateScope();
        var firstItems = first.ServiceProvider.GetRequiredService<IItemRepository>();
        var secondItems = second.ServiceProvider.GetRequiredService<IItemRepository>();
        var firstRead = (await firstItems.GetByIdAsync(created.Id, cancellationToken))!;
        var secondRead = (await secondItems.GetByIdAsync(created.Id, cancellationToken))!;

        firstRead.Rename("First writer", created.Version);
        secondRead.Rename("Second writer", created.Version);

        Assert.True(await firstItems.SaveAsync(firstRead, cancellationToken));
        Assert.False(await secondItems.SaveAsync(secondRead, cancellationToken));
    }

    private static async Task<ItemDto> CreateAsync(HttpClient client, CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync(
            new Uri("/modulename/items", UriKind.Relative), new CreateItemRequest("Item"), cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ItemDto>(cancellationToken))!;
    }
}
