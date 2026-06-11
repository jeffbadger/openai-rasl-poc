# Pattern: Search and Read

A search-and-read automation locates a specific record using search criteria, then reads one or more values from the result.

---

## When This Pattern Applies

- Goal involves finding a record by a key value (customer ID, account number, order number, name)
- Goal requires reading data fields from the located record
- The application requires a search action before the target data is visible

---

## Step Shape

```
1. [Navigate to search screen if not already there]          — navigation, supporting
2. Enter search criteria into search field(s)                — primary
3. Trigger search (click Search button or press Enter)       — primary
── screen change: search results appear ──
4. [Select record from results if multiple results shown]    — primary (if needed)
── screen change: record detail appears ──
5. Read target field(s) from record                         — primary
6. [Store values into variables if used multiple times]      — supporting
```

Steps 1 and 4 are conditional — emit only when the current hierarchy grounds them.

---

## Planning Guidance

### Search criteria
Treat unspecified search key values as runtime input parameters — emit the set-value step with `StaticValue: null`.
Do not emit a `TodoStep` for a missing search key.

### Search trigger
The search trigger (button click or Enter key) is an action-only step — it advances application state.
After the trigger, stop and wait for the results screen to be grounded before generating read steps.
**Exception:** If the result controls are already present in the current hierarchy (same-screen search), complete the read steps in the same turn.

### Result selection
When search results show a list or grid and the goal requires a specific record, emit a selection step before reading detail fields.
If the hierarchy grounds only a single result row, selection may be implicit — check whether a detail view requires explicit selection.

### Reading values
Emit one `ApplicationValueStep` per field to read.
Apply the same-node identity binding rule from `references/application-matching.md` §3 — resolve each field's node independently.

### GoalCompleted
- After search trigger with no results controls grounded → `GoalCompleted = false`
- After reading all required values from the record → `GoalCompleted = true`
- After selection step with detail controls not yet grounded → `GoalCompleted = false`

---

## AutomationCategory
- Turn containing only navigation and search criteria entry → may be `"navigation"` if no business value is captured
- Turn that reads target field values → `"core"`
- Any turn where at least one field value is captured → `"core"`

---

## Common Variations

### Search then update
After reading, the goal continues with modifying the located record.
Plan the read steps first, then continue with update steps in the same turn if the edit controls are already grounded.
If the edit controls require navigating to an edit mode, stop after reading and continue in the next turn.

### Search only (no read)
When the goal is to confirm a record exists (not to read values), the search trigger is the final step.
Set `GoalCompleted = true` after the trigger — this is an action-only goal.

### Multi-criteria search
When multiple search fields are required, emit a set-value step for each field before the trigger.
Order: set all criteria fields, then trigger search.
