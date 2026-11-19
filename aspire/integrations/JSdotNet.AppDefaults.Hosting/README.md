# JSdotNet.AppDefaults.Hosting

The Aspire 17 hosting integration with the resource definitions the JSdotNet application
defaults share. The Aspire config source in this repository lists it under
`integrationSources`, so `aspire add` finds it once the source is accepted.

```csharp
var sql = builder.AddDefaultsSqlServer("sql");          // data volume, persistent lifetime
var db = sql.AddDatabase("orders-db");

builder.AddDotnetProject("orders-api", "../../Orders/Contoso.Orders.Api/Contoso.Orders.Api.csproj")
    .WithReference(db).WaitFor(db)
    .WithDefaultsHealthCheck();                          // healthy once /health answers
```

| Method | Default it applies |
|---|---|
| `AddDefaultsSqlServer` / `AddDefaultsPostgres` | Data volume and `ContainerLifetime.Persistent` |
| `WithDefaultsHealthCheck` | HTTP health check on `/health`, the endpoint ServiceDefaults maps |
