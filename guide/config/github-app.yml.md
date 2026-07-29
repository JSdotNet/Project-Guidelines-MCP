---
title: "GitHub Copilot Agent Configuration (github-app.yml)"
date: 2026-07-30
status: Accepted
tags: [github, copilot-agent, ci-cd, pr-workflow, automation, config]
---
# Config Guideline: github-app.yml

## Purpose

Define the standard configuration for `github-app.yml`, the GitHub Copilot Coding Agent
configuration file. This guideline governs how the agent validates work before opening a
pull request, monitors CI after opening one, and engages with review feedback.

---

## Pre-PR Validation (`pre_flight`)

Before the agent creates a pull request it **must** run the following steps in order.
Any failure aborts the PR creation and must be resolved first.

```yaml
pre_flight:
  - name: Restore dependencies
    run: dotnet restore

  - name: Build (compile)
    run: dotnet build --no-restore --configuration Release

  - name: Run tests
    run: dotnet test --no-build --configuration Release --logger trx --results-directory ./TestResults
```

**Rules:**
- All three steps are mandatory; none may be skipped.
- Use `--no-restore` / `--no-build` flags to avoid repeating earlier steps.
- A non-zero exit code on any step is a hard failure — do not proceed to PR creation.
- Test results must be written to `./TestResults` so CI can pick them up.
- **Only include steps that run a real, meaningful command.** `pre_flight` exists to catch actual
  build/test failures before a PR is opened, not to perform theater. Do not add placeholder steps
  (e.g. `Write-Output` or `Out-Null` no-ops) just to have a step present.
- If the repository has no buildable/testable code (for example, a documentation-only or
  guidance-only repository), **omit the `pre_flight` section entirely** rather than including
  no-op steps. If it is useful to record why it was omitted, leave a short YAML comment explaining
  the reason instead of a fake step.

---

## Post-PR: Pipeline Monitoring (`on_pr_opened`)

After the PR is created, wait **5 minutes** before checking CI pipeline status to allow
all checks to be triggered and reach an initial result.

```yaml
on_pr_opened:
  - name: Wait for CI to start
    wait: 5m

  - name: Check pipeline status
    action: check_ci
    on_failure: fix_and_push
```

**Rules:**
- Check **all** required status checks on the PR, not just the first one.
- If any pipeline is failing or errored, the agent **must** diagnose the root cause, apply
  a fix, push a corrective commit, and then re-check after another 5-minute grace period.
- Repeat the check → fix → push loop up to **3 times**. If pipelines still fail after
  three attempts, leave a comment on the PR explaining what was tried and why it is
  unresolved, then stop.
- Do **not** force-push; use regular commits so reviewers can follow the correction history.
- Only modify code that is directly related to the CI failure — do not refactor unrelated
  areas opportunistically.

---

## Post-PR: Review Comment Handling (`on_review_comment`)

After the PR is created, wait **5 minutes** before checking for review comments to allow
initial automated and human review to arrive.

```yaml
on_pr_opened:
  - name: Wait for initial review
    wait: 5m

  - name: Process review comments
    action: address_review_comments
```

**Rules:**
- For **each** unresolved review comment the agent must post a reply that:
  1. Acknowledges the comment with a brief summary of the concern.
  2. Proposes a concrete resolution (code change, explanation, or clarification).
  3. Indicates whether the agent will apply the change automatically or requires
     human decision.
- If a comment is ambiguous, ask a clarifying question rather than assuming.
- Do **not** resolve/dismiss threads autonomously; leave thread resolution to the human
  reviewer after the proposed fix is accepted.
- If a review comment leads to a code change, apply it and push a commit with message
  format: `fix(review): <short description of what the comment addressed>`.

---

## PR Conventions

### Title Format

PR titles **must** follow Conventional Commits:

```
<type>(<optional-scope>): <short imperative summary>
```

Allowed types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `perf`, `ci`, `build`.

Examples:
- `feat(orders): add order cancellation command`
- `fix(auth): correct token expiry calculation`
- `chore: bump NuGet packages to latest patch`

