---
title: "Building Block: Test Harness"
date: 2026-11-19
status: Accepted
tags: [building-blocks, testing, harness, fakes, integration-tests, aspire]
---
# Building Block: Test Harness

Stand-ins for the external services an app calls but does not own: a payment provider, a
mail service, a partner API (ADR 0010). Part of the
[building-block view](../05-building-block-view.md).

## Two stand-ins per external service

| Stand-in | Where | Used by |
|---|---|---|
| `Fake{Service}` adapter | The integration test project, implementing the module's port | Integration tests, in-process |
| `Company.Product.Harness.{Service}` | `src/Harness/`, a small Api that speaks the service's protocol | The AppHost, for local runs and E2E tests |

```text
src/Harness/
└── Contoso.Orders.Harness.Payments/     # answers like the payment provider, from scripted cases
tests/Contoso.Orders.IntegrationTests/
└── Fakes/
    └── FakePaymentGateway.cs            # implements IPaymentGateway in memory
```

```csharp
// AppHost: in run mode the Api calls the harness instead of the real provider
if (builder.ExecutionContext.IsRunMode)
{
    var payments = builder.AddDotnetProject("payments-harness",
        "../../Harness/Contoso.Orders.Harness.Payments/Contoso.Orders.Harness.Payments.csproj");
    api.WithReference(payments);
}
```

## Rules

- Real dependencies the app owns (its database, cache, broker) are never faked; integration
  tests run them in throwaway containers.
- Mocks (NSubstitute) are for unit tests only; integration tests use `Fake*` adapters.
- A fake records what it received, so a test asserts on the call, not on an internal.
- A harness is never deployed; the AppHost adds it only in run mode.
- A harness answers from scripted cases (success, decline, timeout) a test or person selects,
  so failure paths are reproducible.
