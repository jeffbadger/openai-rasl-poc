---
name: automation-planner
description: >
  Converts a user goal and application surface context into a structured automation
  step plan (Steps JSON) for Pega Robotic Automation. Use when: a goal describes
  work to be automated AND an application surface or pipeline context is available
  (Windows, Web, Excel, Text, or Automation). Produces an ordered step sequence
  with GoalCompleted signal, CompletedStepSummaries, and AutomationCategory —
  consumed by the C# PlannerHost and RASL implementation agent. Do not use for
  RASL code generation (use rasl-generator skill) or for non-automation tasks.
---

# Automation Planner

Generate a structured automation step plan from a user goal and application surface context. This skill manages the full planning session — from initial goal analysis through clarification, pattern selection, and step generation.

---

## Planning Workflow

Follow these phases in order. Do not generate steps until Phase 5.

### Phase 1 — Orient

You will receive:
- **Goal** — what the automation must accomplish (may be a full goal or a current-turn sub-goal)
- **Surface type** — one of: `Windows`, `Web`, `Excel`, `Text`, `Automation` (no live UI — pipeline tasks)
- **Component type** — `Application` or `Automation`
- **Task prefix** — if present, identifies a named pipeline role: `ExcelExtract:`, `ExcelRowLoop:`, `ExcelWriteBack:`, `Callable:`, `Orchestration:`
- **Completed steps** — record of steps planned in prior turns (may be empty on first turn)
- **Durable memory** — cross-session context including callable automation signatures (may be empty)

Read these inputs before doing anything else. Do not call any tool yet.

Determine:
1. Is this the first turn or a continuation turn?
2. Is a task prefix present? If so, note the pipeline pattern — the corresponding reference file will be loaded in Phase 4.
3. Is the goal clear enough to plan, or are there ambiguities that would materially change the step sequence?

### Phase 2 — Clarify (conditional)

The complete set of `ask_user` triggers, with question formats and answer handling, is the **Ask Decision Table** in `references/planning-core.md` §5. Scan it after goal analysis and again whenever a trigger condition is hit during step generation.

Goal-level triggers to evaluate before planning begins:
- The goal can be satisfied by two or more distinct automation approaches
- A required value is unspecified and cannot reasonably be treated as a runtime input parameter
- The scope of the automation is unclear (single record vs. batch, one sheet vs. all sheets)

Control-state and matching triggers arise mid-generation — when one fires, pause step generation, ask, then resume with the answer applied to this turn only.

Do not ask about:
- Values that are clearly runtime parameters (record ID, search term, file name)
- UI details that will be resolved by calling `get_screen_state`
- Pattern choices that have a clear default for this surface type
- Disabled controls whose state is confidently explained by a planned prerequisite (Cases 1–2 in `references/application-matching.md` §2a)

Prefer one batched `ask_user` call over multiple sequential calls. Skip this phase entirely when no trigger fires.

### Phase 3 — Gather runtime context

Call only tools that are relevant to the current goal.

**Call first when surface type is not `Automation`:**
```
get_screen_state()
```
Returns the current application hierarchy. Required before generating any application steps.
Skip when surface type is `Automation` (pipeline tasks have no live UI).

**Call when the goal may be served by an existing automation:**
```
get_callable_signatures()
```
Returns callable automation signatures from durable memory.
Call when the goal involves sub-tasks that existing automations may already cover.

**Call when surface type is `Excel`:**
```
get_excel_structure()
```
Returns sheet names, column headers, and used range.
Call before planning any Excel-specific steps.

### Phase 4 — Load reference files

Based on what you now know about the goal, surface, and gathered context, load the reference files needed for this turn. Load only what is relevant — do not load everything.

**Always load:**
- `references/output-contract.md` — step naming, classification rules, output schema
- `references/planning-core.md` — planning context, goal analysis, advancement rules, todo policy, validation
- `references/anti-patterns.md` — known failure modes as wrong/right pairs; check the plan against these before returning

