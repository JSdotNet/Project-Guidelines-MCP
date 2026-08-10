using JSdotNet.MCP.Publish.Publishing;

namespace JSdotNet.MCP.Publish.Tests;

public sealed class PublishOptionsTests
{
    [Fact]
    public void ResolveRootPath_PrefersConfiguredPath()
    {
        var configured = Path.Join(Path.GetTempPath(), "configured-publish-root");

        var resolved = PublishOptions.ResolveRootPath(configured);

        Assert.Equal(Path.GetFullPath(configured), resolved);
    }

    [Fact]
    public void ResolveRootPath_FallsBackToEnvironmentVariable()
    {
        var expected = Path.Join(
            Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            "env-publish-root");
        var original = Environment.GetEnvironmentVariable(PublishOptions.RootPathEnvironmentVariable);
        Environment.SetEnvironmentVariable(PublishOptions.RootPathEnvironmentVariable, expected);
        try
        {
            var resolved = PublishOptions.ResolveRootPath(null);

            Assert.Equal(Path.GetFullPath(expected), resolved);
        }
        finally
        {
            Environment.SetEnvironmentVariable(PublishOptions.RootPathEnvironmentVariable, original);
        }
    }

    [Fact]
    public void ResolveRootPath_FallsBackToUserFolder()
    {
        var original = Environment.GetEnvironmentVariable(PublishOptions.RootPathEnvironmentVariable);
        Environment.SetEnvironmentVariable(PublishOptions.RootPathEnvironmentVariable, null);
        try
        {
            var resolved = PublishOptions.ResolveRootPath(null);

            Assert.True(Path.IsPathFullyQualified(resolved));
            Assert.EndsWith("PublishedResults", resolved);
        }
        finally
        {
            Environment.SetEnvironmentVariable(PublishOptions.RootPathEnvironmentVariable, original);
        }
    }
}
