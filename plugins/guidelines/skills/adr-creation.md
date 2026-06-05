# Skill: ADR Creation Assistant

**Description:** Create well-structured Architecture Decision Records (ADRs) that capture decisions, rationale, and consequences. Ensures new ADRs follow established patterns and link to related decisions.

---

## What This Skill Does

Helps you create ADRs that:
- Follow the MADR (Markdown Architecture Decision Record) format
- Are scoped and specific (not mixing multiple decisions)
- Include clear consequences and alternatives considered
- Link to related ADRs for traceability
- Are ready for team review with minimal iteration

---

## When to Create an ADR

Create an ADR when you make a **significant technical decision** that:

- ✓ Affects multiple projects or teams
- ✓ Has meaningful trade-offs (alternatives were considered)
- ✓ Will guide future development
- ✓ Requires team buy-in or documentation

**Don't** create ADRs for:
- ✗ Minor bug fixes or refactoring
- ✗ Implementation details (use code comments instead)
- ✗ Decisions already documented in a different ADR

---

## ADR Structure

A well-formed ADR includes:

```
# ADR NNNN: [Concise Title]

Date: YYYY-MM-DD
Status: Proposed | Accepted | Deprecated | Superseded by NNNN

## Context

Background: Why is this decision needed? What problem are we solving?
Problem statement: Be specific about the issue.
Constraints: Budget, time, technology, team skills, etc.

## Decision

What we decided to do.
Keep it specific and actionable.

## Consequences

**Positive:**
- Benefit 1
- Benefit 2

**Negative:**
- Drawback 1
- Drawback 2

**Neutral:**
- Trade-off 1
- Trade-off 2

## Alternatives Considered

1. **Option A**: Description
   - Pros: ...
   - Cons: ...

2. **Option B**: Description
   - Pros: ...
   - Cons: ...

## Related Decisions

- ADR-NNNN (reason for link)
- ADR-NNNN (reason for link)
```

---

## How to Create an ADR

### Step 1: Identify the Decision

**Scenario**: You're deciding whether to use CQRS for a new bounded context.

Ask yourself:
- Is this a design decision that affects architecture?
- Will this decision guide future code?
- Do alternatives have meaningful trade-offs?
- Should the team know about this?

If yes → Create an ADR.

### Step 2: Gather Information

Before writing, collect:

```
1. What is the problem we're solving?
   → E.g., "Query performance is poor; reads and writes have different patterns"

2. What are the constraints?
   → Team experience, timeline, performance requirements, etc.

3. What options did we consider?
   → List 2-3 viable alternatives (even rejected ones)

4. What are the consequences of our choice?
   → Both positive and negative impacts
```

### Step 3: Use This Skill

**Prompt**:
```
Create an ADR for: Decide whether to adopt CQRS in the OrderManagement bounded context

Context:
- Current system has mixed read/write logic in aggregates
- Query performance is degrading
- Team has 1-2 members with CQRS experience

Problem:
- Queries and commands have different performance characteristics
- Writing to aggregates sometimes reads unnecessary data
- No clear separation of concerns

Options considered:
1. CQRS with MediatR (R&W separation)
2. Keep aggregates, optimize with read models
3. Denormalization at persistence layer

Recommendation: CQRS with MediatR

Consequence: Adds complexity for clearer separation
```

### Step 4: Review the Draft

The skill will generate an ADR in the correct format. Review for:

- [ ] Title is concise and specific
- [ ] Context is clear (problem, not solution)
- [ ] Decision states *what* and *why*
- [ ] Consequences are honest (positive and negative)
- [ ] Alternatives section explains trade-offs
- [ ] Related ADRs are linked
- [ ] Language is clear and professional

### Step 5: Submit and Merge

```
1. Number the ADR: NNNN-[slug].md
2. Create PR with title: "adr: NNNN - Title"
3. Team reviews for consensus
4. Merge when approved
5. Reference in code: // ADR-NNNN: [Reason]
```

---

## Example: Creating an ADR

### Input Prompt

```
Create an ADR for deciding on error handling strategy.

Context:
- Current code mixes exceptions and Result<T> types
- Different layers handle errors differently
- Team needs consistency

Problem:
- Unclear when to throw vs. return Result
- Exceptions leak from domain to controllers
- Hard to predict which methods throw

Options:
1. All exceptions (embrace throwing)
2. All Result<T> types (functional approach)
3. Domain exceptions only, Result<T> for cross-layer

Recommendation: Domain exceptions + Result<T> at boundaries

Consequences:
- Domain stays pure, infrastructure handles adaptation
- Clear layer boundaries
- More code at boundaries (translating exceptions)
```

### Generated ADR

