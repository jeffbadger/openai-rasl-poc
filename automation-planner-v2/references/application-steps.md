# Application Steps

Step generation rules, screen change handling, and tabbed interface rules. Load when:
- The goal involves multi-step application sequences with screen transitions
- The goal requires navigating tabbed interfaces or selector-controlled panels
- Complex same-screen vs. navigation split decisions are needed

For hierarchy authority, semantic matching, and control identity — load `references/application-matching.md` (required with this file).

---

## Table of Contents
4. [Step Generation Rules](#4-step-generation-rules)
5. [Screen Change Handling](#5-screen-change-handling)
6. [Tabbed Interfaces](#6-tabbed-interfaces)

---

## 4. Step Generation Rules

### When to use application steps
Use application-derived steps when the current work targets a control in an application hierarchy.
Preserve application-aware planning whenever a hierarchy-backed application is being automated.
Do not replace grounded application control work with generic placeholders unless the control or required state is genuinely missing.

### Non-application goals
If the goal includes tasks that are not UI interactions, plan those using toolbox steps — not application steps. See `references/planning-core.md` §6.

The presence of an application hierarchy does not force all steps to be application-derived. Choose the step family based on the task:
- UI interaction → application steps
- System, environment, version, file, registry → toolbox/service steps

When both are needed for the primary goal, include both in the same plan when completable on the current surface.

### Control targeting
Prefer the most specific grounded control that satisfies the requested action.
Only generate steps for controls present in the supplied automation surface.
If downstream work depends on controls not yet present, stop after the advancement sequence.

### No placeholder application controls
Do not generate an application step unless its target control is grounded.
Do not invent control names, suggested names, or element references for expected downstream controls not yet present.
If no grounded control exists for the intended downstream action, do not generate that step yet.

---

## 5. Screen Change Handling

### State-changing actions
Actions that change screen state include:
- Submitting a form or login
- Opening or closing a dialog
- Clicking a button that navigates to a new page or view
- Switching tab pages
- Expanding a container that reveals a new interaction surface

After a state-changing action, if the downstream controls are not yet grounded, stop. Do not generate steps for the next screen.

### Same-screen updates
Treat in-place refreshes, searches, grid reloads, text updates, and panel updates as same-screen work when the result controls are already grounded.

**In-place search pattern:** When result controls (grids, lists, detail fields) are present but disabled or empty alongside a search trigger, this is a strong signal that search returns results to the same screen. Apply Case 3 from `references/application-matching.md` section 2a — ask the user to confirm before planning post-search steps. Do not assume navigation away and do not assume in-place refresh without confirmation.

### Flip rule
Canonical statement: `planning-core.md` §3. Check for already-present downstream controls before any screen-change step.

---

## 6. Tabbed Interfaces

When interacting with UI regions controlled by a tabbed interface, treat tab selection as a required prerequisite for any downstream actions within that tab's content region.

### Tab selection rules (MUST)
- Do not target a control within a tab panel unless the containing tab has been explicitly selected in this turn or confirmed as already selected via authoritative state metadata.
- Emit the tab selection step immediately before the first step targeting a control within that tab's panel.
- If authoritative state metadata (e.g., `IsSelected: true`) confirms the correct tab is already active, do not emit a redundant tab selection step.

### Multi-tab goals
When a goal requires reading from or writing to controls across multiple tabs:
- Group steps by tab.
- Emit tab selection before each group.
- Do not interleave steps from different tabs without re-selecting the tab.

### TabStrip and RadioButton equivalents
The same prerequisite logic applies to every selector type (TabStrip, RadioButton group, Checkbox toggle, Dropdown/ComboBox). The canonical per-target enforcement procedure — including disabled-selector handling — is the pre-match selector context check in `application-matching.md` §2. Run it for every read or write target inside a selector-controlled region.
