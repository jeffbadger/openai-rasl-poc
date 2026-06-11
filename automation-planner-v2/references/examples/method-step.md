# Example: MethodStep

Reference examples for `MethodStep`. These are illustrative — the schema in `references/output-contract.md` is authoritative.

`ParentObject` is the exact service name from the loaded toolbox catalog, or `"Project"` for project-scoped automations, or the application name for application-scoped automations. Do not use generic verbs as `MethodName` when a grounded catalog name is available.

---

## Toolbox service call

```json
{
  "StepType": "MethodStep",
  "StepNumber": "2",
  "StepName": "Get current date and time",
  "StepDescription": "Call the DateTime service to retrieve the current system date and time.",
  "ParentObject": "DateTime",
  "MethodName": "Now"
}
```

---

## Loop control — Break

When breaking a ForLoop or ListLoop, `ParentObject` is the loop's `LoopName` and `MethodName` is `Break`.

```json
{
  "StepType": "MethodStep",
  "StepNumber": "3",
  "StepName": "Break column search loop",
  "StepDescription": "Break the for loop because the target column header has been found.",
  "ParentObject": "forLoop1",
  "MethodName": "Break"
}
```

---

## Project-scoped automation call

For automations from durable memory where `isApplicationMethod` is false or absent.

```json
{
  "StepType": "MethodStep",
  "StepNumber": "4",
  "StepName": "Look up customer record",
  "StepDescription": "Call the LookupCustomer automation to retrieve the customer record for the input account number.",
  "ParentObject": "Project",
  "MethodName": "LookupCustomer"
}
```
