---
title: "Building Block: Domain Model"
date: 2026-11-19
status: Accepted
tags: [building-blocks, ddd, domain-model, aggregates, value-objects, domain-events, invariants]
---
# Building Block: Domain Model

The `DomainModels/` folder of a [module](module.md): the aggregates, entities and value
objects that hold the module's state and guard its invariants. The approach is in
[04 Solution Strategy](../04-solution-strategy.md#pragmatic-domain-driven-design).

## An aggregate root

```csharp
namespace Contoso.Orders.DomainModels;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];
    private readonly List<object> _domainEvents = [];

    private Order() { } // EF Core

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Address ShippingAddress { get; private set; } = null!;
    public uint Version { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines;
    public IReadOnlyList<object> DomainEvents => _domainEvents;
    public decimal Total => _lines.Sum(l => l.LineTotal);

    public static Order Create(Guid customerId, Address shippingAddress) =>
        new() { Id = Guid.CreateVersion7(), CustomerId = customerId,
                ShippingAddress = shippingAddress, Status = OrderStatus.Draft };

    public Result<OrderLine> AddLine(Guid productId, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Draft)
            return new Invalid("status", "A submitted order cannot change.");
        if (quantity <= 0)
            return new Invalid("quantity", "Quantity must be positive.");

        var line = new OrderLine(productId, quantity, unitPrice);
        _lines.Add(line);
        return new Ok<OrderLine>(line);
    }

    public Result<Order> Submit(DateTimeOffset now)
    {
        if (Status != OrderStatus.Draft)
            return new Invalid("status", "The order is already submitted.");
        if (_lines.Count == 0)
            return new Invalid("lines", "An empty order cannot be submitted.");

        Status = OrderStatus.Submitted;
        _domainEvents.Add(new OrderSubmitted(Id, CustomerId, Total, now));
        return new Ok<Order>(this);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

public sealed class OrderLine
{
    internal OrderLine(Guid productId, int quantity, decimal unitPrice) =>
        (ProductId, Quantity, UnitPrice) = (productId, quantity, unitPrice);

    private OrderLine() { } // EF Core

    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;
}
```

## Aggregate rules

- Outside code references only the aggregate root; child entities are created through its
  methods and have `internal` or `private` constructors.
- Every state change is a method named for the business intent that checks the rule and
  returns a `Result` (ADR 0005). Setters are private; collections are `IReadOnlyList<T>`.
- A `Create` factory builds the root when construction needs a rule.
- An aggregate references another aggregate by id, never by navigation property.
- Aggregate methods never call a repository, clock or other infrastructure; the handler
  passes in what they need (the time from `TimeProvider`).
- The model carries no EF Core attributes; mapping lives in [Data.{Provider}](data-provider.md).
- Every aggregate has a concurrency token (`Version`, ADR 0006).

## Value objects

A value object is a `record` with a factory that validates, used only where several fields
belong together or a rule spans them.

```csharp
public sealed record Address(string Street, string City, string PostalCode, string Country)
{
    public static Result<Address> Create(string street, string city, string postalCode, string country) =>
        string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(city)
            ? new Invalid("address", "Street and city are required.")
            : new Ok<Address>(new(street.Trim(), city.Trim(), postalCode.Trim(), country.ToUpperInvariant()));
}
```

| Concept | Value object? | Why |
|---|---|---|
| Address, money (amount + currency), date range | Yes | Several fields with a rule between them |
| Email, SKU, phone number | When it has format or parsing rules | A rule worth one place |
| Id, first name, age, quantity | No | One primitive; the entity checks it |

## Domain events

A domain event records something the business cares about, as a past-tense record raised by
the aggregate. The [Data.{Provider}](data-provider.md) repository writes the events that
cross the module to the outbox in the same transaction (ADR 0006).

```csharp
public sealed record OrderSubmitted(Guid OrderId, Guid CustomerId, decimal Total, DateTimeOffset OccurredAt);
```

## Specifications

A query rule that recurs (an "active order for this customer") is a named expression on the
module, reused by repositories and tests; a one-off filter stays inline.

```csharp
public static class OrderSpecifications
{
    public static Expression<Func<Order, bool>> ActiveFor(Guid customerId) =>
        o => o.CustomerId == customerId && o.Status != OrderStatus.Cancelled;
}
```

## Tests

Domain behaviour is tested on the entity itself, without doubles: one test per rule and per
outcome, in `Company.Product.{Module}.UnitTests/DomainModels/` (ADR 0010).

```csharp
[Fact]
public void Submit_without_lines_is_invalid()
{
    var order = Order.Create(Guid.NewGuid(), AddressFaker.Valid());

    Assert.IsType<Invalid>(order.Submit(DateTimeOffset.UnixEpoch));
}
```

## Anti-patterns

| Anti-pattern | Instead |
|---|---|
| Anemic entity with public setters, rules in services | Methods on the entity that check and change state |
| A value object for every primitive | Primitives, checked by the entity |
| A repository for a child entity | Load the root and go through it |
| Handlers that set properties directly | Call one domain method and map its `Result` |
| EF Core attributes or `DbContext` in the model | Mapping in the data project |
