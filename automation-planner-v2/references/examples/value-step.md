# Example: ValueStep

Reference examples for `ValueStep`. These are illustrative — the schema in `references/output-contract.md` is authoritative. If an example conflicts with the schema, follow the schema.

`StaticValue` and `GetValueControl` are mutually exclusive — never use both in the same step.

---

## Copy (source → destination)

Transfers a value from a source control or variable to a destination.

```json
{
  "StepType": "ValueStep",
  "StepNumber": "1",
  "StepName": "Store account balance",
  "StepDescription": "Copy the balance value read from the application into a local variable for use in a downstream decision.",
  "GetValueControl": "lblAccountBalance",
  "GetValueProperty": "Value",
  "SetValueControl": "localBalance",
  "SetValueProperty": "Value",
  "IsSensitiveData": false
}
```

---

## Static assign (literal → destination)

Writes a known literal directly into a variable or output parameter.

```json
{
  "StepType": "ValueStep",
  "StepNumber": "1",
  "StepName": "Set result to true",
  "StepDescription": "Set the Result output parameter to true to indicate successful completion.",
  "SetValueControl": "Run.Result",
  "SetValueProperty": "Value",
  "StaticValue": "true",
  "IsSensitiveData": false
}
```
