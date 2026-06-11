# Example: ForLoopStartStep

Iterates a fixed number of times. Requires three sub-arrays:
- `LoopingSteps` — executed on every iteration
- `LoopBreakSteps` — executed when the loop breaks via `MethodStep Break`; must end with `JumpToLabelStep`
- `LoopCompleteSteps` — executed when the counter reaches its limit naturally; must end with `JumpToLabelStep`

The break path and complete path typically route to different labels: break usually means the goal was found, complete usually means it was not.

```json
{
  "StepType": "ForLoopStartStep",
  "StepNumber": "10",
  "StepName": "Find target column header",
  "StepDescription": "Iterate from index 0 to columnCount to locate the target column. Break when the header matches; jump to Error if the loop finishes without a match.",
  "LoopName": "forLoop1",
  "Initial": "0",
  "Increment": "1",
  "Limit": "columnCount.Value",
  "LoopingSteps": [
    {
      "StepType": "ValueStep",
      "StepNumber": "10.1.1",
      "StepName": "Get current loop index",
      "StepDescription": "Read the current for loop index into a local variable for use in the header comparison.",
      "GetValueControl": "forLoop1",
      "GetValueProperty": "Index",
      "SetValueControl": "currentIndex",
      "SetValueProperty": "Value",
      "IsSensitiveData": false
    },
    {
      "StepType": "DecisionStep",
      "StepNumber": "10.1.2",
      "StepName": "Check if header matches target",
      "StepDescription": "Compare the column header at the current index to the target column name.",
      "Decision": "columnHeaders[currentIndex.Value].Equals(targetColumn.Value)",
      "DecisionOperator": "if/else",
      "Cases": {
        "true": [
          {
            "StepType": "ValueStep",
            "StepNumber": "10.1.2.1",
            "StepName": "Store found column index",
            "StepDescription": "Save the current loop index as the found column index for use after the loop.",
            "GetValueControl": "currentIndex",
            "GetValueProperty": "Value",
            "SetValueControl": "foundColumnIndex",
            "SetValueProperty": "Value",
            "IsSensitiveData": false
          },
          {
            "StepType": "MethodStep",
            "StepNumber": "10.1.2.2",
            "StepName": "Break column search loop",
            "StepDescription": "Break the for loop because the target column header has been located.",
            "ParentObject": "forLoop1",
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
      "StepNumber": "10.2.1",
      "StepName": "Jump to process column",
      "StepDescription": "Jump to ProcessColumn since the target column index has been found and stored.",
      "JumpToLabelName": "ProcessColumn",
      "InputParameters": {}
    }
  ],
  "LoopCompleteSteps": [
    {
      "StepType": "JumpToLabelStep",
      "StepNumber": "10.3.1",
      "StepName": "Jump to error column not found",
      "StepDescription": "Jump to Error because the loop completed all iterations without finding the target column.",
      "JumpToLabelName": "Error",
      "InputParameters": {
        "errMsg": "Target column header was not found in the header row"
      }
    }
  ]
}
```

---

