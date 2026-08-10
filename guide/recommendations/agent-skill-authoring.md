---
title: "Agent Skill Authoring: Scope, Preconditions, and Descriptions"
date: 2026-08-10
status: Accepted
tags: [skills, agent-skills, authoring, copilot, orchestration, routing, recommendations]
---
# Recommendation: Agent Skill Authoring — Scope, Preconditions, and Descriptions

## Purpose

Define how to word an Agent Skill (`SKILL.md`) so an agent cannot wrongly exclude a skill that should
have applied.

This guideline covers **authoring semantics**: the frontmatter `description`, the precondition and
scope statements in the body, and the difference between a missing input and a genuine stop
condition. It does not cover packaging, distribution, or MCP exposure — for those, see
*Recommendation: Building an MCP Server with Included Skills*.

The failure this addresses is silent. A skill that excludes itself produces no error, no warning, and
no trace in the transcript: the work simply gets done inline, without the governance, validation, or
review stages the skill existed to enforce.

---

## Context: How a Skill Gets Excluded

Skill selection has **two gates**, and a skill is invoked only if it passes both.

| Gate | What the agent reads | What happens on failure |
|---|---|---|
| 1. Picker | `name` + `description` only | The body is never loaded; the skill is invisible for that request |
| 2. Body | The full `SKILL.md` | The agent opens the skill, reads a precondition it cannot satisfy, and proceeds inline |

Two consequences follow, and they drive every rule below.

- **Gate 1 failures are unrecoverable.** The corrective wording in the body is never read, because
  the body is never opened. Any narrowing in the `description` is therefore final.
- **Gate 2 failures look like correct reasoning.** An agent that reads "this skill assumes the
  specification is already approved" and concludes the skill does not apply to an ad-hoc request has
  reasoned *correctly from the text*. The defect is in the text, not the agent.

A precondition written as a disqualifier is a licence to skip the skill. That licence then propagates
into Gate 1 whenever the same qualifier is summarised into the `description`.

---

## Rules

### R1 — Descriptions state when the skill applies, never when it does not

The `description` must not contain a scope-narrowing qualifier: *"once X is known"*, *"assumes X"*,
*"only after X"*, *"for approved X"*, *"when X has already been agreed"*.

The `description` is the only text read before the body is opened. A qualifier placed there removes
the skill from consideration permanently, for exactly the requests most likely to need it.

### R2 — The description must cover the full range the body handles

Description scope may be equal to or **wider** than body scope. It must never be narrower.

If the body can handle a small, incremental, or ad-hoc request, the `description` must say so in
words a picker can match on. Enumerate the request shapes at the edges of scope; do not rely on the
reader inferring that "feature implementation" includes a one-line change.

### R3 — Preconditions describe coverage, not gates

Write preconditions as *"this skill covers X, Y, Z"*, not *"this skill assumes X, Y, Z are already
done"*.

A precondition an agent can fail is a precondition an agent will use to exit. State what the skill is
responsible for producing and validating, and let the stages describe how each input is obtained.

### R4 — A missing input routes to a discovery stage, never to an exit

If the skill needs a specification, acceptance criteria, a reproduction, or a design that does not
exist yet, **producing it is the skill's first stage**.

Absence of a written artefact is never grounds for declining the work, handing it back to the user,
or letting the agent proceed inline. See *Stage 0: Scope Discovery* below.

### R5 — A stop condition is legitimate only when it routes to a named alternative

Genuine stop conditions exist and must not be removed. A stop condition is legitimate when **both**
hold:

1. It names the specific skill, agent, or orchestration that takes over.
2. It is stated in terms of a **decision that must be made elsewhere**, not a **document that has not
   been written yet**.

An exit with no named successor is a defect. So is an exit that instructs the agent to ask the user
to run something the agent could invoke itself.

### R6 — A specialist and its fallback must not both decline

If a skill's `description` excludes work on the grounds that "a dedicated skill exists", the
catalogue owner must confirm the dedicated skill's `description` actually accepts that work.

Mutual exclusion between a specialist and its fallback creates a dead zone that neither skill's
author can see from their own file. This is a catalogue-level defect and must be checked across
files, not within one.

---

## Distinguishing Derive from Escalate

Use one test: **could a competent engineer write the missing artefact from the request and the
codebase?**

- **Yes** → derive it in Stage 0.
- **No, because a choice with lasting consequences is unresolved** → escalate to the named owner of
  that choice.

