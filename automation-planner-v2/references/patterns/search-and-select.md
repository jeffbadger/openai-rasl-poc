# Pattern: Search and Select

A search-and-select automation locates a specific record in a result set by matching a key value, selects it, and optionally reads values from the opened detail view.

Extends the search-and-read pattern by adding a result row identification and selection step between the search trigger and any detail reads.

---

## When This Pattern Applies

- Goal involves finding a specific record by a key value and opening or acting on it
- Search returns a result set (grid, list, table) rather than navigating directly to a detail view
- The correct row must be identified by matching a key column value before selection
- Selection opens a detail view, enables action controls, or reveals additional fields

---

## Step Shape

```
1. [Navigate to search screen if not already there]          — navigation
2. Enter search criteria into search field(s)               — primary
3. Trigger search                                           — primary
── in-place search or screen change: results appear ──
4. Get row count / last row of result grid                  — supporting
5. MoveFirst or initialise loop index                       — supporting
6. ForLoopStartStep or DoWhileLoopStartStep
7.   Read key column value from current row                 — supporting
8.   Decision: does key value match input parameter?
       Match: store row reference, Break                    — supporting
       No match: continue loop
9. ForLoopEndStep / DoWhileLoopEndStep
     LoopBreakSteps → jump to SelectRecord
     LoopCompleteSteps → jump to Error (no match found)
10. LabelStep: SelectRecord
11. Select the matched row                                  — primary
── selection opens detail or enables controls ──
12. [Read detail field values if goal requires]             — primary
13. LabelStep: Error
14. [Error handling steps]                                  — supporting
```

Steps 1 and 12–14 are conditional — emit only when the current hierarchy grounds them.

---

## Planning Guidance

### Search trigger and result controls
Apply Case 3 from `references/application-matching.md` §2a when result controls are present but disabled or empty before the search trigger. Ask the user to confirm in-place search before planning the loop and selection steps.

### Row count and loop type
**ForLoop** — use when the grid exposes a row count property or a `GetLastRow`/`GetCount` method is available. Iterate from 0 (or 1) to row count.

**DoWhileLoop with MoveFirst/MoveNext** — use when the result set is a cursor-based component (PegaTable, DataTable) or when no row count property is available. Apply the standard cursor iteration pattern from `references/patterns/data-loop.md`.

Prefer ForLoop when the grid exposes a count — it avoids the cursor positioning overhead.

### Key column identification
The key column is the column whose value the planner matches against the input parameter. Identify it from:
- The goal (e.g., "find the record for customer number X" → customer number column)
- The column header labels visible in the hierarchy or returned by `get_screen_state()`
- The input parameter name (e.g., `inputAccountNumber` → account number column)

When the key column cannot be confidently identified, call `ask_user`:
> "Which column in the results grid should I match against the input [parameter name]? [list visible column headers as options]"

### Match decision
Use `DecisionOperator: "decision"` when matching via a method call (e.g., `String.Equals`).
Use `DecisionOperator: "if/else"` when comparing a read cell value directly against an input parameter string.

The match branch:
1. Store the matched row reference or index in a variable (supporting step)
2. Break the loop

The no-match branch: empty — loop continues to next row.

### Loop exit paths
**Break path (match found)** → `JumpToLabelStep` to `SelectRecord`.
**Complete path (loop exhausted without match)** → `JumpToLabelStep` to `Error` with a descriptive `errMsg` parameter — e.g., `"No record found matching input [parameter name]"`.

### Row selection
After jumping to `SelectRecord`, emit the selection step targeting the matched row.

Selection method depends on the grid type and surface:
- Click on the row → `ApplicationMethodStep` with `PerformClick` or equivalent
- Click a link within the row → `ApplicationMethodStep` targeting the link control
- Double-click → `ApplicationMethodStep` with double-click method
- Select via row index method → `MethodStep` on the grid component

When the selection method is ambiguous, call `ask_user`:
> "How is a row selected in this grid? Single click on the row / Click a link in the row / Double-click / Other"

### Post-selection state
Selection may cause a screen transition or an in-place panel reveal. Apply section 2a state validation:
- If detail controls are already grounded after selection → read them in the same turn
- If a screen transition is expected → stop after selection, `GoalCompleted = false`
- If it is unclear whether selection transitions or reveals in-place → ask before planning detail reads

### GoalCompleted
- After search trigger with no results controls grounded → `GoalCompleted = false`
- After loop and selection with no detail read required → `GoalCompleted = true`
- After loop and selection with detail read required and controls grounded → `GoalCompleted = true` when all values captured
- After loop and selection with screen transition expected → `GoalCompleted = false`

---

## AutomationCategory
- Navigation to search screen → `"navigation"`
- Turn containing search criteria entry and trigger → may be `"navigation"` if no value is captured
- Turn containing the loop, selection, and any detail reads → `"core"`
- Any turn where at least one field value is captured or a record is selected for business purposes → `"core"`

---

## Error Label
Always include a `LabelStep: Error` with steps that:
1. Set `Run.Result` to `false`
2. Set `Run.errMessage` to a descriptive message identifying the unmatched key value
3. Exit cleanly

The error label is reached via the loop's `LoopCompleteSteps` when no match is found.

---

## Common Variations

### Single result — skip the loop
When the search criteria are precise enough that only one result is expected, the loop may be replaced by:
1. A decision on row count (if count = 1, proceed; if count ≠ 1, jump to Error)
2. Direct selection of the single row

This is appropriate when the input parameter is a unique identifier (account number, record ID) and the application's search behaviour guarantees at most one result.

Ask before using this variation if the uniqueness of the search result cannot be confirmed from the goal or durable memory.

### Select and update
After selection, the goal continues with modifying the selected record. Plan the selection steps, then continue with form-fill steps if the edit controls are grounded on the same screen. If opening the record for edit requires a further action (Edit button, mode switch), apply section 2a state validation before planning the edit steps.

### Select and delete
After selection, the goal requires deleting the record. The delete action typically requires a confirmation dialog. Plan the selection step, emit the delete trigger, then stop — the confirmation dialog is a new screen. `GoalCompleted = false` after the delete trigger unless the hierarchy grounds the confirmation in the current screen.
