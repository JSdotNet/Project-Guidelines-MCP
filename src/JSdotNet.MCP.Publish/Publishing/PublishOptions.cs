namespace JSdotNet.MCP.Publish.Publishing;

/// <summary>
/// Configuration for the publish target. The root path is the only location this server
/// is allowed to write to; every published file is resolved beneath it.
/// </summary>
public sealed class PublishOptions
{
    /// <summary>Configuration section name bound from environment variables or command line.</summary>
    public const string SectionName = "Publish";

    /// <summary>Environment variable that overrides the publish root path.</summary>
    public const string RootPathEnvironmentVariable = "JSDOTNET_PUBLISH_PATH";

    /// <summary>Directory results are written to. Empty means "resolve from environment or default".</summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>When true, publishing over an existing file is allowed without opting in per call.</summary>
    public bool AllowOverwriteByDefault { get; set; }

    /// <summary>
    /// Resolves the effective root path: explicit configuration wins, then the environment
    /// variable, then a per-user default folder.
    /// </summary>
    public static string ResolveRootPath(string? configuredPath)
    {
        var path = configuredPath;

        if (string.IsNullOrWhiteSpace(path))
            path = Environment.GetEnvironmentVariable(RootPathEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(path))
        {
            path = Path.Join(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
                "JSdotNet",
                "PublishedResults");
        }

        return Path.GetFullPath(Environment.ExpandEnvironmentVariables(path.Trim()));
    }
}
