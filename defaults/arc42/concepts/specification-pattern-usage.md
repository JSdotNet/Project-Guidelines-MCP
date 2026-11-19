---
title: "Specification Pattern for Business Rules"
date: 2026-06-01
status: Accepted
tags: [domain, ddd, specification-pattern, business-rules, csharp, recommendations]
---
# Recommendation: Specification Pattern for Business Rules

## Purpose

Define when and how to use the Specification pattern to encapsulate complex or reusable business rules, while keeping simple invariants directly inside aggregate methods.

## Core Principle

Business rules and invariants belong **inside the aggregate root**. The Specification pattern is a focused tool for specific scenarios — not a default for all validation logic.

## When to Use Specifications

| Scenario | Recommendation |
|---|---|
| Simple field validation (quantity > 0, required fields) | ❌ Keep inline in aggregate method |
| Complex multi-condition business rule | ✅ Use Specification |
| Rule reused across multiple aggregates or contexts | ✅ Use Specification |
| Combinable rule needing And / Or / Not operators | ✅ Use Specification |
| Rule requiring external data or cross-aggregate validation | ✅ Use Specification (via domain service) |

## Interface

Prefer the shared kernel's `ISpecification<T>` if one exists. Create a local interface only if no shared one is available.

```csharp
// Shared kernel (preferred)
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
}
```

## Simple Business Rule — Keep Inline

```csharp
public sealed class Order
{
    public void AddItem(string productName, int quantity, decimal price)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(price));

        _orderLines.Add(new OrderLine(productName, quantity, price));
    }
}
```

## Complex Business Rule — Use Specification

```csharp
public sealed class CustomerEligibleForDiscountSpecification : ISpecification<Customer>
{
    public bool IsSatisfiedBy(Customer customer) =>
        customer.IsActive &&
        customer.TotalOrdersAmount > 1000m &&
        customer.MembershipLevel == MembershipLevel.Premium;

    // Static helper for direct usage in aggregates
    public static bool Check(Customer customer) =>
        new CustomerEligibleForDiscountSpecification().IsSatisfiedBy(customer);
}

// Aggregate using the specification
public sealed class Order
{
    public void ApplyDiscount(Customer customer)
    {
        if (!CustomerEligibleForDiscountSpecification.Check(customer))
            throw new InvalidOperationException("Customer is not eligible for discount");

        // Apply discount...
    }
}
```

## Combinable Specifications

Use `AndSpecification<T>` (and equivalent `OrSpecification<T>`, `NotSpecification<T>`) when specifications need to be composed at runtime.

```csharp
public sealed class AndSpecification<T> : ISpecification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public bool IsSatisfiedBy(T entity) =>
        _left.IsSatisfiedBy(entity) && _right.IsSatisfiedBy(entity);
}

// Usage
var eligible = new AndSpecification<Customer>(
    new CustomerIsActiveSpecification(),
    new CustomerHasMinimumOrdersSpecification());

if (!eligible.IsSatisfiedBy(customer))
    throw new InvalidOperationException("Customer does not meet criteria");
```

## Best Practices

- Name specifications in ubiquitous language: `CustomerEligibleForDiscount`, not `CheckCustomer`.
- Prefer a static `Check` / `IsSatisfiedBy` method on the specification for stateless rules.
- Test specifications independently of aggregates.
- Use domain services to coordinate cross-aggregate validation that requires external data.
- Place specifications in the same feature folder as the aggregate that uses them, or in a shared `Specifications/` subfolder when they are reused across features.

## Anti-Patterns to Avoid

- Wrapping every single-field guard in a specification class (over-engineering).
- Placing specification logic in application or infrastructure layers.
- Using generic / CRUD-named specifications (`ValidateCustomer`, `CheckOrder`).
- Creating specifications for rules that are only ever used once inside a single aggregate method.

## References

- Eric Evans, *Domain-Driven Design: Tackling Complexity in the Heart of Software*
- Vaughn Vernon, *Implementing Domain-Driven Design*
- Martin Fowler, [Specification pattern](https://www.martinfowler.com/apsupp/spec.pdf)
- Design: Pragmatic Domain-Driven Design Approach