| Situation | Classification | Action |
|---|---|---|
| Specification or acceptance criteria not written down | Input not yet captured | Derive in Stage 0 |
| Bug has no reproduction steps | Input not yet captured | Derive in Stage 0 |
| Request is small, ad-hoc, or a one-line change | Input is trivially derivable | Derive in Stage 0 |
| Change requires a new architectural decision | Decision belongs elsewhere | Escalate to `orch-adr` / `orch-architecture` |
| Change requires a new bounded context or service boundary | Decision belongs elsewhere | Escalate to `orch-create-service` / `orch-architecture` |
| Requester's intent is genuinely ambiguous between outcomes | Human input required | Ask one clarifying question inside Stage 0, then continue |

Do not over-correct. Removing all stop conditions is as harmful as writing them as disqualifiers: it
lets a skill absorb work that belongs to an architecture or decision-record workflow. Some
operational preconditions are also legitimate — for example, a dependency-update skill requiring a
green build before it starts. Such a condition passes R5 as long as it names what to do when the
build is red, rather than declining the request.

---

## Stage 0: Scope Discovery

Every code-modifying or artefact-modifying skill that depends on an upstream input should open with a
discovery stage of this shape:

```markdown
## Stage 0 — Scope Discovery

Determine the working scope before implementing.

- If a specification, issue, or acceptance criteria already exist, read them and continue to Stage 1.
- If they do not, derive them from the request and the codebase: state the intended behaviour, the
  affected components, and the acceptance criteria you will validate against. Record them in the
  session artifacts; do not require a separate approved document.
- Ask at most one clarifying question, and only when two reasonable outcomes are genuinely
  indistinguishable from the request.
- Escalate only if the work requires a decision this skill does not own (see the escalation table).
```

Rules for the stage:

- It must be reachable for **every** entry path, including the smallest request.
- It must terminate in continuation, never in "ask the user to run another skill first".
- The derived scope is an input to later stages, not a deliverable requiring approval.

---

## Anti-Patterns

### Anti-pattern 1: a narrowing qualifier in the frontmatter description

Real wording from `orch-feature`, before:

> Orchestrate feature implementation through local run and monitoring using GitHub Copilot App
> canvas. Focuses on coding, validation, review, and personal approval **once the feature scope is
> already known.**

The trailing clause removed the skill from the picker for every request that did not arrive with a
written scope — which is most of them. **Fix:** drop the qualifier and state coverage of ad-hoc and
incremental requests explicitly.

### Anti-pattern 2: a precondition written as a disqualifier

Real wording from `orch-feature`, before:

> This skill assumes the feature specification, acceptance criteria, and architecture are already
> approved... Use this skill to implement that approved scope, **not to discover it from scratch.**

Given a request such as *"make sub-items draggable"*, an agent reads this and correctly concludes the
skill does not apply. **Fix:** replace with a Stage 0 that derives the specification and acceptance
criteria when they are absent, and proceeds directly when they are present.

### Anti-pattern 3: a shared instructions file that repeats the disqualifier

A shared phase or instruction file that restates the same precondition for a whole family of skills
overrides any fix made in an individual `SKILL.md`. Audit shared files first: a rule stated there
applies everywhere and is the highest-leverage place for this defect to hide.

### Anti-pattern 4: an exit with no invocable successor

> Stop and ask the user to run a specification orchestration first.

This ends the turn without producing anything and asks the human to do work the agent could do.
**Fix:** either derive the input (R4) or invoke the successor directly (R5).

### Anti-pattern 5: scoping by process maturity instead of work type

Phrases like "for approved work", "for planned features", or "for specified changes" describe how
mature the *process* around the request is, not what *kind of work* it is. Pickers match on work
type. Scope by work type.

---

## Audit Checklist

Apply per `SKILL.md`. Any **fail** requires a fix before the skill is considered compliant.

| # | Check | Fail condition |
|---|---|---|
| 1 | Frontmatter `description` narrowing | Contains "once", "assumes", "already", "only after", "approved", or an equivalent qualifier that restricts scope |
| 2 | Description covers the edges | The body handles small, incremental, or ad-hoc requests but the `description` does not say so |
| 3 | Body preconditions | Body contains "assumes", "already approved", "already known", "not to discover", or an equivalent gate |
| 4 | Missing inputs have a producer | Any input the skill declares as required has no stage that produces it when absent |
| 5 | Exits name a successor | Any stop condition does not name the specific skill, agent, or orchestration that takes over |
| 6 | Shared files clean | An included or shared instruction/phase file reintroduces a disqualifier |
| 7 | No mutual exclusion | A sibling fallback skill's description excludes this skill's work while this skill also excludes it |

A reviewer should be able to reach a verdict on each row from the file text alone.

---

## References

- Recommendation: Building an MCP Server with Included Skills
- Recommendation: Copilot Instruction-File Setup
- Config Guideline: .mcp.json
- Agent Skills specification: https://agentskills.io/specification
- GitHub Copilot CLI — add skills: https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-skills
