# Skill: Decision Validation

**Description:** Validate architectural and technical decisions before implementation. Ensure decisions align with guidelines, have proper rationale, and consider trade-offs and consequences.

---

## What This Skill Does

Guides you through:
- **Identifying decision type** — Architecture, patterns, infrastructure, tooling
- **Checking guideline alignment** — What do we already say about this?
- **Evaluating alternatives** — Are we missing a better option?
- **Validating rationale** — Is the "why" sound?
- **Assessing consequences** — What are the side effects?
- **Getting team consensus** — Is the decision well-understood?

---

## When to Validate

Validate decisions when:
- ✓ The decision affects multiple projects or layers
- ✓ You're uncertain if the approach aligns with guidelines
- ✓ The team hasn't made this decision before
- ✓ The decision has meaningful trade-offs
- ✓ You want to document the decision for future reference

**Don't validate** if:
- ✗ It's an implementation detail (not architecture)
- ✗ The decision is already documented in guidelines
- ✗ You're just choosing variable names

---

## Validation Framework

### Step 1: Identify the Decision Type

**Question: What kind of decision is this?**

```
Architecture Decisions
├─ How to organize layers (Domain, Application, Infrastructure)
├─ How to separate concerns (DDD vs. Transaction Script)
├─ How to communicate between services (HTTP vs. Events vs. Queues)
└─ How to structure projects (vertical slices vs. horizontal layers)

Pattern Decisions
├─ Which pattern to use (CQRS, Event Sourcing, Repository, etc.)
├─ When to apply a pattern (when is CQRS worth the complexity?)
├─ How to implement a pattern (lightweight vs. full CQRS)
└─ How to combine patterns (CQRS + Event Sourcing)

Infrastructure Decisions
├─ Which database (SQL vs. NoSQL)
├─ Which ORM (EF Core vs. Dapper vs. None)
├─ Which messaging system (RabbitMQ vs. Service Bus vs. Kafka)
└─ Which cache (Redis vs. In-Memory vs. None)

Tooling Decisions
├─ Which frameworks (ASP.NET vs. MinimalAPIs)
├─ Which libraries (MediatR vs. Manual)
├─ Which testing tools (xUnit vs. NUnit)
└─ Which build system (Docker vs. Native)
```

### Step 2: Check Guidelines

**Use: guidelines-mcp skill**

```
Invoke: guidelines-mcp
→ search_guides("your decision topic")
→ search_guides_by_tag("relevant tags")
→ get_guide("adr-NNNN") if found
```

**Example:**
```
Decision: Should we use CQRS?

MCP Search Results:
✓ ADR-0004: CQRS Decision
  "Use CQRS when read/write patterns differ significantly"
  
✓ Recommendation: CQRS Trade-offs
  "Simple systems don't benefit; adds complexity"
  
→ Conclusion: Guidelines support CQRS under certain conditions
→ Next: Verify those conditions apply to our case
```

### Step 3: Clarify the Problem

**Question: What problem are we solving?**

```
Good problem statement:
- "Query performance degrades during peak hours"
- "Business logic is duplicated in 3 controllers"
- "Testing handlers requires full database setup"

Vague problem statement:
- "We should modernize"
- "Everyone else uses this pattern"
- "It's best practice"
```

### Step 4: Validate Alternatives

**Question: Did we consider other options?**

```
Decision: Should we use CQRS?

Alternatives Considered:
1. CQRS with separate read/write models
   Pros: Performance isolation, clear separation
   Cons: Complexity, eventual consistency
   
2. Query optimization (indexes, denormalization)
   Pros: Simple, no new patterns
   Cons: Limited benefit, still tight coupling
   
3. Caching layer (Redis)
   Pros: Easy to add, no code changes
   Cons: Cache invalidation complexity
   
Decision: CQRS
Rationale: Query optimization insufficient; caching adds complexity
          CQRS aligns with guidelines for high-volume reads
```

