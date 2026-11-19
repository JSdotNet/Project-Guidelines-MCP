using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace JSdotNet.AppDefaults.Hosting.Tests;

public sealed class AppDefaultsResourceExtensionsTests
{
    [Fact]
    public void AddDefaultsSqlServer_KeepsDataInAPersistentVolume()
    {
        var builder = DistributedApplication.CreateBuilder();

        var sql = builder.AddDefaultsSqlServer("sql");

        AssertPersistentWithVolume(sql.Resource);
    }

    [Fact]
    public void AddDefaultsPostgres_KeepsDataInAPersistentVolume()
    {
        var builder = DistributedApplication.CreateBuilder();

        var postgres = builder.AddDefaultsPostgres("postgres");

        AssertPersistentWithVolume(postgres.Resource);
    }

    [Fact]
    public void WithDefaultsHealthCheck_AddsAHealthCheck()
    {
        var builder = DistributedApplication.CreateBuilder();
        var api = builder.AddContainer("api", "example/api").WithHttpEndpoint(targetPort: 8080);

        api.WithDefaultsHealthCheck();

        Assert.Single(api.Resource.Annotations.OfType<HealthCheckAnnotation>());
    }

    [Fact]
    public void Extensions_RejectANullBuilder()
    {
        Assert.Throws<ArgumentNullException>(() => ((IDistributedApplicationBuilder)null!).AddDefaultsSqlServer("sql"));
        Assert.Throws<ArgumentNullException>(() => ((IDistributedApplicationBuilder)null!).AddDefaultsPostgres("postgres"));
        Assert.Throws<ArgumentNullException>(() => ((IResourceBuilder<ContainerResource>)null!).WithDefaultsHealthCheck());
    }

    private static void AssertPersistentWithVolume(IResource resource)
    {
        var lifetime = Assert.Single(resource.Annotations.OfType<ContainerLifetimeAnnotation>());
        Assert.Equal(ContainerLifetime.Persistent, lifetime.Lifetime);
        Assert.Contains(resource.Annotations.OfType<ContainerMountAnnotation>(), m => m.Type == ContainerMountType.Volume);
    }
}
