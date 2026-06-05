# Skill: Code Review Against Guidelines

**Description:** Validate code changes against the JSdotNet Project Guidelines. Review pull requests, commits, or code snippets to ensure they follow architectural patterns, coding standards, and documented decisions.

---

## What This Skill Does

Reviews code for:
- **Architecture alignment** — Does the code follow Hexagonal/Clean Architecture patterns?
- **Layer isolation** — Are concerns properly separated (Domain, Application, Infrastructure)?
- **Coding standards** — Does code follow C# conventions and ADR requirements?
- **Pattern compliance** — Are established patterns (DDD, CQRS, error handling) correctly applied?
- **Documentation references** — Are design decisions referenced with ADR numbers?

---

## How to Use This Skill

### Quick Review: A Single File or Snippet

**Scenario**: You have a file and want a quick architectural review.

```
1. Paste the code or provide the file path
2. Ask: "Review this code against guidelines"
3. The skill will:
   - Identify the likely layer (Domain, Application, Infrastructure)
   - Check against relevant ADRs
   - Flag any pattern violations or improvements
```

**Example prompt:**
```
Here's my domain entity. Please review it against the guidelines:

public class Order
{
    public decimal Total { get; set; }
    public List<OrderLine> Lines { get; set; }
    
    public void CalculateTotal()
    {
        Total = Lines.Sum(l => l.Quantity * l.Price);
    }
}
```

### Comprehensive PR Review

**Scenario**: You're reviewing a pull request with multiple files.

```
1. List the changed files and their purpose
2. Ask: "Review this PR for architectural and style compliance"
3. For each file, the skill will:
   - Suggest which ADRs/recommendations apply
   - Identify patterns that should be used
   - Recommend searches for detailed guidance
```

**Example prompt:**
```
PR #42: Implement Order Management

Changes:
- OrderAggregate.cs (new)
- CreateOrderCommand.cs + CreateOrderCommandHandler.cs (new)
- OrderRepository.cs (new)
- OrderController.cs (modified)

Please review against guidelines for architectural compliance.
```

### Design Review Before Implementation

**Scenario**: You want to validate your design before coding.

```
1. Describe what you're building
2. Ask: "Is this design aligned with the guidelines?"
3. The skill will:
   - Suggest which layer(s) this belongs in
   - Recommend relevant ADRs and patterns
   - Flag potential architectural issues early
```

**Example prompt:**
```
I need to implement a feature:
- Users can receive notifications via email/SMS
- Each notification method has different retry logic
- Notifications should be sent asynchronously

Should I use:
a) A single NotificationService with strategies?
b) Separate domain/adapter pattern?
c) A message bus?

Review against guidelines.
```

---

## Review Checklist

When reviewing code, the skill checks:

### Architecture & Layering

- [ ] Domain layer contains only business logic (no EF, no external APIs)
- [ ] Application layer orchestrates; doesn't contain domain logic
- [ ] Infrastructure is isolated behind ports/interfaces
- [ ] Controllers/HTTP layer only handle I/O, not business logic
- [ ] Test projects are properly structured

### Patterns & Practices

- [ ] Value Objects are immutable with validation in factory methods
- [ ] Aggregates enforce invariants and expose only behavior
- [ ] Domain Events represent state changes, not actions
- [ ] Repository pattern used for persistence abstraction
- [ ] Dependency Injection used pervasively (no service locators)
- [ ] Constructor injection preferred, fields marked readonly

### C# Coding Standards

- [ ] Nullable reference types enabled; nulls handled properly
- [ ] File-scoped namespaces used (not nested)
- [ ] `var` only when RHS type is obvious
- [ ] Expression-bodied members for trivial code
- [ ] Async/await used; no `.Result` or `.Wait()`
- [ ] Guard clauses at method entry for validation
- [ ] Sealed classes unless extension intended
- [ ] `ILogger<T>` for logging, not static loggers

### Error Handling

- [ ] Domain exceptions for domain validation failures
- [ ] Exceptions translated at layer boundaries, not leaked
- [ ] Result types or exceptions used consistently (not mixed)
- [ ] Exceptions logged once at boundary
- [ ] No silent exception swallowing

### Testing

- [ ] Domain logic has pure unit tests (no mocks)
- [ ] Application handlers tested with fakes for ports
- [ ] Integration tests for infrastructure
- [ ] Naming follows `Method_Should_When` pattern
- [ ] One assertion per conceptual rule (or grouped with FluentAssertions)

---

## Common Review Findings

### Finding 1: Business Logic in Controller

**Pattern:**
```csharp
[HttpPost]
public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
{
    var total = dto.Lines.Sum(l => l.Qty * l.Price);  // ❌ Business logic
    var order = new Order { Total = total, Lines = dto.Lines };
    await _db.SaveChangesAsync();
    return Ok(order);
}
```

**Review:** 
- ❌ Business logic in controller
- ❌ Direct database access
- ❌ DTO to domain mapping not shown

**Guidance:**
```
Search: search_docs("cqrs")
         search_docs_by_tag("application-layer")
Read: get_doc("adr-NNNN: Vertical Slice Pattern")

Recommendation:
1. Move calculation to Order aggregate
2. Create CreateOrderCommand handler
3. Map DTO → Command in controller only
4. Inject IOrderRepository, not _db
```

### Finding 2: Infrastructure Leaking into Domain

**Pattern:**
```csharp
[Table("Orders")]
public class Order
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public string CustomerName { get; set; }
    
    [NotMapped]
    public decimal Total { get; }
}
```

**Review:**
- ❌ EF Core attributes on domain entity
- ❌ Database concerns in domain
- ❌ Violates hexagonal architecture

