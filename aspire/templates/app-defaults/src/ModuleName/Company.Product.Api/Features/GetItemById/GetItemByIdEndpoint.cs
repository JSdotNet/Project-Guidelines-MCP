using Company.Product.Abstractions.DataTransferObjects;
using Company.Product.Core;
using Company.Product.Features.GetItemById;

namespace Company.Product.Api.Features.GetItemById;

internal static class GetItemByIdEndpoint
{
    public static RouteGroupBuilder MapGetItemById(this RouteGroupBuilder group)
    {
        group.MapGet("/items/{id:guid}", Handle).WithName("GetItemById");
        return group;
    }

    private static async Task<IResult> Handle(
        Guid id,
        IQueryHandler<GetItemByIdQuery, ItemDto> handler,
        CancellationToken cancellationToken) =>
        await handler.Handle(new GetItemByIdQuery(id), cancellationToken) switch
        {
            Ok<ItemDto> ok => TypedResults.Ok(ok.Value),
            NotFound => TypedResults.Problem(statusCode: StatusCodes.Status404NotFound),
            Invalid invalid => TypedResults.ValidationProblem(invalid.Errors),
            Conflict => TypedResults.Problem(statusCode: StatusCodes.Status409Conflict),
        };
}
