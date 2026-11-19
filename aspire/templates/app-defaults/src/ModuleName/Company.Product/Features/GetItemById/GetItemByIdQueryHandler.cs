using Company.Product.Abstractions.DataTransferObjects;
using Company.Product.Core;

namespace Company.Product.Features.GetItemById;

public sealed class GetItemByIdQueryHandler(IItemRepository items) : IQueryHandler<GetItemByIdQuery, ItemDto>
{
    public async Task<Result<ItemDto>> Handle(GetItemByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await items.GetByIdAsync(query.Id, cancellationToken) is { } item
            ? new Ok<ItemDto>(item.ToDto())
            : new NotFound();
    }
}
