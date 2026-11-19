---
title: "07 Deployment View"
date: 2026-11-19
status: Accepted
tags: [arc42, deployment, aspire, aspire-deploy, azure-container-apps, bicep, secrets, ci-cd, github-actions, dependabot, auto-merge, rollback]
---
# 07 Deployment View

How a JSdotNet application gets from a merged pull request to Azure. Aspire is the
infrastructure code ([ADR 0002](adr/0002-aspire-configuration-and-infrastructure.md)); the
resources it models are the [AppHost](building-blocks/apphost.md)'s. A project's own
`07-deployment-view.md` links this page and adds only its environments and anything Aspire
cannot model.

```text
pull request ──► required checks ──► auto-merge ──► main
                                                     │
                                                     ▼
                              aspire publish  (AppHost ──► Bicep + container images)
                                                     │
                                                     ▼
                              aspire deploy --environment <env>
                                                     │
              ┌──────────────────────────────────────┴───────────────────────┐
              ▼                                                              ▼
  Azure Container Apps environment                         Backing resources from the AppHost
  ├── migration runner (job, runs first, then exits)       ├── the app's database
  ├── {Module}.Api container apps                          ├── Key Vault (per-environment secrets)
  ├── App (the UI)                                         ├── managed identities
  └── harness services: never deployed                     └── monitoring + alert rules
```

## Aspire is the infrastructure code

- The AppHost models every resource: projects, the UI, the database, the identity and the
  monitoring. Nothing is written by hand next to it, and nothing is changed in the portal.
- `aspire publish` generates the deployment artifacts (Bicep and container images);
  `aspire deploy` applies them. There is no second IaC tool.
- A resource Aspire cannot model yet is added through the AppHost's customization hooks.
- Alert rules are modelled in the AppHost and deploy with it.
- Development-only resources (harness services, resource MCP servers, seeding commands) are
  excluded from publish.

## Azure Container Apps

- Each `{Module}.Api` and the UI run as a container app in one Container Apps environment per
  application environment (for example `test` and `prod`).
- The migration runner runs as a job before the APIs start; an API never migrates on startup
  ([ADR 0006](adr/0006-persistence-and-messaging.md)).
- Each container app authenticates to its dependencies with its own managed identity.
- Health probes come from ServiceDefaults' health endpoints; a revision that fails them never
  takes traffic.

## Migrations

- A migration lives in the owning module's `Data.{Provider}` project and is generated in the
  same change as the model change.
- It is named for its business intent and reviewed like code; an operation that loses data is
  called out in the pull request with its rollback or mitigation.
- The migration runner applies it in every environment, local included; nothing migrates
  destructively at startup.

## Per-environment secrets

- Every setting and secret is an Aspire parameter; a required parameter rejects an empty value.
- Secrets are set per environment with `aspire secret set --environment <env>` and land in
  that environment's Key Vault through `aspire deploy`. Nothing secret is committed, and CI
  holds no long-lived cloud credential.
- CI signs in to Azure through OpenID Connect federation with a GitHub Environment per
  application environment; non-secret values are environment variables there.
- How the app keeps secrets once it runs is in [08 Secrets at rest](08-crosscutting-concepts.md#secrets-at-rest).

## Pipeline

GitHub Actions, in three parts:

1. **Pull request** — the required checks: locked restore, build (warnings are errors,
   [ADR 0001](adr/0001-platform-and-build.md)), unit, architecture and integration tests, E2E
   when there is a UI ([ADR 0010](adr/0010-testing.md)), CodeQL and dependency review. Results
   and failure artifacts are published.
2. **Main** — builds the images once and runs `aspire deploy` to each environment in order;
   a later environment deploys the same images, never a rebuild.
3. **After deploy** — smoke checks against the health endpoints and the critical journeys;
   a hard failure triggers the rollback below.

- A failing main-branch workflow opens one GitHub issue with a stable title, skipped when an
  open one for that workflow exists; pull-request workflows report through their checks.
- Official actions are pinned to their current major version and kept current by Dependabot.

## Dependency updates

Dependabot runs **weekly** for NuGet, npm and GitHub Actions:

- minor and patch updates are **grouped** into one pull request per ecosystem;
- an **Aspire group** (`Aspire.*`) moves every Aspire package together, majors included, so
  the AppHost and its integrations never drift apart;
- a major update outside the Aspire group gets its own pull request.

```yaml
version: 2
updates:
  - package-ecosystem: nuget
    directory: /
    schedule: { interval: weekly }
    groups:
      aspire:
        patterns: ["Aspire.*"]
      minor-and-patch:
        update-types: [minor, patch]
  - package-ecosystem: github-actions
    directory: /
    schedule: { interval: weekly }
    groups:
      minor-and-patch:
        update-types: [minor, patch]
```

## Auto-merge

- A pull request merges automatically once every check the branch ruleset requires is green;
  auto-merge never bypasses a required check or a review the ruleset demands.
- A draft is never merged. Marking it ready for review starts auto-merge; converting it back
  to a draft stops it.
- Dependabot's grouped updates take the same path: green checks merge them, red checks leave
  them for a person.

## Rollback

- **Roll back only while the schema is unchanged**: redeploy the previous images with
  `aspire deploy`, or shift traffic back to the previous Container Apps revision.
- **After a migration has run, fix forward** with a new change; a migration is never reversed
  in production ([ADR 0006](adr/0006-persistence-and-messaging.md)).
- A migration that removes or rewrites data is split so the deploy before it stays
  compatible: expand first, contract in a later release.
- A risky feature ships behind a flag ([08 Feature flags](08-crosscutting-concepts.md#feature-flags)),
  so turning it off is the first rollback.
