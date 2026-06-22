---
title: "Caching Strategy"
date: 2026-06-04
status: Accepted
tags: [caching, performance, redis, cache-aside, recommendations]
---
# Recommendation: Caching Strategy

## Purpose

Define when caching is appropriate, where it belongs, and how to avoid stale or misleading data in .NET solutions that follow these guidelines.

## Recommendation

- Use caching only for clearly identified read-performance or cost-reduction goals.
- Prefer **cache-aside** as the default application pattern.
- Keep caching in adapters or dedicated read services, not in domain entities or handlers.
- Cache read models and external lookup results, not mutable aggregates.
- Define invalidation rules before adding the cache entry.

## Where caching is allowed

Caching is a good fit for:

- Frequently requested read models
- Reference data with predictable refresh windows
- Expensive external API lookups
- Computed views that are expensive to rebuild

Avoid caching:

- Security-sensitive authorization decisions unless explicitly designed for it
- Write-side aggregate state
- Data with unclear invalidation ownership

## Key design rules

- Use explicit, versionable cache keys.
- Set bounded TTLs; no unbounded cache lifetime.
- Record cache hit/miss metrics for meaningful caches.
- Treat cache content as disposable. The source of truth remains the backing system.

## Invalidation guidance

- Prefer event-driven invalidation when a module already emits reliable integration events.
- Otherwise, use short TTLs plus explicit eviction on known writes.
- Do not silently accept indefinite staleness.

## References

- ADR 0014: Persistence Strategy and Repository Boundaries
- ADR 0015: Resilience Strategy for Outbound Dependencies
- ADR 0016: Messaging and Integration-Event Delivery
