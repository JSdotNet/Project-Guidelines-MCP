---
name: feedback-loop
description: "Analyze MCP usage logs, identify improvement opportunities, and propose new ADRs or documentation enhancements as GitHub issues."
---

# Feedback loop

Turns how the `jsdotnet-app-defaults` server is used into improvement issues for
`JSdotNet/app-defaults`. Analysis and issue templates: [reference.md](reference.md).

## Workflow

1. **Collect**: `get_usage_logs(count)` with a count large enough for the period. The log
   holds this server process's calls only; say which period it covers.
2. **Analyse**: tool call counts, searches that returned nothing, documents read most, and
   failed calls.
3. **Find the opportunities**:
   - a query that returned nothing twice or more → a missing document or tag;
   - one document read often while its related ones are not → a missing cross-link;
   - the same need searched with different words → a missing tag or synonym;
   - a failing call → a server bug.
4. **Check before proposing**: `search_guides` and `search_guides_by_tag` for the topic, so
   the issue never asks for a document that exists.
5. **Draft one issue per opportunity** from the template: problem, evidence quoted from the
   log, proposal, acceptance criteria.
6. **Show the drafts** and file only the ones the person approves, with `gh issue create`.
