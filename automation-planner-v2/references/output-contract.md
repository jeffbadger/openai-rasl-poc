# Output Contract

Governs the shape, naming, and classification rules for all planner output. This file takes precedence over all other reference files when rules conflict.

---

## Planning Controls

Read these from the request context before generating any steps.

### AllowTodoSteps
- When `false` — `TodoStep` is **prohibited** in all output this turn. Resolve every required step using grounded controls, toolbox methods, or durable memory automations. If no viable step exists for a required action, omit it rather than emitting a `TodoStep`.
- When `true` (or absent) — standard TodoStep policy in `references/planning-core.md` §7 applies.
- This is a transitional control. As product surface coverage grows, `TodoStep` will become unnecessary.

---

## Response Shape

Return a single JSON object. No natural language before or after. No wrapper objects.

If valid steps cannot be produced, return `{}`.
**Exception — Callable tasks:** Never return `{}`. Always return a valid response. See `references/planning-callable.md` for Callable handling.
**Exception — Pipeline tasks** (`ExcelExtract:`, `ExcelRowLoop:`, `ExcelWriteBack:`): Never return `{}`. Pipeline tasks always have a valid planning path.

---

## Top-Level Fields

### Required every turn

| Field | Type | Notes |
|---|---|---|
| `AutomationName` | string | PascalCase identifier for this automation |
| `AutomationDescription` | string | Brief plain-language description of what the automation does |
| `AutomationContext` | string | See AutomationContext values below. Set on first primary step. |
| `AutomationCategory` | string | `"navigation"`, `"core"`, or `"core-reset"`. Set on first primary step. |
| `Includes` | string[] | Application names this automation includes. Empty array if none. |
| `Steps` | Step[] | Ordered step sequence. See Step Types below. |
| `GoalCompleted` | boolean | Whether this turn completes the current goal. |
| `CompletedStepSummaries` | string[] | One past-tense entry per step generated this turn. Prefixed `navigation: ` or `core: ` matching the turn's AutomationCategory. Carried forward by the host each turn — do not re-emit prior turn entries. |
| `StepDescription` | string | Plain-language summary of what this turn's steps accomplish. |

### Optional fields

| Field | Type | Notes |
|---|---|---|
| `CandidateAutomation` | string \| null | Name of an existing public method that substantially matches the generated steps. Null if none. |
| `CandidateAutomationSummary` | string \| null | Brief description of why the candidate matches. Null if `CandidateAutomation` is null. |
| `DurableMemoryWrites` | object[] \| null | Approved durable-memory items discovered this turn. Omit or empty array if none. |
| `PlanningTrace` | object \| null | Debug output. Populate only when request includes `"debug": true`. See SKILL.md for shape. |

---

## AutomationContext Values

Set on the first primary step (`Tier: "primary"`) of every response.

| Value | Role |
|---|---|
| `"Automation"` | Standard automation with no named pipeline or callable pattern |
| `"Callable"` | A sub-automation (TopLevel=false) called by an orchestrator |
| `"Orchestration"` | The orchestrator that calls Callable automations |
| `"ExcelExtract"` | Pipeline role 1 — exports worksheet used range to `out PegaTable outputTable` |
| `"ExcelRowLoop"` | Pipeline role 2 — receives PegaTable input, modifies in-place row by row, returns as `out PegaTable outputTable` |
| `"ExcelWriteBack"` | Pipeline role 3 — receives enriched PegaTable and imports back to workbook. No out PegaTable. |

This list will grow as new automation contexts are introduced. When a task prefix implies a context not listed here, use the closest match and note it in `PlanningTrace` if debug is enabled.

**Pipeline tasks always have a planning surface.** When the task goal begins with `ExcelRowLoop:`, `ExcelExtract:`, or `ExcelWriteBack:`, the absence of an application hierarchy is expected. Plan using PegaTable method steps, durable memory automation signatures, and toolbox methods.

---

## AutomationCategory Classification

Classify the purpose of the steps generated in this turn as a whole.

| Value | Meaning |
|---|---|
| `"navigation"` | All steps exist solely to advance the application to the correct starting state. No direct business value is produced. |
| `"core"` | Steps directly deliver the task's stated business value — entering data, reading results, submitting a form, processing records. |
| `"core-reset"` | A Callable reset turn. Restores application to pre-invocation state. |

