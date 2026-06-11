# Example: TodoStep

Reference example for `TodoStep`. These are illustrative — the schema in `references/output-contract.md` is authoritative.

`TodoStep` is a last-resort placeholder. Apply the full todo gating rule from `references/planning-core.md` §7 before emitting one. Check `AllowTodoSteps` in Planning Controls first.

---

## TodoStep

`StepDescription` must describe what needs to be accomplished — written as an instruction to a developer. Do not explain why automation failed, which controls were examined, or what matching was attempted.

`TodoDescription` is the developer-facing description of the intended action or outcome at a business level.

```json
{
  "StepType": "TodoStep",
  "StepNumber": "14",
  "StepName": "Select matching grid row",
  "StepDescription": "Select the row in the results grid whose account number matches the input account number.",
  "TodoDescription": "Select the row in the results grid whose account number matches the input account number.",
  "IsComplex": false
}
```

---

## What makes a valid TodoStep description

✅ Describes the intended action at a business level: `"Select the row in the results grid whose account number matches the input account number."`

❌ Explains why automation failed: `"Semantic matching was attempted on Name, AutomationId, ClassName. No DataGrid control type was found in the hierarchy."`

❌ References controls not in the current hierarchy: `"After navigating to the results screen, select the matching row."`

❌ Generic and uninstructive: `"Control not found."`
