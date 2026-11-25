using Company.Product.Data.SqlServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

namespace Company.Product.IntegrationTests;

/// <summary>
/// Hosts the ModuleName Api in-process against a throwaway SQL Server container; it never starts
/// the AppHost (ADR 0010).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _database = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        // Test-only schema setup; the app itself migrates through the AppHost's runner (ADR 0006).
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ModuleNameDbContext>().Database.EnsureCreatedAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:modulename-db", _database.GetConnectionString());

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }
}
