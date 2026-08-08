using JSdotNet.MCP.Publish.Publishing;

namespace JSdotNet.MCP.Publish.Tests;

/// <summary>Creates an isolated temp publish root that is removed when the test finishes.</summary>
public sealed class TempPublishRoot : IDisposable
{
    public TempPublishRoot(bool allowOverwriteByDefault = false)
    {
        RootPath = Path.Combine(Path.GetTempPath(), "jsdotnet-publish-tests", Guid.NewGuid().ToString("N"));
        Publisher = new FileResultPublisher(new PublishOptions
        {
            RootPath = RootPath,
            AllowOverwriteByDefault = allowOverwriteByDefault,
        });
    }

    public string RootPath { get; }

    public FileResultPublisher Publisher { get; }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(RootPath))
                Directory.Delete(RootPath, recursive: true);
        }
        catch (IOException)
        {
            // Best-effort cleanup.
        }
    }
}
