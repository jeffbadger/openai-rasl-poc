# Example: ApplicationMethodStep

Reference examples for `ApplicationMethodStep`. These are illustrative — the schema in `references/output-contract.md` is authoritative.

Used to invoke a method on a UI control: clicking a button, clicking a radio button, selecting a menu item. For radio buttons, always pair with a decision step that reads the current `Checked` state first and skips the click if already selected.

Identity binding rule: `ControlName` and `ElementReferenceId` must come from the same resolved hierarchy node. Append the node resolution trace to `StepDescription`.

---

## Button click

```json
{
  "StepType": "ApplicationMethodStep",
  "StepNumber": "3",
  "StepName": "Click search button",
  "StepDescription": "Click the search button to execute the customer lookup. [Node: btnSearch | ID: 15 | Match: AccessibilityName:Search]",
  "ApplicationName": "CRMApp",
  "ApplicationId": "WindowsAdapter-5A3B2C",
  "ApplicationType": "WindowsApp",
  "UserActionId": 0,
  "ElementReferenceId": 15,
  "IsInterrogated": false,
  "ControlName": "btnSearch",
  "SuggestedName": "btnSearch",
  "MethodName": "PerformClick"
}
```
