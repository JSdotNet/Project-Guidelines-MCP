using Microsoft.Extensions.DependencyInjection;

namespace Company.Product.Data.SqlServer;

public static class ModuleNameDataModule
{
    /// <summary>Registers the module's repositories; the Api registers the DbContext itself.</summary>
    public static IServiceCollection AddModuleNameData(this IServiceCollection services) =>
        services.AddScoped<IItemRepository, ItemRepository>();
}
