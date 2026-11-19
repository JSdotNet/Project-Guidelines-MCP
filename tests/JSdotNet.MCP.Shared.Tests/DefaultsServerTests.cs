using System.IO.Pipelines;
using System.Reflection;
using JSdotNet.MCP.Shared.Abstractions;
using JSdotNet.MCP.Shared.Hosting;
using JSdotNet.MCP.Shared.Logging;
using JSdotNet.MCP.Shared.Resources;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace JSdotNet.MCP.Shared.Tests;

/// <summary>
/// Drives the defaults server over an in-memory stream with a real MCP client, so every
/// assertion is about what a host sees on the wire.
/// </summary>
public sealed class DefaultsServerTests : IAsyncLifetime
{
    private static readonly DocumentInfo Adr = new("0003-aspire", "ADR 0003: Aspire", "Aspire is mandatory.", "adrs", "arc42/adr/0003-aspire.md", ["aspire"]);
    private static readonly DocumentInfo Color = new("01-color", "Color", "Palette.", "design", "design\\01-color.md", ["design"]);

    private readonly CancellationTokenSource _cts = new();
    private ServiceProvider? _services;
    private Task? _serverRun;
    private McpClient? _client;

    private McpClient Client => _client!;

    public async ValueTask InitializeAsync()
    {
        var clientToServer = new Pipe();
        var serverToClient = new Pipe();

        var services = new ServiceCollection();
        services.AddSingleton<IDocumentCatalog>(new FakeDocumentCatalog([Adr, Color], content: "# ADR 0003"));
        services.AddSingleton<IUsageLog>(new FakeUsageLog());
        services
            .AddMcpServer(o => o.ServerInfo = new Implementation { Name = DefaultsServer.ServerId, Version = "1.2.3" })
            .WithStreamServerTransport(clientToServer.Reader.AsStream(), serverToClient.Writer.AsStream())
            .WithDefaults();
        _services = services.BuildServiceProvider();
        _serverRun = _services.GetRequiredService<McpServer>().RunAsync(_cts.Token);

        _client = await McpClient.CreateAsync(
            new StreamClientTransport(clientToServer.Writer.AsStream(), serverToClient.Reader.AsStream()),
            cancellationToken: TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null) await _client.DisposeAsync();
        await _cts.CancelAsync();
        if (_serverRun is not null)
        {
            try { await _serverRun; } catch (OperationCanceledException) { }
        }
        if (_services is not null) await _services.DisposeAsync();
        _cts.Dispose();
    }

    [Fact]
    public void ServerInfo_ReportsConfiguredIdentity()
    {
        Assert.Equal(DefaultsServer.ServerId, Client.ServerInfo.Name);
        Assert.Equal("1.2.3", Client.ServerInfo.Version);
    }

    [Fact]
    public async Task Resources_ListEveryDocumentAtItsDefaultsUri()
    {
        var result = await Client.ListResourcesAsync(new ListResourcesRequestParams(), TestContext.Current.CancellationToken);

        Assert.Equal(
            ["defaults://arc42/adr/0003-aspire", "defaults://design/01-color"],
            result.Resources.Select(r => r.Uri));
        Assert.Equal(["0003-aspire", "01-color"], result.Resources.Select(r => r.Name));
        Assert.All(result.Resources, r => Assert.Equal(DefaultsResources.MarkdownMimeType, r.MimeType));
        Assert.Equal(DefaultsServer.ListTimeToLive, result.TimeToLive);
    }

    [Fact]
    public async Task Resources_ReadReturnsTheDocumentMarkdown()
    {
        var result = await Client.ReadResourceAsync("defaults://arc42/adr/0003-aspire", cancellationToken: TestContext.Current.CancellationToken);

        var text = Assert.IsType<TextResourceContents>(Assert.Single(result.Contents));
        Assert.Equal("# ADR 0003", text.Text);
        Assert.Equal("defaults://arc42/adr/0003-aspire", text.Uri);
        Assert.Equal(DefaultsServer.ListTimeToLive, result.TimeToLive);
    }

