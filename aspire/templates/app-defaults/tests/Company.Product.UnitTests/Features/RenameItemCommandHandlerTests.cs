using Company.Product.Abstractions.DataTransferObjects;
using Company.Product.Core;
using Company.Product.DomainModels;
using Company.Product.Features.RenameItem;
using NSubstitute;
using Xunit;

namespace Company.Product.UnitTests.Features;

/// <summary>The concurrency rule of ADR 0006: a stale version is a conflict, never a lost update.</summary>
public sealed class RenameItemCommandHandlerTests
{
    private readonly IItemRepository _items = Substitute.For<IItemRepository>();
    private readonly Item _item = Assert.IsType<Ok<Item>>(Item.Create("Original", DateTimeOffset.UnixEpoch).Value).Value;

    public RenameItemCommandHandlerTests()
    {
        _items.GetByIdAsync(_item.Id, Arg.Any<CancellationToken>()).Returns(_item);
        _items.SaveAsync(_item, Arg.Any<CancellationToken>()).Returns(true);
    }

    [Fact]
    public async Task Handle_CurrentVersion_RenamesAndIncrementsTheVersion()
    {
        var handler = new RenameItemCommandHandler(_items);

        var result = await handler.Handle(new RenameItemCommand(_item.Id, "Renamed", 0), TestContext.Current.CancellationToken);

        var ok = Assert.IsType<Ok<ItemDto>>(result.Value);
        Assert.Equal("Renamed", ok.Value.Name);
        Assert.Equal(1, ok.Value.Version);
        await _items.Received(1).SaveAsync(_item, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_StaleVersion_IsAConflictAndSavesNothing()
    {
        var handler = new RenameItemCommandHandler(_items);

        var result = await handler.Handle(new RenameItemCommand(_item.Id, "Renamed", 7), TestContext.Current.CancellationToken);

        Assert.IsType<Conflict>(result.Value);
        Assert.Equal("Original", _item.Name);
        await _items.DidNotReceiveWithAnyArgs().SaveAsync(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Handle_ConcurrentWriteWonTheSave_IsAConflict()
    {
        _items.SaveAsync(_item, Arg.Any<CancellationToken>()).Returns(false);
        var handler = new RenameItemCommandHandler(_items);

        var result = await handler.Handle(new RenameItemCommand(_item.Id, "Renamed", 0), TestContext.Current.CancellationToken);

        Assert.IsType<Conflict>(result.Value);
    }

    [Fact]
    public async Task Handle_UnknownItem_IsNotFound()
    {
        var handler = new RenameItemCommandHandler(_items);

        var result = await handler.Handle(new RenameItemCommand(Guid.NewGuid(), "Renamed", 0), TestContext.Current.CancellationToken);

        Assert.IsType<NotFound>(result.Value);
    }
}
