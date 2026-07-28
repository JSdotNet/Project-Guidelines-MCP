---
title: "Style Guide: Voice and Tone"
date: 2026-07-28
tags: [style-guide, voice, tone, copy, content, ux-writing]
---
# Voice and Tone

> This document is canonical for UI copy rules and message examples. Layout, timing, and interaction behavior for these moments live in [09 - Interaction Patterns](09-interaction-patterns.md).

---

## Personality

The products in this organization communicate with users in a way that is:

| Dimension | Description |
|---|---|
| **Clear** | Plain language, short sentences, no jargon unless the audience is technical and expects it |
| **Direct** | State the point first. Don't bury the action or outcome in qualifications |
| **Helpful** | Anticipate what the user needs to know next and provide it proactively |
| **Respectful** | Treat users as capable adults. Avoid condescending micro-copy |
| **Calm** | Even in error states, avoid alarming or accusatory language |

### What we are not

- We are not **casual or playful** - no jokes, puns, or excessive friendliness in task-critical UI.
- We are not **formal or stiff** - no legalistic phrasing or passive voice where active is clearer.
- We are not **vague** - never say "something went wrong" without a path to resolution.

---

## Grammar and Mechanics

| Rule | Correct | Incorrect |
|---|---|---|
| Sentence case for UI labels | "Save changes" | "Save Changes" |
| Title case for page titles only | "User Settings" (page H1) | "save changes" |
| Active voice | "Your file was saved." → "We saved your file." | "Your file has been successfully saved by the system." |
| Contractions are fine in messages | "We couldn't find that page." | "We were unable to locate that page." |
| Oxford comma | "Name, email, and password" | "Name, email and password" |
| Numbers < 10 spelled out in prose | "three retries" | "3 retries" |
| Numbers >= 10 as numerals | "12 items" | "twelve items" |

---

## Button Labels

Buttons must use **verb + noun** phrasing that describes the action and its target.

| ✅ Correct | ❌ Incorrect | Why |
|---|---|---|
| "Save changes" | "Save" | Ambiguous when multiple save targets exist |
| "Delete project" | "Delete" | Makes the target explicit; avoids accidental deletions |
| "Add team member" | "Add" | Specifies what is being added |
| "Cancel" | "No" / "Abort" | Universal convention; users expect it |
| "Send report" | "Submit" | "Submit" is generic; "Send report" is specific |
| "Sign in" | "Login" | Prefer natural language over technical terms |
| "Sign out" | "Logout" | Same - natural language |

---

## Error Messages

Errors must follow a three-part structure: **what happened → why → what to do next**.

### Required elements

1. **What** - state the problem clearly and specifically.
2. **Why** - only if it adds actionable context (skip if obvious).
3. **Next step** - always provide a way forward.

### Examples

**Form validation error:**

| ✅ Good | ❌ Poor |
|---|---|
| "Enter a valid email address (e.g. name@example.com)." | "Invalid email." |
| "Password must be at least 8 characters and include one number." | "Password too weak." |
| "This email is already registered. [Sign in instead](#) or [reset your password](#)." | "Email already exists." |

**System / network error:**

| ✅ Good | ❌ Poor |
|---|---|
| "We couldn't save your changes. Check your connection and try again." | "An error occurred." |
| "We couldn't load the project list. [Try again](#) or [contact support](#) if this keeps happening." | "Error 500. Please try again later." |
| "Your session expired. [Sign in](#) to continue." | "Unauthorized." |

### Tone in error messages

- **Never blame the user.** Write "We couldn't complete that action" not "You entered an invalid value" where avoidable.
- **Never use raw error codes** as the only message. Codes may appear as secondary detail text but never as the primary message.
- **Avoid exclamation marks** in error and warning messages.

---

## Empty States

Use the layout, icon, and CTA structure from [09 - Interaction Patterns](09-interaction-patterns.md). This section defines the **copy** for those states.

| Context | Message | Action |
|---|---|---|
| No projects yet | "You haven't created any projects yet." | "Create your first project" |
| No search results | "No results for '[query]'. Try different keywords or [clear filters](#)." | "Clear filters" |
| No notifications | "You're all caught up. No new notifications." | _(no action needed)_ |
| Empty inbox | "Your inbox is empty." | _(no action needed)_ |
| No team members | "This project has no team members. Invite someone to collaborate." | "Invite team member" |

---

## Confirmation Dialogs

Use the modal and destructive-action behavior from [09 - Interaction Patterns](09-interaction-patterns.md). This section defines the **wording** inside those dialogs.

Confirmation dialogs must:

- Title: state the action being confirmed (not "Are you sure?").
- Body: describe the consequence, especially for irreversible actions.
- Primary action: repeat the verb from the title.
- Secondary action: "Cancel".

| Element | ✅ Good | ❌ Poor |
|---|---|---|
| Title | "Delete project?" | "Are you sure?" |
| Body | "This will permanently delete **My App** and all its data. This action cannot be undone." | "This action is irreversible." |
| Primary button | "Delete project" | "Yes" / "OK" / "Confirm" |
| Secondary button | "Cancel" | "No" / "Go back" |

---

## Loading and Progress

Use the loading-state selection rules from [09 - Interaction Patterns](09-interaction-patterns.md). This section defines the **copy style** for each loading moment.

| State | Copy |
|---|---|
| Short operation (< 2 s) | No copy needed - spinner only |
| Medium operation (2–10 s) | "Saving..." / "Loading..." / "Processing..." |
| Long operation (> 10 s) | "Uploading file (2 of 5)..." - show progress and context |
| Background operation complete | Toast: "Changes saved." / "Report ready." |

- Always use present participle form for in-progress state: "Saving..." not "Save in progress".
- Use past tense for completion toasts: "Saved." / "Deleted." / "Sent."

---

## Tooltips

- Keep tooltip copy to **one short sentence or phrase** (<= 10 words).
- Tooltips clarify the function of an icon or control - they must not repeat the label.
- Never put critical information in a tooltip. If the user must read it to avoid an error, it belongs inline.

| ✅ Correct | ❌ Incorrect |
|---|---|
| "Download as CSV" | "This button downloads a CSV file of your data." |
| "Required field" | "You need to fill in this field before submitting the form." |

---

## Placeholders

- Placeholder text is a **format hint**, not a label.
- Use short, concrete examples: `e.g. name@example.com`, `YYYY-MM-DD`, `Search projects...`
- Never write requirements in placeholders: "Must be at least 8 characters" belongs in helper text, not the placeholder.

---

## Capitalization Quick Reference

| UI Element | Case |
|---|---|
| Page / view titles (H1) | Title Case |
| Section headings (H2–H4) | Sentence case |
| Button labels | Sentence case |
| Navigation items | Sentence case |
| Table column headers | Sentence case |
| Form field labels | Sentence case |
| Toast / notification messages | Sentence case |
| Error messages | Sentence case |
| Tooltip text | Sentence case |



