---
title: "Modular Solution Structure Template"
date: 2025-11-12
status: Accepted
tags: [structure, modular, architecture, aspire, template]
---
# Modular Solution Structure Template

**Status**: Accepted  
**Date**: 2025-11-12  
**Author**: Design Guidelines Team

## Overview

This document defines the canonical solution and filesystem scaffold for .NET modular monolith projects, especially web-enabled applications (Web APIs, Web Apps).

## Scope

This template is intentionally prescriptive about placement and naming.

Included here:

- Folder and project layout
- Naming conventions
- Per-project responsibilities
- Copy/paste examples

Intentionally not covered in depth here:

- Architectural rationale and trade-offs
- Boundary and consistency strategy
- Evolution and extraction strategy

For those concerns, see `designs/modular-monolith-architecture-design.md`.

## Proposed Solution Structure

This template covers the **multi-module** solution profile.

For single-module solutions, use `structures/simple-solution-structure.md`.

All test projects are placed under `solution-root/tests/`.

### High-Level Organization

```
solution-root/
├── src/
│   ├── App/                                       # Frontend application (Angular, Blazor, React)
│   ├── Aspire/                                    # Orchestration projects (web-enabled only)
│   │   ├── Company.Webshop.Aspire.AppHost/
│   │   └── Company.Webshop.Aspire.ServiceDefaults/
│   ├── Core/                                      # Shared CQRS interfaces (ICommandHandler, IQueryHandler, IClock)
│   ├── ModuleName/                                # Domain/Module folders (short name: Orders, Conferences)
│   │   ├── Company.Webshop.ModuleName/
│   │   ├── Company.Webshop.ModuleName.Abstractions/
│   │   ├── Company.Webshop.ModuleName.Api/        # Every web-exposed module has its own API
│   │   ├── Company.Webshop.ModuleName.Data.{StorageType}/
│   └── Shared/                                    # Optional: Cross-module integration events
│       └── Company.Webshop.IntegrationEvents/
├── tests/
│   ├── Company.Webshop.ModuleName.UnitTests/
│   ├── Company.Webshop.IntegrationTests/
│   ├── Company.Webshop.ArchitectureTests/
│   └── Company.Webshop.E2ETests/
└── Company.Webshop.sln
```

### Example: Multi-Module Solution

For a product called "Webshop" by company "Company" with Inventory, Users, and Catalog modules:

```
Company.Webshop/
├── src/
│   ├── App/                                           (Angular / Blazor frontend)
│   ├── Aspire/
│   │   ├── Company.Webshop.Aspire.AppHost/
│   │   └── Company.Webshop.Aspire.ServiceDefaults/
│   ├── Core/
│   │   └── Company.Webshop.Core/                    (ICommandHandler, IQueryHandler, IClock)
│   ├── Inventory/
│   │   ├── Company.Webshop.Inventory/
│   │   ├── Company.Webshop.Inventory.Abstractions/
│   │   ├── Company.Webshop.Inventory.Api/
│   │   ├── Company.Webshop.Inventory.Data.SqlServer/
│   │   └── README.md
│   ├── Users/
│   │   ├── Company.Webshop.Users/
│   │   ├── Company.Webshop.Users.Abstractions/
│   │   ├── Company.Webshop.Users.Api/
│   │   ├── Company.Webshop.Users.Data.CosmosDb/
│   │   └── README.md
│   └── Catalog/
│       ├── Company.Webshop.Catalog/
│       ├── Company.Webshop.Catalog.Abstractions/
│       ├── Company.Webshop.Catalog.Api/
│       ├── Company.Webshop.Catalog.Data.MongoDb/
│       └── README.md
├── tests/
│   ├── Company.Webshop.Inventory.UnitTests/
│   ├── Company.Webshop.Users.UnitTests/
│   ├── Company.Webshop.Catalog.UnitTests/
│   ├── Company.Webshop.IntegrationTests/
│   ├── Company.Webshop.ArchitectureTests/
│   └── Company.Webshop.E2ETests/
└── Company.Webshop.sln
```

