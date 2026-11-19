---
title: "Building Block: Module"
date: 2026-11-19
status: Accepted
tags: [building-blocks, module, feature-slices, cqrs, abstractions, structure]
---
# Building Block: Module

A module owns one business capability: its domain model, its use cases as feature slices,
its data and its public contract (ADR 0003). Part of the
[building-block view](../05-building-block-view.md).

## Projects

| Project | Holds | References |
|---|---|---|
| `Company.Product.{Module}` | Domain model, feature slices, repository ports, module registration | `Abstractions`, `Core`, other modules' `Abstractions` |
| `Company.Product.{Module}.Abstractions` | DTO records, service ports other modules call, nothing else | Framework only |

## Layout

```text
src/Orders/
├── Contoso.Orders/
│   ├── DomainModels/                 # building-blocks/domain-model.md
│   │   ├── Order.cs
│   │   └── OrderLine.cs
│   ├── Features/
│   │   ├── SubmitOrder/
│   │   │   ├── SubmitOrderCommand.cs
│   │   │   └── SubmitOrderCommandHandler.cs
│   │   └── GetOrderById/
│   │       ├── GetOrderByIdQuery.cs
│   │       └── GetOrderByIdQueryHandler.cs
│   ├── IOrderRepository.cs           # port, implemented in Data.{Provider}
│   ├── OrdersTelemetry.cs            # ActivitySource and Meter for the module
│   └── OrdersModule.cs               # AddOrdersModule()
└── Contoso.Orders.Abstractions/
    ├── DataTransferObjects/
    │   └── OrderDto.cs
    └── IOrderLookup.cs               # port another module may call
```

## A feature slice

A slice is one use case: a command or query record and its handler (ADR 0005). Its endpoint
lives in the [Api](api.md) under the same feature name.

```csharp
namespace Contoso.Orders.Features.SubmitOrder;

public sealed record SubmitOrderCommand(Guid OrderId, uint Version);

public sealed class SubmitOrderCommandHandler(IOrderRepository orders, TimeProvider time)
    : ICommandHandler<SubmitOrderCommand, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(SubmitOrderCommand command, CancellationToken ct)
    {
        if (await orders.GetByIdAsync(command.OrderId, ct) is not { } order)
            return new NotFound();
        if (order.Version != command.Version)
            return new Conflict();

        if (order.Submit(time.GetUtcNow()) is Invalid invalid)
            return invalid;

        await orders.SaveAsync(order, ct);
        return new Ok<OrderDto>(order.ToDto());
    }
}
```

```csharp
namespace Contoso.Orders;

public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services) => services
        .AddScoped<ICommandHandler<SubmitOrderCommand, OrderDto>, SubmitOrderCommandHandler>()
        .AddScoped<IQueryHandler<GetOrderByIdQuery, OrderDto>, GetOrderByIdQueryHandler>();
}
```

## Rules

- Commands, queries, DTOs and results are `sealed record` types.
- Commands and queries stay inside the module; only DTOs and ports go in `Abstractions`.
- DTOs never carry domain or EF Core types; handlers return DTOs, never entities.
- One handler per feature, called directly by its endpoint; no mediator.
- One `Add{Module}Module` registers every handler of the module.
- The module emits its own traces and metrics through one `ActivitySource` and `Meter`
  named after it (ADR 0007).
- Create a new module for a distinct capability with its own data and rules, not for a
  technical layer.
