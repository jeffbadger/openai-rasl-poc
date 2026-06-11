# Application Matching

Hierarchy authority, semantic control matching, control state validation, and identity binding. Load whenever application steps are needed — always paired with the appropriate surface file.

For step generation rules, screen change handling, and tabbed interface rules — load `references/application-steps.md` when those areas are needed.

---

## Table of Contents
1. [Hierarchy Authority](#1-hierarchy-authority)
2. [Semantic Control Matching](#2-semantic-control-matching)
2a. [Control State Validation](#2a-control-state-validation)
3. [Control Identity Binding](#3-control-identity-binding)

---

## 1. Hierarchy Authority

The App UI Element hierarchy is the primary authoritative source for grounded control planning.

**A control is grounded when it is present in the App UI Element hierarchy** — even if the interrogated control hierarchy is incomplete or sparse.

- Use the interrogated control hierarchy only as supplemental metadata (interrogation awareness, availability assessment, downstream identity support).
- Do not discard a clearly grounded control from the App UI Element hierarchy because the interrogated hierarchy lacks a matching detailed node.
- When the two hierarchies differ, prefer the App UI Element hierarchy for deciding what steps can be planned.

### Sparse interrogated hierarchy
A sparse, partial, or incomplete interrogated hierarchy does not invalidate controls grounded in the App UI Element hierarchy.

- Do not reject a control step solely because the interrogated hierarchy contains only factories, parent containers, or lacks a matching detailed node.
- Use missing interrogated detail only to limit supplemental metadata fields — not to block grounded control planning.

### No invented controls (MUST)
- Do not invent control names, IDs, element references, automation IDs, user action IDs, or application IDs.
- Do not rename a grounded control unless the contract explicitly requires a friendly suggested name field in addition to the grounded identity.
- Do not generate an application step unless its target control is grounded in the supplied hierarchy.
- `ElementReferenceId` must come from a grounded control node — never invented.

---

## 2. Semantic Control Matching

Before declaring that no grounded control exists for a goal-driven action, perform semantic matching against the App UI Element hierarchy. Declaring "no grounded control" without completing semantic matching first is prohibited.

### Pre-match selector context check (MUST — run before matching any read target)

Before running semantic matching on any field the goal asks to read, inspect that field's container in the hierarchy for selector controls (radio buttons, tabs, checkboxes, dropdowns). This check is mandatory — do not skip it because a field name appears to match the goal directly.

**Check procedure:**
1. Identify the candidate field's parent container (panel, group, tab, region).
2. Scan that container and its siblings for selector controls — `RadioButton`, `TabItem`, `CheckBox`, `ComboBox`, or equivalent.
3. If selector controls are present, check their state metadata (`Checked`, `IsSelected`, `Selected`, `Enabled`).
4. If selector controls are **disabled** (`Enabled: false`) — apply the same reasoning as control state validation before deciding whether to ask:
   - If a search, populate, or data-load trigger is a planned preceding step in this turn, treat the selector controls as "will be enabled after that trigger." Identify the correct selector value from the goal qualifier and plan the selector step after the trigger — do not ask.
   - If no preceding trigger explains the disabled state, apply Case 4 from section 2a and ask before proceeding.
5. If selector controls are enabled and **all are unselected** (`Checked: false` / `IsSelected: false`) — a selector step is required before the read. Identify which selector value corresponds to the goal's qualifier (account type, category, mode) and emit that selector step first.
6. If the correct selector is already active (one control shows `Checked: true` or `IsSelected: true` matching the goal qualifier) — proceed to matching without emitting a selector step.
7. If selector controls are present but state metadata is absent — treat as unselected and emit the selector step.

**A strong field name match does not override this check.** Emitting a read step or `TodoStep` without completing this check when selector controls are present is a planning error.

Apply matching in this priority order:

### Priority 1 — Purpose labels from hierarchy metadata
Check `AccessibilityName`, `Text`, `Name` (or surface-equivalent metadata fields).
A control whose metadata label matches the goal's intent — even when different terminology is used — is a valid candidate.

### Priority 2 — ControlType appropriateness
The candidate control's type must be semantically valid for the intended action:
- Read/display target → `TextBox`, `Label`, `EditableText`, or equivalent
- Trigger action → `Button`, `MenuItem`, or equivalent
- Selection → `ComboBox`, `RadioButton`, `CheckBox`, or equivalent

Deprioritize a control whose ControlType is a poor fit for the intended action, regardless of name proximity.

### Priority 3 — Container context
Narrow the search using parent group, panel, tab, section, or region whose name aligns with a qualifier in the goal.

Example: A goal qualifier of "shipping address" narrows the search to controls within a container whose label matches that qualifier.

### Priority 4 — Synonym reasoning
Treat linguistically equivalent or synonymous terms as matching. Apply semantic equivalence — do not require exact string matches. If the goal and control metadata convey the same real-world concept, the match is valid.

### Confirmation after Priority 4 fallthrough (MUST)

When Priority 4 was the only route to a candidate — Priorities 1, 2, and 3 produced no viable match — the synonym match is uncertain enough to warrant confirmation before binding identity.

Call `ask_user` with the proposed match:
> "I matched [goal action] to [ControlName] using synonym reasoning ([goal term] → [control label]). Is that the right control? Yes / No — it's [user describes the correct control] / No — skip this step"

When two candidates score equally at any priority level and cannot be distinguished — same ControlType, similar labels, neither clearly superior — also ask:
> "I found two possible controls for [goal action]: [Candidate A label] and [Candidate B label]. Which should I use? [Candidate A] / [Candidate B] / Neither — [user describes]"

**Applying the answer:**
- **Yes / Candidate selected** → bind identity using the same-node rule from section 3. The user's confirmation does not bypass node resolution — `ControlName` and `ElementReferenceId` must still come from the matched hierarchy node.
- **User provides a hint** → use the hint as a new search key. Re-run Priorities 1–4 against the hierarchy using the hint's terminology. Resolve identity from the newly matched node. Do not use the hint text directly as a `ControlName`.
- **Skip** → omit this step. Do not emit a `TodoStep` without the separate TodoStep confirmation in `references/planning-core.md` §7.

Ask at most once per control per turn. If re-running with the user's hint still finds no clear match, proceed to the TodoStep confirmation flow.

### Semantic matching scope
Dependency proof before assuming scope: Do not treat an entire form or result set as dependent on a selector merely because one field is qualified. A field is selector-dependent only when the hierarchy grounds that dependency through selector-controlled region membership, nearby variant-specific labels, or explicit semantic coupling.

---

## 2a. Control State Validation

The hierarchy is a snapshot taken when the screen first appeared. Control state (enabled, disabled, visible, populated) reflects that moment — not the state after planned interactions execute. Actions taken on the current screen may change the state of other controls before they are used.

**The hierarchy snapshot is valid as-is only when:**
- The planned steps do not interact with any control on the current screen before the target control is reached (i.e., no in-place state changes occur)
- A screen transition is the result of the planned action (the next screen's controls are a new hierarchy)

**When a disabled control is encountered during planning:**
Apply the following decision in order — stop at the first match.

### Case 1 — Confident inference: selector prerequisite (proceed without asking)
The control is disabled because a selector on the current screen (tab, radio button, checkbox, dropdown) has not yet been set to the correct value, AND the hierarchy grounds that dependency structurally (the control is within a selector-controlled region).

→ Emit the selector step before the disabled control. Plan proceeds. No question needed.

### Case 2 — Confident inference: standard form Submit (proceed without asking)
The control is a Submit, Save, OK, or equivalent action button that is disabled because required input fields have not yet been filled, AND the goal includes filling those fields as preceding steps.

→ Plan the field-fill steps first. The Submit step follows. Plan proceeds. No question needed.

### Case 3 — Confident inference: in-place search result controls (ask before post-search steps)
Result controls (grids, lists, detail fields, action buttons operating on results) are present in the hierarchy but disabled or empty. A search, find, or lookup trigger is a planned step in this turn. The structural presence of result controls alongside a search trigger is strong evidence of an in-place search pattern — results populate on the same screen rather than navigating away.

→ Plan the search trigger step. Before planning any post-search steps, call `ask_user` to confirm the result controls will be enabled and populated after the search. See state validation question format below.
→ If confirmed: plan the post-search steps in the same turn. `GoalCompleted` may be `true` if all required work completes.
→ If not confirmed or redirected: stop after the search trigger. `GoalCompleted = false`.

### Case 4 — Not confident: all other disabled controls (ask before using or discarding)
The control is disabled and none of Cases 1–3 apply. The cause is unknown — could be server-side, permission-based, data-dependent, or triggered by a preceding action whose effect on this control cannot be confidently inferred.

**MUST ask before either using or discarding the control (MUST):**
- Do not plan a step targeting the control without asking.
- Do not substitute an alternative control without asking.
- Do not emit a `TodoStep` for the work without asking.

→ Call `ask_user` with a state validation question. See format below.
→ Apply the user's answer as a planning assumption for this turn only.
→ Record the assumption in `PlanningTrace` when debug mode is enabled.

### State validation question format

Questions must be concrete and answerable. Present the situation, the specific control, and what the planner needs to know. Offer yes/no or named options — do not ask open-ended questions.

**In-place search confirmation (Case 3):**
> "After the search executes, will [control name / result area] become enabled and populated on this screen? Yes / No — navigates away instead"

**Unknown disabled state (Case 4):**
> "The [control name] control is currently disabled. Will it become enabled after [the preceding steps / some other condition]? Yes — it enables after [preceding steps] / No — it requires [describe condition] / No — use [alternative] instead"

**Multiple disabled controls of the same type:**
Group related controls into a single question rather than asking separately for each.

### User answer handling (MUST)
- **Yes / confirmed** → plan assuming the enabled state. Note assumption in `PlanningTrace`.
- **No / navigates away** → stop after the trigger step. `GoalCompleted = false`. Do not plan post-trigger steps.
- **Steering answer** (user describes an alternative or correction) → incorporate the correction into the plan for this turn. Do not carry the correction forward to future turns or write it to durable memory.
- **Never** ask the same state validation question twice in the same turn.

---

## 3. Control Identity Binding

### Same-node rule (MUST)
`ControlName` and `ElementReferenceId` must come from the same resolved hierarchy node.

The correct sequence:
1. Use semantic matching to identify the target node.
2. From that node, read `ControlName` from the node's own name/identifier field.
3. From the same node, read `ElementReferenceId` from the node's own ID field.

**Prohibited:**
- Deciding `ControlName` before the node is found (e.g., from the goal description), then searching by that name.
- Obtaining `ElementReferenceId` from any node other than the one identified in step 1.
- Using positional or structural proximity (sibling, parent, adjacent row) to select either field.
- Carrying forward identity values from a prior reasoning step that do not correspond to the resolved node.

Perform this procedure explicitly for every application step. Do not skip it for steps that appear obvious.

### Node resolution trace (MUST)
Append a short trace to the end of `StepDescription` for every application step:

`[Node: <ControlName> | ID: <ElementReferenceId> | Match: <how found>]`

`<how found>` must be one of:
- `AccessibilityName:<value>`
- `Text:<value>`
- `Name:<value>`
- `ControlType:<type> under <container>`
- `Synonym:<goal term> → <node label>`

Example: `[Node: txtUserName | ID: 42 | Match: AccessibilityName:Username]`

### Preserve downstream identity
Keep all application and control identity fields required by the output contract.
Preserve: application name, application ID, application type, control name, element reference, suggested naming, and related identity fields.
Use only identity values grounded in the supplied hierarchy.
