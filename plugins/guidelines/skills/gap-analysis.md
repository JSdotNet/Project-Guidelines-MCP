# Skill: Project Guidelines Gap Analysis

**Description:** Scan a .NET project to identify architectural gaps, missing layers, and structural issues. Compare the project structure against established guidelines and create improvement issues.

---

## What is a Gap Analysis?

A gap analysis compares your actual project structure against the recommended architecture from the JSdotNet Project Guidelines. It helps identify:

- Missing architectural layers (Domain, Application, Infrastructure, Adapters, Interface)
- Inconsistent naming conventions
- Misplaced responsibilities (e.g., business logic in controllers)
- Missing test projects or scaffolding issues
- Configuration and dependency injection setup gaps

---

## How to Run a Gap Analysis

### Step 1: Use the MCP Server to Scan Your Project

If you have the `start_gap_analysis` tool available (from the **guidelines-feedback** extension or plugin):

```
Call: start_gap_analysis(projectPath: "/path/to/MyProject")
```

This scans your project and returns:
- **Project summary**: Detected layers, project files, structure visualization
- **Structured workflow**: Step-by-step instructions to complete the analysis

### Step 2: Review the Project Scan Results

The scan output will show:
- **Detected layers**: Which subdirectories match architectural patterns (Domain, Application, Infrastructure)
- **File counts per layer**: How many classes are in each layer
- **Missing conventions**: Gaps in naming, structure, or layering

Example output:
```
Root: /Users/username/MyProject

## Project Structure

Domain Layer
- MyProject.Domain/ (8 files)
  Contains: entities, value objects, domain services

Application Layer
- MyProject.Application/ (12 files)
  Contains: command handlers, query handlers, DTOs

Infrastructure Layer
- MyProject.Infrastructure/ (5 files)
  Contains: repositories, external service adapters

⚠ Missing: Interface/Delivery layer
⚠ Missing: Dedicated test project structure
```

### Step 3: Consult Guidelines for Each Gap

For each gap identified, use the MCP tools to find relevant guidance:

**Example 1: Missing Domain Layer**
```
search_docs("domain layer structure")
search_docs_by_tag("ddd")
list_docs_by_type("structures")
get_doc("adr-NNNN-domain-layering")
```

**Example 2: Controllers with Business Logic**
```
search_docs("clean controllers")
search_docs_by_tag("cqrs")
search_docs_by_tag("application-layer")
```

**Example 3: Missing Repository Pattern**
```
search_docs("repository pattern")
search_docs_by_tag("persistence")
list_docs_by_type("structures")
```

### Step 4: Create an Improvement Plan

Based on the gaps and guidance, create a plan:

1. **Prioritize**: Which gaps have the most impact?
2. **Reference**: Which ADRs/recommendations guide each fix?
3. **Estimate**: Can gaps be fixed incrementally or require refactoring?
4. **Document**: Create issues or a design document to track improvements

### Step 5: (Optional) Track with GitHub Issues

If you want to track gap fixes as GitHub issues, use the **guidelines-feedback** skill workflow:

```
Call: prepare_feedback_session()
  ↓
Call: analyze_guidelines_usage()
  ↓
Call: draft_guidelines_issue(title, body, labels)
  ↓
Call: submit_guidelines_issue(draftId)
```

---

## Common Gap Patterns & Solutions

### Gap: No Separate Domain Layer

**Problem**: Business logic scattered in controllers or services.

**Solution**:
```
1. search_docs_by_tag("ddd")
2. list_docs_by_type("structures")
3. get_doc("ADR-NNNN: Domain-Driven Design Structure")
4. Create MyProject.Domain/ with Entities/, ValueObjects/, Services/
```

### Gap: Missing Application Orchestration Layer

**Problem**: Controllers calling repositories directly; no command/query separation.

**Solution**:
```
1. search_docs_by_tag("cqrs")
2. search_docs_by_tag("application-layer")
3. get_doc("ADR-NNNN: Vertical Slice Pattern")
4. Scaffold MyProject.Application/ with Handlers/, Commands/, Queries/
```

### Gap: Infrastructure Dependencies in Domain

**Problem**: EF Core attributes on domain entities; repository interfaces in domain.

**Solution**:
```
1. search_docs("hexagonal architecture")
2. search_docs_by_tag("ports-adapters")
3. get_doc("ADR-NNNN: Port/Adapter Boundaries")
4. Move EF configurations to Infrastructure; define ports in Application
```

### Gap: Insufficient Test Coverage

**Problem**: No dedicated test project or coverage tracking.

**Solution**:
```
1. search_docs_by_tag("testing")
2. list_docs_by_type("structures")
3. search_docs("unit test organization")
4. Create MyProject.Tests/ with subdirs: Domain.Tests/, Application.Tests/, Integration.Tests/
```

### Gap: Missing Error Handling Strategy

**Problem**: No consistent exception/result pattern; exceptions leaking across layers.

**Solution**:
```
1. search_docs("error handling")
2. search_docs_by_tag("exceptions")
3. get_doc("ADR-NNNN: Exception Strategy")
4. Implement domain exceptions, translate at layer boundaries
```

---

## Integration with the Feedback Loop

If you use **guidelines-feedback** skill, you can:

1. **Analyze usage logs** to see which gaps users report most often
2. **Draft improvement issues** with evidence from scan results
3. **Submit issues** to track improvements across your team

---

## Workflow: From Gap to Fix

```
┌─────────────────────────────────────────────┐
│ 1. Run gap_analysis scan                    │
├─────────────────────────────────────────────┤
│ 2. Identify gaps (e.g., missing layers)     │
├─────────────────────────────────────────────┤
│ 3. Search guidelines for each gap           │
├─────────────────────────────────────────────┤
│ 4. Read full ADRs/recommendations           │
├─────────────────────────────────────────────┤
│ 5. Create action items or GitHub issues     │
├─────────────────────────────────────────────┤
│ 6. Implement fixes, referencing ADRs        │
├─────────────────────────────────────────────┤
│ 7. Re-run gap analysis to confirm fixes     │
└─────────────────────────────────────────────┘
```

---

## Tips

- **Start with the scan**: Let the tool do the heavy lifting to identify structural gaps.
- **Cross-reference ADRs**: For each gap, find the corresponding ADR or recommendation.
- **Use tags for speed**: `search_docs_by_tag()` is faster than free-text search when the gap is clear.
- **Create a baseline**: Run the scan once, document gaps, then periodically re-run to track improvements.
- **Engage the team**: Share scan results and ADRs with your team to align on improvements.
