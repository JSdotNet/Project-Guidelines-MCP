using Company.Product.Abstractions.DataTransferObjects;
using Company.Product.Core;
using Company.Product.Features.RenameItem;

namespace Company.Product.Api.Features.RenameItem;

internal static class RenameItemEndpoint
{
    public static RouteGroupBuilder MapRenameItem(this RouteGroupBuilder group)
    {
        group.MapPut("/items/{id:guid}", Handle).WithName("RenameItem");
        return group;
    }

    private static async Task<IResult> Handle(
        Guid id,
        RenameItemRequest request,
        ICommandHandler<RenameItemCommand, ItemDto> handler,
        CancellationToken cancellationToken) =>
        await handler.Handle(new RenameItemCommand(id, request.Name, request.Version), cancellationToken) switch
        {
            Ok<ItemDto> ok => TypedResults.Ok(ok.Value),
            NotFound => TypedResults.Problem(statusCode: StatusCodes.Status404NotFound),
            Invalid invalid => TypedResults.ValidationProblem(invalid.Errors),
            Conflict => TypedResults.Problem(
                statusCode: StatusCodes.Status409Conflict,
                detail: "The item changed after it was read; read it again and retry."),
        };
}
