using Company.Product.Api.Features.CreateItem;
using Company.Product.Api.Features.GetItemById;

namespace Company.Product.Api;

internal static class ModuleNameRoutes
{
    /// <summary>The /modulename route group: tags, authorization and rate limits go here.</summary>
    public static IEndpointRouteBuilder MapModuleNameRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/modulename")
            .WithTags("ModuleName")
            .MapCreateItem()
            .MapGetItemById();

        return app;
    }
}