## Detailed Component Guidelines

### 1. Aspire Folder (Web-Enabled Projects Only)

**When to use**: Projects exposing HTTP endpoints (Web APIs, Web Apps, gRPC services)

**Contents**:

- **`Company.Webshop.Aspire.AppHost`**: Aspire orchestration project that defines service topology, dependencies, and local development environment
- **`Company.Webshop.Aspire.ServiceDefaults`**: Shared configurations for observability, health checks, service discovery, and common middleware

**Purpose**: Centralizes distributed application orchestration and shared service configurations.

**Reference**: See ADR 0003 for detailed Aspire adoption guidance.

### 2. Module/Domain Folders

Each business domain or bounded context gets its own folder containing related projects.

#### Core Library: `Company.Webshop.ModuleName`

**Purpose**: Contains domain logic, business rules, and application services.

**Contents**:

- Domain entities and value objects
- Domain services
- Application use cases/handlers (CQRS commands/queries)
- Validators and business rules
- Internal implementations

**Dependencies**:

- May reference `Company.Webshop.ModuleName.Abstractions`
- Should NOT reference other modules directly (use abstractions)

**Example**:

```
Company.Webshop.Persons/
├── Entities/
│   ├── Person.cs
│   └── Address.cs
├── Services/
│   └── PersonService.cs
├── Commands/
│   └── CreatePersonCommand.cs
└── Validators/
    └── PersonValidator.cs
```

#### Abstractions Library: `Company.Webshop.ModuleName.Abstractions`

**Purpose**: Defines contracts that other modules can depend on without circular references.

**Contents**:

- DTOs as C# `record` types
- Repository interfaces (`IPersonRepository`)
- Service interfaces (`IPersonService`)
- Event definitions
- Shared enums and constants

**Dependencies**: Minimal - only framework dependencies and shared kernel abstractions

**Why separate abstractions**:

- Enables other modules to depend on contracts without implementation
- Reduces coupling between modules
- Facilitates testing with mocks/stubs
- Supports plugin architectures

**Example**:

```csharp
namespace Company.Webshop.Persons.Abstractions;

// DTO
public record PersonDto(Guid Id, string FirstName, string LastName, string Email);

// Repository interface
public interface IPersonRepository
{
    Task<PersonDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> CreateAsync(PersonDto person, CancellationToken cancellationToken);
}

// Service interface
public interface IPersonService
{
    Task<PersonDto?> GetPersonAsync(Guid id);
    Task<Guid> CreatePersonAsync(PersonDto person);
}
```

#### Data Access: `Company.Webshop.ModuleName.Data.{StorageType}`

**When to use**: Module requires persistent storage

**Naming convention**: `{StorageType}` replaced with actual storage technology:

- `SqlServer`
- `PostgreSQL`
- `MongoDb`
- `CosmosDb`
- `Redis`
- `TableStorage`
- `Blob` (for blob/file storage)

**Contents**:

- Repository implementations
- Entity configurations (EF Core)
- Database context
- Migrations
- Data access utilities

**Dependencies**:

- References `Company.Webshop.ModuleName.Abstractions` (implements interfaces)
- Storage-specific NuGet packages (Npgsql, MongoDB.Driver, etc.)

**Example**:

```csharp
namespace Company.Webshop.Persons.Data.SqlServer;

public class PersonRepository : IPersonRepository
{
    private readonly PersonDbContext _context;
    
    public PersonRepository(PersonDbContext context)
    {
        _context = context;
    }
    
    public async Task<PersonDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Persons.FindAsync(new object[] { id }, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }
}
```

#### Tests (Root-Level): `tests/Company.Webshop.*Tests`

**Mandatory**: Every module MUST have unit tests

**Framework**: xUnit (see recommendations/testing-unit-xunit-moq-bogus.md)

**Contents**:

- Unit tests for domain logic
- Service tests
- Validator tests
- Test fixtures and helpers

