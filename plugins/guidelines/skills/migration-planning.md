# Skill: Migration Planning

**Description:** Plan and execute incremental refactoring to align code with guidelines. Track progress, manage dependencies, and ensure teams can adopt new patterns without disruption.

---

## What This Skill Does

Guides you through:
- **Assessing migration scope** — What needs to change and why
- **Creating migration roadmap** — Breaking large changes into phases
- **Managing dependencies** — Coordinating changes across projects
- **Maintaining stability** — Keeping the system working during migration
- **Team communication** — Getting buy-in and managing knowledge transfer
- **Tracking progress** — Measuring migration completion

---

## When to Migrate

Migrate when:
- ✓ Code significantly violates guidelines (not just minor tweaks)
- ✓ Multiple projects need the same change
- ✓ You have time to plan and execute incrementally
- ✓ The team understands the rationale (reference ADRs)

**Don't migrate** if:
- ✗ It's a one-off edge case
- ✗ The system is in emergency/crisis mode
- ✗ There's no team alignment on why change is needed
- ✗ The cost of migration exceeds the benefit

---

## Phase 1: Assessment

### Understand the Current State

**Use: gap-analysis skill**
```
Invoke: gap-analysis
→ Identify what doesn't align with guidelines
→ Document scope of violation
```

**Example findings:**
```
Violation: Business logic in controllers

Current state:
- 25 controllers with inline business logic
- Logic duplicated across 3+ controllers
- No application layer orchestration
- Tests are integration-only (slow)

Impact:
- Hard to test business rules
- Duplication increases maintenance cost
- New developers confused about code organization
```

### Assess Impact

Ask:
- **How many files?** Small (1-5) vs. Medium (6-20) vs. Large (20+)
- **How many layers?** Isolated (one place) vs. Spread (multiple places)
- **How coupled?** Independent vs. Highly interdependent
- **How risky?** Low (refactoring) vs. High (logic changes)

### Estimate Effort

```
Effort = (Files Affected) × (Complexity) × (Risk Factor)

Simple (1-3 days):
- <10 files
- One layer
- Low risk
- Example: Extracting value object

Medium (1-2 weeks):
- 10-30 files
- 2-3 layers
- Medium risk
- Example: Introducing repository pattern

Complex (2-8 weeks):
- 30+ files
- 3+ layers
- High risk
- Example: Implementing CQRS across domain

Estimate as team → add buffer (2x estimate)
```

---

## Phase 2: Planning

### Create a Roadmap

**Step 1: Define the end state**

```
Current State (as-is):
OrderController handles:
  - Input validation
  - Business logic (calculate totals, apply discounts)
  - Database queries
  - HTTP response mapping

Desired State (to-be):
OrderController handles:
  - Input validation
  - HTTP response mapping
  
CreateOrderHandler handles:
  - Business logic orchestration
  - Domain calls
  
Order aggregate handles:
  - Business rules
  - Calculations
```

**Step 2: Break into phases**

```
Phase 1 (Week 1-2): Foundation
  - Create Application layer project
  - Create first handler (CreateOrderCommand)
  - Create abstractions (IOrderRepository)

Phase 2 (Week 3-4): Gradual Migration
  - Migrate CreateOrder endpoint to use handler
  - Keep GetOrder in controller (unchanged)
  - Dual-write: controller and handler both work

Phase 3 (Week 5): Finish Migration
  - Migrate remaining endpoints
  - Remove old controller logic
  - Delete unused code paths

Phase 4 (Week 6): Test & Stabilize
  - Load testing
  - Verify no regressions
  - Document new patterns
```

**Step 3: Identify dependencies**

```
What must be done first?
  ↓
Phase 1: Foundation (must be done first)
  ├─ Create projects
  ├─ Set up DI
  └─ Create port interfaces
  ↓
Phase 2: Handlers (depends on Phase 1)
  ├─ Implement first handlers
  ├─ Wire to endpoints
  └─ Add tests
  ↓
Phase 3: Cleanup (depends on Phase 2)
  ├─ Remove old code
  └─ Verify no breakage
```

### Get Team Buy-In

**Create a migration charter:**

```markdown
## Order Management Migration Charter

### Why
- Improve testability (now: integration-only tests)
- Reduce duplication (same logic in 3 controllers)
- Enable scaling (new team members need less guidance)

### What
- Extract business logic from OrderController into handlers
- Create Application layer with CQRS pattern
- Introduce repository abstraction

### When
- Start: Monday, Week 1
- Complete: Friday, Week 6
- Minimal disruption (dual-write during Week 2-4)

### Who
- Backend team leads design
- All backend developers participate in coding
- QA validates each phase

### Success Metrics
- All business logic in handlers (0 in controllers)
- Test suite includes 40+ handler unit tests
- No performance regression (load tests pass)
- Documentation updated with new patterns
```

---

## Phase 3: Execution

### Approach: Small, Frequent Changes

**Don't:** Rewrite everything at once

**Do:** Migrate one endpoint at a time

