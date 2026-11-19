---
title: "Folder Structure Reference"
date: 2026-06-01
status: Accepted
tags: [structure, folders, templates, modular-monolith, feature-slices]
---
# Structure: Folder Structure Reference

Date: 2026-06-01
Type: Project Structure Template

## Purpose

Provide a lightweight, high-level folder map and navigation guide.

Detailed and scenario-specific folder trees remain in their own structure documents.

Use this page to choose the right structure document:

- `modular-solution-structure-template.md`
- `simple-solution-structure.md`
- `feature-slices-module-structure.md`
- `minimal-api-endpoint-organization.md`

## High-Level Map

```text
solution-root/
├── src/      # Production code and app hosts
├── tests/    # Automated test projects
├── docs/     # ADRs, designs, recommendations, structures
└── *.sln
```

## Where Details Live

- Multi-module solution layout and examples: `modular-solution-structure-template.md`
- Simple single-solution layout and examples: `simple-solution-structure.md`
- Feature-slices module folder layout: `feature-slices-module-structure.md`
- Minimal API endpoint file layout: `minimal-api-endpoint-organization.md`
