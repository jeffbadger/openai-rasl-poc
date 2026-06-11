# Example: WhileLoopStartStep

Identical shape to DoWhileLoopStartStep. The behavioral difference: While checks the condition before the first iteration; DoWhile checks after. Use When one guaranteed execution is not required before the condition is meaningful.

```json
{
  "StepType": "WhileLoopStartStep",
  "StepNumber": "13",
  "StepName": "Poll for record to appear",
  "StepDescription": "Poll the results panel until at least one row is present or the condition variable is set to exit.",
  "LoopName": "whileLoop1",
  "LoopConditionVariable": "recordNotFound",
  "InitialValue": true,
  "DoSteps": [
    {
      "StepType": "MethodStep",
      "StepNumber": "13.1.1",
      "StepName": "Pause before next check",
      "StepDescription": "Brief pause before re-checking the results panel for the expected row.",
      "ParentObject": "Pause",
      "MethodName": "Sleep"
    }
  ],
  "WhileSteps": [
    {
      "StepType": "DecisionStep",
      "StepNumber": "13.2.1",
      "StepName": "Check if results row is present",
      "StepDescription": "If the results panel row count is greater than zero, set recordNotFound to false to stop the loop.",
      "Decision": "resultsGrid.RowCount.Value > 0",
      "DecisionOperator": "if/else",
      "Cases": {
        "true": [
          {
            "StepType": "ValueStep",
            "StepNumber": "13.2.1.1",
            "StepName": "Mark record as found",
            "StepDescription": "Set recordNotFound to false to prevent the loop from repeating.",
            "SetValueControl": "recordNotFound",
            "SetValueProperty": "Value",
            "StaticValue": "false",
            "IsSensitiveData": false
          }
        ],
        "false": []
      }
    }
  ],
  "LoopCompleteSteps": [
    {
      "StepType": "JumpToLabelStep",
      "StepNumber": "13.3.1",
      "StepName": "Jump to select record",
      "StepDescription": "Jump to SelectRecord now that the expected result row is visible.",
      "JumpToLabelName": "SelectRecord",
      "InputParameters": {}
    }
  ]
}
```