**Guidance:**
```
Search: search_docs("hexagonal architecture")
         search_docs_by_tag("ports-adapters")
Read: get_doc("adr-NNNN: Domain/Infrastructure Separation")

Recommendation:
1. Remove all EF attributes from domain class
2. Create OrderDb mapped class in Infrastructure
3. Define IOrderRepository port in Application
4. Map Order ↔ OrderDb in repository adapter
```

### Finding 3: Immutability Violation

**Pattern:**
```csharp
public class Money
{
    public decimal Amount { get; set; }      // ❌ Mutable
    public string Currency { get; set; }     // ❌ Mutable
    
    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
}
```

**Review:**
- ❌ Properties are mutable
- ❌ No validation in constructor
- ❌ Validation missing

**Guidance:**
```
Search: search_docs_by_tag("value-object")
Read: get_doc("adr-NNNN: Value Object Pattern")

Recommendation:
1. Use record struct or sealed record for immutability
2. Add static Create factory with validation
3. Throw ArgumentException for invalid inputs
4. Reference in code: // ADR-NNNN: Value Object immutability
```

### Finding 4: Missing ADR Reference

**Pattern:**
```csharp
// Retry logic inline
for (int i = 0; i < 3; i++)
{
    try
    {
        return await _client.CallExternalApiAsync();
    }
    catch (HttpRequestException)
    {
        if (i == 2) throw;
        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
    }
}
```

**Review:**
- ⚠️ Resilience logic present but not referenced
- ⚠️ No clear decision documented

**Guidance:**
```
Search: search_docs_by_tag("resilience")
         search_docs("retry")
Read: get_doc("adr-NNNN: Resilience Policy")

Recommendation:
1. Check if Polly is the standard (per guidelines)
2. Use Polly policy instead of manual retry
3. Add comment: // ADR-NNNN: Exponential backoff via Polly
4. Consider: Should this be in an adapter?
```

---

## Integration with Other Skills

### With guidelines-mcp

Review often leads to "I need to understand this ADR":
```
1. Skill: code-review
   → Find "Use CQRS pattern" recommendation

2. Skill: guidelines-mcp
   → search_docs_by_tag("cqrs")
   → get_doc("adr-NNNN")
   → Understand full decision + consequences
```

### With gap-analysis

Review code to identify structural gaps:
```
1. Run: Skill: gap-analysis
   → Identify missing Application layer

2. Use: Skill: code-review
   → Review existing code to see why Application layer is missing
   → Propose refactoring to extract handlers
```

### With feedback-loop

If review findings repeat across projects:
```
1. Review code, find same pattern violation 3+ times
2. Use: Skill: feedback-loop
   → analyze_guidelines_usage() to see search logs
   → draft_guidelines_issue("Clarify: X pattern usage")
   → Propose better documentation
```

---

## Example Review Workflows

### Workflow 1: Pre-Commit Check

```
1. Write code (e.g., new domain entity)
2. Invoke: Skill: code-review
   → Paste code
   → Ask: "Is this a proper value object?"
3. Review feedback
4. If issues: Search guidelines, fix
5. If clean: Commit with reference: // ADR-NNNN
```

### Workflow 2: PR Review

```
1. Author creates PR with 5 new files
2. Reviewer invokes: Skill: code-review
   → List files and context
   → Ask: "Architectural compliance check"
3. Skill identifies:
   - Domain logic in controller (fix needed)
   - EF attributes on entity (fix needed)
   - Missing error handling (warning)
4. Reviewer creates PR comments with guidance links
5. Author reads ADRs, fixes, pushes again
```

### Workflow 3: Design Before Coding

```
1. Design a feature: "Multi-tenant support"
2. Describe architecture approach
3. Invoke: Skill: code-review
   → Ask: "Is this design aligned with guidelines?"
   → Get feedback: Which ADRs apply? Any red flags?
4. Adjust design based on feedback
5. Code with confidence that design is sound
```

---

## Tips for Effective Reviews

- **Be specific**: Paste actual code, not just descriptions
- **Ask targeted questions**: "Is this immutable?" vs. "Review this code"
- **Reference ADRs after reviews**: Always follow up with `get_doc()` to read full context
- **Document findings**: Add ADR references in comments for traceability
- **Use checklists**: The checklist above helps ensure consistency
- **Pair with feedback-loop**: If findings repeat, propose documentation improvements
- **Review early**: Review designs before implementation to catch issues sooner

---

## Limitations

This skill:
- ✓ Reviews for architectural and pattern compliance
- ✓ Suggests relevant ADRs and improvements
- ✓ Checks C# coding standards
- ✗ Cannot execute code or detect runtime bugs
- ✗ Cannot check performance without profiling data
- ✗ Cannot verify security vulnerabilities (use security tools)

For security/performance concerns, combine with:
- Security scanners (DevSkim, GitHub code scanning)
- Profilers (.NET profiler, benchmarking tools)
- Unit tests (detect logic errors)

---

## Common Questions

**Q: Should I review every commit with this skill?**
A: No—use for architectural concerns, new patterns, or when uncertain. Daily commits don't need reviews. Review PRs and design decisions.

**Q: Can this replace a human code review?**
A: It's a complement, not a replacement. This skill checks architecture and patterns; humans should verify logic, intent, and context.

**Q: What if the code violates a guideline I disagree with?**
A: Good! Use the feedback-loop skill to propose updating the guideline. Discuss with the team before committing.

**Q: How do I handle legacy code that violates guidelines?**
A: Refactor incrementally. Use this skill to identify violations, then use the feedback-loop to track improvements.
