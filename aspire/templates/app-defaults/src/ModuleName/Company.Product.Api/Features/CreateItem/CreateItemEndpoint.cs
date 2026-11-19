using Company.Product.Abstractions.DataTransferObjects;
using Company.Product.Core;
using Company.Product.Features.CreateItem;

namespace Company.Product.Api.Features.CreateItem;

internal static class CreateItemEndpoint
{
    public static RouteGroupBuilder MapCreateItem(this RouteGroupBuilder group)
    {
        group.MapPost("/items", Handle).WithName("CreateItem");
        return group;
    }

    private static async Task<IResult> Handle(
        CreateItemRequest request,
        ICommandHandler<CreateItemCommand, ItemDto> handler,
        CancellationToken cancellationToken) =>
        await handler.Handle(new CreateItemCommand(request.Name), cancellationToken) switch
        {
            Ok<ItemDto> ok => TypedResults.Created($"/modulename/items/{ok.Value.Id}", ok.Value),
            Invalid invalid => TypedResults.ValidationProblem(invalid.Errors),
            NotFound => TypedResults.Problem(statusCode: StatusCodes.Status404NotFound),
            Conflict => TypedResults.Problem(statusCode: StatusCodes.Status409Conflict),
        };
}
