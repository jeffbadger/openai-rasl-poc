# Example: DoWhileLoopStartStep — PegaTable / DataTable row iteration

Key distinction from ForLoop and ListLoop: uses `DoSteps` (repeating work) and `WhileSteps` (condition re-evaluation after each iteration). `LoopCompleteSteps` is required. Break exits immediately through `LoopCompleteSteps` — there is no separate `LoopBreakSteps`.

Call `MoveFirst` before the loop and guard against an empty table. The `true` branch of the MoveNext decision in `WhileSteps` is intentionally empty — the loop continues automatically.

```json
[
  {
    "StepType": "MethodStep",
    "StepNumber": "2",
    "StepName": "Move to first row",
    "StepDescription": "Position the DataTable cursor at the first row before entering the loop.",
    "ParentObject": "dtData",
    "MethodName": "MoveFirst"
  },
  {
    "StepType": "DecisionStep",
    "StepNumber": "2.1",
    "StepName": "Check MoveFirst result",
    "StepDescription": "If MoveFirst returned false the table is empty — jump directly to Done, skipping the loop entirely.",
    "Decision": "dtData.MoveFirst().Result",
    "DecisionOperator": "if/else",
    "Cases": {
      "true": [],
      "false": [
        {
          "StepType": "JumpToLabelStep",
          "StepNumber": "2.1.1",
          "StepName": "Jump to done — empty table",
          "StepDescription": "Table is empty — jump to Done, matching the LoopCompleteSteps destination.",
          "JumpToLabelName": "Done",
          "InputParameters": {}
        }
      ]
    }
  },
  {
    "StepType": "DoWhileLoopStartStep",
    "StepNumber": "3",
    "StepName": "Loop DataTable rows",
    "StepDescription": "Iterate over each row in the input DataTable. DoSteps perform per-row work; WhileSteps advance the cursor and break when no more rows remain.",
    "LoopName": "doWhileLoop1",
    "LoopConditionVariable": "hasMoreRows",
    "InitialValue": true,
    "DoSteps": [
      {
        "StepType": "MethodStep",
        "StepNumber": "3.1.1",
        "StepName": "Read account number",
        "StepDescription": "Read the AccountNumber column value from the current DataTable row.",
        "ParentObject": "dtData",
        "MethodName": "GetCellStringValue"
      }
    ],
    "WhileSteps": [
      {
        "StepType": "MethodStep",
        "StepNumber": "3.2.1",
        "StepName": "Move to next row",
        "StepDescription": "Advance the DataTable cursor to the next row.",
        "ParentObject": "dtData",
        "MethodName": "MoveNext"
      },
      {
        "StepType": "DecisionStep",
        "StepNumber": "3.2.2",
        "StepName": "Check MoveNext result",
        "StepDescription": "If MoveNext returned false there are no more rows — break the loop to exit through LoopCompleteSteps.",
        "Decision": "dtData.MoveNext().Result",
        "DecisionOperator": "if/else",
        "Cases": {
          "true": [],
          "false": [
            {
              "StepType": "MethodStep",
              "StepNumber": "3.2.2.1",
              "StepName": "Break loop",
              "StepDescription": "No more rows — break the loop. Exits immediately through LoopCompleteSteps.",
              "ParentObject": "doWhileLoop1",
              "MethodName": "Break"
            }
          ]
        }
      }
    ],
    "LoopCompleteSteps": [
      {
        "StepType": "JumpToLabelStep",
        "StepNumber": "3.3.1",
        "StepName": "Jump to done",
        "StepDescription": "All rows processed — jump to Done label.",
        "JumpToLabelName": "Done",
        "InputParameters": {}
      }
    ]
  }
]
```

---