**Tie-breaker:** Any turn containing at least one core step is classified `"core"`, even if it also contains navigation steps.

**GoalCompleted = true** always means `"core"` or `"core-reset"`. Never `"navigation"`.

### When to ask before classifying

| Situation | Action |
|---|---|
| All steps solely advance toward the operational target; no step reads, writes, or captures a business value (launch, login, menu navigation, open search screen) | Classify `"navigation"` — do not ask |
| At least one step directly delivers the stated business value (enter data, read result, submit record, process row) | Classify `"core"` (tie-breaker) — do not ask |
| Steps are genuinely ambiguous between advancing and delivering — e.g., a button that both navigates and triggers a business action; a read that could be a prerequisite or an output; selecting a record from results | Ask before classifying |

When ambiguous, call `ask_user`:
> "This turn includes [describe the ambiguous steps]. Should this be classified as a navigation turn — setting up for the real work — or a core turn that delivers business value?"

Apply the answer to this turn only. Do not carry it forward as a rule.

For the Callable post-first-core constraint (navigation invalid after first core turn), see `planning-callable.md` §4.

---

## StepName Rules

### Casing — sentence case (MUST)
Capitalize the first word only. All remaining words lowercase unless they are proper nouns.

✅ `Click save button`
✅ `Enter customer ID into search field`
❌ `Click Save Button` (title case — prohibited)
❌ `clickSaveButton` (camelCase — prohibited)

### Approved proper names
Always capitalized regardless of position in the StepName:
- `PegaTable`
- `DataTable`

To add a name: insert it in this list. One name per line.

### Literal values — single quotes (MUST)
When a StepName includes a concrete string, sheet name, option label, file name, or any fixed runtime value, wrap it in single quotes.

✅ `Activate 'Sheet1' sheet`
✅ `Select 'Yes' option`
✅ `Click 'Submit' button`
✅ `Open 'Report.xlsx' file`

---

## Step Tier Classification

Assign `Tier` to every step. Exactly one of: `"primary"`, `"supporting"`, `"structural"`.

### Structural — assign first (deterministic)
- `LabelStep` → always `"structural"`
- `JumpToLabelStep` → always `"structural"`

### Primary — business-visible milestones
- Any step that interacts with a UI control to enter, click, or read a value
- Any loop start step
- `TodoStep`
- A `MethodStep` performing a named business action (open, export, send, get, set, launch)

### Supporting — technical prerequisites
- `DecisionStep` → always `"supporting"`
- `ApplicationDecisionStep` → always `"supporting"`
- `ValueStep` → always `"supporting"`
- Cursor/positioning steps (`MoveFirst`, `MoveNext`)
- Metadata-bounding steps (`GetLastRow`, `GetLastColumn`, `GetCount`, `RowCount`)
- Any `MethodStep` whose result is consumed only by the immediately following step with no independent business meaning
- Error logging / tracing steps → always `"supporting"`

**When in doubt:** If a user would mention this step when describing what the automation does — `"primary"`. If it only exists to make the next step valid — `"supporting"`.

---

## Variable Minimization

Do not create `ValueStep` variables to store a value used only once.
Create a `ValueStep` variable only when the same value is referenced 2 or more times (across decisions, branches, loops, or output fields).
Reference single-use values directly from their source in the step that consumes them.

### Direct output mapping (MUST)
When reading a UI control value to return as a `Run.` output parameter, map directly in the `ApplicationValueStep` — do not read into a local variable and transfer in a subsequent `ValueStep`.

Set `SetValueControl: "Run.paramName"` and `SetValueProperty: "Value"` directly on the `ApplicationValueStep`.

**Prohibited pattern** — reading to control name then transferring with a ValueStep:

Step N: ApplicationValueStep — GetValue → stores to txtLastName (wrong intermediate)
Step N+1: ValueStep — copies txtLastName → Run.lastName (wrong extra step)

**Correct pattern** — single ApplicationValueStep mapping directly to output:

Step N: ApplicationValueStep — GetValue, SetValueControl: "Run.lastName" (correct)

A follow-on `ValueStep` after an `ApplicationValueStep` read is only valid when the same value must be consumed in two or more places — for example, used in a decision branch AND written to an output parameter.



---

## CandidateAutomation Selection

Populate only when an existing public method:
1. Has a description, **and**
2. Substantially matches the generated step sequence in purpose and coverage

