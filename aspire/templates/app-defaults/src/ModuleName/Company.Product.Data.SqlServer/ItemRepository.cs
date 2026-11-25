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

    public async Task<bool> SaveAsync(Item item, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // ADR 0006: another write moved Version on after this item was read.
            return false;
        }
    }
}
