using Company.Product.Abstractions.DataTransferObjects;
using Company.Product.Core;
using Company.Product.Features.CreateItem;
using Company.Product.Features.GetItemById;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Company.Product;

public static class ModuleNameModule
{
    /// <summary>Registers every handler of the ModuleName module.</summary>
    public static IServiceCollection AddModuleNameModule(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        return services
            .AddScoped<ICommandHandler<CreateItemCommand, ItemDto>, CreateItemCommandHandler>()
            .AddScoped<IQueryHandler<GetItemByIdQuery, ItemDto>, GetItemByIdQueryHandler>();
    }
}