Selection is step-driven — compare the candidate method's described behavior against the concrete steps emitted, not just the goal text. The candidate must cover the same controls, actions, or toolbox operations the steps describe.

Do not populate because a public method is loosely related to the goal or merely exists in the application.
Set both `CandidateAutomation` and `CandidateAutomationSummary` to `null` when no substantial match exists.

---

## Step Types

All steps share these common fields:

| Field | Type | Required | Notes |
|---|---|---|---|
| `StepType` | string | Yes | Identifies the step variant. See list below. |
| `StepNumber` | string | Yes | Sequential string integer: `"1"`, `"2"`, etc. |
| `StepName` | string | Yes | Sentence case. Literals in single quotes. |
| `StepDescription` | string | Yes | Plain-language description of what the step does. |
| `Tier` | string | Yes | `"primary"`, `"supporting"`, or `"structural"` |

---

### ValueStep
Transfers a value between a source and destination, or assigns a static value.

`StaticValue` and `GetValueControl` are mutually exclusive — never both.

| Field | Type | Notes |
|---|---|---|
| `GetValueControl` | string \| null | Source control or variable name. Null when using StaticValue. |
| `GetValueProperty` | string \| null | Property on the source (e.g., `"Value"`, `"Text"`). Null when using StaticValue. |
| `SetValueControl` | string | Destination variable or control name. |
| `SetValueProperty` | string | Property on the destination. |
| `StaticValue` | string \| null | Literal value to assign. Null when using GetValueControl. |

Example file: `references/examples/value-step.md`

---

### ApplicationValueStep
Reads from or writes to a UI control in the application hierarchy.

| Field | Type | Notes |
|---|---|---|
| `ApplicationName` | string | Name of the application containing the control. |
| `ApplicationId` | string | Application identifier. |
| `ControlName` | string | Control identifier from the hierarchy node. |
| `ElementReferenceId` | string | Element reference ID from the same hierarchy node as ControlName. |
| `UserActionId` | integer | User action ID. Use `0` if none provided in context. |
| `Action` | string | `"GetValue"` or `"SetValue"` |
| `StaticValue` | string \| null | Value to set. Null for GetValue actions. |

**Identity binding rule (MUST):** `ControlName` and `ElementReferenceId` must come from the same resolved hierarchy node. Identify the node first via semantic matching, then read both fields from that node. Never decide `ControlName` before finding the node.

Node resolution trace — append to `StepDescription`:
`[Node: <ControlName> | ID: <ElementReferenceId> | Match: <how found>]`

Match methods: `AccessibilityName:<value>`, `Text:<value>`, `Name:<value>`, `ControlType:<type> under <container>`, `Synonym:<goal term> → <node label>`

Example file: `references/examples/application-value-step.md`

---

### ApplicationMethodStep
Invokes a method on a UI control.

Same fields as `ApplicationValueStep` plus:

| Field | Type | Notes |
|---|---|---|
| `MethodName` | string | Method to invoke on the control. |
| `Parameters` | object \| null | Method parameters if required. |

Example file: `references/examples/application-method-step.md`

---

### MethodStep
Invokes a toolbox service method or calls a durable memory automation.

| Field | Type | Notes |
|---|---|---|
| `ParentObject` | string | Service name (e.g., `"String"`, `"File"`) or `"Project"` for project-scoped automations. For application-scoped automations, use the application name and add it to `Includes`. |
| `MethodName` | string | Exact method name from the toolbox catalog or durable memory signature. |
| `Parameters` | object \| null | Parameter name-value pairs. |

**Grounding rule:** Only emit a concrete `MethodStep` when the service name and method name are grounded in a loaded toolbox catalog or durable memory signature. When the capability area is known but the catalog is not loaded, use a semantic placeholder — see `references/planning-core.md` §6.

Example file: `references/examples/method-step.md`

---

### DecisionStep
Branches execution based on a condition.

| Field | Type | Notes |
|---|---|---|
| `DecisionOperator` | string | See operator selection rules below. |
| `LeftOperand` | string | Left side of the condition. |
| `RightOperand` | string \| null | Right side. Null for `decision` operator. |
| `Cases` | object | Branch steps. Keys depend on operator. |
| `DefaultCaseSteps` | Step[] \| null | Only valid for `switch` and `stringSwitch`. Prohibited on `if/else` and `decision`. |

