---
title: "Style Guide: Interaction Patterns"
date: 2026-07-28
tags: [style-guide, interaction, patterns, forms, navigation, ux]
---
# Interaction Patterns

> This document defines the standard UX patterns for recurring interactions across all products. Consistent patterns reduce cognitive load and make products predictable.

---

## Form Validation

### Validation Strategy

Use **progressive validation**: validate inline, field-by-field, as the user moves between fields. Do not wait until form submission to surface all errors.

| Trigger | Validation Timing |
|---|---|
| Field blur (on tab / click away) | Validate format / required for that field immediately |
| Real-time input (while typing) | Only for character counters and password strength; avoid other real-time errors |
| Form submission | Re-validate all fields; scroll to and focus the first error |

### Error Placement

- Display the error message **immediately below the field** it relates to.
- Use `color-error` surface and foreground text (never color alone).
- Link error text to the input via `aria-describedby`.
- Scroll behavior on submit: smooth-scroll to the first invalid field and focus it.

### Form Submission States

```
[Default] → [Loading] → [Success] | [Error]
```

| State | Primary Button | Field State | Feedback |
|---|---|---|---|
| Default | Active | Editable | — |
| Loading | Spinner, `aria-busy="true"` | Disabled | "Saving…" |
| Success | Restored / hidden | — | Success toast or inline confirmation |
| Error (field) | Restored | Highlighted | Inline error messages |
| Error (system) | Restored | Unchanged | Error banner above form |

### Required Fields

- Mark required fields with an asterisk `*` and include a legend: `* Required field`.
- Place the legend at the top of the form, before the first field.
- Do not mark optional fields — marking only required fields reduces visual noise.

---

## Empty States

Empty states are UI moments where a container has no content. Every empty state must provide:

1. **A clear explanation** of why the view is empty.
2. **A call to action** (when the user can do something about it).
3. **An icon or illustration** to make the state scannable (`icon-xl` or `icon-2xl` from the iconography guide).

### Empty State Layout

```
        [icon-xl]

     Nothing here yet.

  You haven't added any items.
    [Primary action button]
```

### Empty State Variants

| Variant | When | Has Action? |
|---|---|---|
| **First-use** | User has never created content | Yes — primary CTA |
| **Filtered / no results** | Search or filter returns nothing | Yes — "Clear filters" |
| **Permissions** | User lacks access to content | No (or link to request access) |
| **Error** | Content failed to load | Yes — "Try again" |
| **Complete** | All items processed (e.g., empty inbox) | No |

---

## Loading States

### Skeleton Loaders

Use skeleton loaders (content-shaped placeholders) when loading data for content areas (lists, cards, tables). This sets user expectations for the incoming layout.

- Match the shape and size of expected content as closely as possible.
- Animate with a subtle shimmer pulse at `2s` cycle.
- Disable the pulse animation when `prefers-reduced-motion` is active.
- Show skeletons for a maximum of **10 s**; fall back to an error state if content has not loaded.

### Spinner

Use a spinner (indeterminate indicator) for:

- Action-triggered operations (button press → result).
- Global page-level loading when no layout preview is available.

Do not use a spinner as a content placeholder where a skeleton is possible.

### Progress Bar

Use a determinate progress bar for:

- File uploads or downloads with a known total.
- Multi-step processes where the step count is known.

### Stale Data

When refreshing data in an already-populated view:

- Keep existing content visible.
- Show a subtle loading indicator (spinner at `icon-sm` in the header area).
- Do not flash/blank the entire view.

---

## Error Handling

### Error Hierarchy

| Level | Scope | Display Pattern |
|---|---|---|
| **Page-level error** | Entire page fails to load | Full-page error state with retry action |
| **Section error** | One section/widget fails | Inline error card within that section |
| **Action error** | An action (button press) fails | Error toast + restore button state |
| **Field error** | Form field validation fails | Inline error below the field |

### Page-Level Error State

```
        [error icon — icon-xl]

    We couldn't load this page.

  Check your connection and try again.
        [Retry]   [Go back]
```

- Show the error page only when the entire view is unusable.
- Always provide a retry action and a navigation escape (back / home).

### Action Error (Toast)

- Show an error toast for failed button actions.
- The toast message must include the specific failure and a corrective action where possible.
- Re-enable the button after the error so the user can retry.

### Network Offline

Detect offline state and:

1. Show a persistent offline banner at the top of the viewport.
2. Disable form submissions and data-writing actions.
3. Allow reading cached content where available.
4. Automatically retry and dismiss the banner when connectivity is restored.