### Branch Naming

Agent-created branches **must** follow:

```
<type>/<short-slug>
```

Examples: `feat/order-cancellation`, `fix/token-expiry`, `docs/adr-0014-caching`.

### Description Template

The agent **must** populate the PR description with:

```markdown
## Summary
<!-- What does this PR do and why? -->

## Changes
<!-- Bullet list of notable changes -->

## Testing
<!-- How was this tested? Reference test files or test results. -->

## Related
<!-- Issue refs, ADR refs, or links -->
```

---

## GitHub & Git Account Management

When the agent executes GitHub operations (PR creation, issue updates, API calls) and git
operations (push, pull, fetch), it **must** authenticate and operate under the correct account
context.

### Configuration

```yaml
automation:
  auto_issue_session: false
  remote_control: false

instructions:
  github_account: "Use the repository account for all GitHub operations in this repo."
  gh_cli: "Before running gh commands such as pr create, pr merge, issue create, or api calls, switch to the git_transport."
  git_transport: "Use the repository SSH remote/account for push, pull, fetch, and clone operations."
```

Alternatively, if the platform supports natural-language instructions:

```yaml
instructions: |
  Use the repository account for all GitHub operations in this repository.
  When running gh commands, authenticate as the repo account, not the Copilot subscription account.
  Use the repo SSH identity for git push/pull/fetch.
```

**Do not duplicate instruction-file prose here.** The `instructions:` block in `github-app.yml` is
for short, `github-app.yml`-specific policy (account/transport selection, as above) — it must not
restate routing, tool-selection, or workflow guidance that already lives in
`.github/instructions/*.md`. Where routing or workflow behavior is already documented there,
reference the instruction file by path/name instead of copying its content, e.g.:

```yaml
instructions: |
  Follow .github/instructions/mcp-tool-usage.md for MCP/tool selection order.
  Follow .github/instructions/workflow-routing.md for task routing.
  Use the repository account for all GitHub operations in this repository (see above).
```

### Rules

- **GitHub operations (`gh` CLI):** Switch to the repository account (not the Copilot subscription
  account) before executing `gh pr create`, `gh pr merge`, `gh issue create`, or any GitHub API calls.
- **Git operations (push/pull/fetch):** Use the repository's SSH remote and SSH key/identity to avoid
  credential conflicts. This ensures pushes are attributed to the repo account.
- **No account switching mid-operation:** Once an operation begins, do not switch accounts. Complete
  the entire workflow (e.g., create PR, check CI, comment) under a single account context.
- **Clarify failures:** If a push is denied with a 403 error, explicitly report the account name and
  request that the user verify access rights or re-authenticate.

---

## Scope & Safety Boundaries

The agent is permitted to:
- Create and push commits on its own branch.
- Open, update, and comment on PRs.
- Run build and test commands listed in this file.
- Propose code changes in response to review comments.

The agent is **not** permitted to:
- Merge PRs without a human-approved review.
- Delete branches other than its own short-lived ones.
- Modify `.github/`, `docs/config/`, or any infrastructure-as-code files
  (`*.tf`, `*.bicep`, `*.yaml` in deployment folders) without an explicit human instruction.
- Push directly to `main`, `develop`, or any protected branch.
- Access, log, or store secrets — always use repository or environment secrets via
  `secrets.*` references.

---

## Secrets & Environment Variables

- Never hard-code credentials or tokens in code or config files.
- Reference secrets exclusively via `${{ secrets.SECRET_NAME }}` in workflow files.
- If a required secret is missing, fail loudly with a clear error message rather than
  proceeding with degraded behaviour.

---

## References

- [GitHub Copilot Coding Agent docs](https://docs.github.com/en/copilot/using-github-copilot/using-copilot-coding-agent)
- [Conventional Commits specification](https://www.conventionalcommits.org/)
- ADR 0001: Adopt .NET 10 as Target Framework
- ADR 0002: Central Package Management
