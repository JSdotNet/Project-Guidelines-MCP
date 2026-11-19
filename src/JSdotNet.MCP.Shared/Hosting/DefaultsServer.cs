using System.Reflection;
using JSdotNet.MCP.Shared.Abstractions;
using JSdotNet.MCP.Shared.Resources;
using JSdotNet.MCP.Shared.Tools;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;

namespace JSdotNet.MCP.Shared.Hosting;

/// <summary>
/// Wires the defaults server's primitives onto an MCP server builder: the tools, the prompts,
/// every document as a <c>defaults://</c> resource, and a <c>ttlMs</c> on every list result.
/// </summary>
public static class DefaultsServer
{
    /// <summary>The server id clients configure, and the name it reports.</summary>
    public const string ServerId = "jsdotnet-app-defaults";

    /// <summary>
    /// How long a client may cache a list or read result. The documents change only when a new
    /// package version or a new commit on the default branch ships.
    /// </summary>
    public static readonly TimeSpan ListTimeToLive = TimeSpan.FromMinutes(5);

    /// <summary>Adds the tools, prompts, resources and list cache hints.</summary>
    public static IMcpServerBuilder WithDefaults(this IMcpServerBuilder builder)
    {
        var assembly = typeof(GuidesTools).Assembly;
        return builder
            .WithToolsFromAssembly(assembly)
            .WithPromptsFromAssembly(assembly)
            .WithListResourcesHandler((request, ct) =>
                DefaultsResources.ListAsync(Catalog(request.Services), ListTimeToLive, ct))
            .WithReadResourceHandler((request, ct) =>
                DefaultsResources.ReadAsync(Catalog(request.Services), request.Params?.Uri, ListTimeToLive, ct))
            .WithRequestFilters(filters => filters
                .AddListToolsFilter(next => async (request, ct) => WithTimeToLive(await next(request, ct)))
                .AddListPromptsFilter(next => async (request, ct) => WithTimeToLive(await next(request, ct))));
    }

    /// <summary>
    /// The version to report as <c>ServerInfo.Version</c>: the informational version GitVersion
    /// stamps at build time, without the source-link commit suffix, else the assembly version.
    /// </summary>
    public static string VersionOf(Assembly assembly)
    {
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plus = informational.IndexOf('+', StringComparison.Ordinal);
            return plus > 0 ? informational[..plus] : informational;
        }

        return assembly.GetName().Version?.ToString() ?? "0.0.0";
    }

    internal static T WithTimeToLive<T>(T result) where T : ICacheableResult
    {
        result.TimeToLive ??= ListTimeToLive;
        return result;
    }

    private static IDocumentCatalog Catalog(IServiceProvider? services)
        => (services ?? throw new InvalidOperationException("The request carries no service provider."))
            .GetRequiredService<IDocumentCatalog>();
}