**Naming convention**: `{ClassUnderTest}Tests.cs`

**Coverage requirement**: Minimum 80% line coverage (see ADR 0001)

### Testing Project Layout Options

All test projects are placed under `solution-root/tests/`.

Recommended test project set:

- `tests/Company.Webshop.{Module}.UnitTests/`
- `tests/Company.Webshop.IntegrationTests/` (single shared integration project)
- `tests/Company.Webshop.ArchitectureTests/` (shared) or `tests/Company.Webshop.{Module}.ArchitectureTests/` (module-specific)
- `tests/Company.Webshop.E2ETests/`

Why this policy:

- Improves discoverability and onboarding.
- Simplifies CI pipeline targeting by test category.
- Keeps production code and test code physically separated.
- Supports cross-module end-to-end tests without coupling them to one module folder.

For architecture tests, each production assembly should provide a marker type (for example `AssemblyReference.cs`) so tests can reference assemblies safely.

**Example**:

```csharp
namespace Company.Webshop.Persons.Tests;

public class PersonServiceTests
{
    [Fact]
    public async Task CreatePerson_WithValidData_ReturnsPersonId()
    {
        // Arrange
        var repository = new Mock<IPersonRepository>().Object;
        var service = new PersonService(repository);
        var dto = new PersonDto(Guid.Empty, "John", "Doe", "john@example.com");
        
        // Act
        var result = await service.CreatePersonAsync(dto);
        
        // Assert
        Assert.NotEqual(Guid.Empty, result);
    }
}
```

#### Per-Module API: `Company.Webshop.ModuleName.Api`

**Purpose**: Every web-exposed module has its own independently deployable API project.

**Contents**:

- Minimal API endpoints
- Authorization policies
- Background services
- OpenAPI/Scalar configuration
- Program.cs

**Example structure**:

```
Company.Webshop.Users.Api/
├── Endpoints/
│   ├── UserEndpoints.cs
│   └── AuthEndpoints.cs
├── Authorization/
│   └── UserAuthorizationHandler.cs
├── Program.cs
└── appsettings.json
```

**Program.cs** wires Aspire service defaults and the module:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapDefaultEndpoints();
app.MapUserEndpoints();
app.MapOpenApi();
app.Run();
```

**Example endpoint**:

```csharp
public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/users").WithTags("Users");
        
        group.MapGet("/{id:guid}", GetUserAsync)
            .WithName("GetUser")
            .Produces<UserDto>()
            .Produces(404);
            
        group.MapPost("/", CreateUserAsync)
            .WithName("CreateUser")
            .Produces<Guid>(201);
    }
}
```

## Naming Conventions

### Project Names

**Pattern**: `{Company}.{Product}.{Module}[.{Layer}][.{Technology}]`

**Components**:

- `Company`: Organization name (e.g., Company, Contoso, Fabrikam)
- `Product`: Product/solution name (e.g., Webshop, Ordering, Catalog)
- `Module`: Domain/module name (e.g., Persons, Inventory, Shipping)
- `Layer`: Optional layer designation (Abstractions, Data, Api)
- `Technology`: For data projects, the storage type (SqlServer, MongoDb)

**Examples**:

- ✅ `Company.Webshop.Persons`
- ✅ `Company.Webshop.Persons.Abstractions`
- ✅ `Company.Webshop.Persons.Data.PostgreSQL`
- ✅ `Company.Webshop.Persons.Api`
- ✅ `Company.Webshop.Persons.UnitTests`
- ✅ `Company.Webshop.IntegrationTests`
- ✅ `Company.Webshop.E2ETests`
- ❌ `Persons` (too generic)
- ❌ `Company.Webshop.PersonsAbstractions` (missing dot separator)
- ❌ `Company.Webshop.Persons.Postgres` (use PostgreSQL for consistency)

### Folder Names

**Pattern**: Use module name without company/product prefix

**Examples**:

- ✅ `Persons/` (not `Company.Webshop.Persons/`)
- ✅ `Inventory/`
- ✅ `Aspire/`
- ✅ `SharedKernel/`

## Dependency Rules

### Allowed Dependencies

```
Aspire Projects
  ↓ (can orchestrate)
