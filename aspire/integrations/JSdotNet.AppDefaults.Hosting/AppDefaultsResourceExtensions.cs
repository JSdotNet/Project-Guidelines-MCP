using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

/// <summary>
/// The resource definitions every JSdotNet application shares (ADR 0002). Each one applies the
/// defaults once, so an AppHost does not repeat them per resource.
/// </summary>
public static class AppDefaultsResourceExtensions
{
    /// <summary>The path every service maps its readiness check on (ServiceDefaults).</summary>
    public const string HealthPath = "/health";

    /// <summary>
    /// Adds a SQL Server container that keeps its data in a volume and outlives one
    /// <c>aspire run</c>, so a restart does not lose the local database.
    /// </summary>
    public static IResourceBuilder<SqlServerServerResource> AddDefaultsSqlServer(
        this IDistributedApplicationBuilder builder, string name)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddSqlServer(name).WithDataVolume().WithLifetime(ContainerLifetime.Persistent);
    }

    /// <summary>
    /// Adds a PostgreSQL container that keeps its data in a volume and outlives one
    /// <c>aspire run</c>, so a restart does not lose the local database.
    /// </summary>
    public static IResourceBuilder<PostgresServerResource> AddDefaultsPostgres(
        this IDistributedApplicationBuilder builder, string name)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddPostgres(name).WithDataVolume().WithLifetime(ContainerLifetime.Persistent);
    }

    /// <summary>
    /// Marks the resource healthy only when its <c>/health</c> endpoint answers, the endpoint
    /// ServiceDefaults maps in every service.
    /// </summary>
    public static IResourceBuilder<T> WithDefaultsHealthCheck<T>(this IResourceBuilder<T> builder)
        where T : IResourceWithEndpoints
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithHttpHealthCheck(HealthPath);
    }
}
