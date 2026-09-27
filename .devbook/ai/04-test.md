# Test

```meta
status: trial
type: stage
```

## CodeQL findings fixed by Autofix

```meta
status: trial
type: practice
stage: [test]
date: 2026-09-27
```

A CodeQL finding raised on a pull request by `pr-check.yml` is fixed with a Copilot Autofix
suggestion committed to the branch, instead of by hand.

- **Used for** — security findings on pull requests.
- **Adopted by** — the maintainer, once.
- **Evidence** — #52 took an Autofix commit for a `Path.Combine` finding.
- **Limits** — a suggestion is reviewed like any other commit; nothing is applied unread.
