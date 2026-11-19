---
title: "Building Block: Api"
date: 2026-11-19
status: Accepted
tags: [building-blocks, api, minimal-api, endpoints, openapi, problem-details, structure]
---
# Building Block: Api

`Company.Product.{Module}.Api` is a module's HTTP edge and its host (ADR 0004). It binds and
validates the request, calls one handler, and maps the `Result<T>` into a response. Part of
the [building-block view](../05-building-block-view.md).

## Layout

```text
Contoso.Orders.Api/
├── Features/
│   ├── SubmitOrder/SubmitOrderEndpoint.cs
│   └── GetOrderById/GetOrderByIdEndpoint.cs
├── OrdersRoutes.cs           # the /orders route group: tags, auth, rate limits
├── Program.cs
└── appsettings.json
```

## Endpoint class

One class per feature slice, named after it, mapping its routes on the module's group.

```csharp
namespace Contoso.Orders.Api.Features.SubmitOrder;

public static class SubmitOrderEndpoint
{
    public static RouteGroupBuilder MapSubmitOrder(this RouteGroupBuilder orders)
    {
        orders.MapPost("/{id:guid}/submit", Handle).WithName("SubmitOrder");
        return orders;
    }

    private static async Task<IResult> Handle(
        Guid id, SubmitOrderRequest request,
        ICommandHandler<SubmitOrderCommand, OrderDto> handler, CancellationToken ct) =>
        await handler.Handle(new SubmitOrderCommand(id, request.Version), ct) switch
        {
            Ok<OrderDto> ok => TypedResults.Ok(ok.Value),
            NotFound        => TypedResults.Problem(statusCode: 404),
            Invalid i       => TypedResults.ValidationProblem(i.Errors),
            Conflict        => TypedResults.Problem(statusCode: 409),
        };
}
```

## Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<OrdersDbContext>("orders-db");
builder.Services.AddOrdersModule().AddOrdersData();
builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseExceptionHandler();
app.MapDefaultEndpoints();
app.MapGroup("/orders").WithTags("Orders").MapSubmitOrder().MapGetOrderById();
app.MapOpenApi();
app.Run();
```

## Rules

- Endpoints stay thin: bind, call the handler, map the result. No business logic.
- Every non-success response is Problem Details; the `Result<T>` switch is exhaustive (ADR 0004,
  0005).
- Authorization, rate limiting and tags are set on the route group; the fallback policy
  requires an authenticated user (ADR 0008).
- `Program.cs` composes the module, its `Data.{Provider}` and ServiceDefaults, nothing more.
- Background services of the module run here, as hosted services.
- Integration tests host this project in-process with `WebApplicationFactory` (ADR 0010).
