using JSdotNet.MCP.Shared.Abstractions;
using JSdotNet.MCP.Shared.Hosting;
using JSdotNet.MCP.Shared.Infrastructure;
using JSdotNet.MCP.Shared.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<HttpClient>();

builder.Services.AddSingleton<IUsageLog>(_ =>
{
    var baseDir = Environment.GetEnvironmentVariable("JSDOTNET_LOG_PATH")
        ?? Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
            "JSdotNet", "AppDefaultsMcpServer");
    var logFile = Path.Join(baseDir, $"usage-{Environment.ProcessId}.jsonl");
    return new JsonFileUsageLog(logFile);
});

builder.Services.AddMemoryCache();

// One catalog serves all of defaults/; design is one of its categories.
builder.Services.AddSingleton<IDocumentCatalog>(sp =>
{
    var localPath = FileSystemDocumentCatalog.ResolveDocumentsPath(Environment.GetEnvironmentVariable("JSDOTNET_DOCS_PATH"));
    if (localPath is not null)
    {
        return new FileSystemDocumentCatalog(localPath);
    }

    return new GitHubDocumentCatalog(cache: sp.GetRequiredService<IMemoryCache>(), httpClient: sp.GetRequiredService<HttpClient>());
});

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation
        {
            Name = DefaultsServer.ServerId,
            Version = DefaultsServer.VersionOf(typeof(Program).Assembly),
        };
    })
    .WithStdioServerTransport()
    .WithDefaults();

await builder.Build().RunAsync();