**Load when application steps are needed:**
- `references/application-matching.md` — hierarchy authority, semantic matching, control identity (required for all application surfaces)
- `references/application-steps.md` — step generation rules, screen change handling, tabbed interfaces (load when multi-screen, tabs, or screen-change logic is involved)
- Surface file matching the surface type:
  - Windows → `references/surfaces/windows.md`
  - Web → `references/surfaces/web.md`
  - Text → `references/surfaces/text.md`
  - Excel → `references/surfaces/excel.md`
  - Automation (PegaTable) → `references/surfaces/pega-table.md`

**Load when non-application steps are needed:**
- `references/language-capabilities.md` — loops, decisions, value steps, method steps

**Load for callable/orchestration goals:**
- `references/planning-callable.md` — callable lifecycle, reset turns, callable GoalCompleted gate, post-first-core constraint
- Load when: task prefix is `Callable:` or `Orchestration:`, or the goal requires a Callable/Orchestration split

**Load when the current surface may be an authentication surface:**
- `references/planning-auth.md` — auth goal disambiguation, mixed-signal surface, auth gate, login GoalCompleted gate
- Load when: `get_screen_state()` returns a hierarchy containing credential input fields (username, password, login, sign-in inputs), OR auth-related controls (Login button, SSO button, Sign In link), OR the goal is semantically an auth goal (login, sign in, authenticate)
- **Critical:** load based on what the surface contains, not what the goal says. Any goal on an auth surface requires this file — the auth gate override (section 3) will suppress non-auth steps and set `GoalCompleted = false`.

**Load when durable memory is present:**
- `references/planning-durable-memory.md` — durable memory usage, callable invocation from signatures, write policy
- Load when: the request contains a non-empty `# Durable Memory` section, or `get_callable_signatures()` returned signatures, or the response contract includes `DurableMemoryWrites`

**Load pattern files based on goal shape** — a single goal may use multiple patterns:

| Goal shape | Pattern file |
|---|---|
| Find a record, read its values | `references/patterns/search-and-read.md` |
| Find a specific record in a result set, select it | `references/patterns/search-and-select.md` |
| Navigate to a record, read fields, return as output parameters | `references/patterns/read-and-report.md` |
| Enter data into a form, submit | `references/patterns/form-fill.md` |
| Multi-screen sequential data entry with Next/Back/Finish | `references/patterns/wizard.md` |
| Read all rows from a populated grid into a PegaTable | `references/patterns/grid-extraction.md` |
| Task prefix is ExcelExtract/ExcelRowLoop/ExcelWriteBack | `references/patterns/excel-pipeline.md` |
| Task prefix is Callable or Orchestration | `references/patterns/callable-orchestration.md` |
| Process each item in a list, table, or grid | `references/patterns/data-loop.md` |

**Load toolbox files based on what capability the non-UI work requires.** Use the keyword signals as routing hints — but the primary question is: *what kind of operation is this?* Identify the capability type first, then load the matching file. Do not require exact keyword matches; route by intent.

