# Planning — Callable & Orchestration

Rules specific to Callable and Orchestration automations. Load this file when:
- Task prefix is `Callable:` or `Orchestration:`
- The goal requires a Callable/Orchestration split that has not been externally requested

For core planning rules that apply to all turns, see `references/planning-core.md`.

---

## Table of Contents
1. [Callable Boundary Confirmation](#1-callable-boundary-confirmation)
2. [Surface Isolation](#2-surface-isolation)
3. [Callable GoalCompleted Gate](#3-callable-goalcompleted-gate)
4. [Post-First-Core Constraint](#4-post-first-core-constraint)
5. [Callable Lifecycle](#5-callable-lifecycle)

---

## 1. Callable Boundary Confirmation

When the planner determines that a goal requires a Callable/Orchestration split, it must confirm that decomposition before committing to it — unless the split was explicitly requested externally.

### When to ask (MUST)
Ask when **all** of the following are true:
- No task prefix (`Callable:`, `Orchestration:`) was provided in the request
- The planner has determined the goal requires multiple reusable automations or a loop over callables
- The planner has formed a specific proposed decomposition

Do not ask when:
- A task prefix was provided — the boundary is externally decided
- The goal is clearly a single automation with no repetition or reuse implied
- `get_callable_signatures()` returned an existing callable that already covers a required sub-task — use it directly

### Question format
Present the proposed decomposition concretely. Yes/no/redirect — not open-ended.

> "This goal looks like it needs more than one automation. I'm planning to structure it as:
> - Orchestrator: [describe what it does]
> - Callable A — [name]: [describe what it does, what it accepts, what it returns]
> - Callable B — [name]: [describe what it does, what it accepts, what it returns]
>
> Is that the right decomposition? Yes / No — make it a single automation / No — different split: [user describes]"

### Applying the answer
- **Yes** → proceed with the proposed decomposition. Plan the orchestrator or the first callable per the task prefix that will be assigned.
- **No — single automation** → plan as a single automation. Do not use Callable/Orchestration structure.
- **Redirect** → adopt the user's decomposition. Confirm understanding by summarising the revised structure before generating steps, if the redirect is complex.

Do not generate steps until the decomposition is confirmed when this rule applies.

---

## 2. Surface Isolation (MUST)

A Callable automation is scoped to a single application surface. Every step must interact only with the hierarchy provided in the current turn.

**Prohibited in callable step planning:**
- Invoking a durable memory automation from a different application domain.
- Emitting a `TodoStep` as a substitute for a blocked cross-surface lookup — plan to accept and enter the value directly.
- Planning steps that span more than one application type within the same callable turn.

### Durable memory on first callable turn (MUST)
On the first turn of a callable automation, emit `callableAutomation` to `DurableMemoryWrites` with the `AutomationName` as its value. If durable memory already contains a `callableAutomation` entry for this automation name, do not emit a duplicate.

---

## 3. Callable GoalCompleted Gate

Run this before setting `GoalCompleted` on any `Callable:` task.

1. Count lines in `# Completed Steps` beginning with exactly `core:`. Call this **N**.

2. **N = 0 AND core logic completes this turn → Path A:**
   Set `GoalCompleted = true`, `AutomationCategory = "core"`.
   Do not append a reset sentinel. Do not plan reset steps. Stop.

3. **N > 0 AND core logic completes this turn → Path B/C:**
   Append `"core: Callable core complete — reset required for <AutomationName>"` to `CompletedStepSummaries`.
   Set `GoalCompleted = false`.
   The reset turn(s) follow — see section 5 reset turn procedure for whether reset is single-step (Path B) or multi-step (Path C).

4. **Core logic does not complete this turn → continue:**
   Set `GoalCompleted = false`.
   Do not append any sentinel.

5. **Reset in progress (`"Reset in progress for"` sentinel present) AND landing screen reached this turn:**
   Set `GoalCompleted = true`, `AutomationCategory = "core-reset"`.
   Do not append any sentinel.

6. **Reset in progress AND landing screen not yet reached:**
   Set `GoalCompleted = false`, `AutomationCategory = "core-reset"`.
   Append `"core-reset: Reset in progress for <AutomationName>"`.

**Prohibited — Path A when prior core turns exist:** If `# Completed Steps` already contains any `core:` entry, do not set `GoalCompleted = true` even if core logic completes. Use Path B/C unconditionally.

---

## 4. Post-First-Core Constraint (MUST)

Each callable automation has its own independent lifecycle. Within a single callable's lifecycle:

- Navigation turns are valid **before** the first core turn only.
- Once any turn in this callable's lifecycle has been classified as `"core"` (confirmed by a `core:` entry in `# Completed Steps`), all subsequent turns for this callable must be classified as `"core"` or `"core-reset"` only.
- `"navigation"` is not a valid classification after the first core turn. If steps that would previously have been navigation appear after a core turn, re-examine whether they are actually core steps that were not recognized as such — or whether the callable lifecycle has an unexpected structure that warrants asking the user.

**Each callable resets independently.** When an orchestrator calls Callable A and then Callable B, Callable B starts its own fresh lifecycle — navigation turns are permitted at the start of Callable B regardless of Callable A's lifecycle state.

**What the orchestrator owns vs. the callable:** Session-level setup (login, application launch) belongs to the orchestrator. Any navigation the callable itself must perform after invocation is the callable's responsibility and counts toward that callable's own lifecycle.

---

## 5. Callable Lifecycle

### Reset sentinel types

Two sentinel entries are used in `CompletedStepSummaries` to signal reset state:

| Sentinel | Meaning |
|---|---|
| `"core: Callable core complete — reset required for <AutomationName>"` | Core work is done. Reset has not yet started. |
| `"core-reset: Reset in progress for <AutomationName>"` | Reset has started but is not yet complete. |

### Reset turn recognition

**First reset turn** — recognised when `CompletedStepSummaries` contains `"Callable core complete — reset required for"` but does not contain `"Reset in progress for"`.

**Continuation reset turn** — recognised when `CompletedStepSummaries` contains `"Reset in progress for <AutomationName>"`.

**Not a reset turn** — neither sentinel is present.

### Reset target

Determine the landing screen before generating any reset steps:

1. Find the last `navigation:` entry in `# Completed Steps` — the final navigation action before the first `core:` entry.
2. The screen arrived at as a result of that navigation is the callable's landing screen and the reset target.
3. All reset steps work toward returning to this screen and no further.

### First reset turn procedure

On the first reset turn:

**Step 1 — Identify dismiss options.**
Examine the current hierarchy for all viable dismiss controls: Cancel, Close, Back, X buttons, Escape equivalents, and any other controls whose label or type suggests navigation away from the current state.

**Step 2 — Single dismiss option: proceed without asking.**
If exactly one viable dismiss control is present, emit it without confirmation. Proceed to Step 4.

**Step 3 — Multiple dismiss options: ask before choosing (MUST).**
When two or more viable dismiss controls are present, call `ask_user` before emitting any step:

> "I need to return to [landing screen description]. These dismiss options are available:
> - [Control A label]
> - [Control B label]
> - [Control C label — if present]
> - None of these — [user describes]
>
> Which one returns there cleanly? And will it complete the return in one step, or will it require additional steps (for example a confirmation dialog or an intermediate screen)?"

Apply the user's answer:
- **Single-step confirmed** → emit the chosen dismiss step. Proceed to Step 4.
- **Multi-step indicated** → emit the chosen dismiss step. Append `"core-reset: Reset in progress for <AutomationName>"` to `CompletedStepSummaries`. Set `GoalCompleted = false`. Stop — the next reset turn continues from where this left off.
- **None of these** → incorporate the user's described approach. Apply single-step or multi-step logic based on whether the user indicated completion in one step.

**Step 4 — Set completion.**
If the reset completes this turn (landing screen reached, no further steps needed):
- Set `GoalCompleted = true`, `AutomationCategory = "core-reset"`.
- Do not append any sentinel.

**No business logic on reset turns:** Reset steps only undo what the callable introduced. Do not re-execute business logic.

**No-reset when core completes on first core turn (Path A):** When `# Completed Steps` contains no `core:` entries and core logic completes this turn, set `GoalCompleted = true` directly. Do not signal a reset turn. Do not plan reset steps.

### Continuation reset turn procedure

On a continuation reset turn (`"Reset in progress for"` sentinel is present):

1. Examine the current hierarchy — this is the screen reached by the prior reset step.
2. Determine whether the landing screen has been reached.
   - **Landing screen reached** → set `GoalCompleted = true`, `AutomationCategory = "core-reset"`. No sentinel. Done.
   - **Not yet reached** → generate the next minimal steps toward the landing screen. If multiple dismiss options are present, ask again per Step 3 above. Append `"core-reset: Reset in progress for <AutomationName>"`. Set `GoalCompleted = false`.
3. Never navigate past the landing screen. Stop exactly when it is reached.

### Reset overshoot prohibition
When multiple navigation options exist during reset, always choose the one that returns exactly to the landing screen. Options that skip past the landing screen are prohibited. When in doubt, ask.

### Callable turn sequence

**Path A — first core turn completes the goal (no reset needed):**
1. Navigation turn(s): `GoalCompleted = false`, `AutomationCategory = "navigation"`. Emit `callableAutomation` to durable memory on first turn.
2. Core turn: `GoalCompleted = true`, `AutomationCategory = "core"`. Core steps only.

**Path B — standard multi-turn, single-step reset:**
1. Navigation turn(s): `GoalCompleted = false`, `AutomationCategory = "navigation"`. Emit `callableAutomation` to durable memory on first turn.
2. Core turn(s): `GoalCompleted = false`, `AutomationCategory = "core"`. Append `"Callable core complete — reset required for <AutomationName>"` on final core turn.
3. Reset turn: ask if multiple dismiss options present. Single step confirmed. `GoalCompleted = true`, `AutomationCategory = "core-reset"`.

**Path C — standard multi-turn, multi-step reset:**
1. Navigation turn(s): `GoalCompleted = false`, `AutomationCategory = "navigation"`. Emit `callableAutomation` to durable memory on first turn.
2. Core turn(s): `GoalCompleted = false`, `AutomationCategory = "core"`. Append `"Callable core complete — reset required for <AutomationName>"` on final core turn.
3. First reset turn: ask if multiple dismiss options present. Multi-step indicated. Emit first dismiss step. Append `"Reset in progress for <AutomationName>"`. `GoalCompleted = false`, `AutomationCategory = "core-reset"`.
4. Continuation reset turn(s): continue toward landing screen. `GoalCompleted = false`, `AutomationCategory = "core-reset"` until landing screen reached.
5. Final reset turn: landing screen reached. `GoalCompleted = true`, `AutomationCategory = "core-reset"`. No sentinel.
