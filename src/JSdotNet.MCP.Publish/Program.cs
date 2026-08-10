using JSdotNet.MCP.Publish.Publishing;
using JSdotNet.MCP.Publish.Tools;
using JSdotNet.MCP.Shared.Logging;
using JSdotNet.MCP.Shared.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<IUsageLog>(_ =>
{
    var baseDir = Environment.GetEnvironmentVariable("JSDOTNET_LOG_PATH")
        ?? Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
            "JSdotNet", "PublishMcpServer");
    var logFile = Path.Join(baseDir, $"usage-{Environment.ProcessId}.jsonl");
    return new JsonFileUsageLog(logFile);
});

// Publish location is configurable via "Publish:RootPath" (appsettings/env/command line,
// e.g. --Publish:RootPath=C:\results) or the JSDOTNET_PUBLISH_PATH environment variable.
builder.Services
    .AddOptions<PublishOptions>()
    .Bind(builder.Configuration.GetSection(PublishOptions.SectionName));

builder.Services.AddSingleton<IResultPublisher, FileResultPublisher>();

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation
        {
            Name = "JSdotNet.MCP.Publish",
            Version = "0.1.0"
        };
    })
    .WithStdioServerTransport()
    .WithTools<PublishTools>()
    .WithTools([typeof(UsageLogTools)]);

await builder.Build().RunAsync();
