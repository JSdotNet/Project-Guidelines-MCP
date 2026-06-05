# Skill: Guidelines Feedback Loop

**Description:** Analyze MCP server usage logs to identify improvement opportunities, draft GitHub issues with evidence, and submit them to the guidelines repository. Close the loop between guidance discovery and continuous improvement.

---

## What is the Feedback Loop?

The feedback loop is a workflow to:

1. **Analyze** how the MCP server is being used (which tools, what queries, what fails)
2. **Identify** patterns and opportunities for improvement
3. **Draft** GitHub issues with evidence from the logs
4. **Submit** issues to track improvements

This helps the team understand what guidance is missing, confusing, or underutilized.

---

## The Feedback Loop Workflow

### Phase 1: Analyze Usage Logs

**Goal**: Understand which MCP tools are used, what queries return zero results, and what fails most often.

**Tool**: Use the MCP server's `get_usage_logs(count)` tool to retrieve recent usage data, or if available, call the extension's `analyze_guidelines_usage` tool:

```
Call: analyze_guidelines_usage(maxEntries: 200, maxAgeDays: 30)
```

**What you get**:
- **Tool call frequencies**: Which tools are called most (e.g., search_docs, list_docs_by_type)
- **Zero-result searches**: Which queries returned no guidance (opportunity to add docs)
- **Top queries**: What users are searching for most
- **Failed calls**: Which tool calls errored or returned unexpected results
- **Most-accessed documents**: Which ADRs/recommendations are actually used

**Example output**:
```
## Usage Analysis (Last 30 Days)

### Tool Frequencies
- search_docs: 47 calls (47%)
- search_docs_by_tag: 23 calls (23%)
- get_doc: 15 calls (15%)
- list_docs_by_type: 12 calls (12%)
- list_docs: 3 calls (3%)

### Zero-Result Searches
1. "async state machine" (5 times) — No matching docs
2. "decorator pattern c#" (3 times) — Limited guidance
3. "options pattern edge cases" (2 times) — Missing recommendation

### Most Accessed Documents
1. ADR-0001: Hexagonal Architecture (15 reads)
2. ADR-0002: CQRS Pattern (12 reads)
3. Rec-005: Error Handling (8 reads)

### Failed Calls
- 1 timeout in search_docs (date filter issue)
- 2 malformed JSON in get_doc request
```

### Phase 2: Identify Improvement Opportunities

From the analysis, look for patterns:

- **Missing documentation**: Zero-result searches that appeared 2+ times → Create new ADR or recommendation
- **Unclear guidance**: High access to one doc but related docs untouched → Clarify or link docs
- **Underutilized tools**: Some tools are never used → Update onboarding or add examples
- **Confusing terminology**: Similar queries with different keywords → Add synonyms or cross-references

**Example improvements from analysis**:
```
Opportunity 1: "async state machine" search returned 0 results
→ Create ADR-0008: State Machine Pattern for Async Workflows

Opportunity 2: "decorator pattern" had low results
→ Add section to existing ADR on patterns, with C# examples

Opportunity 3: search_docs never used, but search_docs_by_tag popular
→ Update guidelines-mcp skill to emphasize tag-based search
```

### Phase 3: Draft a GitHub Issue

Once you've identified an improvement, use the `draft_guidelines_issue` tool to prepare an issue (if available in the feedback extension):

```
Call: draft_guidelines_issue(
  title: "Add ADR: State Machine Pattern for Async Workflows",
  body: "From usage analysis: 'async state machine' search returned 0 results 5 times in the last 30 days...",
  labels: ["documentation", "adr", "feature"]
)
```

**What to include in the body**:
- **Evidence**: Quote the usage analysis (e.g., "5 zero-result searches")
- **Problem**: What guidance is missing or unclear
- **Proposal**: What doc should be added/improved
- **Acceptance criteria**: What success looks like

**Example**:
```
## Problem

Usage analysis shows "async state machine" returned 0 results 5 times in the last 30 days, indicating a gap in state management guidance.

## Evidence

- 5 identical zero-result searches for "async state machine"
- Related searches ("state pattern", "workflow") also underutilized
- No ADR or recommendation currently covers this pattern

## Proposal

Create ADR-0008: State Machine Pattern for Async Workflows, covering:
- Use cases (order state transitions, workflow orchestration)
- C# example using Stateless or custom implementation
- When to prefer vs. CQRS
- Comparison to saga pattern

## Acceptance Criteria

- [ ] ADR written and merged
- [ ] Cross-linked from ADR-0002 (CQRS) and ADR-0005 (Resilience)
- [ ] Verify search returns new ADR for "async state machine" query
```

### Phase 4: Review and Submit

After drafting, review the issue:
- Does it have clear evidence from usage logs?
- Is the proposal scoped and actionable?
- Do the acceptance criteria make success testable?

If satisfied, submit the issue to the repository:

```
Call: submit_guidelines_issue(draftId: "draft-12345")
```

