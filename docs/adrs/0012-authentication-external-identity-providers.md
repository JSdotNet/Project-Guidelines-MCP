---
title: "ADR 0012: Authentication with External Identity Providers"
date: 2026-06-04
status: Accepted
tags: [authentication, identity, oidc, jwt, oauth2, security, aspnet]
---
# ADR 0012: Authentication with External Identity Providers

## Context

Modern applications must authenticate users securely without managing credentials in-house. Rolling custom username/password stores introduces risks: password storage, hashing, breach response, account recovery, and MFA all need to be built and maintained correctly. External identity providers (IdPs) such as Google, Microsoft (Azure AD / Entra ID), and Facebook have already solved these problems at scale.

We need a consistent, protocol-driven approach to integrate external IdPs so that:

1. All providers are integrated using the same protocol and code patterns.
2. API authentication is stateless and verifiable.
3. Public clients (SPAs, mobile apps) follow best practices for authorization flows.
4. Refresh token handling is explicit and secure.
5. The authentication concern is isolated from business modules.

## Decision

### Protocol: OpenID Connect (OIDC) over OAuth 2.0

We adopt **OpenID Connect (OIDC)** as the mandatory protocol for all external identity provider integrations. OIDC builds on OAuth 2.0 and adds a standardised identity layer (ID tokens as JWTs). Direct OAuth 2.0 integration without OIDC is not permitted — it provides no identity assertion.

All supported providers expose OIDC-compliant endpoints:

| Provider | OIDC Discovery Endpoint |
|---|---|
| Google | `https://accounts.google.com/.well-known/openid-configuration` |
| Microsoft (Entra ID / Azure AD) | `https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid-configuration` |
| Facebook | `https://www.facebook.com/.well-known/openid-configuration` |

### ASP.NET Core Integration

Use the following libraries:

- **`Microsoft.Identity.Web`** — preferred for Microsoft / Entra ID. Handles token validation, token cache, incremental consent, and conditional access out of the box.
- **`Microsoft.AspNetCore.Authentication.OpenIdConnect`** — for Google, Facebook, and any OIDC-compliant provider not covered by Microsoft.Identity.Web.
- **`Microsoft.AspNetCore.Authentication.JwtBearer`** — for API-tier JWT Bearer token validation.

Do **not** use OAuth-only packages (e.g., plain `Microsoft.AspNetCore.Authentication.OAuth`) as they do not provide identity assertions.

### Dedicated Identity Layer

Authentication configuration lives in a **dedicated `Security` infrastructure project** (e.g., `YourSolution.Infrastructure.Security`) or, for modular monoliths, a shared `Core.Security` module. It is **never** inline in feature or business modules.

Responsibilities of the identity layer:

- Register authentication schemes and OIDC handlers.
- Configure token validation parameters.
- Map external claims to internal application claims (`ClaimsPrincipal` transformation).
- Provide extension methods (`AddApplicationAuthentication()`) consumed by the host project.

```csharp
// Core.Security/Extensions/SecurityServiceExtensions.cs
public static class SecurityServiceExtensions
{
    public static IServiceCollection AddApplicationAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration, "AzureAd")
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddInMemoryTokenCaches();

        services
            .AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
            })
            .AddFacebook(options =>
            {
                options.AppId = configuration["Authentication:Facebook:AppId"]!;
                options.AppSecret = configuration["Authentication:Facebook:AppSecret"]!;
            });

        return services;
    }
}
```

### JWT Bearer Tokens for API Authentication

All API endpoints are protected with JWT Bearer tokens. The JWT is validated on every request; no server-side session state is stored.

Required token validation parameters:

```csharp
// Enforced in AddJwtBearer or via Microsoft.Identity.Web configuration
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ClockSkew = TimeSpan.FromSeconds(30) // tight tolerance; default 5 min is too permissive
};
```

Tokens are passed via the `Authorization: Bearer <token>` HTTP header. Cookie-based authentication is permitted only for Blazor Server or MVC applications where the delivery layer manages the session.

### Refresh Token Strategy

