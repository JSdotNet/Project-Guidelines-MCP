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

builder.Services.AddSingleton<HttpClient>();

builder.Services.AddSingleton<IUsageLog>(_ =>
{
    var baseDir = Environment.GetEnvironmentVariable("JSDOTNET_LOG_PATH")
        ?? Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
            "JSdotNet", "DesignMcpServer");
    var logFile = Path.Join(baseDir, $"usage-{Environment.ProcessId}.jsonl");
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
    foreach (var directory in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory }
        .Select(static p => new DirectoryInfo(p)))
    {
        var dir = directory;
        while (dir is not null)
        {
            var candidate = Path.Join(dir.FullName, relativePath);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }
    }

    return null;
}