```markdown
# ADR 0003: Error Handling Strategy

Date: 2026-06-05
Status: Proposed

## Context

Our codebase currently uses a mixed approach to error handling:
- Some methods throw exceptions
- Some return Result<T> types
- Different layers have different error handling patterns

This inconsistency makes it difficult to predict which methods throw,
leads to uncaught exceptions in callers, and violates hexagonal
architecture principles by leaking infrastructure exceptions into
domain code.

## Problem

We need a consistent strategy that:
- Keeps domain layer pure (domain exceptions only)
- Prevents infrastructure exceptions from leaking to domain
- Provides clear semantics for error conditions
- Is practical to implement across layers

## Decision

We adopt a **hybrid error handling strategy**:

1. **Domain layer**: Use domain-specific exceptions for validation failures
   - Example: `InvalidOrderException` when order lines are empty
   - Domain code catches and handles only domain exceptions

2. **Application layer**: Use Result<T> types for orchestration
   - Commands return `CommandResult<T>` (success or app-level error)
   - Queries return data or throw domain exceptions

3. **Infrastructure/Adapter layer**: Catch and wrap exceptions
   - External API errors → Adapter exceptions → Application Result
   - Never throw infrastructure exceptions up the call stack

4. **Presentation layer**: Translate errors to HTTP responses
   - Domain exceptions → 400 Bad Request
   - Application errors → 422 Unprocessable Entity
   - System errors → 500 Internal Server Error

## Consequences

**Positive:**
- Domain layer remains pure and testable without mocks
- Clear separation of concerns per layer
- Predictable error handling semantics
- Infrastructure failures don't corrupt domain logic
- Easy to log and monitor at appropriate layers

**Negative:**
- More code at layer boundaries (exception translation)
- Developers must remember which layer uses which pattern
- Result<T> requires additional handling (no try-catch)
- Requires discipline to maintain boundaries

**Neutral:**
- Aligns with hexagonal architecture principles
- Requires upfront training for new team members

## Alternatives Considered

### 1. All Exceptions
- Pros: Simple, familiar to most developers
- Cons: Infrastructure exceptions leak to domain, hard to catch specific failures, violates pure domain principle

### 2. All Result<T> Types
- Pros: Functional, explicit error handling, no throwing
- Cons: Verbose, requires Result pattern discipline, unfamiliar to team

### 3. Hybrid (Chosen)
- Pros: Best of both worlds, domain stays pure, clear semantics
- Cons: More code, requires understanding multiple patterns

## Related Decisions

- ADR-0001: Hexagonal Architecture (domain purity principle)
- ADR-0002: Dependency Injection (enables layer isolation)

## Implementation Notes

1. Create `DomainException` base class in Domain layer
2. Create `ApplicationError` record in Application layer
3. Define `CommandResult<T>` (success/error) in Application
4. Update repositories to wrap database exceptions
5. Add to coding standards: error handling patterns per layer

## Timeline

- Propose & discuss: 1 week
- Update existing code: 2 weeks
- Full adoption: 1 sprint

---

**Status**: Proposed
**Author**: Team
**Date**: 2026-06-05
```

---

## Integration with Other Skills

### With code-review

When review finds inconsistent error handling:
```
1. Skill: code-review
   → "This code mixes exceptions and Result<T>"

2. Skill: adr-creation
   → Create ADR for error handling strategy

3. After ADR is approved:
   Skill: code-review (again)
   → "Does this follow ADR-NNNN?"
```

### With guidelines-mcp

Before creating an ADR, check if one already exists:
```
1. search_docs("error handling")
   → See if ADR-NNNN exists

2. get_doc("adr-NNNN")
   → Read existing decision

3. If different approach needed:
   Skill: adr-creation
   → Create new ADR or supersede old one
```

### With feedback-loop

ADRs that solve common problems:
```
1. Skill: feedback-loop
   → analyze_guidelines_usage()
   → Find "error handling" is frequently searched

2. Skill: adr-creation
   → Create comprehensive ADR on error handling

3. Add to guidelines repo
```

---

## Example Workflows

### Workflow 1: Document a Decision Made in Sprint

```
1. Team decides: "Use Polly for resilience"
2. During retrospective, create ADR
3. Use Skill: adr-creation
4. Fill in context from sprint discussions
5. Submit PR, merge by Friday
6. Reference in code: // ADR-NNNN: Polly for transient retries
```

### Workflow 2: Propose New Architectural Pattern

```
1. Architect proposes: "Event Sourcing for audit trail"
2. Team needs to understand trade-offs
3. Use Skill: adr-creation
4. Generate ADR comparing: Event sourcing vs. audit table vs. change tracking
5. Present to team, discuss consequences
6. Decide to accept or modify
7. Merge approved ADR before implementation
```

### Workflow 3: Address Review Findings

```
1. PR review finds: "Error handling inconsistent"
2. Reviewer: "Let's document this"
3. Use Skill: adr-creation
4. Create ADR for error handling strategy
5. Merge ADR
6. Rebase PR against new ADR
7. Update code to follow ADR
```

---

## ADR Best Practices

- **Title**: Phrase as decision, not problem (✓ "CQRS" vs ✗ "Performance problems")
- **Context**: Explain the "why", not the "what"
- **Decision**: Be specific; state what you decided, not the process
- **Consequences**: Include negative trade-offs; don't sugarcoat
- **Alternatives**: Show you considered other options
- **Status**: Only maintainers change status (Proposed → Accepted)
- **Never edit accepted ADRs**: Create a new ADR to supersede
- **Link related ADRs**: Help readers find connected decisions
- **Commit message**: Use conventional: `adr: NNNN - Title`

---

## Common Questions

**Q: How detailed should the ADR be?**
A: Detailed enough that someone new to the project understands the decision and can implement it. Usually 1-2 pages.

**Q: Should I create ADRs for every decision?**
A: No—only significant decisions that affect architecture or have meaningful alternatives. Use code comments for implementation details.

**Q: What if the ADR describes something we're not following yet?**
A: Set status to `Proposed` until implementation is done. Once adopted, change to `Accepted`.

**Q: Can I modify an accepted ADR?**
A: Only the status and fixing typos. For changes, create a new ADR with `Supersedes NNNN-old`.

**Q: Who should write ADRs?**
A: Whoever makes or proposes the decision. This skill can help structure it.

**Q: How do I know when to create vs. update?**
A: If you're changing *what* we decided → new ADR. If you're clarifying *why* → edit current ADR (if not accepted).
