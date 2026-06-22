---
title: "Modular Monolith Architecture Design"
date: 2026-06-01
status: Proposed
tags: [design, modular-monolith, architecture, boundaries, integration]
---
# Modular Monolith Architecture Design

## Overview

This document explains the architectural intent behind a modular monolith, including module boundaries, communication rules, consistency choices, and operational concerns. It complements structure templates by describing why the structure exists and what rules make it effective.

Use this design together with the modular structure template in `structures/modular-solution-structure.md`.

## Problem

Teams often adopt a modular folder layout but still end up with distributed-monolith behavior:

- Hidden coupling between modules through direct data access
- Inconsistent transaction and consistency boundaries
- Unclear ownership of business capabilities
- Shared abstractions that become an accidental shared domain
- Difficult extraction paths when a module must later become an independent service

A folder structure alone is not enough. We need explicit architectural rules and trade-off guidance.

## Forces

- Fast delivery in a single deployable unit
- Strong domain boundaries and independent evolution per module
- Simpler operations than microservices
- Predictable developer experience and discoverability
- Ability to split high-pressure modules later with minimal churn

## Proposed Architecture

### 1. Module as Primary Unit of Ownership

Each module owns:

- Its domain model and use cases
- Its persistence implementation
- Its API surface and DTO contracts
- Its observability signals and module-level metrics

Ownership is explicit: no cross-module access to internal domain classes or database tables.

### 2. In-Process Communication by Contracts

Within the monolith, modules communicate through:

- Abstractions project contracts (for direct request/response collaboration)
- Integration events (for decoupled workflows)

Direct references to another module's implementation project are not allowed.

### 3. Consistency and Transactions

- Keep transactional consistency inside one module boundary.
- Cross-module workflows use eventual consistency and idempotent handlers.
- Long-running flows use process orchestration in the application layer (not domain entities).

### 4. Vertical Slices Inside Each Module

Each feature is implemented as a slice (request + handler + endpoint + tests). This keeps coupling local and preserves module cohesion over time.

### 5. Observability by Module

Each module emits:

- Traces via module-specific ActivitySource naming
- Metrics for key business and reliability signals
- Structured logs with module and feature context

This enables operational ownership without service-per-module deployment.

## Design Variants and Trade-Offs

### Variant A: Shared Relational Database, Schema-per-Module

Pros:

- Operationally simple
- Strong query capabilities
- Good default for modular monolith start

Cons:

- Requires strict discipline to avoid cross-schema coupling
- Potential noisy-neighbor effects at scale

### Variant B: Database-per-Module Inside Monolith

Pros:

- Stronger data ownership boundaries
- Better extraction readiness

Cons:

- More operational complexity
- Harder cross-module reporting

Default recommendation: start with schema-per-module plus strict access rules, then evolve when pressure justifies it.

## Risks and Mitigations

- Risk: Shared-kernel sprawl  
  Mitigation: keep shared contracts minimal and versioned.
- Risk: Cross-module synchronous dependency chains  
  Mitigation: prefer events for non-critical read paths.
- Risk: Coupling via shared infrastructure helpers  
  Mitigation: keep helpers technical only, never domain behavior.
- Risk: Large-scale refactors after growth  
  Mitigation: enforce architecture tests for dependencies and forbidden references.

## What Belongs in Design vs Structure

### Design should include

- Architectural intent and constraints
- Boundary rules and dependency direction
- Communication and consistency patterns
- Trade-offs, risks, and evolution strategy
- Decision rationale and links to ADRs

### Structure should include

- Canonical folder and project trees
- Naming conventions and file placement
- Minimal scaffolds and templates
- Concrete "put this here" examples
- Quick-start copy/paste layouts

## Mapping to Existing Guidance

- ADR 0005: baseline modular monolith decision and top-level solution structure
- ADR 0008: vertical slice as feature organization
- ADR 0009: module-internal physical layout rules
- ADR 0010: observability baseline (OpenTelemetry)
- `structures/modular-solution-structure.md`: concrete scaffold for this design

## Evolution Path

When one module outgrows monolith constraints, extraction should follow this order:

1. Stabilize module contracts in abstractions.
2. Replace in-process calls with explicit integration messaging where needed.
3. Isolate persistence and migration concerns.
4. Move module to independent deployable while preserving contract behavior.

This design keeps extraction incremental instead of a rewrite.
