---
title: "Style Guide: Component Patterns"
date: 2026-07-28
tags: [style-guide, components, patterns, accessibility, design-tokens]
---
# Component Patterns

> Component specifications define anatomy, states, variants, and accessibility requirements for every reusable UI element. All tokens referenced here are defined in the preceding style guide documents.

---

## Button

### Purpose

Buttons trigger actions. They are the primary affordance for user intent — submitting a form, opening a dialog, executing a command.

### Anatomy

```
┌─────────────────────────┐
│  [icon?]  Label text    │
└─────────────────────────┘
```

| Sub-element | Required | Notes |
|---|---|---|
| Label | Yes | Descriptive verb phrase (e.g. "Save changes", not "OK") |
| Leading icon | Optional | Reinforces the action; never decorative-only in a button |
| Trailing icon | Avoid | Reserved for chevrons in split buttons only |
| Loading spinner | Conditional | Replaces label+icon during async operations |

### States

| State | Visual |
|---|---|
| Default | `color-primary` fill, `color-text-inverse` label, `border-radius-md`, `shadow-sm` |
| Hover | `color-primary-light` fill, `shadow-md` — transition: `transition-fast`, `ease-in-out` |
| Focus | Default fill + `color-border-focus` outline (`2px`, `2px` offset) |
| Active / Pressed | `color-primary-dark` fill, `shadow-inner`, scale `0.98` |
| Disabled | `color-text-disabled` fill, `cursor: not-allowed`, no hover/focus response |
| Loading | Spinner replaces label; pointer events disabled; `aria-busy="true"` |

### Variants

#### By Emphasis

| Variant | When to Use | Visual |
|---|---|---|
| **Primary** | The single most important action on a page | Filled with `color-primary` |
| **Secondary** | Supporting actions (e.g. "Cancel") | Outlined with `color-primary`, transparent fill |
| **Ghost** | Tertiary actions in toolbars or dense UIs | Text-only, no border/fill |
| **Destructive** | Irreversible actions (e.g. "Delete", "Remove") | Filled with `color-error` surface + strong foreground |
| **Link** | Inline contextual navigation masquerading as an action | Unstyled, `color-text-link`, underline on hover |

#### By Size

| Token | Padding (V × H) | Font Size | Usage |
|---|---|---|---|
| `btn-sm` | `spacing-xs` × `spacing-sm` | `font-size-sm` | Dense toolbars, table row actions |
| `btn-md` | `spacing-sm` × `spacing-md` | `font-size-base` | Default for most UI |
| `btn-lg` | `spacing-md` × `spacing-xl` | `font-size-lg` | Hero / call-to-action sections |

### Accessibility Requirements

- Role: native `<button>` element (preferred) or `role="button"` with `tabindex="0"`.
- Keyboard: `Space` and `Enter` activate the button.
- Loading state: set `aria-busy="true"` and keep the button text visible to screen readers (use `aria-label` if the spinner replaces visible text).
- Disabled state: use the `disabled` attribute on native buttons (not `aria-disabled` alone), which removes the element from tab order.
- Icon-only buttons: must have `aria-label` describing the action; never rely on tooltip alone.
- Minimum touch target: 44 × 44 px on mobile.

### Do / Don't

| ✅ Do | ❌ Don't |
|---|---|
| Use a single Primary button per view | Stack two Primary buttons side by side |
| Write labels as verb phrases: "Save changes" | Use vague labels: "OK", "Yes", "Submit" |
| Show a loading spinner for async actions | Leave button in default state during loading |
| Use Destructive variant for irreversible actions | Use Primary (blue/brand) for delete actions |
| Include an icon only when it adds clarity | Use icons on every button regardless of context |

---

## Form Input (Text)

### Purpose

Collects a single line of free-text input from the user. Extends to email, password, search, and number input types.

### Anatomy

```
Label text                      [optional hint badge]
┌────────────────────────────────────────────────────┐
│ [leading icon?]  Placeholder or value  [trailing?] │
└────────────────────────────────────────────────────┘
Helper text or error message
```

| Sub-element | Required | Notes |
|---|---|---|
| Label | Yes | Always visible; never use placeholder as a label substitute |
| Input field | Yes | `border-radius-md`, `border-width` border in `color-border` |
| Placeholder | Optional | Short format hint, not a requirement statement |
| Leading icon | Optional | Conveys input type (e.g. magnifier for search) |
| Trailing icon | Optional | Clear button, visibility toggle (password), or status icon |
| Helper text | Recommended | Displayed below the field; provides format or context guidance |
| Error message | Conditional | Replaces helper text when the field is in error state |
| Character counter | Conditional | Shown when a `maxlength` constraint exists |

### States

| State | Visual |
|---|---|
| Default | `color-border` border, `color-background` fill, `color-text-primary` value, `color-text-secondary` placeholder |
| Hover | `color-border-strong` border — transition: `transition-fast` |
| Focus | `color-border-focus` border (`2px`), `shadow-sm`, `color-text-primary` value |
| Filled | `color-text-primary` value, `color-border` border |
| Disabled | `color-background-alt` fill, `color-text-disabled` text, `cursor: not-allowed` |
| Read-only | `color-background-alt` fill, `color-text-secondary` text, no focus ring |
| Error | `color-error` border (`2px`), error message below in `color-error` foreground text |
| Success / Valid | `color-success` border (`2px`), success icon in trailing position |

