# Pattern: Callable Orchestration

A callable orchestration decomposes a complex goal into reusable sub-automations (Callables) coordinated by an orchestrator.

---

## When This Pattern Applies

- Goal requires the same sub-task to be performed repeatedly (once per record in a list, once per iteration of a loop)
- Goal involves multiple distinct application interactions that benefit from independent versioning or reuse
- Task prefix is `Callable:` (planning the sub-automation) or `Orchestration:` (planning the coordinator)
- `get_callable_signatures()` returns existing automations that match required sub-tasks

---

## Architecture

```
Orchestrator
├── Calls Callable A (e.g., "Look up customer record")
├── Calls Callable B (e.g., "Update account status")
└── Calls Callable C (e.g., "Log result to file")
```

Each Callable:
- Is a standalone automation (`TopLevel: false`)
- Accepts typed input parameters
- Returns typed output parameters
- Resets its own application state after completing (except on first-core-turn completion)
- Can be called multiple times by the orchestrator

---

## Planning the Orchestrator (`Orchestration:` prefix)

The orchestrator's steps describe the coordination logic — not the UI interactions (those belong to the Callables).

**Step shape:**
```
1. [Loop or iteration setup if calling Callables per row/record]   — primary/supporting
2. Call Callable A with input parameters                           — primary
3. Decision on Callable A result (if branching on outcome)        — supporting
4. Call Callable B with input parameters                           — primary
   ...
N. [Write results or finalize]                                     — primary
```

**Planning guidance:**
- Orchestrator steps are `MethodStep` calls using durable memory signatures from `get_callable_signatures()`.
- Map orchestrator-available values to Callable input parameters.
- The orchestrator does not directly interact with application controls — all UI work is in the Callables.
- `AutomationContext: "Orchestration"`
- `AutomationCategory: "core"` for turns that call Callables delivering business value.

---

## Planning a Callable (`Callable:` prefix)

A Callable is planned like a standard automation but with lifecycle rules applied. Load `references/planning-callable.md` §3 and §5 for the full callable lifecycle.

**Key differences from standard automations:**
- `AutomationContext: "Callable"` on every turn
- First turn emits `callableAutomation` to `DurableMemoryWrites`
- GoalCompleted gate follows the Callable path (N=0 vs. N>0 core turns)
- Reset turn restores application to landing screen after core completion
- Surface isolation: steps interact only with the current surface — no cross-surface calls

**Turn sequence summary:**

*Path A — core completes on first core turn (no reset needed):*
```
Turn 1+: Navigation    GoalCompleted=false, AutomationCategory="navigation"
Turn N:  Core          GoalCompleted=true,  AutomationCategory="core"
```

*Path B — core spans multiple turns or reset is needed:*
```
Turn 1+: Navigation    GoalCompleted=false, AutomationCategory="navigation"
Turn N:  Core          GoalCompleted=false, AutomationCategory="core"
                       + append reset sentinel to CompletedStepSummaries
Turn N+1: Reset        GoalCompleted=true,  AutomationCategory="core-reset"
```

---

## Reset Turn Planning

The reset turn is recognized by the presence of the reset sentinel in prior `CompletedStepSummaries`.

To determine reset steps:
1. Find the last `navigation:` entry in `# Completed Steps` — this identifies the callable's landing screen.
2. Identify what the core turn(s) changed (dialogs opened, fields populated, panels revealed).
3. Generate the minimal reversal steps to return to the landing screen.
4. Stop at the landing screen — do not navigate further.

Typical reset steps:
- Close dialogs opened by the core turn
- Clear search fields that were populated
- Navigate back one level to the landing screen (not to the menu or home)

---

## Durable Memory and Callable Signatures

When `get_callable_signatures()` returns existing automations that match required sub-tasks, prefer them over planning new Callables.

Emit `MethodStep` calls using the exact signature from durable memory:
- Project-scoped: `ParentObject: "Project"`, call as `MethodName.Run(...)`
- Application-scoped: `ParentObject: "<AppName>"`, add to `Includes`, call as `AppName.MethodName(...)`

---

## Common Variations

### Loop of Callable calls
When the orchestrator must call a Callable once per item in a list or table:
Use a loop pattern wrapping the Callable invocation.
The Callable handles one item per invocation — loop logic stays in the orchestrator.

### Conditional Callable invocation
When a Callable is only called under certain conditions, emit a decision step before the Callable invocation.
Branch on a value available to the orchestrator (input parameter, prior Callable result).

### Chained Callables
When Callable B requires output from Callable A:
Capture Callable A's output into a variable (ValueStep) after the call.
Pass that variable as an input parameter to Callable B.
