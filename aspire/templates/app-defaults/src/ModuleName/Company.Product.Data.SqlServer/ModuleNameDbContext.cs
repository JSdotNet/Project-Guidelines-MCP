using Company.Product.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace Company.Product.Data.SqlServer;

/// <summary>The ModuleName module's own schema; no other module reads its tables (ADR 0006).</summary>
public sealed class ModuleNameDbContext(DbContextOptions<ModuleNameDbContext> options) : DbContext(options)
{
    public const string Schema = "modulename";

    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModuleNameDbContext).Assembly);
    }
}
