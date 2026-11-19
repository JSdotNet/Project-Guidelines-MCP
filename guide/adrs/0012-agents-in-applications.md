---
title: "ADR 0012: Agents in Applications"
date: 2026-11-19
status: Accepted
tags: [agents, ai, agent-framework, microsoft-extensions-ai, foundry, a2a, human-in-the-loop, evaluation, red-team, opentelemetry, genai, aspire, adr]
---
# ADR 0012: Agents in Applications

## Context

Agent behaviour built straight on a model SDK ties domain code to one provider, reaches data
around the app's own rules, and is hard to observe or test. Microsoft Agent Framework (1.0)
builds on `Microsoft.Extensions.AI`, and Aspire 17 ships the Foundry client packages, so an
agent can be a feature slice like any other.

## Decision

**Framework and models.** Agents use Microsoft Agent Framework on `Microsoft.Extensions.AI`.
Application code depends on `IChatClient` and `AIAgent`, never on a model SDK. Models come
from Microsoft Foundry behind one adapter project, `{Product}.Infrastructure.Foundry`, with a
stub chat client for tests.

**Hosting.** An agent runs in-process in the Api host of the module that owns its data, and
deploys with it. A Foundry hosted agent is used only when the app's own ADR says why (its own
scale, lifetime or security boundary).

**Tools.** An agent's tools are only the app's own MCP tools (ADR 0011) or in-process
functions that call CQRS handlers (ADR 0005). Every tool result is untrusted input.

**Between agents.** A2A between agents in separate deployables; Agent Framework workflows
inside one process.

**Human in the loop.** An action with side effects needs the user's confirmation, through an
MCP `input_required` round trip or an Agent Framework approval step.

**Identity.** The agent proves its own identity and acts on behalf of the user (ADR 0011).

**Observability.** Agent Framework and `Microsoft.Extensions.AI` emit OpenTelemetry GenAI
spans to the Aspire dashboard (ADR 0007); token usage is a metric per agent.

**Evaluation.** Each agent has an evaluation set in `tests/`. CI runs it and compares the
scores with a stored baseline; a regression fails the build. A red-team configuration runs
before release.

**Aspire.** The AppHost declares the Foundry resources (ADR 0002). Services use the Aspire 17
client packages `Aspire.Azure.AI.Projects` (`AddAzureAIProjectClient`) and
`Aspire.Azure.AI.Extensions.OpenAI`; the discontinued `Aspire.Azure.AI.OpenAI` and
`Aspire.Azure.AI.Inference` are not used.

## Consequences

### Positive

- Agents are tested, observed and secured like any feature slice; the domain does not know a
  model exists.
- Swapping a model or provider is an adapter change and an evaluation re-run.
- An agent can never do more than the app's own handlers allow.

### Negative

- Foundry becomes an Azure dependency for any app with agents.
- Evaluation sets and baselines are extra test data to maintain, and runs cost tokens.
- Confirmations add a round trip to every side-effecting agent action.

## References

- [Microsoft Agent Framework](https://github.com/microsoft/agent-framework)
- [Microsoft.Extensions.AI](https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai)
- [A2A protocol](https://a2a-protocol.org/)
- [What's new in Aspire 17](https://aspire.dev/whats-new/aspire-17/)
- ADR 0002: Aspire, Configuration and Infrastructure
- ADR 0005: Application Layer
- ADR 0007: Observability and Resilience
- ADR 0011: Setting Up MCP

Replaces: none; new in this set.
