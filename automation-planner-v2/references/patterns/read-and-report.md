# Pattern: Read and Report

A read-and-report automation navigates to a record or view, reads specific field values, and returns them as named output parameters to the caller. It is purely extractive — no data is entered or modified.

This pattern is implicitly covered by search-and-read and form-fill when navigation is involved, but read-and-report has distinct characteristics that make it worth planning explicitly: the primary purpose is value extraction, output parameters are the delivery mechanism, and AutomationCategory is always `"core"` as soon as the first value is captured.

---

## When This Pattern Applies

- Goal is to read and return specific field values from a record, screen, or view
- No data entry or state change is required
- Values are returned as output parameters to the caller (`Run.` parameter names)
- The automation may need to navigate to a specific view, tab, or section before fields are readable
- Typically used as a Callable invoked by an orchestrator that supplies the record identifier

---

## Step Shape

```
1. [Navigate to record if not already there]                 — navigation turn(s)
── record or view is now on screen ──
2. [Set scope selectors — tab, radio, panel — if needed]    — supporting
3. Read field value → map directly to Run.outputParam1      — primary
4. Read field value → map directly to Run.outputParam2      — primary
   ... (one ApplicationValueStep per field)
5. Set Run.Result to true                                    — supporting
6. LabelStep: Error
7. Set Run.Result to false                                   — supporting
8. Set Run.errMessage                                        — supporting
```

Navigation turns follow standard advancement rules. The core turn begins when the first field is read.

---

## Planning Guidance

### Output parameter mapping (MUST)
In a read-and-report callable, every field read must map directly to a named `Run.` output parameter. Do not store to a local variable and transfer — read directly into the output parameter.

**Preferred:**
```json
{
  "StepType": "ApplicationValueStep",
  "Action": "GetValue",
  "ControlName": "lblAccountBalance",
  "SetValueControl": "Run.accountBalance",
  "SetValueProperty": "Value"
}
```

**Use a local variable only when:**
- The same field value feeds multiple output parameters
- A transformation is required before the value is output (string formatting, date conversion)
- A conditional read determines which output parameter receives the value

When a local variable is used, follow it immediately with a `ValueStep` transferring to the `Run.` parameter. Do not leave values in local variables at the end of the turn.

### Output parameter names
Derive output parameter names from the goal description or the durable memory callable signature when available. Use camelCase. Match the field's business meaning — not the control name.

- `Run.accountBalance` not `Run.lblAccountBalance`
- `Run.customerStatus` not `Run.txtStatus`
- `Run.lastTransactionDate` not `Run.dtpDate`

### Field ordering
Read fields in the order they appear in the hierarchy, top to bottom, left to right — unless the goal specifies a different order or selector prerequisites require a different sequence.

### Selector prerequisites
Apply the selector prerequisite rules from `references/application-steps.md` §6 and the pre-match check from `references/application-matching.md` §2:
- Activate the correct tab before reading fields within it
- Set radio buttons, dropdowns, or checkboxes before reading selector-dependent fields
- Read selector-independent fields first

A read-and-report automation commonly reads from multiple tabs or sections. Group reads by tab — activate, read all fields in that tab, move to the next.

### Disabled or empty fields
Apply section 2a state validation from `references/application-matching.md`:
- If a target field is empty but expected to contain a value → the record may not be in the expected state. Call `ask_user` before planning the read step.
- If a target field is grounded and populated → read it directly.
- Do not infer a value from adjacent controls or prior context.

### GoalCompleted
- Navigation turns → `GoalCompleted = false`
- Core turn: all required fields read and mapped to output parameters → `GoalCompleted = true`
- Core turn: required fields not all readable this turn → `GoalCompleted = false`

For callable automations, apply the Callable GoalCompleted gate from `references/planning-callable.md` §3.

### No reset required (typical)
Read-and-report automations rarely require reset — reading a field does not change application state. If the automation navigated to reach the record, the navigation is the orchestrator's responsibility to reverse.

**Exception:** If reading the values required opening a dialog, expanding a panel, or changing a selector state that should be restored — apply reset rules from `references/planning-callable.md` §5.

### Result and error parameters
Always emit:
- `Run.Result = true` at the end of a successful read sequence (supporting step)
- A `LabelStep: Error` with `Run.Result = false` and `Run.errMessage` reachable via `JumpToLabelStep` from any step that detects a failure condition

---

## AutomationCategory

| Turn | Classification |
|---|---|
| Navigation to record | `"navigation"` |
| First turn reading a field value | `"core"` |
| Subsequent read turns (if multi-turn) | `"core"` |

---

## Common Variations

### Read after search
When the record must be found before it can be read, prefix with the search-and-select pattern. The search-and-select callable locates and opens the record; the read-and-report callable reads its fields. Plan as two separate callables in an orchestration, or as a single automation when the search and read occur in the same callable's lifecycle.

### Conditional reads
When the set of fields to read depends on a value already on screen (e.g., read different fields for different account types), emit a decision step first, then read the appropriate fields in each branch. Each branch maps its values to the correct `Run.` output parameters.

### Read from multiple screens
When required fields are spread across multiple screens (detail view + history tab + notes section), each screen is a separate turn. The callable navigates to each screen in sequence, reading fields on each. All turns after the first read are `"core"`. Apply the post-first-core constraint from `references/planning-callable.md` §4.

### Read with transformation
When a read value requires transformation before output (e.g., parsing a date string, extracting a substring, converting a currency format), emit a `MethodStep` from `references/toolbox/string.md` or `references/toolbox/datetime.md` after the read step, then map the transformed result to the `Run.` output parameter.