---

## Navigation Patterns

### Tabs

Use tabs to switch between **peer-level content sections** within the same page context.

| Rule | Detail |
|---|---|
| Maximum tab count | 7 tabs; prefer ≤ 5 |
| Tab labels | Noun or short noun phrase ("Overview", "Settings", "Members") |
| Active indicator | `color-primary` underline, `border-width-2` |
| Keyboard navigation | Arrow keys to move between tabs; `Enter`/`Space` to activate |
| ARIA | `role="tablist"`, `role="tab"`, `role="tabpanel"`, `aria-selected` |
| Scrollable tabs | When tabs overflow, use horizontal scroll with fade-out gradient; do not wrap |

### Breadcrumbs

Use breadcrumbs to show hierarchical location within a deep navigation tree.

```
Home  /  Projects  /  My App  /  Settings
```

- Display only when the hierarchy is ≥ 3 levels deep.
- The last item (current page) is plain text, not a link.
- Separate items with `/` (chevron `›` is acceptable but less universal).
- ARIA: wrap in `<nav aria-label="Breadcrumb">`, mark current page with `aria-current="page"`.

### Side Drawer / Slide-Over

Use a side drawer for secondary navigation or detail panels that do not require full-page navigation.

| Property | Value |
|---|---|
| Width (desktop) | 320–480 px |
| Width (mobile) | Full screen width |
| Overlay | `color-background-overlay` scrim behind drawer |
| Entry transition | `transition-slow`, `ease-out`, slide from edge |
| Exit transition | `transition-base`, `ease-in`, slide out |
| Close triggers | Dismiss button, `Escape` key, scrim click |
| Focus management | Focus moves into drawer on open; returns to trigger on close |
| ARIA | `role="dialog"`, `aria-modal="true"`, `aria-label` or `aria-labelledby` |

### Modal Dialog

Use a modal dialog for focused tasks that require the user's full attention before continuing.

| Property | Value |
|---|---|
| Max width | 480 px (small), 640 px (default), 800 px (large) |
| Overlay | `color-background-overlay` scrim |
| Entry transition | `transition-slow`, `ease-out`, fade + scale from `0.95` |
| Exit transition | `transition-base`, `ease-in`, fade + scale out |
| Close triggers | Dismiss button (×), `Escape` key |
| Scrim click | Only closes non-destructive modals (not confirmation dialogs) |
| Focus trap | Tab focus must remain within the modal until closed |
| Scroll | Long modal content scrolls within the dialog; background does not scroll |
| ARIA | `role="dialog"`, `aria-modal="true"`, `aria-labelledby` pointing to modal title |

### Toast Stacking

When multiple toasts are triggered concurrently:

- Stack vertically with `spacing-sm` gap.
- Limit to 3 visible toasts at once; queue the rest.
- Older toasts animate out before newer ones enter when the queue is full.
- Position: **bottom-right** on desktop, **bottom-center** on mobile.

---

## Selection Patterns

### Single Selection

Use radio buttons or a segmented control for mutually exclusive selections from a small set (≤ 5 options).

### Multi-Selection

Use checkboxes for independent toggleable selections. For bulk operations on a list:

- Provide a "Select all" checkbox in the table/list header.
- Show a bulk action bar when ≥ 1 item is selected (position: fixed bottom or top of list).
- Display the count of selected items: "3 items selected".
- Include a "Deselect all" / "Clear selection" control in the bulk bar.

### Indeterminate State

A "Select all" checkbox must use the **indeterminate** state when some (but not all) list items are selected.

---

## Inline Editing

Use inline editing sparingly — primarily for single-field quick-edit scenarios (e.g., renaming an item).

| Rule | Detail |
|---|---|
| Trigger | Single click on the value (show a pencil icon on row hover) |
| Commit | `Enter` key or click away (blur) |
| Cancel | `Escape` key — restores original value |
| Feedback | Subtle flash animation on saved value (`ease-bounce`, `transition-base`) |
| Error | Inline error below the field; do not dismiss on error |

---

## Drag and Drop

Use drag-and-drop for explicit reordering tasks where the positional order is meaningful (e.g., sorting a list).

- Provide a visible drag handle (`grip-vertical` icon at `icon-md`).
- Show a placeholder in the drop target position during drag.
- Provide keyboard-accessible reordering as an alternative (e.g., move-up/move-down buttons or arrow-key handling with `role="listbox"`).
- Announce position changes to screen readers using a live region: "Item moved to position 3 of 8."
