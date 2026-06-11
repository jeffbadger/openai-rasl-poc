# Language Capabilities

Advertises the non-application step families available to the planner. Load when the goal includes non-UI work: decisions, loops, value transfer, toolbox service calls, or label-based control flow.

This file describes *what kinds of steps are valid to plan* — not final syntax or method signatures. Use `references/output-contract.md` for exact field names and `references/examples/` for shape.

---

## Decisions and Branching

Use `DecisionStep` or `ApplicationDecisionStep` for:
- Checking a single boolean condition
- Branching between two or more paths based on a value or method result
- Comparing a grounded value, method result, or application-derived value to a static literal

Operator selection determines which variant to use — see `references/output-contract.md` for the full operator selection rules (`decision`, `if/else`, `switch`, `stringSwitch`).

Do not assume:
- Compound boolean expressions (AND/OR across multiple conditions) are supported in a single step
- Multiple conditions can be compressed into one decision unless explicitly confirmed by loaded rules

---

## Loops

Use loop steps for any repeated action over a collection or while a condition holds.

Four loop families are available:
- **ForLoop** — fixed iteration count
- **ListLoop** — iterates over a list; accesses `CurrentItem` each iteration; supports `Break` with `LoopBreakSteps` and `LoopCompleteSteps` for list-exhausted path
- **DoWhileLoop** — condition checked at end of each iteration (always executes at least once; used for cursor-based iteration)
- **WhileLoop** — condition checked at start of each iteration

Every loop start step has a corresponding loop end step. They always appear in matched pairs.
See `references/patterns/data-loop.md` for loop type selection guidance.

Do not assume:
- Nested loops are prohibited — they are valid when needed
- Break or continue are implicit — use `JumpToLabelStep` with a `LabelStep` target when early exit is needed

---

## Labels and Jumps

Use `LabelStep` and `JumpToLabelStep` for:
- Named execution destinations (loop exit targets, error handling branches)
- Transferring execution to a named branch
- Organizing multi-path workflows where a jump is semantically cleaner than deeply nested decisions

Label names must be unique within the automation.
Every `JumpToLabelStep` must target an existing `LabelStep` in the same automation.

Do not use labels as a substitute for loop end steps — use them only when the control flow genuinely requires a jump.

---

## Value Transfer

Use `ValueStep` for:
- Copying a value from one variable or control to another
- Assigning a static literal to a variable
- Storing a method result for use in multiple downstream steps

`ValueStep` is a supporting-tier step in almost all cases. Apply the variable minimization rule — only create a `ValueStep` when the value is referenced in two or more places.

Do not use `ValueStep` to store a value that is consumed only once — reference it directly from its source in the consuming step.

---

## Toolbox and Service Steps

Use `MethodStep` for:
- Calling a toolbox service method (String, File, Math, Environment, etc.)
- Calling a durable memory automation (project-scoped or application-scoped)
- Any non-UI operation that has a grounded method in a loaded catalog or durable memory signature

Toolbox catalogs are reference files under `references/toolbox/` — load the file covering the capability area (per the SKILL.md toolbox classifier table) before emitting a concrete `MethodStep`.

Available capability areas include (not exhaustive — the toolbox files contain the full grounded catalogs):
- `String` — text manipulation, comparison, formatting
- `Math` — arithmetic, rounding, numeric operations
- `DateTime` — current date/time, date arithmetic, parsing, formatting, component extraction
- `TimeSpan` — durations, intervals, elapsed-time calculations
- `File` — file read/write, path operations
- `Environment` — system info, environment variables
- `Log` — logging and tracing
- `DataTable` — in-memory table operations
- `Clipboard` — system clipboard read/write
- `MessageBox` — user-facing dialogs and notifications
- `AsoManager` — credential and secure storage retrieval
- `SystemInformation` / `Process` — machine info, process inspection and launch
- `ExcelConnector` — Excel workbook operations (component instance)
- `PegaTable` — cursor iteration, cell read/write (component instance)

When the capability area is known but the catalog is not loaded, use a semantic placeholder per `references/planning-core.md` §6.

---

## TodoStep

Use `TodoStep` only as a last resort when no grounded step alternative exists.
Apply the todo gating rule from `references/planning-core.md` §7 before emitting any `TodoStep`.
Check `AllowTodoSteps` in Planning Controls before emitting any `TodoStep`.

`TodoStep` is a `primary`-tier step — it represents intended business work that cannot yet be automated.
