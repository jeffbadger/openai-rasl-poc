# Pattern: Wizard

A wizard automation completes a multi-screen sequential data entry process where each screen must be completed before advancing to the next. The wizard concludes on a confirmation or summary screen where a final submit action completes the process.

---

## When This Pattern Applies

- Goal involves completing a structured multi-step process spread across sequential screens
- The current screen shows two or more of these signals:
  - Step indicator: "Step N of M", numbered breadcrumb, or progress indicator showing position
  - Next button (or equivalent: Continue, Proceed, Forward)
  - Back button (or equivalent: Previous)
  - Finish or Submit button on what appears to be the final or confirmation screen
  - Cancel button that aborts the entire wizard process
- Navigation between screens is enforced — the application does not allow skipping steps

---

## Wizard Detection

The planner infers wizard context from the hierarchy. Wizard context is confirmed when **two or more** of these signals are present in the current screen's controls:

| Signal | Examples |
|---|---|
| Step indicator | "Step 2 of 4", "2 / 4", breadcrumb with numbered or named steps |
| Next control | Button labelled Next, Continue, Proceed, Forward |
| Back control | Button labelled Back, Previous |
| Finish control | Button labelled Finish, Submit, Complete (on final/summary screen) |
| Cancel control | Button labelled Cancel that aborts the wizard |

A single signal (e.g., a Next button alone) is insufficient — standard forms also have Next buttons. Two or more signals together constitute wizard context.

---

## Turn Classification in a Wizard

### Intermediate screens (Next is the primary action)
- `AutomationCategory: "navigation"`
- `GoalCompleted = false`
- Generate all data entry steps for this screen
- Final step: click Next to advance

### Confirmation or summary screen (Finish/Submit is the primary action)
- `AutomationCategory: "core"`
- `GoalCompleted = true` after the submit step
- No reset required — Path A applies (see Callable lifecycle)
- Optionally read a confirmation reference number or success message if grounded

### Back navigation (planner redirected to a prior screen)
- `AutomationCategory: "navigation"`
- `GoalCompleted = false`
- The host will re-present the prior screen — plan its steps from that screen's hierarchy

---

## Step Shape

**Per intermediate screen:**
```
1. [Verify wizard signals are present — confirm wizard context]
2. [Set scope selectors if screen has tabs, radio groups, or panels]  — supporting
3. Enter field values for this screen                                 — primary (one per field)
4. Click Next / Continue                                              — primary
```

**Confirmation/summary screen:**
```
1. [Verify Finish/Submit control is present]
2. [Read summary values to confirm entries if needed]                 — supporting
3. Click Finish / Submit                                              — primary
4. [Read confirmation reference or success message if grounded]      — primary
```

---

## Planning Guidance

### Data entry steps
Apply all standard form-fill rules from `references/patterns/form-fill.md` within each wizard screen. Tab selection, radio buttons, conditional fields, and prerequisite enabling steps all apply per screen.

### Input parameter handling
Wizard fields are typically populated from input parameters supplied by the caller. Treat unspecified values as runtime input parameters (`StaticValue: null`) — do not emit `TodoStep` for missing values.

### Optional screens
Optional wizard screens appear when prior selections trigger additional data collection. The host presents whatever screen the application shows — the planner does not need to predict optional screens in advance. When an unexpected screen appears within wizard context (signals are present):
1. Recognise it as a wizard screen from the signals
2. Plan its fields normally
3. Emit the Next step to advance
4. `GoalCompleted = false`

No special handling is needed — the host-driven discovery loop handles optional screens naturally.

### Confirmation screen recognition
Recognise the confirmation screen when:
- A Finish or Submit button is present (rather than Next)
- A summary or review section is present showing previously entered values
- The step indicator shows the final step (e.g., "Step 4 of 4")

Any one of these signals, in the context of an already-established wizard, is sufficient to classify the screen as the confirmation screen.

### Summary verification (optional)
When the confirmation screen displays a summary of previously entered values, the planner may optionally emit read steps to verify key fields match what was entered in prior turns. This is a supporting step — it does not change `AutomationCategory` or `GoalCompleted`.

Only emit verification reads when:
- The goal explicitly requests confirmation of submitted values
- Key input parameters are high-stakes (financial amounts, identifiers) and a mismatch would be significant

Do not emit verification reads by default — the wizard is trusted to carry values correctly.

### GoalCompleted on confirmation screen
After the Finish/Submit step:
- If a confirmation reference number or success message is grounded and the goal requires capturing it → read it and set `GoalCompleted = true`
- If no read-back is required → `GoalCompleted = true` immediately after the submit step
- Apply the action-only confirmation rule from `references/planning-core.md` §5 when empty confirmation controls are present

### No reset required
Wizard callables use Path A of the callable lifecycle. The wizard represents a complete, bounded transaction — the application's state before the wizard is not something the callable is responsible for restoring. The orchestrator manages session-level state.

Do not append a reset sentinel after the confirmation screen submit. Do not plan reset steps.

### Cancel handling
If the goal requires cancelling a wizard in progress (rare — typically an error recovery scenario), treat the Cancel button as a dismiss action and apply the reset question rules from `references/planning-callable.md` §5 if multiple dismiss options are present.

---

## AutomationCategory Summary

| Screen type | AutomationCategory | GoalCompleted |
|---|---|---|
| Intermediate (Next) | `"navigation"` | `false` |
| Optional intermediate | `"navigation"` | `false` |
| Confirmation/summary (Finish) | `"core"` | `true` after submit |
| Back navigation | `"navigation"` | `false` |

---

## Callable Lifecycle for Wizards

Wizard automations map cleanly to Path A:

```
Turn 1:    First wizard screen    navigation    GoalCompleted=false
Turn 2:    Second wizard screen   navigation    GoalCompleted=false
Turn N-1:  Last data screen       navigation    GoalCompleted=false
Turn N:    Confirmation screen    core          GoalCompleted=true
```

No reset sentinel. No reset turn. `GoalCompleted = true` on the confirmation screen submit.

---

## Common Variations

### Wizard with dynamic field count
Some wizard screens show a variable number of fields depending on prior selections. Plan only the fields grounded in the current hierarchy — do not anticipate fields from prior runs or durable memory unless they are present in the current screen.

### Wizard with save-and-resume
Some wizards allow saving progress and returning later. If the goal involves resuming a saved wizard, the planner treats the current screen as the starting point of the wizard sequence regardless of how many screens were completed in a prior session. `CompletedStepSummaries` from prior turns provides context.

### Wizard embedded in a larger automation
When a wizard is one step in a larger automation (e.g., launch application → navigate to wizard entry point → complete wizard), the navigation to the wizard entry point is handled by the orchestrator or a preceding callable. The wizard callable receives control when the first wizard screen is already displayed.
