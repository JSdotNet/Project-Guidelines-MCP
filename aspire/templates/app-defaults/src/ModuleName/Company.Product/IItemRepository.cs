using Company.Product.DomainModels;

namespace Company.Product;

/// <summary>Port for the Item aggregate, implemented in Data.SqlServer.</summary>
public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Item item, CancellationToken cancellationToken);

    /// <summary>
    /// Saves the changes made to <paramref name="item"/>; <see langword="false"/> when another write
    /// changed it after it was read (ADR 0006).
    /// </summary>
    Task<bool> SaveAsync(Item item, CancellationToken cancellationToken);
}
