using Company.Product.Abstractions.DataTransferObjects;
using Company.Product.Core;
using Company.Product.DomainModels;

namespace Company.Product.Features.CreateItem;

public sealed class CreateItemCommandHandler(IItemRepository items, TimeProvider time)
    : ICommandHandler<CreateItemCommand, ItemDto>
{
    public async Task<Result<ItemDto>> Handle(CreateItemCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        using var activity = ModuleNameTelemetry.ActivitySource.StartActivity("CreateItem");

        return Item.Create(command.Name, time.GetUtcNow()) switch
        {
            Ok<Item> created => await AddAsync(created.Value, cancellationToken),
            Invalid invalid => invalid,
            NotFound notFound => notFound,
            Conflict conflict => conflict,
        };
    }

    private async Task<Result<ItemDto>> AddAsync(Item item, CancellationToken cancellationToken)
    {
        await items.AddAsync(item, cancellationToken);
        ModuleNameTelemetry.ItemsCreated.Add(1);
        return new Ok<ItemDto>(item.ToDto());
    }
}
