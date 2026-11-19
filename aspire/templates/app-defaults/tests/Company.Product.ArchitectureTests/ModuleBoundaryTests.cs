using NetArchTest.Rules;
using Xunit;

namespace Company.Product.ArchitectureTests;

/// <summary>The dependency rules of the building-block view (ADR 0003).</summary>
public sealed class ModuleBoundaryTests
{
    private const string EntityFrameworkCore = "Microsoft.EntityFrameworkCore";

    [Fact]
    public void Module_DoesNotReferenceEntityFrameworkCore() =>
        AssertNoDependency(typeof(AssemblyReference), EntityFrameworkCore);

    [Fact]
    public void Module_DoesNotReferenceItsDataProject() =>
        AssertNoDependency(typeof(AssemblyReference), "Company.Product.Data");

    [Fact]
    public void Abstractions_DoNotReferenceTheModule() =>
        AssertNoDependency(typeof(Abstractions.AssemblyReference), "Company.Product.DomainModels", "Company.Product.Features");

    [Fact]
    public void Core_DoesNotReferenceAnyModule() =>
        AssertNoDependency(typeof(Core.AssemblyReference), "Company.Product.DomainModels", EntityFrameworkCore);

    private static void AssertNoDependency(Type marker, params string[] forbidden)
    {
        var result = Types.InAssembly(marker.Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }
}
