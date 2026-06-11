# Planning Core

Core planning logic for all automation planning turns. Always load this file.

Conditional companions:
- Callable/orchestration lifecycle → `references/planning-callable.md`
- Authentication surfaces → `references/planning-auth.md`
- Durable memory present in request → `references/planning-durable-memory.md`

---

## Table of Contents
1. [Planning Context](#1-planning-context)
2. [Completed Steps Record](#2-completed-steps-record)
3. [Goal Analysis](#3-goal-analysis)
4. [Advancement and Completion](#4-advancement-and-completion)
5. [Ask Decision Table](#5-ask-decision-table)
6. [Toolbox and Service Steps](#6-toolbox-and-service-steps)
7. [Todo Policy](#7-todo-policy)
8. [Input Parameters](#8-input-parameters)
9. [Validation](#9-validation)

---

## 1. Planning Context

Use the current goal as the immediate target.
Use the primary goal and goal history to avoid repetition and choose the next best advancement.
Do not repeat already completed steps unless repeating them is genuinely required by the current goal.

**Vague or retry-oriented goals:** If the current goal is vague (`"try again"`, `"continue"`, `"next"`, `"keep going"`, `"retry"`), fall back to the primary goal and current surface to determine the next best steps. Generate the smallest valid grounded advancement sequence that moves closer to the primary goal. Do not emit a `TodoStep` for work that can be advanced using currently grounded controls.

---

## 2. Completed Steps Record

The `# Completed Steps` section is an authoritative record of steps executed in prior turns.

- Do not regenerate any step already described in this section, even if the current hierarchy or goal wording suggests it is needed.
- Before emitting any step, check whether it or its logical equivalent already appears in completed steps. If so, skip it.
- This applies to all step types: search triggers, credential entry, navigation, tab selection, form fill.

Each entry is prefixed with `core:` or `navigation:` reflecting the `AutomationCategory` of the turn that produced it.

- No `core:` entries → current turn is the **first core turn**. For Callable automations this determines whether the no-reset rule applies.
- Only `navigation:` entries → all prior turns were navigation-only.
- Strip the prefix before comparing an entry against a candidate step to avoid false mismatches.

---

## 3. Goal Analysis

### Grounding rule (MUST — canonical statement)
Use only controls, objects, and actions grounded in the supplied automation surface. This single rule governs all step generation; other files reference it rather than restating it.

**Prohibited everywhere** (steps, descriptions, StepName, CompletedStepSummaries, all string output):
- Controls not present in the current hierarchy
- References to future screens, post-transition state, or speculative UI
- Invented control names, IDs, element references, or suggested names for not-yet-present controls

If completing the goal requires a future screen, name only the data or outcome needed — never the ungrounded screen or control.

### Non-application goals
If the goal includes tasks that are not UI interactions (system info, environment checks, version lookup, file/registry inspection), plan those with toolbox capability areas — not application steps. An application hierarchy's presence does not force all steps to be application-derived.

### Inferred prerequisites (MUST)
Always infer prerequisite actions that make a downstream control valid, visible, or meaningful, and insert them immediately before the dependent action. Hidden dependency families: tabs → panels, radio buttons → content areas, checkboxes → groups, dropdowns → filtered fields. Never assume the UI is already in the correct state when inference suggests a dependency.

**Selector-scoped values:** When a goal requests a value meaningful only within a specific UI scope (tab, radio group, dropdown, checkbox, mode button), the scope must be set before reading dependent fields. The canonical enforcement procedure is the **pre-match selector context check** in `references/application-matching.md` section 2 — run it for every read target. Key constraints: a field is selector-dependent only when the hierarchy grounds the dependency; read selector-independent fields first; skip the selector step when authoritative state metadata confirms the scope is already correct.

### Flip rule (canonical statement)
Before generating a screen-change step, check whether the downstream controls required to complete the goal are already present. If they are, skip the screen-change step and work directly with those controls.

---

## 4. Advancement and Completion

### Decision table — evaluate top to bottom, first match wins

| # | Condition | Action | GoalCompleted |
|---|---|---|---|
| 1 | All controls needed to finish the goal are grounded on the current surface | Complete the goal now: enabling steps + inputs + actions + reads in one sequence | `true` |
| 2 | Goal is action-only (search/submit/send/trigger, no read-back required) and the trigger control is grounded | Plan through the trigger step. **First check the action-only confirmation row in the Ask Decision Table** | `true` after trigger (subject to confirmation) |
| 3 | Completion depends on a state change whose result controls are not yet present | Emit only the grounded advancement sequence (login, navigation, search trigger, open form/dialog/tab), then **stop** | `false` |
| 4 | No grounded advancement exists and no completion is possible | Apply Todo Policy (section 7) — never invent controls | `false` |

### Hard constraints on every row
- **Stop means stop:** after an advancement-only sequence expected to change screen or state, emit nothing further. No placeholders, no likely-next-screen steps, no downstream work.
- **No premature completion (MUST):** never set `GoalCompleted = true` when the goal requires reading values from a post-action screen whose result controls are not yet grounded.
- **In-place search:** when the user confirms results populate in place (Case 3, `references/application-matching.md` section 2a) and result controls are grounded — plan trigger + reads in one turn, row 1 applies. If the user says it navigates away — row 3 applies, stop after the trigger.

---

## 5. Ask Decision Table

All `ask_user` triggers in one place. Scan this table at two points: (a) after goal analysis, before generating steps; (b) whenever a trigger condition is hit mid-generation. Question formats and answer handling live at the listed source.

| Trigger condition | Source for format + answer handling |
|---|---|
| Goal can be satisfied by 2+ distinct automation approaches, or scope is unclear (single vs. batch) | SKILL.md Phase 2 |
| Goal implies a Callable/Orchestration split not externally requested | `planning-callable.md` §1 |
| Auth surface has mixed/ambiguous signals (SSO + local fields, Login button without credential fields) | `planning-auth.md` §2 |
| Turn's steps are ambiguous between navigation and core classification | `output-contract.md` AutomationCategory section |
| Semantic match found only via Priority 4 (synonym reasoning) | `application-matching.md` §2 |
| Two controls are equally plausible candidates for one action | `application-matching.md` §2 |
| Disabled selector controls with no planned trigger explaining the disable | `application-matching.md` §2 pre-match check → §2a Case 4 |
| Result controls present-but-empty alongside a planned search trigger (in-place search) | `application-matching.md` §2a Case 3 |
| Disabled control, cause unknown (no Case 1–3 match) | `application-matching.md` §2a Case 4 |
| Action-only goal with empty confirmation/status controls present, or ambiguous read-back wording | below — Action-only confirmation |
| Semantic matching fully exhausted, TodoStep is the candidate | section 7 — ask before any TodoStep |
| Multiple dismiss options on a callable reset turn | `planning-callable.md` §5 |

**Universal answer-handling rules:**
- Apply answers to this turn only; never carry forward as standing rules or write to durable memory.
- Never ask the same question twice in one turn. Batch related questions into one `ask_user` call.
- A steering answer (user describes a correction) is incorporated into this turn's plan directly.

### Action-only confirmation (MUST when signals present)
Before `GoalCompleted = true` on an action-only goal, check the hierarchy for present-but-empty confirmation controls (status labels, result codes, message areas, reference number displays). Ask when such controls exist alongside the trigger, or when goal wording is ambiguous about needing a result ("process the record", "complete the transaction"). Do not ask when the goal clearly wants only the trigger and no confirmation controls are visible, or the confirmation controls were already read this turn.

> "After [trigger action], this screen has [status/result control] that appears to return a result. Do you need to capture that value? Yes — read [control name] after the trigger / No — the trigger step is sufficient"

Yes → add the read step; `GoalCompleted` reflects whether all required values are captured. No → `GoalCompleted = true` after the trigger.

---

## 6. Toolbox and Service Steps

### Grounding rule
Generate a concrete `MethodStep` only when the service name and method name are grounded in a loaded toolbox catalog or durable memory signature.

**Catalog coverage is deterministic:** if any loaded catalog contains a method covering the work, a concrete `MethodStep` is required. Semantic placeholders and `TodoStep` are prohibited for that work.

### Semantic placeholders
When the capability area is known but the catalog is not loaded:
1. Do not invent a method name, signature, overload, parameter list, or result field.
2. `ParentObject` = capability area name (e.g., `ProductInfo`, `Environment`) or `"Toolbox"` if uncertain.
3. `MethodName` = generic intent verb only: `Get`, `Read`, `List`, `Check`, `Resolve`, `Detect`.
4. `StepDescription` states the value needed, the capability area, and that the exact method is selected at implementation.

If the placeholder's value is needed downstream, follow with a `ValueStep` capturing into a semantic reference (e.g., `ProductInfo.Result`). Never output product-specific method names that appear real, parameter lists, or specific result field names beyond the generic reference.

---

## 7. Todo Policy

### Planning control check — evaluate first
Read `AllowTodoSteps` from Planning Controls in `references/output-contract.md`.
If `false` — `TodoStep` is prohibited for the entire response. Skip all rules below. Omit actions that cannot be completed.

### Ask before emitting a TodoStep (MUST)
A `TodoStep` must never be emitted without first asking. It is a last resort, not a default when matching fails.

> "I couldn't find a control for [specific action]. Can you help?
> - It's the [user describes the control or label] — use that
> - There's no control for this yet — add a placeholder (TodoStep)
> - Skip this step — it's not needed"

**Applying the answer:**
- **Control hint** → use the hint as a search key; re-run semantic matching Priorities 1–4 (`application-matching.md` §2); resolve identity from the matched node via the same-node rule. Never use the hint text directly as `ControlName`. If the re-run fails, present only TodoStep/skip — ask at most once more.
- **Add TodoStep** → emit per the TodoDescription rules in `output-contract.md`. This is explicit consent.
- **Skip** → omit the step entirely.

### Todo gating rule (MUST) — all six must hold
1. Semantic matching fully exhausted, including any hint re-run.
2. User explicitly consented via the question above.
3. No grounded advancement step exists → otherwise emit advancement steps instead.
4. The missing item is not a plain input value (see section 8).
5. No loaded toolbox catalog method covers the work → otherwise emit a `MethodStep`.
6. No durable memory automation covers the work → otherwise emit a `MethodStep`.

### Advancement-first without Todo (MUST)
If any grounded advancement sequence exists, emit only that sequence. Never emit a `TodoStep` for downstream work in the same response.

---

## 8. Input Parameters

When a step requires an input value (search key, ID, name, lookup term) not specified in the goal or context:
- Do not emit a `TodoStep` to obtain the value.
- Treat it as a runtime input parameter supplied by the caller at execution time.
- Emit the step normally with `StaticValue: null`.

A `TodoStep` for a missing input value is only permitted when the goal explicitly states the value must come from a specific external system or credential store that is unavailable.

---

## 9. Validation

Before returning output, confirm each item below. In debug mode, every confirmation must be evidenced in `PlanningTrace.ValidationResults` (see SKILL.md) — citing control names, step numbers, and counts from the generated plan, never a bare "passed". A plan that contradicts its own validation evidence must be fixed before returning.

Checklist:
- Every referenced control or action satisfies the grounding rule (section 3).
- The Advancement and Completion decision table (section 4) was applied — same-screen completion chosen when possible, screen-change steps suppressed when downstream controls were present, stop honored after advancement-only sequences.
- The pre-match selector context check ran for every read target (`application-matching.md` §2).
- No `TodoStep` exists without all six gating conditions met.
- `GoalCompleted` reflects completion on the current grounded surface only.
- `CompletedStepSummaries` has one past-tense entry per step, correctly prefixed.
- `AutomationCategory` matches the steps generated (definitions and ask logic: `output-contract.md`).
- No output text field references future screens, post-transition controls, or speculative UI.
