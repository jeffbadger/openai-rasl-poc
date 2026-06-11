# Web Application Surface

Rules specific to web application automation. Load alongside `references/application-matching.md` (and `references/application-steps.md` for multi-screen or tabbed goals) when surface type is `Web`.

---

## Hierarchy Interpretation

Use the supplied web hierarchy as the source of valid controls and state.
Web hierarchies may include DOM-based element trees, accessibility trees, or hybrid representations depending on the host interrogation method.
Prefer controls with stable identifiers (ID, automationId, accessibility role) over positionally-derived references.

---

## Authentication Surface Identification

Treat a web surface as an authentication surface when grounded metadata indicates a login, sign-in, or credential-entry form.
Signals include: form fields labeled username/email/password, submit buttons labeled Login/Sign In/Continue, page titles or headings containing sign-in language.

Authentication surface rules are defined in `references/planning-auth.md` — load it whenever the surface contains credential fields or auth controls, regardless of the goal.

---

## Screen Change Types

In web automation, screen changes include:
- Full page navigation (URL change)
- Single-page application (SPA) route change that replaces the visible content
- Modal or overlay appearing over the current page
- Dynamic panel expansion that reveals new interactive controls
- Tab or accordion section opening that reveals hidden controls

If the next required controls are not grounded after such a change, stop after the advancement step.

### SPA and dynamic content
Treat dynamic content loads (search results appearing, panels expanding, data grids populating) as same-screen work when the result controls are already grounded in the current hierarchy.
Do not treat a dynamic content load as a screen change unless the host hierarchy reflects a genuinely new surface.

---

## Control Interaction Patterns

### Input fields
Set value using `ApplicationValueStep` with `Action: "SetValue"`.
Read value using `ApplicationValueStep` with `Action: "GetValue"`.
For fields with autocomplete or suggestion dropdowns, emit the set-value step then emit a selection step for the desired suggestion if the hierarchy grounds a suggestion list.

### Buttons and links
Invoke using `ApplicationMethodStep`.
Distinguish between buttons that submit forms (advancement or core action) and links that navigate to new pages (advancement). Apply the appropriate `AutomationCategory` classification.

### Dropdowns and select elements
Select item using `ApplicationMethodStep` with the appropriate select method.
Treat as scope-setting controls when selection changes downstream field behavior.

### Checkboxes and radio buttons
Set state using `ApplicationMethodStep`.
Apply the pre-match selector context check from `references/application-matching.md` §2.

### Data grids and tables
Target individual cell controls when grounded in the hierarchy.
When the hierarchy exposes row-level controls only, use cursor or indexing methods to position before reading cell values.

---

## Frame and iFrame Handling

When the hierarchy indicates the target control is within a frame or iFrame, include the frame context in the step identity fields per the output contract requirements.
Do not target controls within a frame as if they were top-level page controls.

---

## Dynamic State

Do not assume a web control is interactive based on its presence alone — some controls are visually present but disabled or hidden via CSS.
When authoritative state metadata (e.g., `IsEnabled`, `aria-disabled`, `IsVisible`) indicates a control is not interactive, treat it as not grounded for the current step and generate appropriate enabling prerequisites instead.
