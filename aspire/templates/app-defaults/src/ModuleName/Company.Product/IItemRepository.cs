using Company.Product.DomainModels;

namespace Company.Product;

/// <summary>Port for the Item aggregate, implemented in Data.SqlServer.</summary>
public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Item item, CancellationToken cancellationToken);
}
