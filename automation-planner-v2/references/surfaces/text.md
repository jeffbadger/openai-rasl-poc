# Text Application Surface

Rules specific to terminal/text-based application automation (3270, 5250, VT, and similar). Load alongside `references/application-matching.md` (and `references/application-steps.md` for multi-screen or tabbed goals) when surface type is `Text`.

---

## Hierarchy Interpretation

Text application hierarchies represent terminal screen layouts: fields, regions, function key mappings, and screen identifiers.
Use field names, screen IDs, and region labels from the hierarchy as the primary grounding source.
Position-based references (row/column) are valid grounding when named fields are not available, but prefer named fields when both are present.

---

## Authentication Surface Identification

Treat a text surface as an authentication surface when the screen contains credential entry fields (user ID, password, or equivalent terminal labels) combined with a sign-on or login screen identifier.
Common signals: screen title or header containing "Sign On", "Login", "Logon", or "Security"; fields labeled "User ID", "Password", "Userid", "Passcode".

Authentication surface rules are defined in `references/planning-auth.md` — load it whenever the surface contains credential fields or auth controls, regardless of the goal.

---

## Screen Change Types

In text automation, screen changes include:
- A new screen ID appearing after a function key press or Enter submission
- A menu selection that transitions to a new screen
- An option entry that navigates to a sub-screen
- A program function (PF) key that opens a different panel

If the next required fields are not grounded on the current screen after a navigation action, stop after the advancement step.

---

## Field Interaction Patterns

### Input fields
Set value using `ApplicationValueStep` with `Action: "SetValue"`.
Read value using `ApplicationValueStep` with `Action: "GetValue"`.
Clear a field before setting a new value when the field may contain residual data from a prior entry.

### Function keys and Enter
Submit screens or trigger actions using `ApplicationMethodStep` targeting the appropriate function key (PF1–PF24) or Enter.
Map function key semantics from the screen's field labels, help text, or legend region when present in the hierarchy.
Do not assume function key assignments — use only assignments grounded in the current screen hierarchy.

### Menu selection
Enter a menu option using `ApplicationValueStep` to set the option field, followed by `ApplicationMethodStep` to submit (Enter or appropriate PF key).
Treat menu navigation as an advancement sequence — stop after submission when the resulting screen's controls are not yet grounded.

### Option lists
When a screen presents a numbered or lettered option list, target the option entry field and set the appropriate value, then submit.

---

## Screen State and Transitions

Text screens are atomic — the entire screen is replaced on each transition.
Do not carry forward field references from a prior screen after a navigation action.
Each turn operates on the controls grounded in the current screen hierarchy only.

### Partial and transitional states
Some text screens show intermediate states (processing indicators, "Please wait" messages, partially populated data).
Do not generate field-read steps when the screen is in a transitional state.
If the hierarchy indicates a transitional state, emit a wait or re-interrogate advancement step rather than attempting to read data fields.

---

## Protocol-Specific Notes

### 3270
Field attributes (protected, unprotected, numeric) determine which fields accept input.
Only generate set-value steps for unprotected fields grounded in the hierarchy.

### 5250
SBA (Set Buffer Address) fields define the input regions.
Use grounded field names and positions from the hierarchy — do not calculate SBA positions manually.

### VT
Line-mode and character-mode behave differently.
When the hierarchy does not distinguish, treat all visible input regions as potential targets and rely on semantic matching.
