using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Company.Product;

/// <summary>The module's one ActivitySource and Meter, named after it (ADR 0007).</summary>
public static class ModuleNameTelemetry
{
    public const string Name = "Company.Product";

    public static readonly ActivitySource ActivitySource = new(Name);

    private static readonly Meter Meter = new(Name);

    public static readonly Counter<long> ItemsCreated = Meter.CreateCounter<long>("modulename.items.created");
}
