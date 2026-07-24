using JSdotNet.MCP.Shared.Abstractions;
using JSdotNet.MCP.Shared.Infrastructure;
using JSdotNet.MCP.Shared.Logging;
using JSdotNet.MCP.Shared.Tools;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;
using System.Net.Http;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton(new HttpClient());

builder.Services.AddSingleton<IUsageLog>(_ =>
{
    var baseDir = Environment.GetEnvironmentVariable("JSDOTNET_LOG_PATH")
        ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
            "JSdotNet", "DesignMcpServer");
    var logFile = Path.Combine(baseDir, $"usage-{Environment.ProcessId}.jsonl");
    return new JsonFileUsageLog(logFile);
});

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IDocumentCatalog>(sp =>
{
    var localPath = Environment.GetEnvironmentVariable("JSDOTNET_DOCS_PATH");
    if (!string.IsNullOrWhiteSpace(localPath) && Directory.Exists(localPath))
    {
        return new FileSystemDocumentCatalog(localPath);
    }

    var discoveredPath = FindNearestDocsFolder("design");
    if (!string.IsNullOrWhiteSpace(discoveredPath))
    {
        return new FileSystemDocumentCatalog(discoveredPath);
    }

    var client = sp.GetRequiredService<HttpClient>();
    var cache = sp.GetRequiredService<IMemoryCache>();
    return new GitHubDocumentCatalog(cache: cache, httpClient: client, documentsPath: "design");
});

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation
        {
            Name = "JSdotNet.MCP.Design",
            Version = "0.1.0"
        };
    })
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(GuidesTools).Assembly);

await builder.Build().RunAsync();

static string? FindNearestDocsFolder(string relativePath)
{
    foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        var directory = new DirectoryInfo(start);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }
    }

    return null;
}
