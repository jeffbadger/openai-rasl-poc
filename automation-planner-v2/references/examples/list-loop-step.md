# Example: ListLoopStartStep

Iterates over a list. Access the current item each iteration via a `ValueStep` reading `CurrentItem` from the loop. Supports `LoopBreakSteps` when the loop may exit via Break. `LoopCompleteSteps` is always required.

```json
{
  "StepType": "ListLoopStartStep",
  "StepNumber": "11",
  "StepName": "Find matching account in list",
  "StepDescription": "Iterate over the list of account numbers to locate the one matching the input account. Break when found; jump to Error if the list is exhausted without a match.",
  "LoopName": "listLoop1",
  "List": "accountNumbers.Value",
  "LoopingSteps": [
    {
      "StepType": "ValueStep",
      "StepNumber": "11.1.1",
      "StepName": "Get current list item",
      "StepDescription": "Retrieve the current item from the list loop into a local variable for comparison.",
      "GetValueControl": "listLoop1",
      "GetValueProperty": "CurrentItem",
      "SetValueControl": "currentAccount",
      "SetValueProperty": "Value",
      "IsSensitiveData": false
    },
    {
      "StepType": "DecisionStep",
      "StepNumber": "11.1.2",
      "StepName": "Check if account matches input",
      "StepDescription": "Compare the current list item to the input account number.",
      "Decision": "currentAccount.Value.Equals(Run.inputAccount)",
      "DecisionOperator": "if/else",
      "Cases": {
        "true": [
          {
            "StepType": "MethodStep",
            "StepNumber": "11.1.2.1",
            "StepName": "Break account search loop",
            "StepDescription": "Break the list loop because the matching account number has been found.",
            "ParentObject": "listLoop1",
            "MethodName": "Break"
          }
        ],
        "false": []
      }
    }
  ],
  "LoopBreakSteps": [
    {
      "StepType": "JumpToLabelStep",
      "StepNumber": "11.2.1",
      "StepName": "Jump to process account",
      "StepDescription": "Jump to ProcessAccount since the matching account number has been found.",
      "JumpToLabelName": "ProcessAccount",
      "InputParameters": {}
    }
  ],
  "LoopCompleteSteps": [
    {
      "StepType": "JumpToLabelStep",
      "StepNumber": "11.3.1",
      "StepName": "Jump to error account not found",
      "StepDescription": "Jump to Error because the list was exhausted without finding a matching account number.",
      "JumpToLabelName": "Error",
      "InputParameters": {
        "errMsg": "Matching account number was not found in the account list"
      }
    }
  ]
}
```

---