Module APIs
  ↓ (can reference)
Module Core Libraries
  ↓ (can reference)
Module Abstractions ← Module Data Projects
  ↓ (can reference)
Shared Kernel
```

### Forbidden Dependencies

❌ Module Core → Other Module Core (use abstractions instead)  
❌ Module Abstractions → Module Core (circular dependency)  
❌ Module Tests → Other Module Core (test only your module)  
❌ Data Projects → Module Core (should only reference abstractions)

## Decision Guidelines

### When to Create a Separate Module

Create a new module folder when:

- ✅ Represents a distinct bounded context or domain
- ✅ Has its own data model and business rules
- ✅ Could potentially become an independent service
- ✅ Managed by a different team or sub-team
- ✅ Has different scaling or deployment requirements

### When to Use Abstractions Project

Create a separate abstractions project when:

- ✅ Other modules need to call your module's services
- ✅ You want to enable testing without concrete implementations
- ✅ You're building a plugin/extensibility system
- ✅ Multiple data implementations exist (e.g., SQL + NoSQL)

Skip abstractions if:

- ❌ Module is completely isolated with no external consumers
- ❌ Only used internally within a single bounded context

### When to Create Independent API

Create a separate API project when:

- ✅ Module needs to scale independently
- ✅ Module deployed to different infrastructure
- ✅ Module owned by separate team with independent release cycle
- ✅ Module communicates with other modules via events or async messaging
- ✅ Building microservices architecture

Use modular monolith (single API) when:

- ❌ Modules share the same deployment cadence
- ❌ Team is small or just starting
- ❌ No clear need for independent scaling
- ❌ Cross-module transactions are common

## Migration Path

### From Monolith to Modular Monolith

1. Create module folders for existing features
2. Extract abstractions into separate projects
3. Move domain logic into module core libraries
4. Create data projects for each storage boundary
5. Update dependency references

### From Modular Monolith to Microservices

1. Ensure module has its own data project (no shared database)
2. Create `Module.Api` project with HTTP endpoints
3. Replace direct references with HTTP clients or messaging
4. Move module to separate deployment pipeline
5. Update Aspire orchestration configuration

## Tools and Automation

### Solution File Organization

Use solution folders to organize projects:

```
Solution 'Company.Webshop'
├── src
│   ├── Aspire
│   │   ├── Company.Webshop.Aspire.AppHost
│   │   └── Company.Webshop.Aspire.ServiceDefaults
│   ├── Inventory
│   │   ├── Company.Webshop.Inventory
│   │   ├── Company.Webshop.Inventory.Abstractions
│   │   └── Company.Webshop.Inventory.Data.SqlServer
│   └── Users
│       ├── Company.Webshop.Users
│       └── Company.Webshop.Users.Data.CosmosDb
└── tests
  ├── Company.Webshop.Inventory.UnitTests
  ├── Company.Webshop.Users.UnitTests
  ├── Company.Webshop.Catalog.UnitTests
    └── Company.Webshop.IntegrationTests
```

### Directory.Build.props

Place at solution root to enforce consistency:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  
  <PropertyGroup>
    <Company>Company</Company>
    <Product>Webshop</Product>
    <Copyright>Copyright © Company 2025</Copyright>
  </PropertyGroup>
</Project>
```

## Anti-Patterns to Avoid

❌ **Kitchen Sink Module**: Single massive module containing multiple domains  
❌ **Shared Data Project**: One data project accessing multiple module databases  
❌ **Circular Dependencies**: Module A depends on Module B which depends on Module A  
❌ **Leaky Abstractions**: Exposing EF Core entities or storage-specific types in abstractions  
❌ **God Abstractions Project**: Single abstractions project for entire solution  
❌ **Mixed Responsibilities**: Business logic in API controllers or data projects  
❌ **Missing Tests**: Modules without corresponding test projects

