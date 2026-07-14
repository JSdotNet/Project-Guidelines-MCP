---
title: "CI/CD Pipeline for Continuous Deployment"
date: 2026-07-15
status: Accepted
tags: [ci-cd, pipeline, continuous-deployment, github-actions, devops, recommendations]
---
# Recommendation: CI/CD Pipeline for Continuous Deployment

## Purpose

Define a production-grade CI/CD pipeline approach for .NET services where every eligible merge to `main` can be deployed to production safely and automatically.

## Recommendation

- Use a single automated delivery flow from pull request validation to production deployment.
- Treat production as continuously deployable: small, reversible changes with strict quality gates.
- Prefer GitHub-native tooling unless an organizational standard requires otherwise.
- Keep deployment risk controlled through progressive rollout, feature flags, and fast rollback.

## Recommended toolchain

- **Pipeline orchestration:** GitHub Actions.
- **Build and test:** `dotnet restore`, `dotnet build`, `dotnet test`.
- **Quality and security:** CodeQL, dependency scanning, secret scanning, and policy checks.
- **Artifact storage:** GitHub Container Registry (GHCR) or cloud-native registry (for example ACR/ECR).
- **Infrastructure and deployment:** Bicep or Terraform + cloud CLI/actions.
- **Identity and secrets:** OpenID Connect (OIDC) federation and platform secret stores (do not use long-lived static credentials).
- **Release safety:** Feature flags, health checks, and automated rollback triggers.

## Pipeline design (continuous deployment to production)

1. **Pull request pipeline (required checks)**
   - Restore, build, unit tests, integration tests, architecture tests.
   - Run static analysis and security scans.
   - Publish test results and failure artifacts.
2. **Main branch pipeline**
   - Re-run critical checks (or enforce proven artifact reuse).
   - Build immutable deployable artifact once.
   - Deploy automatically to production with progressive rollout (for example canary or ring-based).
3. **Post-deployment verification**
   - Run smoke checks and SLO-focused health validation.
   - Auto-rollback when hard failure thresholds are crossed.

## Setup instructions (GitHub Actions baseline)

1. Add branch protection on `main` requiring PR pipeline checks.
2. Create `.github/workflows/pr-validation.yml` for PR triggers and required validation jobs.
3. Create `.github/workflows/deploy-main.yml` triggered on push to `main`.
4. Configure OIDC trust between GitHub and the target cloud account/subscription.
5. Store non-secret configuration in repository/environment variables, and secrets in the cloud secret manager.
6. Define `production` GitHub Environment with deployment rules and required reviewers only when required by governance.
7. Implement progressive rollout in deployment scripts and ensure rollback is one command/action away.

Example trigger skeleton:

```yaml
name: deploy-main

on:
  push:
    branches: [main]

jobs:
  deploy-production:
    runs-on: ubuntu-latest
    environment: production
    permissions:
      id-token: write
      contents: read
    steps:
      - uses: actions/checkout@v7
      - uses: actions/setup-dotnet@v5
        with:
          dotnet-version: 10.0.x
      - run: dotnet restore
      - run: dotnet build --configuration Release --no-restore
      - run: dotnet test --configuration Release --no-build
      - name: Deploy with progressive rollout
        run: ./deploy/deploy-production.sh
```

## Pipeline failure reporting (main branch only)

Add a `report-failure` job to every **main branch** workflow. This job:
- Runs only when a preceding job fails (`if: failure()`).
- Opens a GitHub issue so failures are tracked and not silently lost.
- Skips creation when an open issue for the same workflow already exists (deduplication).
- Is **not** added to PR pipelines — those report through PR comments and status checks.

```yaml
  report-failure:
    name: Report Pipeline Failure
    needs: [<your-main-job>]
    if: failure()
    runs-on: ubuntu-latest
    permissions:
      issues: write
    steps:
      - name: Create GitHub Issue (skip if open issue exists)
        env:
          GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          GH_REPO: ${{ github.repository }}
          BODY: |
            ## 🔴 Pipeline Failure: ${{ github.workflow }}

            | | |
            |---|---|
            | **Workflow** | ${{ github.workflow }} |
            | **Run** | [#${{ github.run_number }}](${{ github.server_url }}/${{ github.repository }}/actions/runs/${{ github.run_id }}) |
            | **Branch** | `${{ github.ref_name }}` |
            | **Commit** | `${{ github.sha }}` |
            | **Triggered by** | ${{ github.actor }} |

            Please [inspect the workflow run](${{ github.server_url }}/${{ github.repository }}/actions/runs/${{ github.run_id }}) for details.
        run: |
          EXISTING=$(gh issue list \
            --repo "$GH_REPO" \
            --state open \
            --label "bug" \
            --search "Pipeline failure: ${{ github.workflow }} in:title" \
            --json number \
            --jq 'length')
          if [ "$EXISTING" -gt 0 ]; then
            echo "An open failure issue already exists for this workflow — skipping."
            exit 0
          fi
          echo "$BODY" | gh issue create \
            --repo "$GH_REPO" \
            --title "⚠️ Pipeline failure: ${{ github.workflow }}" \
            --body-file - \
            --label "bug"
```

The issue title is kept stable (no run number) so the deduplication search always matches an existing open issue. Close the issue manually once the root cause is resolved — the next failure will then re-open a fresh one.

## Action version pinning

Always pin to the current major version of official GitHub Actions and update them on a regular cadence. As of July 2026, the required versions to avoid Node.js deprecation warnings are:

| Action | Minimum version |
|---|---|
| `actions/checkout` | `@v7` |
| `actions/setup-dotnet` | `@v5` |
| `actions/upload-artifact` | `@v7` |
| `actions/download-artifact` | `@v4` |

## Maintenance best practices

- Keep pipelines fast and deterministic; remove flaky or redundant checks.
- Version and review pipeline files like production code.
- Pin action versions and update them on a scheduled cadence (see Action version pinning above).
- Track DORA metrics and lead time to detect delivery regression.
- Keep rollback, incident response, and release ownership explicit.
- Periodically run disaster/rollback drills to validate recovery readiness.

## References

- Recommendation: Feature Flags and Rollout Safety
- Recommendation: Logging and Audit Logging
- Recommendation: Integration Testing
- Recommendation: End-to-End Testing
- ADR 0010: Adopt OpenTelemetry for Comprehensive Observability
- ADR 0015: Resilience Strategy for Outbound Dependencies
