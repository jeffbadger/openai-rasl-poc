# Example: LabelStep and JumpToLabelStep

Reference examples for `LabelStep` and `JumpToLabelStep`. These are illustrative — the schema in `references/output-contract.md` is authoritative.

A `LabelStep` is only ever reached by a `JumpToLabelStep` — never by normal fall-through. Every `JumpToLabelStep` must target an existing `LabelStep` in the same automation.

---

## LabelStep

Defines a named execution destination with typed `InputParameters` and an inline `LabelSteps` list.

```json
{
  "StepType": "LabelStep",
  "StepNumber": "8",
  "StepName": "Error handler",
  "StepDescription": "Label reached when an error occurs. Assigns the error message to the output parameter and sets Result to false.",
  "LabelName": "Error",
  "InputParameters": {
    "errMsg": "System.String"
  },
  "LabelSteps": [
    {
      "StepType": "ValueStep",
      "StepNumber": "8.1",
      "StepName": "Set error message output",
      "StepDescription": "Copy the label's errMsg input parameter into the errMessage output parameter.",
      "GetValueControl": "Error[errMsg]",
      "GetValueProperty": "Value",
      "SetValueControl": "Run.errMessage",
      "SetValueProperty": "Value",
      "IsSensitiveData": false
    },
    {
      "StepType": "ValueStep",
      "StepNumber": "8.2",
      "StepName": "Set result to false",
      "StepDescription": "Set the Result output parameter to false to signal failure to the caller.",
      "SetValueControl": "Run.Result",
      "SetValueProperty": "Value",
      "StaticValue": "false",
      "IsSensitiveData": false
    }
  ]
}
```

---

## JumpToLabelStep

Passes values through `InputParameters` keyed by the target label's declared parameter names. When no parameters are required, pass an empty object.

```json
{
  "StepType": "JumpToLabelStep",
  "StepNumber": "9",
  "StepName": "Jump to error on timeout",
  "StepDescription": "Transfer execution to the Error label with a descriptive timeout message.",
  "JumpToLabelName": "Error",
  "InputParameters": {
    "errMsg": "Operation timed out waiting for the control to appear"
  }
}
```

---

## JumpToLabelStep — no parameters

```json
{
  "StepType": "JumpToLabelStep",
  "StepNumber": "5",
  "StepName": "Jump to process account",
  "StepDescription": "Jump to ProcessAccount since the matching account has been located.",
  "JumpToLabelName": "ProcessAccount",
  "InputParameters": {}
}
```