### Step 5: Check Alignment with Existing Patterns

**Question: Does this fit with how we already do things?**

```
Existing pattern in codebase:
- Handlers orchestrate domain logic
- Repositories abstract persistence
- Domain events published via handlers

Proposed decision: CQRS

Alignment check:
✓ CQRS handlers extend existing handler pattern
✓ Separate read repository follows existing abstraction
✓ Query handlers fit existing DI setup
✓ Aligns with ADR-0004

Conclusion: Good fit; no conflicts
```

### Step 6: Validate Consequences

**Question: What's the impact?**

**Code Impact:**
```
New code needed:
- 5-10 query handlers
- Read model repository
- Query DTOs
- Integration between write and read models

Complexity added:
- Eventual consistency concerns
- Read model maintenance
- Cache invalidation

Team impact:
- Learning curve (2-3 weeks)
- Testing complexity (need read model tests)
```

**Performance Impact:**
```
Baseline (current):
- Query time: 500ms
- Write time: 50ms
- Peak load: 100 requests/second

With CQRS:
- Query time: 50ms (from cache)
- Write time: 55ms (same + event publishing)
- Peak load: 1000 requests/second

Trade-off: Faster queries, eventual consistency
```

**Maintenance Impact:**
```
Current: 1 model to maintain
With CQRS: 2 models (write + read)

Cost: Developers must understand both
Benefit: Clearer separation, easier to optimize each
```

### Step 7: Get Team Consensus

**Question: Does the team understand and agree?**

**Validation checklist:**
- [ ] Problem is clear to all (not just proposer)
- [ ] Alternatives were discussed (not just one option)
- [ ] Trade-offs are understood (not just benefits)
- [ ] Effort is estimated and accepted
- [ ] Team can explain the "why" (not just "it's the pattern")

**Team discussion example:**
```
Question: Why CQRS instead of just caching?

Good answer:
"Caching doesn't solve the core problem: tight coupling between
read and write models. CQRS separates them, so we can scale
reads independently. Aligns with ADR-0004."

Weak answer:
"CQRS is modern and everyone uses it."
```

---

## Decision Validation Checklist

Before implementing, verify:

```
✓ Problem Statement
  □ Clear and specific
  □ Quantified if possible (performance, duplication, etc.)
  □ Understood by team

✓ Guideline Alignment
  □ Searched MCP for related ADRs
  □ Found one that supports this decision (or noted absence)
  □ Decision aligns with existing principles

✓ Alternatives Considered
  □ At least 2 alternatives explored
  □ Trade-offs documented for each
  □ Selected option clearly justified

✓ Consequences Understood
  □ Code impact estimated (files, complexity)
  □ Performance impact analyzed
  □ Team impact considered
  □ Risk mitigation planned

✓ Team Consensus
  □ Team discussed and agreed
  □ Everyone can explain the rationale
  □ Effort accepted (schedule, resources)

✓ Documentation
  □ Decision will be documented (ADR or comment)
  □ Rationale linked in code
  □ Team knows where to find it
```

---

## Validation Scenarios

### Scenario 1: Validate CQRS Introduction

```
Decision: Implement CQRS for Order Management

Step 1: Problem
"Query performance is slow during peak hours; read and write 
patterns are completely different"

Step 2: Guidelines Check
search_guides_by_tag("cqrs")
get_guide("adr-0004-cqrs-pattern")
Result: ADR-0004 recommends CQRS when read/write patterns differ

Step 3: Alternatives
- Option A: CQRS with eventual consistency (chosen)
- Option B: Optimize existing model with caching
- Option C: No change (maintain status quo)

Step 4: Consequences
- Complexity: High (separate read/write models)
- Performance: Significant improvement (10× faster reads)
- Team: 2-3 week learning curve
- Timeline: 4 weeks to implement

Step 5: Validation
✓ Aligns with ADR-0004
✓ Trade-offs well understood
✓ Team consensus obtained
✓ Timeline accepted

Status: APPROVED - Proceed with implementation
```