```csharp
// Week 1: Create handler + interface
public sealed record CreateOrderCommand(Guid CustomerId, List<OrderLineDto> Lines);

public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    
    public async Task<OrderDto> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(cmd.CustomerId, cmd.Lines);
        await _orders.SaveAsync(order, ct);
        return new OrderDto { Id = order.Id };
    }
}

// Week 2: Wire into controller (keep old code)
[HttpPost]
public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
{
    try
    {
        // New path via handler
        var command = new CreateOrderCommand(dto.CustomerId, dto.Lines);
        var result = await _mediator.Send(command);  // ← New
        
        // Old path still works (for gradual rollout)
        // var order = new Order { ... };  // ← Old (commented)
        
        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "CreateOrder failed");
        return StatusCode(500, new { error = "Server error" });
    }
}

// Week 3: Remove old code
[HttpPost]
public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
{
    var command = new CreateOrderCommand(dto.CustomerId, dto.Lines);
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

### Testing During Migration

```csharp
// New handler tests (fast, no database)
[Fact]
public async Task CreateOrderHandler_ValidInput_SavesAndReturns()
{
    var handler = new CreateOrderHandler(_fakeOrderRepo);
    var cmd = new CreateOrderCommand(Guid.NewGuid(), [...]);
    
    var result = await handler.Handle(cmd, CancellationToken.None);
    
    Assert.NotNull(result.Id);
}

// Controller tests (ensure endpoint still works)
[Fact]
public async Task CreateOrder_ValidInput_Returns200()
{
    var response = await _client.PostAsJsonAsync("/orders", new { ... });
    
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}

// No need to rewrite integration tests yet
```

### Communication

After each phase, share updates:

```markdown
## Migration Update: End of Week 2

✅ Completed
- Application layer created
- CreateOrderHandler implemented
- GetOrderHandler implemented
- 15 handler unit tests passing

🔄 In Progress
- Wiring handlers into controllers
- Adding more tests

📋 Next
- Migrate remaining endpoints
- Load testing

⚠️ Issues
- None blocking progress
```

---

## Phase 4: Validation

### Verify Success

**Checklist:**

```
✓ All endpoints migrated to handlers
✓ Old code removed (no dead code)
✓ Unit tests for all handlers (80%+ coverage)
✓ Load tests pass (same performance)
✓ No regressions in QA environment
✓ Team can explain new patterns
✓ Documentation updated
✓ Code review sign-off from leads
```

### Lessons Learned

After migration, capture what you learned:

```markdown
## Migration Retrospective

### What Went Well
- Incremental approach kept system stable
- Team picked up CQRS quickly
- Handler tests much faster than integration tests

### What Was Difficult
- Dual-write phase was confusing (old + new code)
- Some business logic was scattered (took time to find)

### Next Time
- Create ADR before migration (not after)
- Schedule team training before starting
- Use feature flags for larger rollouts

### Outcome
- Testability: 5× faster test suite
- Duplication: Reduced from 3 copies to 1
- Team confidence: High (new patterns well understood)
```

---

## Common Migration Patterns

### Pattern 1: Introduce Abstraction (Repository Pattern)

```
Before:
  OrderService → EF Core DbContext → Database

After:
  OrderService → IOrderRepository → EF Core Repository → Database
                 (abstraction added)
```

**Steps:**
1. Create `IOrderRepository` interface
2. Create `EfOrderRepository` implementation
3. Update DI to use abstraction
4. Test with fake repository
5. Remove old direct DbContext usage

**Effort:** 3-5 days per repository

### Pattern 2: Extract Business Logic (CQRS)

```
Before:
  Controller → Service → EF Core

After:
  Controller → Command Handler → Domain → Repository
               (logic extracted)
```

**Steps:**
1. Create handler interface
2. Move logic from service to handler
3. Test handler in isolation
4. Wire controller to handler
5. Remove old service

**Effort:** 1-2 weeks (multiple handlers)

### Pattern 3: Separate Reads/Writes (Query Pattern)

```
Before:
  Controller → Repository (finds + calculates)

After:
  Controller → Query Handler → Read Model
              → Command Handler → Domain Model
```

**Steps:**
1. Identify read vs. write queries
2. Create separate read model (denormalized)
3. Create query handler for reads
4. Keep command handler for writes
5. Test both paths

**Effort:** 2-4 weeks

---

## Risk Management

### Risk 1: Performance Degradation

**Mitigation:**
- Benchmark before migration
- Test at production scale
- Keep old code path for rollback
- Use feature flags

```csharp
// Feature flag for gradual rollout
if (_featureFlags.IsEnabled("new_order_handler"))
{
    return await _newHandler.CreateOrder(dto);
}
else
{
    return await _legacyService.CreateOrder(dto);
}
```

### Risk 2: Introducing Bugs

**Mitigation:**
- Test both old and new paths
- Dual-write to verify correctness
- Canary deployment (1% of traffic first)
- Have rollback plan

### Risk 3: Team Confusion

**Mitigation:**
- Pair programming during migration
- Brown bag sessions on new patterns
- Document as you go
- Reference ADRs and guidelines

---

## Integration with Other Skills

### With gap-analysis

Use gap-analysis to identify what needs migrating

### With code-review

Review each migrated file against guidelines

### With guidelines-mcp

Search for patterns and references during migration

### With feedback-loop

Track which patterns were hardest to migrate

---

## Quick Reference

| Scenario | Approach | Effort | Risk |
|----------|----------|--------|------|
| Extract single value object | Refactoring | 1-2 days | Low |
| Introduce repository pattern | Abstraction | 1 week | Low |
| Extract CQRS handlers | Gradual migration | 2-4 weeks | Medium |
| Implement event sourcing | Major refactor | 6-8 weeks | High |
| Separate read/write models | Dual model | 4-6 weeks | High |

---

## Tips

- **Start small**: Pick one endpoint, migrate it completely, learn from it
- **Keep it working**: Every commit should not break the system
- **Test thoroughly**: New tests + old tests during migration
- **Communicate often**: Let team know progress and blockers
- **Document patterns**: As you implement, document for others
- **Celebrate progress**: Each phase is a win
- **Plan rollback**: Know how to get back to current state
- **Be patient**: Migrations take longer than expected; plan accordingly
