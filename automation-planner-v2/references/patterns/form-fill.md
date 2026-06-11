# Pattern: Form Fill

A form-fill automation enters data into one or more fields on a form and submits it.

---

## When This Pattern Applies

- Goal involves entering data into a form, dialog, or data entry screen
- Goal requires submitting or saving after entry
- May follow a search-and-read pattern (navigate to record, then edit)

---

## Step Shape

```
1. [Navigate to form if not already there]                   — navigation
2. [Set scope selectors: tabs, radio buttons, dropdowns]     — supporting (if needed)
3. Enter value into field 1                                  — primary
4. Enter value into field 2                                  — primary
   ... (one step per field)
N. Submit / Save / Confirm                                   — primary
N+1. [Read confirmation or result if grounded]              — primary (if needed)
```

---

## Planning Guidance

### Field ordering
Enter fields in the order they appear in the hierarchy, top to bottom, left to right — unless the goal specifies a different order or dependency relationships require it.

### Scope prerequisites
Apply the pre-match selector context check from `references/application-matching.md` §2.
Emit tab selection, radio button selection, or dropdown selection before targeting any field whose content or meaning depends on that selector.
Read selector-independent fields first.

### Static vs. runtime values
- Known literal values → `StaticValue: "<value>"`
- Values to be supplied by the caller at runtime → `StaticValue: null`
- Values read from a prior step → reference the source control/variable in `GetValueControl`

Do not emit a `TodoStep` for a runtime input value. Treat it as a caller-supplied parameter.

### Submit action
The submit step (Save button, OK button, Enter key, PF key) is always the last step before any result read.
It is a `primary` tier step.
After submission, if a confirmation or result screen is expected and its controls are not yet grounded, stop — set `GoalCompleted = false`.

### Result confirmation
When the goal requires confirming success (reading a confirmation message, a record ID, or a status value), emit read steps only when those controls are grounded in the current hierarchy after submission.
If the form submission causes a screen change, those read steps belong to the next turn.

**Exception — same-screen confirmation:** When a confirmation message or updated field appears in place on the same form (no screen change), complete the read in the same turn.

### GoalCompleted
- After submitting a form with no confirmation read required → `GoalCompleted = true`
- After submitting with confirmation required but result controls not yet grounded → `GoalCompleted = false`
- After reading confirmation → `GoalCompleted = true`

---

## AutomationCategory
- Navigation to the form → `"navigation"`
- Any turn that enters data or submits → `"core"`

---

## Common Variations

### Multi-section form
A single form may have multiple sections controlled by tabs or panels.
Apply the tabbed interface rules from `references/application-steps.md` §6.
Group steps by section — activate section, fill fields, move to next section.

### Conditional fields
Some fields only appear or become enabled based on prior selections.
Infer these dependencies from the hierarchy — do not assume a field is available until its enabling condition is met.
Emit the enabling step first, then the dependent field step.

### Required field validation
If the application validates required fields on submission and returns an error screen, that error screen is a new surface — plan the error handling in the next turn when its controls are grounded.
Do not anticipate validation errors in the planning phase.