| Capability type | Keyword signals | Toolbox file |
|---|---|---|
| Text manipulation, string comparison, encoding, character inspection, regex, format validation | text, string, format, parse, concat, split, trim, replace, compare, encode, regex, pattern, email, URL, IP, hash, base64 string | `references/toolbox/string.md` |
| Dates, times, timestamps, durations, time arithmetic, timezone, elapsed time | date, time, timestamp, timezone, duration, days, hours, minutes, seconds, elapsed, interval, parse date, format date | `references/toolbox/datetime.md` |
| Files, directories, paths, reading/writing disk, zip archives | file, directory, path, read, write, copy, move, delete, zip, folder, archive, disk | `references/toolbox/file-system.md` |
| Numeric operations, arithmetic, rounding, trigonometry, financial calculations, random numbers | math, calculate, round, absolute, square root, power, log, ceiling, floor, truncate, max, min, trig, sine, cosine, random, financial, annuity, interest, NPV, IRR | `references/toolbox/math.md` |
| Excel workbook operations, sheet/cell/range manipulation | Excel, workbook, sheet, cell, row, column, range, spreadsheet, export, import | `references/toolbox/excel-connector.md` |
| PegaTable cursor operations, JSON, GUID, type utilities, OCR, document extraction, geometry | PegaTable, cursor, row iteration, JSON, GUID, unique ID, OCR, document, null check, Point, Rectangle, DataTable | `references/toolbox/data.md` |
| System environment, OS info, processes, Pega product versions, runtime control, screen info, user identity, execution pause, type conversion, memory/GC | system, OS, environment variable, process, version, runtime, terminate, project, deployment, package, screen, display, resolution, user, domain, role, authenticated, pause, sleep, wait, delay, milliseconds, convert, type conversion, encode bytes, base64 bytes, garbage, memory, RAM, heap | `references/toolbox/system.md` |
| Clipboard, credentials, ASO, secure storage, messages, dialogs, logging, application startup, window management | clipboard, credential, ASO, password, login, message, dialog, log, start my day, startup, launch application, window position, window size, organize desktop | `references/toolbox/ui-interaction.md` |

**When to load (MUST):** Load a toolbox file when the goal or any expected step requires work of that capability type — regardless of whether the exact words appear. Examples: "wait 500ms" → execution pause → `system.md`; "is the newline CRLF or LF" → system environment → `system.md`; "how much physical memory is free" → system info → `system.md`; "encode this to Base64" → string encoding → `string.md` (for string Base64) or `system.md` (for byte array Base64 via System.Convert).

When in doubt, load the file — a loaded catalog that isn't used costs less than a missed concrete method that forces a placeholder.

**Fallback (MUST):** If no capability row clearly matches and the work is non-UI, reason from the goal's intent to the closest capability type and load that file. Emit a semantic placeholder (`references/planning-core.md` §6) only when the work genuinely cannot be assigned to any capability area — never because the keyword table didn't match literally.

**Load examples only when needed:**
- `references/examples/` — one file per step type, load only the types present in your plan
- Step type → example file mapping is in the quick reference table at the bottom of this file

### Phase 5 — Generate steps

Generate the full step sequence for the current screen. Do not emit partial output — wait until all context is gathered and all questions are answered.

Apply planning rules from the loaded reference files. When rules conflict, `references/output-contract.md` takes precedence over all others.

After generating steps, produce the final JSON output per the schema in `references/output-contract.md`.

---

## Tools

| Tool | Purpose | When to call |
|---|---|---|
| `ask_user(questions)` | Clarify ambiguous goal aspects | Phase 2, before any other tool |
| `get_screen_state()` | Current application hierarchy | Phase 3, application surfaces only |
| `get_callable_signatures()` | Existing callable automations in project | Phase 3, when sub-tasks may already exist |
| `get_excel_structure()` | Sheet/column/range metadata | Phase 3, Excel surface only |

---

## Output

The output must be a single JSON object conforming to the schema in `references/output-contract.md`.

Required top-level fields every turn:
- `AutomationName`
- `AutomationDescription`
- `AutomationContext`
- `AutomationCategory`
- `Includes`
- `Steps`
- `GoalCompleted`
- `CompletedStepSummaries`
- `StepDescription`

Optional fields:
- `CandidateAutomation` / `CandidateAutomationSummary` — populated only when a described public method substantially matches the generated steps
- `DurableMemoryWrites` — populated only when an approved durable-memory item is discovered this turn
- `PlanningTrace` — populated only when the request includes `"debug": true`

### PlanningTrace

When `debug` mode is enabled, include a `PlanningTrace` object at the top level. For testing and refinement only — never populate in production output.