**Operator selection (MUST):**
- `"decision"` — branching on a boolean method result (`.Result` field from any toolbox or automation call). Never use `if/else` for these.
- `"if/else"` — comparing a value against a static literal. Cases must contain exactly `"true"` and `"false"` keys. When one branch requires no steps, emit that key with `[]`. `DefaultCaseSteps` is prohibited.
- `"switch"` — branching on an exact scalar value or enum.
- `"stringSwitch"` — branching on a partial string match.

**Empty Cases:** An empty cases array is intentional when runtime drives continuation automatically (e.g., the `true` branch of a `MoveNext()` decision inside a doWhile loop). Never fill an intentionally empty branch with placeholder or no-op steps.

Example file: `references/examples/decision-step.md`

---

### ApplicationDecisionStep
Branches based on a condition involving a UI control value. Same structure as `DecisionStep` with additional application identity fields matching `ApplicationValueStep`.

Example file: `references/examples/decision-step.md`

---

### ListLoopStartStep / ListLoopEndStep
Iterates over a list. The current item is accessed each iteration via a `ValueStep` reading `CurrentItem` from the loop variable.

| Field | Type | Notes |
|---|---|---|
| `LoopName` | string | Unique name for this loop instance — used to reference `CurrentItem` and call `Break`. |
| `List` | string | Variable reference to the list to iterate (e.g., `"accountNumbers.Value"`). |
| `LoopingSteps` | array | Steps executed on each iteration. |
| `LoopBreakSteps` | array | Steps executed when the loop exits via `Break`. Omit when no break path exists. |
| `LoopCompleteSteps` | array | Steps executed when the list is exhausted without a break. Always required. |

`ListLoopEndStep` has no additional fields beyond the common fields.

Example file: `references/examples/list-loop-step.md`

---

### ForLoopStartStep / ForLoopEndStep
Iterates a fixed number of times.

| Field | Type | Notes |
|---|---|---|
| `IterationCount` | string | Number of iterations or variable reference. |
| `IndexVariable` | string | Variable name for the loop counter. |

`ForLoopEndStep` has no additional fields beyond the common fields.

Example file: `references/examples/for-loop-step.md`

---

### DoWhileLoopStartStep / DoWhileLoopEndStep
Iterates while a condition holds. Condition evaluated at the end of each iteration (always executes at least once).

| Field | Type | Notes |
|---|---|---|
| `Condition` | string | Condition evaluated after each iteration. |

Example file: `references/examples/dowhile-loop-step.md`

---

### WhileLoopStartStep / WhileLoopEndStep
Iterates while a condition holds. Condition evaluated at the start of each iteration.

| Field | Type | Notes |
|---|---|---|
| `Condition` | string | Condition evaluated before each iteration. |

Example file: `references/examples/while-loop-step.md`

---

### LabelStep
Creates a named execution destination.

| Field | Type | Notes |
|---|---|---|
| `LabelName` | string | Unique label identifier within the automation. |

Example file: `references/examples/label-step.md`

---

### JumpToLabelStep
Transfers execution to a named label.

| Field | Type | Notes |
|---|---|---|
| `LabelName` | string | Must match an existing `LabelStep` in the automation. |

Example file: `references/examples/label-step.md`

---

### TodoStep
Placeholder for work that cannot be automated with currently available surfaces or catalog methods.

| Field | Type | Notes |
|---|---|---|
| `TodoDescription` | string | Plain description of what needs to be accomplished. Written as an instruction to a developer. |

**TodoDescription rules (MUST):**
- Describe the intended action or outcome at a business level.
- Do not explain why automation failed, which controls were examined, or what matching was attempted.
- Do not reference future screens, post-transition controls, or UI elements not in the current hierarchy.
- A description that explains failure is prohibited: describe the goal, not the obstacle.

**Prohibited when:** `AllowTodoSteps` is `false`. A loaded toolbox catalog covers the work. A durable memory automation covers the work. A grounded advancement step is available.

Example file: `references/examples/todo-step.md`

---

## Schema Authority

The declared fields and types in this file are authoritative.
Examples in `references/examples/` are illustrative only.
When an example conflicts with this file, follow this file and ignore the conflicting example content.
Never copy extra fields, renamed fields, or different wrapper structures from an example when they differ from the declared schema.
