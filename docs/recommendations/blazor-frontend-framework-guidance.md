---
title: "Blazor Frontend Framework Guidance"
date: 2026-06-01
status: Accepted
tags: [blazor, frontend, components, data-binding, state-management, caching, security, web-api, recommendations]
---
# Recommendation: Blazor Frontend Framework Guidance

## Purpose

Provide a practical baseline for building frontend applications with Blazor in projects that follow this repository's modular monolith and vertical slice guidance.

## Recommendation

- Prefer Blazor Web App for full-stack solutions where SSR and interactive components are both useful.
- Keep UI concerns in Razor components and move business workflows into application services/handlers.
- Use shared abstractions (DTOs and contracts) to align with existing module boundaries.
- Use typed or named `HttpClient` registrations for outbound API calls.
- Keep state explicit: URL for navigation/filter state, scoped state containers for per-user session state, and persistent storage only when required.

## Setup Instructions

### 1. Choose hosting model intentionally

- Blazor Web App (recommended default): Best for mixed SSR + interactivity and close integration with ASP.NET backend.
- Blazor WebAssembly standalone: Use when frontend must be independently deployed and consumes backend via HTTP APIs only.
- Blazor Hybrid: Use for desktop/mobile native shells, not as default for web applications.

### 2. Create the frontend project

```bash
dotnet new blazor -n MyProject.Frontend
```

Suggested placement in a modular solution:

```text
src/
  App/
    MyProject.Frontend/
  Core/
  Modules/
    Sales/
      Sales.Api/
      Sales/
      Sales.Abstractions/
```

### 3. Configure service registrations

In `Program.cs`, register component services and API clients:

```csharp
builder.Services.AddRazorComponents();

// Same-host API calls
builder.Services.AddHttpClient("BackendApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Backend:BaseUrl"]!);
});

// Example typed client
builder.Services.AddHttpClient<TodoApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Backend:BaseUrl"]!);
});
```

### 4. Align styling with ADR 0011

- Keep design tokens centralized in one variables file.
- Avoid hardcoded visual values in component-local styles.
- Prefer semantic tokens (`--color-primary`, `--spacing-md`) over raw hex/px values.

### 5. Integrate with .NET Aspire when solution is distributed

If the frontend, APIs, and infrastructure resources are part of a distributed solution, use an Aspire AppHost as the single orchestration model (aligned with ADR 0003).

Recommended:

- Model frontend + backend + dependencies in one AppHost.
- Use service discovery/references instead of hardcoded localhost ports.
- Keep backend endpoint configuration environment-driven.
- Use Aspire Dashboard during local development for traces/log correlation across frontend and API.

Illustrative AppHost shape:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.MyProject_Api>("api");

builder.AddProject<Projects.MyProject_Frontend>("frontend")
  .WithReference(api)
  .WaitFor(api);

builder.Build().Run();
```

## Component Structure

Organize components by feature, not by technical type:

```text
MyProject.Frontend/
  Features/
    Orders/
      Pages/
        OrdersPage.razor
      Components/
        OrderList.razor
        OrderEditor.razor
      Services/
        IOrderUiService.cs
        OrderUiService.cs
      State/
        OrdersState.cs
      Models/
        OrderListItemVm.cs
```

Guidelines:

- Keep pages as composition roots for a feature.
- Keep reusable visual parts in `Components/`.
- Keep API/adaptor logic in feature `Services/`, not directly in markup-heavy components.
- Keep transient, UI-only shape models in `Models/` and backend contracts in shared abstractions.

## Backend Integration Guidance

### Preferred integration flow

1. Razor component invokes feature UI service.
2. UI service calls either:
   - Application layer directly (same process/server rendering path), or
   - Backend API through `HttpClient` (client/WebAssembly or cross-process path).
3. UI service maps results into view models and returns UI-friendly data.

### API calls vs direct application-layer calls

Use this rule set:

- Call backend APIs from Blazor when:
  - The component can run client-side (WebAssembly/Auto CSR).
  - The frontend and backend are deployed independently.
  - You need cross-origin or gateway-mediated communication.
  - You need network boundary policies (auth scopes, throttling, BFF/proxy patterns).

- Call application layer directly when:
  - The component runs server-side in the same process.
  - Data access is internal and no network boundary is needed.
  - You want to avoid unnecessary loopback HTTP from server to itself.

Important practice from Microsoft Learn:

- For Blazor Web Apps with mixed render modes, abstract calls behind an interface and provide client/server implementations.
- Do not make server code call back into itself over HTTP unless there is a specific boundary reason.

## Common Patterns

### Data binding

Simple two-way binding:

```razor
<EditForm Model="model" OnValidSubmit="SaveAsync">
    <InputText @bind-Value="model.Name" />
    <InputNumber @bind-Value="model.Quantity" />
    <button type="submit">Save</button>
</EditForm>

@code {
    private OrderEditModel model = new();

    private Task SaveAsync() => Task.CompletedTask;
}
```

Recommended:

- Use `EditForm` with validation components for input-heavy pages.
- Keep bound model small and focused on the current form intent.

### Event handling

```razor
<button @onclick="RefreshAsync" disabled="@isLoading">
    @(isLoading ? "Loading..." : "Refresh")
</button>

@code {
    private bool isLoading;

    private async Task RefreshAsync()
    {
        isLoading = true;
        try
        {
            await LoadDataAsync();
        }
        finally
        {
            isLoading = false;
        }
    }

    private Task LoadDataAsync() => Task.Delay(100);
}
```

Recommended:

- Prefer async handlers for I/O paths.
- Guard against duplicate clicks for long-running operations.
- Surface failure states explicitly in the UI.

### State management

Use the least-complex option that fits:

- URL state for filter/sort/page identifiers.
- Cascading values for shared tree-level state.
- Scoped state container service for per-circuit user interactions.
- Browser storage only for state that must survive reloads.

Minimal state container pattern:

```csharp
public sealed class OrdersState
{
    public event Action? Changed;

