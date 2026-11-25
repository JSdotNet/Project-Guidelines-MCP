---
title: "ADR 0008: Security"
date: 2026-11-25
status: Accepted
tags: [security, keycloak, authentication, authorization, oidc, oauth, jwt, pkce, zero-trust, policies, rate-limiting, audit, entra-id, adr]
---
# ADR 0008: Security

## Context

A credential store of our own means building password hashing, recovery, MFA and breach
response, all of which an identity provider already does. Role strings scattered over
endpoints, internal calls trusted because they are in-process, and records anyone can change
by guessing an id are the usual authorization failures. Anonymous endpoints without limits
invite abuse. Aspire 17's Entra ID integration (preview) creates app registrations but adds no
authentication middleware, so it does not replace any of the rules below.

## Decision

**Authentication: an external OIDC identity provider.**

- Users sign in at an external identity provider over OpenID Connect; apps store no passwords.
  The provider is the app's choice, recorded in its own ADR.
- APIs validate JWT bearer tokens in full: issuer, audience, lifetime and signing key. Partial
  validation is never configured. `Microsoft.Identity.Web` is used for Entra ID,
  `Microsoft.AspNetCore.Authentication.JwtBearer` and `.OpenIdConnect` for other providers.
- Public clients use the authorization code flow with PKCE. Access tokens are short-lived;
  refresh tokens never reach `localStorage`: a browser app uses a backend-for-frontend with
  `HttpOnly` cookies.
- Provider claims map to the app's own claim types in one place, so feature code never reads
  a provider-specific claim.
- The Aspire 17 Entra ID integration (`Aspire.Hosting.Azure.EntraId`, preview) may set up the
  app registrations; the API still configures its own token validation and policies.
- **Local development.** An app whose provider is Keycloak, or that needs no tenant to run
  locally, runs Keycloak as an AppHost resource with the Aspire Keycloak hosting integration
  (`Aspire.Hosting.Keycloak`, preview): a stable port, the admin password as an Aspire
  parameter, and the realm, clients and test users imported from a realm file in the repo
  with `WithRealmImport`. The realm import is development-only and is not published by
  `aspire deploy`. The API keeps the standard handlers above with full validation;
  `RequireHttpsMetadata` is relaxed only in Development, and a deployed app sets its
  `Authority` explicitly.

**Authorization: zero trust, checked per call.**

- A fallback policy requires an authenticated user; an endpoint is anonymous only through an
  explicit `.AllowAnonymous()`.
- Authorization is expressed as named, claims-based policies registered in one place.
  `[Authorize(Roles = ...)]` and role strings in feature code are not used.
- Per-record access (ownership, tenancy) uses resource-based authorization through
  `IAuthorizationService` in the handler, returning `Forbidden` as a `Result` (ADR 0005).
- No implicit trust: a call between modules carries the caller's `ClaimsPrincipal` and is
  checked again. Background workers run under their own narrowly scoped identity, never an
  elevated ambient one.
- Every write and every denied attempt writes an audit record with who, what, the outcome and
  the trace id (ADR 0007).

**Rate limiting.** Every public or anonymous endpoint has an ASP.NET Core rate-limiting
policy, partitioned per client or user. Exceeding it returns 429 Problem Details (ADR 0004).

## Consequences

### Positive

- No credential storage, and MFA and account recovery come from the provider.
- One place to read every access rule; a forgotten attribute fails closed instead of open.
- Module boundaries are trust boundaries, so a compromised module cannot act for another.

### Negative

- Every app depends on an identity provider being reachable, locally included; the local
  Keycloak resource removes the network dependency but its realm file must track the real
  provider's clients and claims.
- Resource-based checks add a lookup and a call to every handler that touches owned data.
- A browser app needs a backend-for-frontend to keep tokens out of script reach.

## References

- [NIST SP 800-207 Zero Trust Architecture](https://csrc.nist.gov/pubs/sp/800/207/final)
- [Policy-based authorization in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authorization/policies)
- [Resource-based authorization](https://learn.microsoft.com/aspnet/core/security/authorization/resourcebased)
- [Rate limiting middleware](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [What's new in Aspire 17](https://aspire.dev/whats-new/aspire-17/)
- [Aspire.Hosting.Keycloak](https://www.nuget.org/packages/Aspire.Hosting.Keycloak)
- ADR 0004: HTTP API
- ADR 0005: Application Layer
- ADR 0007: Observability and Resilience

Replaces: ADR 0012 (Authentication with External Identity Providers), ADR 0013 (Authorization & Zero Trust Security Model).
