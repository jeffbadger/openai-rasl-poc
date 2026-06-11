# Excel Surface

Rules specific to Excel automation. Load alongside `references/application-matching.md` (and `references/application-steps.md` for multi-screen or tabbed goals) when surface type is `Excel`. Also call `get_excel_structure()` to retrieve sheet names, column headers, and used range before generating steps.

---

## Surface Characteristics

Excel automation operates on workbook structure (sheets, ranges, cells) rather than a traditional UI hierarchy.
The primary grounding source is the structure returned by `get_excel_structure()` — sheet names, column headers, data range boundaries.
Application hierarchy controls (ribbon buttons, dialog elements) may also be present and follow standard `references/application-matching.md` rules.

---

## Sheet and Range Grounding

Only reference sheets, columns, and ranges grounded in the structure returned by `get_excel_structure()`.
Do not assume sheet names, column positions, or range boundaries — always use grounded values.

Sheet names in StepName literals use single quotes per output contract rules:
- ✅ `Activate 'Sheet1' sheet`
- ✅ `Read 'Revenue' column header`

---

## ExcelConnector Component

Excel automation uses the `ExcelConnector` component instance as `ParentObject` for method steps.
`ExcelConnector` is a component instance — not a static service. Use the instance name from the automation context.

Common method categories (load `references/toolbox/excel-connector.md` for the full grounded catalog):
- Sheet activation and navigation
- Cell read and write
- Range operations
- Row and column operations
- Used range boundaries

---

## Pipeline Pattern Integration

Excel automation commonly uses the Export/Process/Import pipeline pattern. When a task prefix indicates a pipeline role, load `references/patterns/excel-pipeline.md` for the full pattern structure.

| Task prefix | Pipeline role | Surface behavior |
|---|---|---|
| `ExcelExtract:` | Export worksheet to PegaTable | Excel surface active |
| `ExcelRowLoop:` | Process PegaTable row by row | No live Excel surface — PegaTable methods only |
| `ExcelWriteBack:` | Import enriched PegaTable back to workbook | Excel surface active |

For `ExcelRowLoop:` tasks, surface type is `Automation` — do not call `get_excel_structure()` and do not generate ExcelConnector steps. Use PegaTable method steps instead. See `references/surfaces/pega-table.md`.

---

## Sheet Activation

Always activate the target sheet before reading or writing any cells on that sheet.
Emit a sheet activation step as the first step when the goal targets a specific sheet.
If authoritative state metadata confirms the correct sheet is already active, do not emit a redundant activation step.

---

## Range Boundaries

When the goal requires iterating rows or processing a data range:
1. Call `get_excel_structure()` to retrieve the used range boundaries.
2. Emit a step to get the last row (or last column) as a supporting step before any loop.
3. Use the boundary value as the loop iteration count or termination condition.

Do not hardcode row counts or column counts — always derive from the grounded structure.

---

## Cell Reference Patterns

### Reading a specific cell
Use ExcelConnector method step targeting the grounded sheet and cell address.

### Writing a specific cell
Use ExcelConnector method step with the value to write.

### Iterating rows
Use a loop pattern — see `references/patterns/excel-pipeline.md` or the loop example files in `references/examples/`.
Cursor positioning (`MoveFirst`, `MoveNext`) applies to PegaTable iteration, not direct Excel cell iteration.
For direct Excel iteration, use row index incrementation within a loop.

---

## Header Row Handling

When the structure returned by `get_excel_structure()` identifies a header row:
- Do not read or write the header row as data.
- Begin data iteration from the first data row (typically row 2 when row 1 is headers).
- Reference column headers by name when available rather than by column letter when the ExcelConnector method supports named column access.