### Variants

| Variant | When to Use |
|---|---|
| **Default** | Any single-line text, email, number collection |
| **Password** | Sensitive credentials; include show/hide toggle |
| **Search** | Filter / search UIs; leading magnifier icon, optional clear button |
| **Prefix / Suffix** | Values with known units (e.g. "$", "px", ".com") — use inline affix text |

### Accessibility Requirements

- Every input must have an associated `<label>` using `for`/`id` pairing (preferred) or `aria-labelledby`.
- Never use `placeholder` as the only label; placeholders disappear on focus.
- Error messages must be linked via `aria-describedby` so screen readers announce the error.
- Required fields must be indicated visually (asterisk + legend) **and** via `aria-required="true"` or the `required` attribute.
- Group related inputs with `<fieldset>` and `<legend>` (e.g., address fields).
- Autocomplete attributes (`autocomplete="email"`, `autocomplete="new-password"`) must be set for common field types to support password managers and autofill.

### Do / Don't

| ✅ Do | ❌ Don't |
|---|---|
| Always show a visible label above the field | Use placeholder text as the only label |
| Show inline error messages immediately below the field | Display errors only in a summary banner at the top |
| Link error messages with `aria-describedby` | Rely on color alone to signal error state |
| Use helper text for format guidance (e.g. "DD/MM/YYYY") | Leave users to discover format requirements after an error |
| Preserve the user's input when showing a validation error | Clear the field on error |

---

## Card

### Purpose

Groups related content (text, media, actions) into a bounded, scannable surface. Cards are used in grids, lists, and dashboard layouts.

### Anatomy

```
┌──────────────────────────────────┐
│  [optional media / image]        │
│──────────────────────────────────│
│  Title                           │
│  Body text or metadata           │
│──────────────────────────────────│
│  [Action 1]   [Action 2]         │
└──────────────────────────────────┘
```

| Sub-element | Required | Notes |
|---|---|---|
| Title | Yes | Identifies the card's subject |
| Body | Recommended | Supporting text, metadata, or summary |
| Media | Optional | Image or icon header |
| Actions | Optional | Button(s) or link(s) in card footer |

### States

| State | Visual |
|---|---|
| Default | `color-background` fill, `border-radius-md`, `border-width` in `color-border`, `shadow-md` |
| Hover (interactive) | `shadow-lg`, slight `color-background-alt` tint — `transition-base`, `ease-in-out` |
| Focus (keyboard) | `color-border-focus` outline (`2px`, `2px` offset) |
| Selected | `color-border-focus` border (`2px`), optional `color-primary` accent strip |
| Disabled | `color-background-alt` fill, all children at `color-text-disabled` opacity |

### Accessibility Requirements

- If the entire card is clickable, wrap content in a single `<a>` or `<button>` with a descriptive `aria-label`.
- If the card has multiple interactive children, do **not** make the entire card a single link — keep individual actions.
- Cards in a list must be navigable by keyboard tab order.
- Images within cards require `alt` text describing the image content (not "card image").

---

## Badge / Chip

### Purpose

Displays a compact status label, count, or metadata tag. Chips may be interactive (filterable, removable); badges are always decorative/informational.

### Anatomy

| Sub-element | Badge | Chip |
|---|---|---|
| Label | Yes | Yes |
| Icon | Optional | Optional |
| Remove button | No | Optional |

### Variants

| Variant | Background | Text | When to Use |
|---|---|---|---|
| Default | `color-background-alt` | `color-text-secondary` | Neutral metadata tags |
| Primary | `color-primary` | `color-text-inverse` | Active selection, highlighted status |
| Success | `color-success` | Foreground on success surface | Positive status |
| Warning | `color-warning` | Foreground on warning surface | Cautionary status |
| Error | `color-error` | Foreground on error surface | Negative status, validation errors |
| Info | `color-info` | Foreground on info surface | Informational metadata |

### Accessibility Requirements

- Use `role="status"` for dynamically updated badge counts (e.g., notification count).
- Removable chips require the remove button to have `aria-label="Remove [tag name]"`.
- Color-based variants must include a text label or icon — never convey state with color alone.

---

## Toast / Snackbar Notification

### Purpose

Provides brief, non-blocking feedback about an operation result. Appears automatically and dismisses itself after a timeout.

### Anatomy

```
┌──────────────────────────────────────────────┐
│ [icon]  Message text           [Dismiss ×]  │
└──────────────────────────────────────────────┘
```

### Variants

| Variant | Icon | Color |
|---|---|---|
| Success | ✓ checkmark | `color-success` surface |
| Warning | ⚠ triangle | `color-warning` surface |
| Error | ✕ or ! | `color-error` surface |
| Info | ℹ circle | `color-info` surface |

### Behavior

- Auto-dismiss after **5 s** (success/info) or **8 s** (warning/error).
- Pause auto-dismiss on hover or focus.
- Allow manual dismissal at all times via the dismiss button.
- Stack multiple toasts vertically; limit to 3 visible at once.
- Position: bottom-right on desktop, bottom-center on mobile.

### Accessibility Requirements

- Use `role="status"` for success/info and `role="alert"` for warning/error.
- `role="alert"` is announced immediately by screen readers; use it only for errors and warnings.
- The dismiss button must have `aria-label="Dismiss notification"`.
- Toasts must not interrupt keyboard focus — they appear in the DOM but do not steal focus.
