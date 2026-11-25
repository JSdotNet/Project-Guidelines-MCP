using Company.Product.Abstractions.DataTransferObjects;
using Company.Product.Core;
using Company.Product.DomainModels;

namespace Company.Product.Features.RenameItem;

public sealed class RenameItemCommandHandler(IItemRepository items) : ICommandHandler<RenameItemCommand, ItemDto>
{
    public async Task<Result<ItemDto>> Handle(RenameItemCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        using var activity = ModuleNameTelemetry.ActivitySource.StartActivity("RenameItem");

        if (await items.GetByIdAsync(command.Id, cancellationToken) is not { } item)
        {
            return new NotFound();
        }

        return item.Rename(command.Name, command.Version) switch
        {
            Ok<Item> renamed => await SaveAsync(renamed.Value, cancellationToken),
            Invalid invalid => invalid,
            NotFound notFound => notFound,
            Conflict conflict => conflict,
        };
    }

    private async Task<Result<ItemDto>> SaveAsync(Item item, CancellationToken cancellationToken) =>
        await items.SaveAsync(item, cancellationToken)
            ? new Ok<ItemDto>(item.ToDto())
            : new Conflict();
}
