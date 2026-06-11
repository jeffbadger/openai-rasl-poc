# Example: DecisionStep

Reference examples for `DecisionStep` and `ApplicationDecisionStep`. These are illustrative — the schema in `references/output-contract.md` is authoritative.

See `references/output-contract.md` for operator selection rules. The key distinction: use `"decision"` for boolean method results (`.Result` fields), use `"if/else"` for static literal comparisons, use `"switch"` / `"stringSwitch"` for multi-way branching.

---

## if/else — static literal comparison

Both `"true"` and `"false"` keys are required. When one branch requires no steps, emit that key with `[]`. `DefaultCaseSteps` is prohibited on `if/else`.

```json
{
  "StepType": "DecisionStep",
  "StepNumber": "4",
  "StepName": "Check if record is active",
  "StepDescription": "Branch based on whether the active flag local variable is true.",
  "Decision": "isActive.Value == true",
  "DecisionOperator": "if/else",
  "Cases": {
    "true": [
      {
        "StepType": "ValueStep",
        "StepNumber": "4.1",
        "StepName": "Set result to active",
        "StepDescription": "Set the output parameter result to indicate the record is active.",
        "SetValueControl": "Run.result",
        "SetValueProperty": "Value",
        "StaticValue": "true",
        "IsSensitiveData": false
      }
    ],
    "false": [
      {
        "StepType": "JumpToLabelStep",
        "StepNumber": "4.2",
        "StepName": "Jump to error",
        "StepDescription": "Jump to the Error label because the record is not active.",
        "JumpToLabelName": "Error",
        "InputParameters": {
          "errMsg": "Record is not active"
        }
      }
    ]
  }
}
```

---

## decision — boolean method result

Use `"decision"` when branching on a `.Result` field from a toolbox method, automation call, or component method. Do not use `"if/else"` for these even when the intent is phrased as `x.Result == true`.

The `true` branch inside a doWhile `WhileSteps` MoveNext decision is intentionally empty — the loop continues automatically.

```json
{
  "StepType": "DecisionStep",
  "StepNumber": "5",
  "StepName": "Branch on MoveNext result",
  "StepDescription": "If MoveNext returned false there are no more rows — break the loop.",
  "Decision": "dtData.MoveNext().Result",
  "DecisionOperator": "decision",
  "Cases": {
    "true": [],
    "false": [
      {
        "StepType": "MethodStep",
        "StepNumber": "5.1",
        "StepName": "Break data loop",
        "StepDescription": "No more rows — break the loop to exit through LoopCompleteSteps.",
        "ParentObject": "doWhileLoop1",
        "MethodName": "Break"
      }
    ]
  }
}
```

---

## stringSwitch — partial string match with default

`DefaultCaseSteps` is valid (and often needed) for `switch` and `stringSwitch`. It handles unmatched values.

```json
{
  "StepType": "DecisionStep",
  "StepNumber": "6",
  "StepName": "Route by order status",
  "StepDescription": "Route execution based on the order status string. Unrecognized statuses fall through to the default case.",
  "Decision": "orderStatus",
  "DecisionOperator": "stringSwitch",
  "Cases": {
    "Approved": [
      {
        "StepType": "JumpToLabelStep",
        "StepNumber": "6.1",
        "StepName": "Jump to process approved",
        "StepDescription": "Jump to ProcessApproved for approved orders.",
        "JumpToLabelName": "ProcessApproved",
        "InputParameters": {}
      }
    ],
    "Rejected": [
      {
        "StepType": "JumpToLabelStep",
        "StepNumber": "6.2",
        "StepName": "Jump to process rejected",
        "StepDescription": "Jump to ProcessRejected for rejected orders.",
        "JumpToLabelName": "ProcessRejected",
        "InputParameters": {}
      }
    ]
  },
  "DefaultCaseSteps": [
    {
      "StepType": "JumpToLabelStep",
      "StepNumber": "6.3",
      "StepName": "Jump to error on unknown status",
      "StepDescription": "Jump to Error for any unrecognized order status.",
      "JumpToLabelName": "Error",
      "InputParameters": {
        "errMsg": "Unrecognized order status"
      }
    }
  ]
}
```

---

## switch — numeric value

Case keys are numeric values expressed as strings. `DefaultCaseSteps` handles unmatched numeric values.

```json
{
  "StepType": "DecisionStep",
  "StepNumber": "7",
  "StepName": "Set rate by priority level",
  "StepDescription": "Look up the processing rate for the given priority level. Known levels copy a rate from a source variable; unmatched levels fall through to a static default rate.",
  "Decision": "priorityLevel",
  "DecisionOperator": "switch",
  "Cases": {
    "1": [
      {
        "StepType": "ValueStep",
        "StepNumber": "7.1",
        "StepName": "Copy high priority rate",
        "StepDescription": "Copy the high priority processing rate into the output parameter.",
        "GetValueControl": "highPriorityRate",
        "GetValueProperty": "Value",
        "SetValueControl": "Run.processingRate",
        "SetValueProperty": "Value",
        "IsSensitiveData": false
      }
    ],
    "2": [
      {
        "StepType": "ValueStep",
        "StepNumber": "7.2",
        "StepName": "Copy medium priority rate",
        "StepDescription": "Copy the medium priority processing rate into the output parameter.",
        "GetValueControl": "medPriorityRate",
        "GetValueProperty": "Value",
        "SetValueControl": "Run.processingRate",
        "SetValueProperty": "Value",
        "IsSensitiveData": false
      }
    ]
  },
  "DefaultCaseSteps": [
    {
      "StepType": "ValueStep",
      "StepNumber": "7.3",
      "StepName": "Set default processing rate",
      "StepDescription": "Assign the standard fallback processing rate for all other priority levels.",
      "SetValueControl": "Run.processingRate",
      "SetValueProperty": "Value",
      "StaticValue": "0.05",
      "IsSensitiveData": false
    }
  ]
}
```
