# Toolbox: UI Interaction Services

Covers clipboard, message boxes, ASO credential management, and user interaction utilities.

---

## Clipboard

`ParentObject: "Clipboard"` — static service.

| MethodName | Intent |
|---|---|
| `Copy` | copy a text string to the clipboard |
| `GetText` | get text from the clipboard |
| `SetText` | set text on the clipboard with optional format |
| `GetImage` | get an image from the clipboard, optionally saving to a file |
| `SetImage` | set an image on the clipboard from an image object or file path |
| `ContainsText` | check whether the clipboard contains text |
| `ContainsImage` | check whether the clipboard contains an image |
| `Clear` | clear all content from the clipboard |

---

## MessageBox

`ParentObject: "MessageBox"` — static service.

Use sparingly — these pause automation and require user interaction.

| MethodName | Intent |
|---|---|
| `MessageBox` | show a modal message dialog with configurable buttons and icon |
| `InputBox` | show a dialog prompting the user to enter a text value |

---

## MessageManifest

`ParentObject: "MessageManifest"` — static service.

Use when the project uses a configured message manifest for user-facing messages.

| MethodName | Intent |
|---|---|
| `GetMessage` | retrieve individual message properties by code from the manifest |
| `GetMessageDetails` | retrieve a MessageDetails object by code from the manifest |
| `ShowMessage` | display a message from the manifest by code or MessageDetails object |

---

## AsoManager

`ParentObject: "AsoManager"` — static service.

Use for credential retrieval and ASO store management. Retrieve credentials before authentication steps.

| MethodName | Intent |
|---|---|
| `GetCredentials` | retrieve domain, username, and password for an application key |
| `GetUserName` | get the username for an application key |
| `GetPassword` | get the password for an application key |
| `GetDomain` | get the domain for an application key |
| `GetEncryptedCredentials` | retrieve encrypted domain, username, and password |
| `GetEncryptedUserName` | get the encrypted username for an application key |
| `GetEncryptedPassword` | get the encrypted password for an application key |
| `GetEncryptedDomain` | get the encrypted domain for an application key |
| `ApplicationExists` | check whether an application key exists in the ASO store |
| `PerformLogin` | execute the login sequence for a registered application |
| `AddApplication` | register an application in the ASO store with credentials |
| `RemoveApplication` | remove an application from the ASO store |
| `SetCredentials` | set domain, username, and password for an application key |
| `SetUserName` | set the username for an application key |
| `SetPassword` | set the password for an application key |
| `SetDomain` | set the domain for an application key |
| `IsPasswordSet` | check whether a password has been set for an application key |
| `GetDatePasswordChanged` | get the date the password was last changed |
| `GetDaysSincePasswordChanged` | get the number of days since the password was last changed |
| `Encrypt` | encrypt a string value using ASO encryption |
| `Decrypt` | decrypt an ASO-encrypted string value |
| `GetAllApplications` | get a list of all application keys in the ASO store |
| `GetLoadedApplications` | get a list of currently loaded application keys |
| `ShowCredentialDialog` | show the credential management dialog to the user |
| `ShowCredentialDialogByCategory` | show the credential dialog filtered to a category |
| `ClearPasswords` | clear all stored passwords from the ASO store |
| `CreateAsoFile` | create the ASO file |
| `DeleteAsoFile` | delete the ASO file |
| `SaveAsoFile` | save changes to the ASO file |
| `GetLastSaveTime` | get the date and time the ASO file was last saved |
| `Initialize` | initialize the AsoManager with a sign-on service instance |

---

## Log — ParentObject: "Log"

Always `Tier: "supporting"`.

| MethodName | Intent |
|---|---|
| `LogError` | write an error-level log message |
| `LogWarning` | write a warning-level log message |
| `LogInfo` | write an informational log message |
| `LogVerbose` | write a verbose/debug log message |
| `Log` | write a message at an explicitly specified trace level |

---

## LogController — ParentObject: "LogController"

Use for logging configuration — enable, disable, or restore logging globally or per named log.

| MethodName | Intent |
|---|---|
| `TurnLoggingOn` | enable logging globally, at a level, or for a named log at a level |
| `TurnLoggingOff` | disable logging globally or for a named log |
| `RestoreLogging` | restore logging to its initial configuration |
| `GetLogNames` | get the names of all configured logs |
| `Enabled` | check whether logging is currently enabled (property) |

---

## StartMyDayControl

`ParentObject: "StartMyDayControl"` — static service.

Use for managing and launching startup application configurations.

| MethodName | Intent |
|---|---|
| `StartMyDay` | start all configured startup applications |
| `StartApplication` | start a single application by friendly name |
| `StartApplications` | start one or more applications by friendly name |
| `StartApplicationsList` | start applications from an existing string array |
| `GetStartupApplications` | get the ordered list of all configured startup applications |
| `AddExeApplication` | add an executable application to the Start My Day list |
| `AddWebApplication` | add a web application to the Start My Day list |
| `SetApplicationOrder` | set the order of an application in the startup list |
| `GetApplicationPositionAndSize` | get the current window position and size for a named application |
| `SetApplicationPositionAndSize` | set the window position and size for a named application |
| `OrganizeApplication` | restore the saved position and size for a named application window |
| `OrganizeDesktop` | restore saved positions and sizes for all startup application windows |
| `ShowDialog` | show the startup application management dialog |
| `ShowStartDialog` | show the interactive dialog for the user to select applications to start |
| `Initialize` | initialize StartMyDayControl with a service instance |
