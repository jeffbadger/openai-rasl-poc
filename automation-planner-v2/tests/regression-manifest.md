# Regression Test Manifest

Not loaded by the model. For PlannerHost regression runs against the mock data subfolders. Each scenario pairs a known production failure with the expected behaviors that the fixed rules must produce. Run after any edit to SKILL.md or references/.

Fill in `mock_data` with the matching PlannerHost mock subfolder for each scenario.

---

## Scenario 1 — auth-surface-business-goal

Origin: planner emitted login steps + TodoSteps for a customer search when the screen was a login form.
Fixed by: surface-driven load of planning-auth.md; auth gate override (planning-auth.md §3); anti-pattern 1.

- mock_data: `<login screen hierarchy — credential fields + login button>`
- goal: any business task whose controls are not on the login screen (e.g., a record search)
- task_prefix: none

Expected:
- [ ] `references/planning-auth.md` appears in `PlanningTrace.ReferencesLoaded`
- [ ] Steps contain only credential entry + submit (3 steps typical)
- [ ] Zero TodoSteps, zero steps referencing post-login controls
- [ ] `GoalCompleted = false`
- [ ] `AutomationCategory = "navigation"`
- [ ] No output text field mentions post-login screens or controls

---

## Scenario 2 — selector-scoped-read

Origin: planner emitted a TodoStep for a category-scoped value instead of clicking the radio button; on retry it asked about a disabled radio whose disable was explained by the planned search trigger.
Fixed by: pre-match selector context check incl. disabled-selector handling (application-matching.md §2); anti-pattern 2.

- mock_data: `<search form + selector-controlled detail region; radio group all Checked:false, Enabled:false pre-search>`
- goal: search for a record and read fields including one value scoped by a selector qualifier
- task_prefix: none

Expected:
- [ ] Search trigger step emitted
- [ ] Selector click step (correct radio for the goal qualifier) emitted after the trigger and before the dependent read
- [ ] Zero TodoSteps
- [ ] No ask_user question about the disabled selector (Case: planned trigger explains the disable)
- [ ] In-place search confirmation question IS asked once (Case 3) before post-search reads are planned
- [ ] `PlanningTrace.ValidationResults.SelectorChecksRun` has an evidenced entry for the scoped field

---

## Scenario 3 — direct-output-mapping

Origin: every read was followed by a redundant ValueStep copying to Run.* while the trace claimed minimization was applied.
Fixed by: direct output mapping rule (output-contract.md Variable Minimization); ValidationResults evidence requirement; anti-patterns 3 and 4.

- mock_data: `<populated detail screen with N readable fields>`
- goal: read N fields and return them as output parameters
- task_prefix: none

Expected:
- [ ] Every ApplicationValueStep maps directly via SetValueControl: "Run.<param>"
- [ ] Zero ValueSteps whose only purpose is read-to-output transfer
- [ ] `PlanningTrace.ValidationResults.DirectOutputMapping` cites the read count and zero intermediates
- [ ] Any ValueStep present is justified by 2+ consumers of the same value

---

## Scenario 4 — callable-lifecycle-smoke (cross-reference integrity)

Origin: restructuring moved callable rules across files; 24 stale references were found and fixed. This scenario confirms the moved rules resolve in a full lifecycle.
Fixed by: reference cleanup in the restructure.

- mock_data: `<multi-turn callable scenario covering navigation → core → reset>`
- goal: a callable that opens a record, reads values, and must reset to its landing screen
- task_prefix: `Callable:`

Expected:
- [ ] First turn writes `callableAutomation` to DurableMemoryWrites
- [ ] Navigation turns: `GoalCompleted = false`, category `"navigation"`
- [ ] Final core turn appends the reset-required sentinel, `GoalCompleted = false`
- [ ] No `"navigation"` classification after the first core turn
- [ ] Reset turn(s) stop exactly at the landing screen; `GoalCompleted = true`, category `"core-reset"`
- [ ] If multiple dismiss options exist in the mock, exactly one dismiss question is asked

---

## Running

For each scenario: load mock_data in PlannerHost, send the goal with `"debug": true`, and check every box against the returned Steps JSON + PlanningTrace. Any unchecked box is a regression — locate the governing rule named under "Fixed by" and verify it still loads and reads correctly before editing further.
