# Feedback loop templates

## Usage analysis

```markdown
## Usage analysis: <period>, <n> calls

### Tool calls
| Tool | Calls | Share |
|---|---|---|

### Searches with no result
| Query | Times |
|---|---|

### Most-read documents
| Doc id | Reads |
|---|---|

### Failed calls
| Tool | Arguments | Error |
|---|---|---|
```

## Issue

```markdown
## Problem
What guidance is missing or unclear.

## Evidence
- "<query>" returned nothing <n> times in <period>
- Related documents checked: <doc ids>

## Proposal
A new ADR, a section in an existing document (name its doc id), a tag, or a cross-link.

## Acceptance criteria
- [ ] The document or change is merged and `defaults/index.json` updated
- [ ] `search_guides("<query>")` returns it
```

Labels: `documentation`, plus `adr` for a new decision.
