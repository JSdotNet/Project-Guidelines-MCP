---
title: "ADR 0013: Authorization & Zero Trust Security Model"
date: 2026-06-04
status: Accepted
tags: [authorization, zero-trust, security, claims, policies, audit, aspnet]
---
# ADR 0013: Authorization & Zero Trust Security Model

## Context

Authorization determines what an authenticated principal is allowed to do. Without a coherent authorization strategy, common failure modes appear:

- Role strings (`"Admin"`, `"Manager"`) scattered across controllers, handlers, and queries with no single source of truth.
- Implicit trust between internal modules: a call from Module A to Module B is assumed to be pre-authorized.
- Missing per-record access control: any authenticated user can mutate any record by guessing an ID.
- Insufficient audit trails: sensitive operations succeed or fail silently, making incident investigation difficult.

A **Zero Trust** posture removes implicit trust from every layer: network, service, module, and request. Every access decision is evaluated explicitly, with minimal privilege and comprehensive logging.

## Decision

### Zero Trust Principles

This project adopts the three foundational Zero Trust principles as defined by NIST SP 800-207:

#### 1. Verify Explicitly

Every request to a protected resource is authenticated and authorized, regardless of origin. There is no trusted network zone. An internal service-to-service call receives the same scrutiny as an external user request.

- All API endpoints require a valid, unexpired JWT (see ADR 0012).
- Authorization policies are evaluated on every request; there is no "skip auth for internal calls" escape hatch.
- Token validation requirements (issuer, audience, expiry, and signing key) are all enforced — partial validation is not permitted.

#### 2. Use Least Privilege

Access is scoped to the minimum necessary. Broad role checks are replaced by fine-grained, semantically-named policies.

- Tokens are requested with minimal OAuth scopes; never request `openid profile email offline_access` as a blanket set — only what the flow needs.
- Authorization policies are defined in terms of claims, not raw role strings.
- Service accounts and background workers use separate, narrowly scoped identities.

#### 3. Assume Breach

Design as though the perimeter has already been compromised. Audit, detect, and contain.

- All access attempts — both successful and denied — are logged with sufficient context for incident investigation.
- Access tokens are short-lived (see ADR 0012).
- Module boundaries are treated as trust boundaries: no module can bypass the authorization layer on behalf of another.

---

### Claims-Based Authorization with Centrally Registered Policies

Role strings embedded in `[Authorize(Roles = "Admin")]` attributes across feature code are **prohibited**. All authorization logic is expressed as named policies registered in one place.

#### Policy Registration

Policies live in the `Core.Security` shared project (or `Infrastructure.Security` for non-modular solutions) and are registered via an extension method:

```csharp
// Core.Security/Extensions/AuthorizationPolicyExtensions.cs
public static class AuthorizationPolicyExtensions
{
    public static IServiceCollection AddApplicationAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.ReadConferences, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(AppClaimTypes.Scope, Scopes.ConferencesRead))
            .AddPolicy(Policies.ManageConferences, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(AppClaimTypes.Role, Roles.ConferenceManager))
            .AddPolicy(Policies.AdminOnly, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(AppClaimTypes.Role, Roles.SystemAdmin))
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build()); // deny unauthenticated requests everywhere by default

        services.AddSingleton<IAuthorizationHandler, ResourceOwnerHandler>();

        return services;
    }
}
```

Policy names, claim type constants, and role value constants are co-located in the `Core.Security` project:

```csharp
// Core.Security/Authorization/Policies.cs
public static class Policies
{
    public const string ReadConferences = "ReadConferences";
    public const string ManageConferences = "ManageConferences";
    public const string AdminOnly = "AdminOnly";
}
```

Feature code references only the constant name, not the policy logic:

```csharp
// Conferences.Api/Endpoints/ConferenceEndpoints.cs
group.MapGet("/", handler)
     .RequireAuthorization(Policies.ReadConferences);
```

#### Fallback Policy

A default fallback policy that requires an authenticated user is configured. All endpoints are denied unless explicitly opened with `.AllowAnonymous()`. Anonymous access is the exception, not the default.

---

### Resource-Based Authorization

For per-record access control (e.g., "can this user edit *this* conference?"), use ASP.NET Core's `IAuthorizationService` with resource-based handlers. This is required wherever ownership or tenancy restricts access to specific records.

```csharp
// Core.Security/Authorization/ResourceOwnerHandler.cs
public sealed class ResourceOwnerRequirement : IAuthorizationRequirement { }

public sealed class ResourceOwnerHandler : AuthorizationHandler<ResourceOwnerRequirement, IOwnedResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement,
        IOwnedResource resource)
    {
        var userId = context.User.FindFirstValue(AppClaimTypes.UserId);
        if (userId is not null && userId == resource.OwnerId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
```

Usage in a command handler:

