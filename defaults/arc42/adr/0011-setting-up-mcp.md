---
title: "ADR 0011: Setting Up MCP"
date: 2026-11-19
status: Accepted
tags: [mcp, model-context-protocol, streamable-http, oauth, cimd, pkce, token-exchange, rfc-8693, zero-trust, audit, skills, mcp-apps, adr]
---
# ADR 0011: Setting Up MCP

## Context

Applications expose their data and actions to agents through MCP servers. The 2026-07-28 MCP
specification and the C# SDK 2.x make the core stateless, move server-initiated elicitation
to multi-round-trip results, and deprecate Roots, Sampling, Logging and Dynamic Client
Registration. An MCP tool is an API endpoint that an agent calls on behalf of a user, so it
needs at least the protection of ADR 0008, plus a way to tell which agent is calling.

## Decision

**Shape.**

- C# SDK 2.x (`ModelContextProtocol`), stateless Streamable HTTP, hosted inside the app's Api
  host and run and tested through Aspire like any endpoint.
- Tools for actions, resources for read-only data, prompts for workflows. Roots, Sampling and
  Logging are not used.
- Each server ships its own skill (which tool when, the order of a workflow, what to avoid),
  in the same plugin, and also serves it as a prompt and a resource for clients without the
  plugin. Tool descriptions stay short; the skill carries the workflow.

**Two-way communication.** Questions and confirmations use `input_required` round trips;
change notifications use `subscriptions/listen`; long work runs as a Task with progress and
resume; an interaction that needs UI uses an MCP App. No call holds a stream open while it
waits.

**Security.**

- Each server is an OAuth 2.1 protected resource that publishes protected-resource metadata;
  the app's identity provider (ADR 0008) is the authorization server. Clients identify with
  Client ID Metadata Documents, never Dynamic Client Registration, and PKCE is always on.
- **Two identities.** The agent proves its own identity with a managed or federated
  credential, never a shared key. Working for a user, it exchanges the user's token for one
  scoped to the server (RFC 8693) that names the user as subject and the agent as actor.
  Downstream calls repeat the exchange; a token is never passed through.
- A call is allowed only when the user may make it **and** this agent may make it for that
  user, through the same per-call policies as the HTTP API (ADR 0008).
- Tools are annotated read-only or destructive; a destructive tool asks for confirmation
  through `input_required` before it runs.
- Every tool validates its input and is rate-limited per user and per agent. An Azure API
  Management gateway in front is optional and exists in Azure only; locally, clients call the
  server directly and every check stays in the server.
- Every call writes an audit record: user, agent, tool, outcome and a hash of the arguments,
  correlated with its trace (ADR 0007).

## Consequences

### Positive

- An agent can do exactly what its user may do, never more, and every call shows who did it.
- Stateless servers scale and deploy like the rest of the Api host.
- Clients without the plugin still get the server's workflow guidance.

### Negative

- Token exchange and the agent's own identity need identity-provider setup per app.
- Confirmations and Tasks make tool handlers more complex than a single request and response.
- Some pieces (MCP Apps, the Aspire Entra ID and API Management integrations) are newer than
  the rest of the stack.

## References

- [MCP specification 2026-07-28](https://modelcontextprotocol.io/specification/2026-07-28)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [RFC 8693 OAuth 2.0 Token Exchange](https://www.rfc-editor.org/rfc/rfc8693)
- ADR 0007: Observability and Resilience
- ADR 0008: Security

Replaces: none; new in this set.