    [Fact]
    public async Task Resources_ReadUnknownUriFails()
    {
        await Assert.ThrowsAsync<McpProtocolException>(() =>
            Client.ReadResourceAsync("defaults://nope", cancellationToken: TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Tools_ListCarriesTtlAndTheGuideTools()
    {
        var result = await Client.ListToolsAsync(new ListToolsRequestParams(), TestContext.Current.CancellationToken);

        Assert.Equal(DefaultsServer.ListTimeToLive, result.TimeToLive);
        Assert.Contains(result.Tools, t => t.Name == "get_guide");
        Assert.Contains(result.Tools, t => t.Name == "list_guides_by_type");
    }

    [Fact]
    public async Task Prompts_ListCarriesTtlAndTheThreePrompts()
    {
        var result = await Client.ListPromptsAsync(new ListPromptsRequestParams(), TestContext.Current.CancellationToken);

        Assert.Equal(DefaultsServer.ListTimeToLive, result.TimeToLive);
        Assert.Equal(
            ["new-app", "propose-adr", "review-against-defaults"],
            result.Prompts.Select(p => p.Name).Order());
    }

    [Theory]
    [InlineData("new-app", "name", "Contoso.Orders")]
    [InlineData("review-against-defaults", "target", "src/Orders")]
    [InlineData("propose-adr", "title", "Use gRPC between modules")]
    public async Task Prompts_GetEmbedsTheArgument(string prompt, string argument, string value)
    {
        var result = await Client.GetPromptAsync(
            prompt, new Dictionary<string, object?> { [argument] = value }, cancellationToken: TestContext.Current.CancellationToken);

        var message = Assert.Single(result.Messages);
        var text = Assert.IsType<TextContentBlock>(message.Content);
        Assert.Contains(value, text.Text);
    }

    [Fact]
    public void ToUri_StripsMarkdownExtensionAndNormalizesSeparators()
    {
        Assert.Equal("defaults://design/01-color", DefaultsResources.ToUri(Color));
        Assert.Equal("defaults://ai/config/.mcp.json",
            DefaultsResources.ToUri(Color with { RelativePath = "/ai/config/.mcp.json.md" }));
        Assert.Equal("defaults://notes.txt", DefaultsResources.ToUri(Color with { RelativePath = "notes.txt" }));
    }

    [Fact]
    public void VersionOf_UsesInformationalVersionWithoutCommitSuffix()
    {
        var version = DefaultsServer.VersionOf(typeof(DefaultsServer).Assembly);

        Assert.False(string.IsNullOrWhiteSpace(version));
        Assert.DoesNotContain('+', version);
        Assert.NotEqual("0.1.0", version);
    }

    [Fact]
    public void VersionOf_FallsBackToAssemblyVersion()
    {
        var assembly = new FakeAssembly(informational: null, version: new Version(4, 5, 6, 0));
        Assert.Equal("4.5.6.0", DefaultsServer.VersionOf(assembly));

        Assert.Equal("2.0.0-beta.1", DefaultsServer.VersionOf(new FakeAssembly("2.0.0-beta.1+abc123", null)));
        Assert.Equal("0.0.0", DefaultsServer.VersionOf(new FakeAssembly(null, null)));
    }

    [Fact]
    public void WithTimeToLive_KeepsAnExplicitValue()
    {
        var result = new ListToolsResult { Tools = [], TimeToLive = TimeSpan.FromSeconds(1) };
        Assert.Equal(TimeSpan.FromSeconds(1), DefaultsServer.WithTimeToLive(result).TimeToLive);
    }

    private sealed class FakeAssembly(string? informational, Version? version) : Assembly
    {
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            var match = informational is not null && attributeType.IsAssignableFrom(typeof(AssemblyInformationalVersionAttribute));
            var attributes = (object[])Array.CreateInstance(attributeType, match ? 1 : 0);
            if (match) attributes[0] = new AssemblyInformationalVersionAttribute(informational!);
            return attributes;
        }

        public override AssemblyName GetName() => new("Fake") { Version = version };
    }
}
