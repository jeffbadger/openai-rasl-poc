# Example: ApplicationValueStep

Reference examples for `ApplicationValueStep`. These are illustrative — the schema in `references/output-contract.md` is authoritative.

`IsSensitiveData` must be `true` for passwords and other sensitive inputs.

Identity binding rule: `ControlName` and `ElementReferenceId` must come from the same resolved hierarchy node. Append the node resolution trace to `StepDescription`.

---

## Write from runtime input

The runtime value (e.g., an input parameter) is assigned at execution time. `StaticValue` is omitted.

```json
{
  "StepType": "ApplicationValueStep",
  "StepNumber": "2",
  "StepName": "Enter customer number",
  "StepDescription": "Type the input customer number into the customer number text field. [Node: txtCustomerNumber | ID: 14 | Match: AccessibilityName:Customer Number]",
  "ApplicationName": "CRMApp",
  "ApplicationId": "WindowsAdapter-5A3B2C",
  "ApplicationType": "WindowsApp",
  "UserActionId": 0,
  "ElementReferenceId": 14,
  "IsInterrogated": false,
  "ControlName": "txtCustomerNumber",
  "SuggestedName": "txtCustomerNumber",
  "IsSensitiveData": false
}
```

---

## Write a static literal

Sets a known value directly — e.g., selecting a tab index or setting a checkbox state.

```json
{
  "StepType": "ApplicationValueStep",
  "StepNumber": "2",
  "StepName": "Select 'Inventory' tab",
  "StepDescription": "Set the tab control to index 2 to display the Inventory tab. [Node: tabControl1 | ID: 16 | Match: ControlType:TabControl under mainPanel]",
  "ApplicationName": "CRMApp",
  "ApplicationId": "WindowsAdapter-5A3B2C",
  "ApplicationType": "WindowsApp",
  "UserActionId": 0,
  "ElementReferenceId": 16,
  "IsInterrogated": false,
  "ControlName": "tabControl1",
  "SuggestedName": "tabControl1",
  "PropertyName": "SelectedIndex",
  "StaticValue": "2",
  "IsSensitiveData": false
}
```

---

## Read

Reads the current value of a control. `StaticValue` is omitted.

```json
{
  "StepType": "ApplicationValueStep",
  "StepNumber": "3",
  "StepName": "Read account balance",
  "StepDescription": "Read the current balance from the account balance label. [Node: lblAccountBalance | ID: 17 | Match: AccessibilityName:Account Balance]",
  "ApplicationName": "CRMApp",
  "ApplicationId": "WindowsAdapter-5A3B2C",
  "ApplicationType": "WindowsApp",
  "UserActionId": 0,
  "ElementReferenceId": 17,
  "IsInterrogated": false,
  "ControlName": "lblAccountBalance",
  "SuggestedName": "lblAccountBalance",
  "IsSensitiveData": false
}
```
