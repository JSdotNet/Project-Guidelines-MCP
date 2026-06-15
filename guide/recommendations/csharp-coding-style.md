---
title: "C# Coding Style"
date: 2026-06-01
status: Accepted
tags: [csharp, coding-style, conventions, dotnet, recommendations]
---
# Recommendation: C# Coding Style

## Purpose

Define a consistent coding style for all C# (.NET) code in projects following these guidelines, aligned with the official Microsoft .NET C# coding conventions.

## Recommendation

Follow the official [Microsoft .NET C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) with the additions below.

### Naming Conventions

- Use `PascalCase` for class, method, and property names.
- Use `camelCase` for local variables and method parameters.
- Use `ALL_CAPS` for constants.
- Prefix interfaces with `I` (e.g., `IOrderService`).
- Use meaningful, descriptive names; avoid abbreviations.

### Formatting

- Use 4 spaces for indentation (no tabs).
- Use **file-scoped namespaces** for all new files; refactor existing files during updates or maintenance.
- Add a blank line between method definitions.
- Place opening braces on a new line for methods, properties, and types.

```csharp
// ✅ File-scoped namespace (required for all new files)
namespace MyNamespace;

public class ExampleClass
{
    // ...
}
```

### Variable Declaration

- Use `var` for local variable declarations when the RHS type is obvious.
- Prefer explicit types when it improves clarity.

```csharp
// ✅ Type obvious — use var
var order = new Order(Guid.NewGuid());
var items = new List<string>();

// ✅ Type non-obvious — explicit is better
IEnumerable<Order> orders = GetOrders();
```

### Sealed Classes

Make classes `sealed` by default. Only omit `sealed` (or use `abstract`) when inheritance is explicitly designed and justified.

```csharp
// ✅
public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
}
```

### Exceptions and Guard Clauses

- Use guard clauses at the top of public methods.
- Use `nameof` to refer to parameter names in exceptions — never hardcode the string.

```csharp
// ✅
public void Process(Order order)
{
    ArgumentNullException.ThrowIfNull(order);
    if (!order.IsValid) throw new InvalidOperationException("Order is not valid");
    // ...
}

// ✅ nameof — not a hardcoded string
throw new ArgumentNullException(nameof(parameterName));
```

### Modern C# Features

- Use pattern matching and expression-bodied members where appropriate.
- Prefer object and collection initializers.
- Use `async`/`await` pervasively; never block on async code (`.Result`, `.Wait()`).

### Code Structure

- One type per file (class, interface, enum, record, etc.).
- Organize files by feature/domain, not by technical layer.
- Group `using` directives at the top of the file, outside the namespace.
- Use partial classes only when necessary (e.g., code generation).

### Comments and Documentation

- Use XML documentation comments (`///`) for public APIs.
- Write comments to explain *why*, not *what*.
- Remove commented-out code before committing.

## Anti-Patterns to Avoid

- Hardcoding parameter names in exception messages instead of using `nameof`.
- Non–file-scoped namespaces in new files.
- Non-sealed classes without an explicit inheritance design.
- Synchronous wrappers around async code (`.Result`, `.Wait()`).
- Multiple types in a single file.

## References

- Microsoft [C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- ADR 0001: Adopt .NET 10 as Target Framework