    private IReadOnlyList<OrderListItemVm> items = [];
    public IReadOnlyList<OrderListItemVm> Items
    {
        get => items;
        set
        {
            items = value;
            Changed?.Invoke();
        }
    }
}
```

Register as:

- `AddScoped` for server-side/user-circuit state.
- `AddSingleton` for WebAssembly app-wide client state (when appropriate).

## Caching Strategies

Apply caching deliberately as a performance optimization, not as a correctness mechanism.

Recommended baseline:

- Cache at boundaries (UI service/API client/adapters), not in domain rules.
- Prefer short-lived caches for list/read models that are expensive to fetch repeatedly.
- Use explicit invalidation when writes occur (or event-driven invalidation where available).
- Include cache expiration policy intentionally (`AbsoluteExpirationRelativeToNow` plus optional sliding expiration).

Hosting-model guidance:

- Blazor Server: use `IMemoryCache` for per-node read caching; use distributed cache when instances scale out and shared cache coherence is required.
- Blazor WebAssembly: use browser storage only for non-sensitive state that must survive refreshes.
- Distributed topologies: keep cache keys stable and versioned, and avoid hardcoded cache settings in components.

Practical rules:

- Do not cache authorization decisions or sensitive payloads in browser storage.
- Prefer backend HTTP caching (`ETag`/`Cache-Control`) for API-backed read endpoints where possible.
- Treat stale-data tolerance as an explicit product decision per feature.

## Security Recommendations

Treat client-side checks as UX aids only. Enforce all security decisions on server/API boundaries.

Authentication and authorization:

- Use ASP.NET Core authentication/authorization for protected endpoints and pages.
- Choose cookie-based auth for same-site server apps; use token-based auth for decoupled API scenarios when required.
- Enforce policy/role checks on backend handlers/endpoints, not only in component visibility logic.

Transport and boundary protection:

- Enforce HTTPS everywhere, including local/dev where feasible.
- Configure CORS explicitly for allowed origins/methods/headers when frontend and API are separated.
- Use anti-forgery protection for cookie-authenticated form posts and sensitive state-changing requests.

Data and secret handling:

- Never embed secrets in frontend code or static assets.
- Keep credentials, keys, and environment-specific endpoints in secure configuration providers.
- Avoid storing tokens or sensitive user data in `localStorage` unless risk-assessed and required.

UI and observability hygiene:

- Validate all server inputs regardless of client-side validation.
- Avoid rendering untrusted HTML; if unavoidable, sanitize first.
- Do not log secrets, access tokens, or sensitive personal data from UI/service layers.

## Testing Strategy (Blazor + E2E)

Use a layered approach:

- Component unit tests first (fast): bUnit + xUnit for Razor component behavior.
- API/application tests next: validate handlers/endpoints separately.
- E2E tests (selective): Playwright for critical user journeys.

### Recommended E2E scope

Prioritize only business-critical paths:

- Authentication and authorization flow.
- Form submission and validation.
- Cross-page workflows (create/edit/confirm patterns).
- JS/DOM-dependent behavior that unit tests cannot validate.

### E2E implementation recommendations

- Prefer Playwright for .NET as the default E2E runner for Blazor.
- Keep E2E scenarios deterministic with seeded data and stable selectors (`data-testid`).
- Avoid asserting on styling details unless visual regression is the explicit goal.
- Run E2E against an environment started through Aspire/AppHost in CI for topology parity.
- Keep E2E suite small and stable; move logic-heavy checks down to unit/integration tests.

### Minimal Playwright test example

```csharp
using Microsoft.Playwright;
using Xunit;

public sealed class OrdersE2ETests
{
  [Fact]
  public async Task CreateOrder_ShouldShowSuccessMessage()
  {
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
    {
      Headless = true
    });

    var page = await browser.NewPageAsync();
    await page.GotoAsync("https://localhost:5001/orders/new");

    await page.GetByTestId("order-name").FillAsync("Conference Ticket");
    await page.GetByTestId("submit-order").ClickAsync();

    await Expect(page.GetByTestId("save-success")).ToBeVisibleAsync();
  }

  private static ILocatorAssertions Expect(ILocator locator)
    => Microsoft.Playwright.Assertions.Expect(locator);
}
```

## Best Practices Checklist

- Keep components focused on rendering and interaction orchestration.
- Push business decisions to application/domain layers.
- Prefer DTOs over exposing persistence models to the UI.
- Use cancellation tokens for long-running operations where possible.
- Handle API errors explicitly (status-code-aware behavior).
- Avoid loopback HTTP from server-rendered components to same-process endpoints.
- Test component behavior and UI services independently.

## References

- Microsoft Learn: ASP.NET Core Blazor overview
  - <https://learn.microsoft.com/aspnet/core/blazor>
- Microsoft Learn: Call a web API from Blazor
  - <https://learn.microsoft.com/aspnet/core/blazor/call-web-api>
- Microsoft Learn: Blazor state management overview
  - <https://learn.microsoft.com/aspnet/core/blazor/state-management>
- Microsoft Learn: Test Razor components in Blazor
  - <https://learn.microsoft.com/aspnet/core/blazor/test>
- Aspire Docs: What is Aspire?
  - <https://aspire.dev/get-started/what-is-aspire/>
- ADR 0005: Modular Monolith Project Structure
- ADR 0006: CQRS recommendation for ASP.NET API projects
- ADR 0009: Feature slices within module projects
- ADR 0011: Centralized frontend styling variables
