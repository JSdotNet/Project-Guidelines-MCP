---
title: "Object Calisthenics for Domain Code"
date: 2026-06-01
status: Accepted
tags: [domain, csharp, object-calisthenics, clean-code, recommendations]
---
# Recommendation: Object Calisthenics for Domain Code

## Purpose

Apply the 9 Object Calisthenics rules primarily to business domain classes (aggregates, entities, value objects, domain services) to produce clean, maintainable, and robust domain logic.

## Scope

| Layer | Application |
|---|---|
| Domain (aggregates, entities, value objects, domain services) | **Required** |
| Application layer (handlers, use case services) | **Recommended** |
| Infrastructure, DTOs, API models, configuration classes | **Exempt** |

## The 9 Rules

> ⚠️ These are the 9 original Object Calisthenics rules. No additional rules should be added, and none should be removed.

### 1. One Level of Indentation per Method

Methods must not exceed one level of indentation. Extract helper methods or use LINQ to flatten nested logic.

```csharp
// ❌ Multiple levels of indentation
public void SendNewsletter()
{
    foreach (var user in users)
    {
        if (user.IsActive)
        {
            mailer.Send(user.Email);
        }
    }
}

// ✅ Extracted / LINQ-filtered
public void SendNewsletter()
{
    foreach (var user in users.Where(u => u.IsActive))
        mailer.Send(user.Email);
}
```

### 2. Don't Use the `else` Keyword

Avoid `else`. Use early returns and guard clauses (fail fast) instead.

```csharp
// ❌
public void ProcessOrder(Order order)
{
    if (order.IsValid) { /* ... */ }
    else { throw new InvalidOperationException("Invalid order"); }
}

// ✅
public void ProcessOrder(Order order)
{
    ArgumentNullException.ThrowIfNull(order);
    if (!order.IsValid) throw new InvalidOperationException("Invalid order");
    // ...
}
```

### 3. Wrap All Primitives and Strings

Avoid using raw primitives to represent domain concepts that carry validation or behavior. Wrap them in value objects.

```csharp
// ❌ Raw primitive — no validation, no semantics
public void Register(string email) { ... }

// ✅ Wrapped — validation and semantics at construction
public void Register(EmailAddress email) { ... }
```

### 4. First Class Collections

A class that contains a collection as its only meaningful data should encapsulate all collection behavior. A class with a collection attribute must not contain other attributes.

```csharp
// ✅ First-class collection
public sealed class OrderLines
{
    private readonly List<OrderLine> _lines = new();

    public void Add(OrderLine line) { ... }
    public IReadOnlyList<OrderLine> Active() => _lines.Where(l => !l.IsRemoved).ToList();
    public decimal TotalPrice() => _lines.Sum(l => l.UnitPrice * l.Quantity);
}
```

### 5. One Dot per Line

Limit method chain depth to one dot per line to avoid tight coupling and improve debuggability.

```csharp
// ❌
var email = order.Customer.GetContact().Email.ToUpperInvariant().Trim();

// ✅
var contact = order.Customer.GetContact();
var email = contact.Email.ToUpperInvariant().Trim();
```

### 6. Don't Abbreviate

Use full, meaningful names for classes, methods, and variables.

```csharp
// ❌
var ord = new Ord();
int qty = 3;

// ✅
var order = new Order();
int quantity = 3;
```

### 7. Keep Entities Small

| Constraint | Limit |
|---|---|
| Methods per class | ≤ 10 |
| Lines per class | ≤ 50 |
| Classes per namespace/package | ≤ 10 |

Extract new classes when approaching these limits. Each class should have a single, clear responsibility.

### 8. No Classes with More Than Two Instance Variables

Limit constructor-injected dependencies to two (loggers are excluded from the count). If more collaborators are needed, introduce a facade or composite service.

```csharp
// ❌ Too many collaborators
public CreateOrderCommandHandler(
    IOrderRepository orders,
    IEmailService email,
    ISmsService sms,
    ILogger<CreateOrderCommandHandler> logger) { ... }

// ✅ Collapsed into a facade
public CreateOrderCommandHandler(
    IOrderRepository orders,
    INotificationService notifications,   // facade over email + sms
    ILogger<CreateOrderCommandHandler> logger) { ... }
```

### 9. No Getters/Setters in Domain Classes

Domain entity and aggregate state must only change through explicit behavior methods. Properties must use `private set` or be init-only. Public setters are forbidden on domain objects.

```csharp
// ❌ Domain class with public setter
public class Customer
{
    public string Name { get; set; }
}

// ✅
public sealed class Customer
{
    public string Name { get; private set; }

    private Customer(string name) => Name = name;

    public static Customer Register(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Customer(name);
    }

    public void Rename(string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);
        Name = newName;
    }
}
```

## Exemptions

Rules 3 (wrap primitives), 8 (two instance variables), and 9 (no getters/setters) may be relaxed for:

- DTOs and API request/response models
- Configuration classes
- Infrastructure code where pragmatism outweighs ceremony

## References

- [Object Calisthenics — Jeff Bay (original)](https://www.cs.helsinki.fi/u/luontola/tdd-2009/ext/ObjectCalisthenics.pdf)
- [ThoughtWorks — Object Calisthenics](https://www.thoughtworks.com/insights/blog/object-calisthenics)
- Design: Pragmatic Domain-Driven Design Approach
- Recommendation: C# Coding Style



