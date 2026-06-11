# Pattern: Excel Pipeline

A three-automation pipeline for processing Excel worksheet data: extract the data to a PegaTable, process it row by row, then write results back to the workbook.

---

## When This Pattern Applies

- Goal involves reading all rows from an Excel worksheet, processing each row, and writing results back
- Task prefix is `ExcelExtract:`, `ExcelRowLoop:`, or `ExcelWriteBack:`
- Goal describes a batch operation over Excel data (enrich, validate, transform, look up)

A single goal may require all three automations. The pipeline is always planned as three separate automations — not one.

---

## Pipeline Structure

| Automation | Task prefix | Surface | Input | Output |
|---|---|---|---|---|
| 1. Extract | `ExcelExtract:` | Excel | Workbook connection | `out PegaTable outputTable` |
| 2. Row Loop | `ExcelRowLoop:` | Automation (no UI) | `in PegaTable inputTable` | `out PegaTable outputTable` |
| 3. Write Back | `ExcelWriteBack:` | Excel | `in PegaTable inputTable` | None |

Each automation is planned independently. The orchestrator calls them in sequence, passing the PegaTable between them.

---

## Automation 1: ExcelExtract

**Purpose:** Open the workbook (if not already open), activate the target sheet, and export the used range to an output PegaTable.

**Step shape:**
```
1. Activate target sheet                                     — primary
2. Get used range boundaries (last row, last column)        — supporting
3. Export used range to outputTable                         — primary
```

**Planning guidance:**
- Call `get_excel_structure()` to confirm sheet name and used range before generating steps.
- Sheet name in StepName uses single quotes: `Activate 'Data' sheet`
- The export step uses the ExcelConnector method that converts the range to a PegaTable — load `references/toolbox/excel-connector.md` to confirm the exact method name.
- `GoalCompleted = true` after the export step — extraction is a single-turn operation.
- `AutomationContext: "ExcelExtract"`

---

## Automation 2: ExcelRowLoop

**Purpose:** Receive the PegaTable from extraction, iterate every row, apply per-row processing (lookups, enrichment, transformation), and return the modified table.

**Step shape:**
```
1. MoveFirst on inputTable                                   — supporting
2. [Decision on MoveFirst result — skip if empty]           — supporting (if table may be empty)
3. DoWhileLoopStartStep
4.   Read cell value(s) from current row                    — supporting or primary
5.   [Call service / callable automation for enrichment]    — primary
6.   Write result value(s) to current row                   — primary
7.   MoveNext on inputTable                                  — supporting
8.   Decision on MoveNext result                            — supporting
       true: [] (empty — loop continues automatically)
       false: exit loop
9. DoWhileLoopEndStep
```

**Planning guidance:**
- Surface type is `Automation` — no Excel hierarchy, no `get_excel_structure()` call.
- Load `references/surfaces/pega-table.md` for cursor iteration rules.
- Call `get_callable_signatures()` if per-row processing invokes existing automations.
- Load the relevant `references/toolbox/` files for any string, math, or service operations needed per row.
- The `true` branch of the `MoveNext()` decision is intentionally empty — do not add placeholder steps.
- `GoalCompleted = true` after the loop completes — row loop is a single-turn operation.
- `AutomationContext: "ExcelRowLoop"`

---

## Automation 3: ExcelWriteBack

**Purpose:** Receive the enriched PegaTable and write its values back to the workbook worksheet.

**Step shape:**
```
1. Activate target sheet                                     — primary
2. MoveFirst on inputTable                                   — supporting
3. DoWhileLoopStartStep
4.   Get current row index or key value                     — supporting
5.   Write cell value(s) to corresponding worksheet row     — primary
6.   MoveNext on inputTable                                  — supporting
7.   Decision on MoveNext result                            — supporting
8. DoWhileLoopEndStep
```

**Planning guidance:**
- Call `get_excel_structure()` to confirm the target sheet and column positions for write-back.
- Coordinate column positions between the PegaTable column names and the worksheet column letters/indices.
- No `out PegaTable outputTable` for this automation — it writes to the workbook and returns nothing.
- `GoalCompleted = true` after the loop completes.
- `AutomationContext: "ExcelWriteBack"`

---

## AutomationCategory for Pipeline Tasks
All three pipeline automations are single-turn, single-concern operations.
`AutomationCategory: "core"` for all three — each delivers direct business value.
`GoalCompleted = true` at the end of each automation's single turn.

---

## Common Variations

### Skip header row
When the first row of the PegaTable is a header row (not data), begin iteration from row 2.
Emit a `MoveNext` step after `MoveFirst` to skip the header before the main loop begins.

### Conditional write-back
When only modified rows should be written back, emit a decision step per row before the write steps.
Base the decision on a flag column set during the ExcelRowLoop phase.

### Multiple target sheets
When write-back targets multiple sheets, activate each sheet before writing its corresponding rows.
Group steps by sheet — activate, write all rows for that sheet, move to next sheet.
