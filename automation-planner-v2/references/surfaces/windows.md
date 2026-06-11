# Windows Application Surface

Rules specific to Windows UI automation. Load alongside `references/application-matching.md` (and `references/application-steps.md` for multi-screen or tabbed goals) when surface type is `Windows`.

---

## Hierarchy Interpretation

Use the supplied Windows hierarchy as the source of valid controls and state.
Prefer controls that are actively present in the current interrogated surface.
Do not assume controls on unopened forms, hidden dialogs, or inactive tabs are available unless the hierarchy grounds them as available now.

---

## Authentication Surface Identification

Treat a Windows surface as an authentication surface when grounded metadata indicates credential-entry or sign-in behavior.
Use control names, labels, accessibility names, button text, and related metadata together to infer authentication purpose.

Authentication surface rules are defined in `references/planning-auth.md` — load it whenever the surface contains credential fields or auth controls, regardless of the goal.

---

## Screen Change Types

In Windows automation, screen changes include:
- Opening a new form or child window
- Opening or closing a dialog
- Switching tab pages
- Changing MDI child state
- Expanding or collapsing a container that reveals a new interaction surface

If the next required controls are not grounded after such a change, stop after the advancement step.

---

## Same-Screen Updates

Treat in-place refreshes, searches, grid reloads, text updates, and panel updates as same-screen work when the result controls are already grounded.
Complete read/validation steps now rather than waiting for another prompt.

---

## Inferred Prerequisites

Infer gating controls such as tabs, radio buttons, checkboxes, grouped panels, modal confirmations, and search buttons.
Insert prerequisite actions immediately before the dependent action.

---

## Control Interaction Patterns

### Text input fields
Set value using `ApplicationValueStep` with `Action: "SetValue"`.
Read value using `ApplicationValueStep` with `Action: "GetValue"`.

### Buttons
Invoke using `ApplicationMethodStep` with the appropriate click or invoke method.
Buttons are submit/navigation controls — never credential entry fields regardless of label.

### Dropdowns and ComboBoxes
Select item using `ApplicationMethodStep`.
Treat as scope-setting controls when the selected value changes what downstream fields represent.

### Radio buttons
Select using `ApplicationMethodStep`.
Treat as scope-setting controls — always emit radio selection before targeting dependent fields.
Apply the pre-match selector context check from `references/application-matching.md` §2.

### Checkboxes
Set state using `ApplicationMethodStep`.
When a checkbox enables or disables a section, emit the checkbox step before targeting any field in that section.

### Grids and lists
Read cell values using `ApplicationValueStep` targeting the specific cell control.
When iterating grid rows, use cursor methods (`MoveFirst`, `MoveNext`) as supporting steps before each row's read steps.

---

## Modal Dialogs

When a modal dialog is present in the hierarchy, it takes precedence over the parent window for step targeting.
Do not generate steps targeting parent window controls while a modal dialog is active.
Dismiss or complete the dialog before returning to parent window interaction.