- **Short-lived access tokens**: 15–60 minutes. The exact duration is set per deployment environment.
- **Refresh tokens**: Stored server-side in a token cache (in-memory for development; distributed cache — Redis or similar — for production). Never stored in `localStorage`; use `HttpOnly` cookies or a back-end-for-frontend (BFF) pattern for SPAs.
- Refresh token rotation: Enabled to mitigate replay attacks. Each use issues a new refresh token and invalidates the old one.
- `Microsoft.Identity.Web` handles token acquisition and refresh automatically when `EnableTokenAcquisitionToCallDownstreamApi()` is configured.

### PKCE for Public Clients

All public clients (SPAs, mobile apps, desktop apps) **must** use the **Authorization Code flow with PKCE** (Proof Key for Code Exchange). The implicit flow is prohibited.

PKCE requirements:

- `code_challenge_method`: `S256`.
- The `code_verifier` is generated client-side, never sent to the server except in the final token exchange.
- SPAs should use a certified OIDC client library (e.g., `@azure/msal-browser` for Microsoft, or `oidc-client-ts` for other providers).

Confidential clients (server-side web apps, APIs) use the standard Authorization Code flow or Client Credentials flow; PKCE is recommended but optional for confidential clients.

### Claims Mapping

External provider claims are mapped to a canonical, provider-agnostic internal claims schema in the `IClaimsTransformation` implementation:

```csharp
// Core.Security/Claims/ApplicationClaimsTransformation.cs
public sealed class ApplicationClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Normalise sub/oid/nameidentifier to a canonical user ID claim
        // Map provider-specific email claims to ClaimTypes.Email
        // Remove sensitive provider-specific claims not needed downstream
        return Task.FromResult(principal);
    }
}
```

### Configuration

All client IDs, secrets, and tenant identifiers are stored in **environment variables or Azure Key Vault** (via `IConfiguration`). They are never committed to source control.

```json
// appsettings.json (structure only — no real values)
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "",
    "ClientId": "",
    "ClientSecret": ""
  },
  "Authentication": {
    "Google": { "ClientId": "", "ClientSecret": "" },
    "Facebook": { "AppId": "", "AppSecret": "" }
  }
}
```

Use `dotnet user-secrets` for local development.

## Consequences

### Positive

1. **Reduced attack surface**: No password storage, no credential breach risk for the application itself.
2. **MFA and account security by default**: All IdPs provide their own MFA, account recovery, and breach detection.
3. **Protocol standardisation**: A single OIDC integration pattern regardless of provider reduces cognitive overhead.
4. **Compliance support**: OIDC/OAuth 2.0 is the industry standard; easier to satisfy GDPR, SOC 2, and similar requirements.
5. **Stateless API auth**: JWT Bearer validation is horizontally scalable without shared session state.
6. **Separation of concerns**: Authentication logic is isolated in a dedicated layer, away from business modules.

### Negative

1. **External dependency**: Authentication availability depends on third-party IdP uptime (Google, Microsoft, Facebook).
2. **Provider lock-in risk**: Each provider has quirks in their claim schemas and token responses; the claims-mapping layer mitigates this but adds indirection.
3. **Complexity for simple applications**: OIDC and token management are overkill for very small internal tools; evaluate whether simpler auth (e.g., Windows Auth or API keys) suffices.
4. **Refresh token management**: Requiring a distributed token cache in production adds infrastructure complexity.
5. **PKCE on SPAs**: Requires a modern OIDC-capable JS library; not trivial to retrofit to legacy frontends.

## References

- OpenID Connect specification: <https://openid.net/specs/openid-connect-core-1_0.html>
- OAuth 2.0 Security Best Current Practice (BCP): <https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics>
- Microsoft.Identity.Web documentation: <https://learn.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web>
- ASP.NET Core Authentication overview: <https://learn.microsoft.com/en-us/aspnet/core/security/authentication/>
- PKCE (RFC 7636): <https://datatracker.ietf.org/doc/html/rfc7636>
- Google OIDC: <https://developers.google.com/identity/openid-connect/openid-connect>
- Facebook Login with OIDC: <https://developers.facebook.com/docs/facebook-login/guides/advanced/oidc-token>
