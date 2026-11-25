var builder = DistributedApplication.CreateBuilder(args);

// JSdotNet.AppDefaults.Hosting (ADR 0002): a persistent data volume and the shared /health check.
var database = builder.AddDefaultsSqlServer("sql").AddDatabase("modulename-db");

builder.AddDotnetProject("modulename-api", "../../ModuleName/Company.Product.Api/Company.Product.Api.csproj")
    .WithReference(database)
    .WaitFor(database)
    .WithDefaultsHealthCheck();

builder.Build().Run();
