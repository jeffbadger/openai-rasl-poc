# PegaTable Surface

Rules for PegaTable-based automation. Load when surface type is `Automation` and the task involves PegaTable cursor iteration — typically `ExcelRowLoop:` pipeline tasks or automations that receive a PegaTable as input.

---

## Surface Characteristics

PegaTable automation has no live application hierarchy. Planning is based on:
- PegaTable method steps (cursor iteration, cell read/write)
- Durable memory automation signatures (callable automation invocations)
- Toolbox method steps (string processing, math, services)

The absence of an application hierarchy is expected and correct. Do not treat it as an error or return `{}`.

---

## PegaTable as Component Instance

`PegaTable` (or the specific instance name from the automation context) is the `ParentObject` for all cursor and cell method steps.
PegaTable is a component instance — not a static service. Use the instance name from the automation signature or input parameter.

---

## Cursor Iteration Pattern

PegaTable row iteration always follows this pattern:

1. `MoveFirst` — position cursor to first row (`Tier: "supporting"`)
2. `DoWhileLoopStartStep` — begin loop
3. Per-row steps — read cells, process values, call services, write results
4. `MoveNext` — advance cursor (`Tier: "supporting"`)
5. Decision on `MoveNext().Result` — continue or exit loop (`Tier: "supporting"`)
6. `DoWhileLoopEndStep` — end loop

### MoveFirst
Always emit `MoveFirst` before the loop begins.
`MoveFirst` returns a boolean result indicating whether the table has any rows.
When the table may be empty, emit a decision on `MoveFirst().Result` before the loop — skip the loop body if the result is false.

### MoveNext and the doWhile pattern
`MoveNext` advances the cursor and returns `true` if another row is available, `false` if the end of the table has been reached.
Use `DecisionOperator: "decision"` (not `"if/else"`) when branching on `MoveNext().Result` — it is a boolean method result.
The `true` case of a `MoveNext()` decision inside a doWhile `on (While)` handler is intentionally empty — the doWhile loop advances to the next iteration automatically. Do not fill this branch with placeholder steps.

---

## Cell Operations

### Reading a cell value
Use PegaTable method step `GetCellStringValue` (or typed variant) targeting the column name or index.
Column references should use names from the table schema when available, indices otherwise.

### Writing a cell value
Use PegaTable method step `SetCellValue` targeting the column name or index.
`SetCellValue` modifies the current row in place — the cursor must be positioned before writing.

### Tier classification
- `MoveFirst` → `"supporting"`
- `MoveNext` → `"supporting"`
- `GetCellStringValue` → `"supporting"` when the value feeds only the immediately following step; `"primary"` when the value is a direct business output
- `SetCellValue` → `"primary"` (modifying data is a business action)

---

## Calling Other Automations

When durable memory contains `automationSignatures` relevant to the per-row processing goal, emit `MethodStep` calls per the durable memory invocation rules in `references/planning-durable-memory.md`.

For project-scoped automations: `ParentObject: "Project"`, call as `MethodName.Run(...)`.
For application-scoped automations: `ParentObject: "<AppName>"`, add app to `Includes`.

---

## Output Table

`ExcelRowLoop:` tasks receive a PegaTable input and return an enriched PegaTable as `out PegaTable outputTable`.
Modifications are made in place on the input table — no separate output table construction is needed.
The final step of an `ExcelRowLoop:` automation should confirm all rows have been processed and the output parameter is populated.

---

## GoalCompleted for Pipeline Tasks

Pipeline tasks (`ExcelRowLoop:`, `ExcelExtract:`, `ExcelWriteBack:`) complete in a single turn when the full row iteration or range operation can be planned from the available structure.
Set `GoalCompleted = true` when the loop pattern is complete and all per-row processing steps are included.
Set `GoalCompleted = false` only when additional context (e.g., a missing callable signature) prevents completing the plan this turn.