```json
"PlanningTrace": {
  "GoalInterpretation": "What the planner understood the goal to mean",
  "AmbiguitiesFound": ["list of ambiguities identified"],
  "QuestionsAsked": ["questions sent to ask_user, if any"],
  "UserAnswers": ["answers received, if any"],
  "PatternsApplied": ["pattern names considered and why"],
  "ReferencesLoaded": ["list of reference files loaded"],
  "ToolCallSequence": ["ordered list of tools called and what they returned"],
  "KeyDecisions": ["significant planning decisions and their rationale"],
  "RulesApplied": ["specific rules from reference files that governed key steps"],
  "ValidationResults": {
    "GroundingCheck": "evidence — e.g. '14 steps, all ControlName/ElementReferenceId resolved from hierarchy nodes; no future-screen references in any text field'",
    "SelectorChecksRun": ["one entry per read/write target — e.g. 'tbAccount → rbtnCreditCard group inspected, all Checked:false, search trigger planned → selector step emitted after trigger'"],
    "AdvancementTableRow": "which row of the Advancement and Completion table matched and why",
    "DirectOutputMapping": "evidence — e.g. '6 reads, 6 mapped directly to Run.* on ApplicationValueStep, 0 intermediate ValueSteps'",
    "TodoStepGate": "either 'no TodoSteps emitted' or per-TodoStep evidence that all 6 gating conditions held, including the consent question and answer",
    "AskTriggersEvaluated": ["each Ask Decision Table row that fired or was explicitly ruled out, with the reason"],
    "GoalCompletedJustification": "why the value is true or false, citing the gate applied (standard / callable / login)"
  }
}
```

**ValidationResults rules:** Every field must contain concrete evidence drawn from the generated steps — control names, step numbers, counts. A bare "applied" or "passed" without evidence is invalid. If a check reveals a violation, fix the plan before returning output; do not return output that contradicts its own ValidationResults.

---

## Surface type routing

| Surface type | Load surface file | Call get_screen_state | Call get_excel_structure |
|---|---|---|---|
| `Windows` | `surfaces/windows.md` | Yes | No |
| `Web` | `surfaces/web.md` | Yes | No |
| `Text` | `surfaces/text.md` | Yes | No |
| `Excel` | `surfaces/excel.md` | Yes | Yes |
| `Automation` | `surfaces/pega-table.md` | No | No |

For `Automation` surface (pipeline tasks): planning is based on PegaTable method steps, durable memory automation signatures, and toolbox methods. The absence of a hierarchy is expected — do not treat it as an error.

---

## Multi-turn behavior

The host drives the planning loop, calling this skill once per screen. Each turn:

1. Receives updated context: current goal, completed steps from prior turns, current screen state
2. Plans only what is achievable on the current screen
3. Returns steps + `GoalCompleted` signal + `CompletedStepSummaries` to carry forward

The planner does not manage its own loop. `GoalCompleted = true` signals to the host that no further planning turns are needed for this goal.

See `references/planning-callable.md` for callable GoalCompleted gate logic, callable lifecycle, and AutomationCategory classification rules for callables.

---

## Quick reference — step type to example file

| Step type | Example file |
|---|---|
| `ValueStep` | `references/examples/value-step.md` |
| `ApplicationValueStep` | `references/examples/application-value-step.md` |
| `ApplicationMethodStep` | `references/examples/application-method-step.md` |
| `MethodStep` | `references/examples/method-step.md` |
| `DecisionStep` | `references/examples/decision-step.md` |
| `ApplicationDecisionStep` | `references/examples/decision-step.md` |
| `ForLoopStartStep` / `ForLoopEndStep` | `references/examples/for-loop-step.md` |
| `ListLoopStartStep` / `ListLoopEndStep` | `references/examples/list-loop-step.md` |
| `DoWhileLoopStartStep` / `DoWhileLoopEndStep` | `references/examples/dowhile-loop-step.md` |
| `WhileLoopStartStep` / `WhileLoopEndStep` | `references/examples/while-loop-step.md` |
| `LabelStep` / `JumpToLabelStep` | `references/examples/label-step.md` |
| `TodoStep` | `references/examples/todo-step.md` |
