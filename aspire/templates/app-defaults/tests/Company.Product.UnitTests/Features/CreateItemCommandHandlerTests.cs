using Bogus;
using Company.Product.Abstractions.DataTransferObjects;
using Company.Product.Core;
using Company.Product.DomainModels;
using Company.Product.Features.CreateItem;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Xunit;

namespace Company.Product.UnitTests.Features;

public sealed class CreateItemCommandHandlerTests
{
    private readonly IItemRepository _items = Substitute.For<IItemRepository>();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 11, 19, 12, 0, 0, TimeSpan.Zero));
    private readonly Faker _faker = new();

    [Fact]
    public async Task Handle_ValidName_StoresTheItemAndReturnsIt()
    {
        var name = _faker.Commerce.ProductName();
        var handler = new CreateItemCommandHandler(_items, _time);

        var result = await handler.Handle(new CreateItemCommand(name), TestContext.Current.CancellationToken);

        var ok = Assert.IsType<Ok<ItemDto>>(result.Value);
        Assert.Equal(name, ok.Value.Name);
        Assert.Equal(_time.GetUtcNow(), ok.Value.CreatedAt);
        await _items.Received(1).AddAsync(Arg.Is<Item>(item => item.Name == name), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BlankName_IsInvalidAndStoresNothing()
    {
        var handler = new CreateItemCommandHandler(_items, _time);

        var result = await handler.Handle(new CreateItemCommand(" "), TestContext.Current.CancellationToken);

        var invalid = Assert.IsType<Invalid>(result.Value);
        Assert.Contains(nameof(Item.Name), invalid.Errors.Keys);
        await _items.DidNotReceiveWithAnyArgs().AddAsync(default!, TestContext.Current.CancellationToken);
    }
}
