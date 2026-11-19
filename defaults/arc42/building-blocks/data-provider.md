---
title: "Building Block: Data.{Provider}"
date: 2026-11-19
status: Accepted
tags: [building-blocks, data, ef-core, persistence, migrations, outbox, repository, structure]
---
# Building Block: Data.{Provider}

`Company.Product.{Module}.Data.{Provider}` is the only place a module touches its database
(ADR 0006). `{Provider}` names the database the app chose in its own ADR: `Postgres`,
`SqlServer`, `Cosmos`. Part of the [building-block view](../05-building-block-view.md).

## Layout

```text
Contoso.Orders.Data.Postgres/
├── Configurations/
│   └── OrderConfiguration.cs     # IEntityTypeConfiguration<Order>
├── Migrations/
├── Outbox/
│   └── OutboxMessage.cs
├── OrdersDbContext.cs
├── OrderRepository.cs            # implements IOrderRepository
└── OrdersDataModule.cs           # AddOrdersData()
```

## Mapping and repository

Mapping lives here, never as attributes on the entities. Value objects map as owned types.

```csharp
internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders", "orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Version).IsRowVersion();
        builder.OwnsOne(o => o.ShippingAddress);
        builder.HasMany(o => o.Lines).WithOne().HasForeignKey("OrderId");
        builder.Ignore(o => o.DomainEvents);
    }
}

internal sealed class OrderRepository(OrdersDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Orders.Include(o => o.Lines).SingleOrDefaultAsync(o => o.Id == id, ct);

    public async Task SaveAsync(Order order, CancellationToken ct)
    {
        db.AddOutboxMessages(order.DomainEvents);   // same transaction as the state change
        await db.SaveChangesAsync(ct);
        order.ClearDomainEvents();
    }
}
```

## Rules

- One repository per aggregate root; never one for a child entity.
- The module owns its schema; it never reads or writes another module's tables.
- Every aggregate has a concurrency token; a stale write becomes `Conflict` and a 409.
- Integration events go to the outbox in the same `SaveChangesAsync`; a dispatcher
  publishes them (ADR 0006).
- Migrations live here and run through the `MigrationRunner` resource in the
  [AppHost](apphost.md); the Api never migrates on startup.
- Query handlers may project straight to DTOs with EF Core; no second data-access library.