## Examples

### Small Project (Modular Monolith)

```
Contoso.Shop/
├── src/
│   ├── Aspire/
│   │   ├── Contoso.Shop.Aspire.AppHost/
│   │   └── Contoso.Shop.Aspire.ServiceDefaults/
│   ├── Catalog/
│   │   ├── Contoso.Shop.Catalog/
│   │   ├── Contoso.Shop.Catalog.Abstractions/
│   │   ├── Contoso.Shop.Catalog.Data.SqlServer/
│   │   └── README.md
│   └── Orders/
│       ├── Contoso.Shop.Orders/
│       ├── Contoso.Shop.Orders.Abstractions/
│       ├── Contoso.Shop.Orders.Data.SqlServer/
│       └── README.md
├── tests/
│   ├── Contoso.Shop.Catalog.UnitTests/
│   ├── Contoso.Shop.Orders.UnitTests/
│   ├── Contoso.Shop.IntegrationTests/
│   ├── Contoso.Shop.ArchitectureTests/
│   └── Contoso.Shop.E2ETests/
└── Contoso.Shop.sln
```

### Large Project (Microservices)

```
Contoso.Ecommerce/
├── src/
│   ├── Aspire/
│   │   ├── Contoso.Ecommerce.Aspire.AppHost/
│   │   └── Contoso.Ecommerce.Aspire.ServiceDefaults/
│   ├── Catalog/
│   │   ├── Contoso.Ecommerce.Catalog/
│   │   ├── Contoso.Ecommerce.Catalog.Abstractions/
│   │   ├── Contoso.Ecommerce.Catalog.Data.MongoDb/
│   │   ├── Contoso.Ecommerce.Catalog.Api/
│   │   └── README.md
│   ├── Orders/
│   │   ├── Contoso.Ecommerce.Orders/
│   │   ├── Contoso.Ecommerce.Orders.Abstractions/
│   │   ├── Contoso.Ecommerce.Orders.Data.SqlServer/
│   │   ├── Contoso.Ecommerce.Orders.Api/
│   │   └── README.md
│   ├── Payments/
│   │   ├── Contoso.Ecommerce.Payments/
│   │   ├── Contoso.Ecommerce.Payments.Abstractions/
│   │   ├── Contoso.Ecommerce.Payments.Data.CosmosDb/
│   │   ├── Contoso.Ecommerce.Payments.Api/
│   │   └── README.md
│   └── SharedKernel/
│       ├── Contoso.Ecommerce.SharedKernel/
│       └── Contoso.Ecommerce.SharedKernel.Abstractions/
├── tests/
│   ├── Contoso.Ecommerce.Catalog.UnitTests/
│   ├── Contoso.Ecommerce.Orders.UnitTests/
│   ├── Contoso.Ecommerce.Payments.UnitTests/
│   ├── Contoso.Ecommerce.ArchitectureTests/
│   ├── Contoso.Ecommerce.IntegrationTests/
│   └── Contoso.Ecommerce.E2ETests/
└── Contoso.Ecommerce.sln
```

## Related Documents

- **ADR 0005**: Modular Monolith Project Structure
- **ADR 0003**: Recommend Aspire for ASP.NET Projects
- **ADR 0006**: CQRS Recommendation for ASP.NET API
- **Design**: Simple Solution Structure
- **Recommendation**: Unit Testing with xUnit, Moq, Bogus
- **Recommendation**: Integration Testing
- **Recommendation**: End-to-End Testing

## Summary

This structure provides:

- ✅ Clear separation of concerns
- ✅ Explicit dependencies via abstractions
- ✅ Testability with xUnit
- ✅ Flexibility to evolve from monolith to microservices
- ✅ Consistent naming across teams
- ✅ Technology-agnostic module core
- ✅ Aspire integration for modern distributed apps

Follow these guidelines to create maintainable, scalable, and well-organized .NET solutions.