### Scenario 2: Validate Repository Pattern Introduction

```
Decision: Extract repository pattern from data access layer

Step 1: Problem
"Data access code is scattered; hard to test business logic 
without database; no abstraction for persistence"

Step 2: Guidelines Check
search_guides_by_tag("repository")
get_guide("adr-0003-repository-pattern")
Result: ADR-0003 recommends repository for persistence abstraction

Step 3: Alternatives
- Option A: Repository pattern (chosen)
- Option B: Entity Framework Core DbContext directly
- Option C: Data mapper pattern

Step 4: Consequences
- Complexity: Low (straightforward pattern)
- Performance: Minimal impact (same queries)
- Team: Easy to learn (familiar pattern)
- Timeline: 1-2 weeks

Step 5: Validation
✓ Aligns with ADR-0003
✓ Low complexity, high benefit
✓ Team agrees
✓ Low risk

Status: APPROVED - Start immediately
```

### Scenario 3: Reject Premature Complexity

```
Decision: Should we implement event sourcing from the start?

Step 1: Problem
"We want a robust audit trail for all order changes"

Step 2: Guidelines Check
search_guides("event sourcing")
Result: No specific guideline; recommendations suggest incremental adoption

Step 3: Alternatives
- Option A: Event sourcing (proposed)
- Option B: Audit table with triggers
- Option C: Application-level audit logging

Step 4: Consequences
- Event sourcing: Very high complexity (architectural change)
- Team: 4-6 week learning curve, significant ongoing maintenance
- Timeline: 8-12 weeks to implement correctly
- Risk: High (not proven in your context)

Step 5: Validation
✗ Audit trail requirement doesn't justify event sourcing complexity
✗ Simpler alternatives exist (audit table, logging)
✗ No guideline recommends this for your use case
✗ Risk/reward doesn't justify effort

Status: REJECTED - Use audit table instead
        Re-evaluate event sourcing after system stabilizes
```

---

## Red Flags

Stop and reconsider if:

```
🚩 No clear problem statement
   "We should do X" (why?)

🚩 Not aligned with guidelines
   "I googled this and it looks cool"

🚩 No alternatives considered
   "There's only one way to do this"

🚩 Solving non-existent problems
   "It might help someday" (but not now)

🚩 Too much complexity for current scale
   "Enterprise-grade when we're startup-scale"

🚩 Team doesn't understand
   "I'll explain after we implement it"

🚩 Pressure-driven decisions
   "We need this done by Friday"
   (→ Usually means insufficient validation)
```

---

## Integration with Other Skills

### With guidelines-mcp

Search for related ADRs and recommendations

### With code-review

Code review should check if implementation matches validated decision

### With gap-analysis

Gap analysis might identify decisions that need validation

### With migration-planning

Migration decisions should be validated before execution

---

## Quick Checklist Before Coding

```
Before you write a single line of code for a decision:

□ Problem is clear and specific
□ I searched the guidelines (MCP)
□ At least 2 alternatives considered
□ Team understands the trade-offs
□ Timeline and effort estimated
□ Risk mitigation planned
□ I can explain the "why" in one sentence

If any checkbox is unchecked: STOP
→ Spend 30 minutes validating before proceeding
→ Often catches issues that would take days to fix later
```

---

## Tips

- **Validate early**: Catch issues before coding starts
- **Document the decision**: Write it down (even if not an ADR)
- **Reference guidelines**: Always cite the ADR or recommendation
- **Consider trade-offs**: Every decision sacrifices something
- **Get team buy-in**: Consensus is cheaper than conflicts later
- **Record alternatives**: Helps future maintainers understand why
- **Be willing to reject**: "No decision" is sometimes the right call
- **Revisit periodically**: Valid today might not be valid in 6 months
