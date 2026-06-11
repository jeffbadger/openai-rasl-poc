# Pattern: Grid Extraction

A grid extraction automation reads all visible rows from a populated result grid and builds a PegaTable output containing the extracted data. The PegaTable is passed to subsequent automations in a pipeline for processing or write-back.

This is distinct from search-and-select (find one row) and data-loop (process each row of an existing table) — grid extraction reads the full visible result set from an application grid into a structured PegaTable for downstream use.

---

## When This Pattern Applies

- Goal is to extract all rows from a populated application grid into a PegaTable
- The grid is already populated (search has already been triggered, or the screen opens to a populated state)
- The extracted data will be passed to a subsequent automation (typically an ExcelRowLoop or another Callable)
- Output is always a PegaTable, not individual output parameters

---

## Step Shape

```
1. [Confirm grid is populated — row count > 0]              — supporting
2. Get column count or identify target columns              — supporting
3. Initialise output PegaTable structure                    — supporting
   (set column names matching grid columns)
4. Get last row / row count of source grid                  — supporting
5. [Handle pagination if multi-page — see pagination below]
6. ForLoopStartStep — iterate grid rows
7.   Read cell value(s) from current row                    — supporting
8.   Add row to output PegaTable                            — supporting
9.   Write cell value(s) to new PegaTable row               — supporting
10. ForLoopEndStep
      LoopCompleteSteps → jump to Done
11. LabelStep: Done
12. Map output PegaTable to Run.outputTable                 — supporting
13. Set Run.Result to true                                  — supporting
14. LabelStep: Error
15. Set Run.Result to false                                 — supporting
16. Set Run.errMessage                                      — supporting
```

---

## Planning Guidance

### Grid state confirmation
Before planning extraction steps, confirm the grid is populated. If the grid row count is zero or the grid appears empty:

Call `ask_user`:
> "The results grid appears to be empty. Should I proceed with extraction (returning an empty table) or treat this as an error condition?"

- **Proceed with empty table** → skip the loop, emit an empty PegaTable, `Run.Result = true`
- **Error** → jump to Error label, `Run.Result = false`

Do not assume an empty grid is always an error — the goal context determines which is correct.

### Column identification
Identify which grid columns to extract from:
- The goal (e.g., "extract customer number, name, and balance")
- Column headers visible in the hierarchy or returned by `get_screen_state()`
- All columns when the goal says "extract all data" or similar

When the goal specifies columns by business name but the grid uses different labels, apply synonym matching. When ambiguous, call `ask_user`:
> "Which columns should I extract? All columns / [list specific columns from hierarchy]"

### PegaTable initialisation
Before the loop, initialise the output PegaTable with the column structure matching the extracted columns.

**Note:** PegaTable table-building methods (column definition, row addition) are not yet documented in `references/toolbox/data.md`. Use semantic placeholder steps for these operations — see `references/planning-core.md` §6 for placeholder rules. The implementation agent will ground the exact method names.

Placeholder step example:
```json
{
  "StepType": "MethodStep",
  "StepNumber": "3",
  "StepName": "Initialise output PegaTable columns",
  "StepDescription": "Set the column names of the output PegaTable to match the source grid columns. Exact method to be confirmed during implementation.",
  "ParentObject": "PegaTable",
  "MethodName": "SetColumnNames"
}
```

### Loop type for grid extraction
Use **ForLoop** when the grid exposes a row count or last-row property — this is the preferred approach for grid extraction since the row count is known before the loop begins.

Use **DoWhileLoop with MoveFirst/MoveNext** when the grid is a cursor-based component and no row count property is available.

### Per-row extraction
Inside the loop:
1. Read each target column value from the current row (`ApplicationValueStep` with `Action: "GetValue"`)
2. Add a new row to the output PegaTable (semantic placeholder — see note above)
3. Write each read value to the corresponding column in the new PegaTable row

Tier classification:
- Read cell steps → `"supporting"` (feeding the output table, not direct business output)
- Add row / write cell steps → `"supporting"` (infrastructure for the output)

All extraction steps are supporting — the business value is the completed PegaTable, not any individual cell read.

### Pagination
When the grid is paginated (a Next Page button, page number indicator, or total record count exceeding visible rows is present), handle all pages within a single turn:

**Pagination detection signals:**
- Next Page / Previous Page buttons present
- Page indicator (Page 1 of N, showing X of Y records)
- Total record count visible that exceeds the grid row count

**Pagination step shape:**
```
[After ForLoopEndStep for page 1]
Decision: Is Next Page button enabled?
  true:
    Click Next Page                                         — primary
    [Brief wait or re-interrogation if needed]
    Get last row for new page                               — supporting
    ForLoopStartStep — iterate new page rows
    ... (same per-row steps as page 1)
    ForLoopEndStep
    [Repeat decision — continue until Next Page disabled]
  false:
    Jump to Done
```

When pagination is detected, call `ask_user` before planning:
> "This grid appears to have multiple pages (I can see a Next Page control / page indicator showing [N] pages). Should I extract all pages in one pass? Yes — extract all pages / No — extract current page only"

- **All pages** → plan the paginated loop structure above
- **Current page only** → plan a single-page ForLoop, no pagination steps

### Output PegaTable
The output PegaTable is always named `Run.outputTable` for consistency with the ExcelPipeline pattern.

Map the completed PegaTable to `Run.outputTable` as the final supporting step before `Run.Result = true`.

### GoalCompleted
Grid extraction is a single-turn operation when the full grid (all pages if paginated) can be planned from the current hierarchy.

- `GoalCompleted = true` after the loop completes and the output PegaTable is populated
- `GoalCompleted = false` only when additional context is needed before extraction can be planned

### No reset required (typical)
Reading a grid does not change application state. No reset is needed unless the extraction required opening a dialog or changing a selector state that should be restored.

---

## AutomationCategory

Grid extraction delivers direct business value — the populated PegaTable is the business output.

| Turn | Classification |
|---|---|
| Navigation to the populated grid | `"navigation"` |
| Extraction turn (loop + PegaTable build) | `"core"` |

---

## Pipeline Integration

Grid extraction is the first stage of a data processing pipeline:

```
GridExtraction callable    → out PegaTable outputTable
    ↓
ExcelRowLoop callable      → in PegaTable inputTable, out PegaTable outputTable  
    ↓
ExcelWriteBack callable    → in PegaTable inputTable
```

Or paired with a read-and-report callable per row:

```
GridExtraction callable    → out PegaTable outputTable
    ↓
Orchestrator loops rows    → calls ReadAndReport callable per row
```

The output PegaTable column names should be chosen to be meaningful to the downstream automation — use business field names, not control names or column indices.

---

## Common Variations

### Filtered extraction
When only rows meeting a condition should be extracted, emit a decision step inside the loop before the add-row step. Only add rows to the output PegaTable when the condition is met. Describe the filter condition clearly in the `StepDescription`.

### Multi-grid extraction
When the goal requires extracting from multiple grids on the same screen (or across tabs), plan each grid's extraction loop separately. Each loop builds columns into the same output PegaTable or into separate output PegaTables depending on the downstream need. Ask the user when the goal is ambiguous about whether results should be combined or kept separate.

### Extraction after search-and-select
When a search must be executed first to populate the grid, prefix with the search pattern. The search populates the grid; grid extraction reads its contents. Plan as separate turns or separate callables depending on whether the search and extraction occur on the same screen.