This creates an actual GitHub issue in the guidelines repo, tagged for team review.

---

## Integration Points

### With Gap Analysis

If you've run a gap analysis on a project, use the feedback loop to propose guidance for gaps you found:

```
1. Run start_gap_analysis("/path/to/MyProject")
   → Identify missing layers or patterns

2. Search MCP for matching guidance
   → Confirm what guidance exists

3. If gap is not covered:
   Call: analyze_guidelines_usage()
   → Check if this gap appears in other projects

4. If others have the same gap:
   Call: draft_guidelines_issue(
     title: "Add recommendation: Implementing X Layer",
     body: "Gap analysis found N projects missing X layer; users search for 'X pattern' without results"
   )

5. Call: submit_guidelines_issue(draftId)
```

### With Other Skills

- **guidelines-mcp**: Use MCP tools to verify documentation, read full ADRs before drafting issues
- **gap-analysis**: Analyze logs to see which gaps are most common across projects

---

## Example Workflows

### Workflow 1: Close a Documentation Gap

```
1. Run: analyze_guidelines_usage()
   → Find "my specific pattern" has 3 zero-result searches

2. Run: search_docs("my specific pattern")
   → Verify no docs exist

3. Call: draft_guidelines_issue(
     title: "Add ADR: My Specific Pattern",
     body: "Zero-result searches: 3. No ADR or recommendation covers this..."
   )

4. Review the draft

5. Call: submit_guidelines_issue(draftId)
   → Issue created, team can now discuss and implement
```

### Workflow 2: Clarify Existing Guidance

```
1. Run: analyze_guidelines_usage()
   → Find "error handling" has 15 reads for one doc, but 2 reads for similar doc

2. Call: get_doc("adr-error-handling")
   → Read current error handling guidance

3. Call: draft_guidelines_issue(
     title: "Clarify: Error Handling vs. Exception Strategy",
     body: "Usage shows users read ADR-X but skip related ADR-Y. Suggestion: add cross-links or consolidate..."
   )

4. Call: submit_guidelines_issue(draftId)
```

### Workflow 3: Add Examples for Underutilized Guidance

```
1. Run: analyze_guidelines_usage()
   → Find "cqrs" search returns docs, but get_doc("adr-cqrs") rarely used

2. Call: draft_guidelines_issue(
     title: "Enhance ADR-0002: Add CQRS Implementation Examples",
     body: "Log analysis shows users search for CQRS (7 times) but rarely read the full ADR. Recommendation: add concrete C# examples..."
   )

3. Call: submit_guidelines_issue(draftId)
```

---

## Tips for Effective Feedback

- **Run analysis regularly**: Monthly or after major releases to catch trends
- **Look for patterns**: Don't fix one-off searches; focus on recurring gaps
- **Include evidence**: Always cite the usage analysis in issue descriptions
- **Link to ADRs**: When proposing improvements, reference existing ADRs
- **Collaborate**: Share analysis results with the team before drafting issues
- **Follow up**: Monitor submitted issues to see which gaps get addressed, then re-run analysis

---

## Full Feedback Loop Workflow

```
┌─────────────────────────────────────────────────────┐
│ Step 1: Analyze MCP Usage Logs                      │
│ → Identify zero-result searches, underutilized docs │
├─────────────────────────────────────────────────────┤
│ Step 2: Consult Guidelines via MCP                  │
│ → Verify what docs exist, read full ADRs            │
├─────────────────────────────────────────────────────┤
│ Step 3: Identify Improvement Opportunities          │
│ → New ADR needed? Existing doc needs clarification? │
├─────────────────────────────────────────────────────┤
│ Step 4: Draft GitHub Issue                          │
│ → Include evidence from usage logs                  │
├─────────────────────────────────────────────────────┤
│ Step 5: Review Draft                                │
│ → Verify scope, acceptance criteria, evidence       │
├─────────────────────────────────────────────────────┤
│ Step 6: Submit Issue                                │
│ → Create GitHub issue in guidelines repo            │
├─────────────────────────────────────────────────────┤
│ Step 7: Track Improvements                          │
│ → Monitor PR/issue, re-run analysis after merge     │
└─────────────────────────────────────────────────────┘
```

---

## Common Questions

**Q: How often should I run the feedback loop?**
A: Monthly is a good starting point. Increase frequency if you have many projects using the MCP server.

**Q: Should I run gap-analysis before feedback-loop?**
A: Both are useful in combination. Gap analysis finds project-specific gaps; feedback loop finds documentation gaps. Run both to get the full picture.

**Q: Can I submit issues from the analysis even if I don't have a gap-analysis scan?**
A: Yes. The feedback loop is independent. If usage logs show "X was searched but not found", you can propose adding documentation for X.

**Q: Who should review the drafted issues?**
A: Team maintainers of the guidelines repo. The draft sits locally until you submit it, so you can iterate before submitting.