```csharp
// Conferences.Module/Features/UpdateConference/UpdateConferenceHandler.cs
public sealed class UpdateConferenceHandler(
    IConferenceRepository repository,
    IAuthorizationService authorizationService)
    : ICommandHandler<UpdateConferenceCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateConferenceCommand command, ClaimsPrincipal user, CancellationToken ct)
    {
        var conference = await repository.GetByIdAsync(command.ConferenceId, ct);
        if (conference is null)
            return Result.NotFound();

        var authResult = await authorizationService.AuthorizeAsync(user, conference, new ResourceOwnerRequirement());
        if (!authResult.Succeeded)
            return Result.Forbidden();

        // proceed with update
    }
}
```

---

### No Implicit Trust Between Internal Modules

In a modular monolith, a call from the `Registrations` module to the `Conferences` module is **not** implicitly trusted. Module-to-module calls that cross a security boundary must carry the originating `ClaimsPrincipal` and are subject to the same policy evaluation.

Rules:
- Shared infrastructure contracts (e.g., `IConferenceService` called from another module) accept a `ClaimsPrincipal` parameter where the operation is security-relevant.
- Modules do not grant each other elevated permissions by virtue of being in-process.
- Background workers and scheduled jobs use a dedicated service identity, not an elevated ambient principal.

---

### Audit Logging for Sensitive Operations

All sensitive authorization decisions must be recorded. "Sensitive" includes:

- Any write operation on a domain aggregate (create, update, delete).
- Explicit authorization failures (`Result.Forbidden()`).
- Administrative operations (role changes, policy overrides, bulk deletes).
- Authentication events (login, logout, token refresh, failed authentication).

The audit log entry must include: timestamp (UTC), user ID, action, resource type, resource ID, outcome (allowed/denied), and correlation ID.

```csharp
// Core.Security/Audit/IAuditLogger.cs
public interface IAuditLogger
{
    void LogAccess(string userId, string action, string resourceType, string resourceId, bool allowed, string? correlationId = null);
}
```

Audit log entries are written to a dedicated structured log sink (separate from the application diagnostic log) so they can be retained and queried independently.

---

### Token Validation Requirements

Every JWT accepted by the API layer must satisfy all of the following:

| Validation | Requirement |
|---|---|
| Issuer (`iss`) | Must match the configured trusted issuer(s); reject unknown issuers |
| Audience (`aud`) | Must match the API's registered client ID / application URI |
| Expiry (`exp`) | Must be in the future; clock skew tolerance ≤ 30 seconds |
| Signing key | Must be validated against the IdP's published JWKS endpoint |
| Algorithm | Must be `RS256` or `ES256`; symmetric `HS256` is prohibited for externally issued tokens |
| `nbf` (not before) | Validated when present |

Partial validation — e.g., `ValidateAudience = false` — is **not permitted in production**. Any deviation requires an explicit ADR addendum with documented justification.

---

## Consequences

### Positive

1. **Centralised policy management**: Authorization rules are in one place; changing a policy does not require hunting across feature files.
2. **Fine-grained, auditable access control**: Claims-based policies express intent clearly and are easier to audit than scattered role checks.
3. **Per-record security by default**: Resource-based handlers prevent horizontal privilege escalation (IDOR vulnerabilities).
4. **Zero Trust compliance**: Explicit verification at every layer aligns with NIST SP 800-207 and modern security frameworks.
5. **Incident investigation**: Structured audit logs reduce mean time to investigate (MTTI) during security events.
6. **Defence in depth**: Short-lived tokens + strict validation + audit logging creates multiple independent security layers.

### Negative

1. **Increased boilerplate**: Every protected handler must thread `ClaimsPrincipal` and call `IAuthorizationService` where resource-based auth is required.
2. **Policy proliferation**: Without governance, the policy registry can grow unwieldy; regular review is required.
3. **Audit log volume**: High-traffic systems generate large volumes of audit log data; a retention and archival strategy is required.
4. **Module complexity**: Passing `ClaimsPrincipal` across module boundaries breaks the clean "inputs only" contract of command handlers; an explicit `AuthorizationContext` wrapper may be needed.
5. **Learning curve**: Developers accustomed to `[Authorize(Roles = "Admin")]` must learn the claims-based policy model.

### Mitigation Strategies

- Provide a base handler class or decorator that handles the resource-based authorization boilerplate.
- Schedule quarterly policy registry reviews to retire stale policies.
- Configure log sink retention separately for audit vs. diagnostic logs (e.g., 7 days diagnostic, 1 year audit).
- Document the `AuthorizationContext` pattern as the canonical way to convey identity across module boundaries.

## References

- NIST SP 800-207 Zero Trust Architecture: <https://csrc.nist.gov/publications/detail/sp/800/207/final>
- ASP.NET Core Authorization documentation: <https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction>
- Resource-based authorization in ASP.NET Core: <https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased>
- Claims-based authorization: <https://learn.microsoft.com/en-us/aspnet/core/security/authorization/claims>
- Policy-based authorization: <https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies>
- OWASP Authorization Cheat Sheet: <https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html>
- OAuth 2.0 Threat Model and Security Considerations (RFC 6819): <https://datatracker.ietf.org/doc/html/rfc6819>
- ADR 0012: Authentication with External Identity Providers
