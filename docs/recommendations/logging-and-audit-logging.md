---
title: "Logging and Audit Logging"
date: 2026-06-04
status: Accepted
tags: [logging, audit, observability, security, recommendations]
---
# Recommendation: Logging and Audit Logging

## Purpose

Differentiate diagnostic logging from audit logging and standardize what each one should capture.

## Recommendation

- Use structured diagnostic logs for operational insight.
- Use dedicated audit logs for security-sensitive or compliance-relevant actions.
- Never treat audit logging as just another application log event.
- Redact secrets, tokens, and sensitive personal data.

## Diagnostic logging

Diagnostic logs should capture:

- Module and feature context
- Correlation or trace identifiers
- Failure reasons and dependency context
- Operational state changes that matter during support

## Audit logging

Audit logs should capture:

- Who performed the action
- What action was attempted
- Which resource was targeted
- Whether it succeeded or failed
- When it happened

Audit records must be queryable independently from normal application logs.

## References

- ADR 0010: Adopt OpenTelemetry for Comprehensive Observability
- ADR 0013: Authorization & Zero Trust Security Model
