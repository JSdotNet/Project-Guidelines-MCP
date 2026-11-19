using Company.Product.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace Company.Product.Data.SqlServer;

internal sealed class ItemRepository(ModuleNameDbContext db) : IItemRepository
{
    public Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Items.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task AddAsync(Item item, CancellationToken cancellationToken)
    {
        db.Items.Add(item);
        await db.SaveChangesAsync(cancellationToken);
    }
}
