---
name: design-content
description: Authoring rules for the UX style guide under design/ and its index.
paths:
  - "design/**"
---

# Design content

`design/` is the UX style guide served by the design MCP server. `design/README.md` is its
entry point and structure overview; keep its Structure table and "What Is and Isn't
Customizable" table in step with any document you add, remove, or rescope.

## Front matter

Every document opens with `title` (`Style Guide: <Topic>`), `date` and `tags`, the first tag
being `style-guide`. Update `date` — and the README's `Last updated` row — on a meaningful
content change, not on a typo, grammar, or formatting-only edit. Documents are numbered
`NN-topic.md` within the Foundations, Components, Content and Interactions areas.

## Content boundaries

- Stay framework-agnostic: how tokens are consumed in code belongs to
  [ADR 0011](../../guide/adrs/0011-centralized-frontend-styling-variables.md), not here.
- Color tokens are the only per-project customization. Never add guidance that lets a project
  override typography, spacing, radius, shadow, motion, or a `color-text-*`,
  `color-background-*` or `color-border-*` token; a new customization point is a change to
  `05-customization-guide.md` and the README table, made through the repository change process.
- Every color token has a light and a dark value, and every rule meets WCAG 2.1 AA.
- When a topic appears in more than one document, the more specific one is authoritative;
  change it there and link to it rather than restating it.
- Keep the SVGs in `design/assets/` in step with the tokens they depict.

## `design/index.json`

Update it in the same change whenever a document is added, removed, retitled, or retagged:
one entry per markdown file with `id` (the file name without `.md`, lower-case), `title` and
`tags` from the front matter, a one-sentence `description`, `category` `style-guide`, and
`relativePath` relative to `design/`. Update the `generated` timestamp.
