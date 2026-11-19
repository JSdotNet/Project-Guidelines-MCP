using Company.Product;
using Company.Product.Api;
using Company.Product.Data.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSqlServerDbContext<ModuleNameDbContext>("modulename-db");
builder.Services.AddModuleNameModule().AddModuleNameData();
builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();
app.MapModuleNameRoutes();
app.MapOpenApi();

app.Run();
