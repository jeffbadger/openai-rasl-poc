# Toolbox: System Services

Covers environment, system information, process management, screen information, conversion, and runtime.

---

## Environment — ParentObject: "Environment"

High-level environment utility service.

| MethodName | Intent |
|---|---|
| `GetComputerName` | get the name of the current computer |
| `GetEnvironmentFolder` | get the path to a Windows special folder |
| `GetEnvironmentVariable` | get the value of an environment variable |

---

## System.Environment — ParentObject: "System.Environment"

BCL static environment class. Use for variable expansion, setting variables, and system properties.

| MethodName | Intent |
|---|---|
| `ExpandEnvironmentVariables` | replace %VAR% tokens in a string with their resolved values |
| `GetEnvironmentVariables` | retrieve all environment variable names and their values |
| `SetEnvironmentVariable` | create, modify, or delete an environment variable |
| `Is64BitOperatingSystem` | check whether the operating system is 64-bit (property) |
| `NewLine` | get the platform newline string (property) |
| `TickCount` | get the number of milliseconds elapsed since the system started (property) |

---

## SystemInformation

`ParentObject: "SystemInformation"` — static service.

Use for machine name, OS details, memory, and drive information.

| MethodName | Intent |
|---|---|
| `GetDeviceName` | get the machine hostname |
| `GetOSName` | get the OS display name e.g. "Windows 10" |
| `GetOSBuild` | get the OS build and patch string |
| `GetOSDetails` | get a key-value map of OS properties |
| `Is64BitOS` | check whether the OS is 64-bit |
| `LocalIpAddress` | get the local IPv4 address |
| `GetProgramFilesLocation` | get the path to the Program Files directory |
| `GetLastBootUpTime` | get the date and time the machine last booted |
| `GetTotalPhysicalMemory` | get total physical RAM in bytes |
| `GetAvailablePhysicalMemory` | get available physical RAM in bytes |
| `GetTotalVirtualMemory` | get total virtual memory in bytes |
| `GetAvailableVirtualMemory` | get available virtual memory in bytes |
| `GetDriveInformation` | get drive name, total space, and available space |
| `GetSystemInformation` | get a full system snapshot |

---

## Process

`ParentObject: "Process"` — static service.

Use to start, stop, and inspect running processes.

| MethodName | Intent |
|---|---|
| `StartProcess` | start a process from a file name, arguments, credentials, or ProcessStartInfo |
| `KillProcess` | stop a process by ID or stop all instances by name |
| `IsProcessRunning` | check whether a process with a given name is currently running |
| `GetProcesses` | get all running processes, optionally on a named machine |
| `GetProcessesByName` | get all processes with a given name, optionally on a named machine |
| `GetProcessById` | get a process by its ID |
| `GetCurrentProcess` | get the currently executing process |
| `GetProcessOwner` | get the owner username of a process by ID |
| `GetProcessRunningTime` | get how long a named process has been running as a TimeSpan |
| `CreateProcessStartInfo` | build a ProcessStartInfo object |
| `OpenInFileExplorer` | open a path in File Explorer, optionally selecting the item |

---

## ProductInfo

`ParentObject: "ProductInfo"` — static service.

Use to check installed Pega product versions. All methods return a Version. Returns 0.0.0.0 if not installed.

| MethodName | Intent |
|---|---|
| `GetPNFVersion` | get the installed Pega Native Foundation version |
| `GetPBEVersion` | get the installed Pega Browser Extension version |
| `GetRPAServiceVersion` | get the installed RPA service version |
| `GetSyncEngineVersion` | get the installed sync engine / updater version |
| `GetOCRVersion` | get the installed OCR Essentials version |
| `GetChromeVersion` | get the installed Google Chrome version |
| `GetEdgeVersion` | get the installed Microsoft Edge version |

---

## Runtime

`ParentObject: "Runtime"` — static service.

| MethodName | Intent |
|---|---|
| `GetProjectName` | get the current project name |
| `GetProjectId` | get the current project ID |
| `GetProjectPath` | get the current project path |
| `GetRuntimeVersion` | get the Pega runtime version |
| `GetDeploymentVersion` | get the package deployment version |
| `ListPackageMiscFiles` | list miscellaneous deployment files matching a pattern |
| `ReadPackageFile` | read the contents of a package file |
| `SavePackageFile` | save a package file to a disk location |
| `TerminateRuntime` | terminate the Pega runtime |

---

## ScreenInformation

`ParentObject: "ScreenInformation"` — static service.

| MethodName | Intent |
|---|---|
| `GetScreenCount` | get the total number of connected displays |
| `GetPrimaryScreen` | get the primary display |
| `GetAllScreens` | get an array of all connected displays |
| `GetBounds` | get the full bounds rectangle of a display |
| `GetScreenWorkingArea` | get the working area rectangle (excludes taskbar) |
| `GetScreenResolution` | get the width and height of a display |
| `GetVirtualScreenResolution` | get the combined width and height of all displays |
| `IsPrimary` | check whether a display is the primary screen |
| `GetScreenInformation` | get device name, bounds, working area, and primary flag |

---

## CurrentUser

`ParentObject: "CurrentUser"` — static service.

| MethodName | Intent |
|---|---|
| `Name` | get the current user's account name (property) |
| `UserDomain` | get the domain of the current user (property) |
| `IsInRole` | check whether the current user belongs to a named or built-in role |
| `IsAuthenticated` | check whether the current user is authenticated (property) |
| `IsAnonymous` | check whether the current user is anonymous (property) |
| `IsGuest` | check whether the current user is a guest account (property) |
| `IsSystem` | check whether the current user is the system account (property) |
| `AuthenticationType` | get the authentication type of the current user (property) |

---

## Pause

`ParentObject: "Pause"` — static service.

| MethodName | Intent |
|---|---|
| `Sleep` | pause automation execution for a given number of milliseconds |

---

## System.Convert

`ParentObject: "System.Convert"` — static service.

| MethodName | Intent |
|---|---|
| `ToBoolean` | convert a value to Boolean |
| `ToInt32` | convert a value to Int32 |
| `ToDouble` | convert a value to Double |
| `ToDecimal` | convert a value to Decimal |
| `ToDateTime` | convert a value to DateTime |
| `ToString` | convert a value to String |
| `ToChar` | convert a value to a Unicode character |
| `ChangeType` | convert a value to the type specified by a TypeCode enum value |
| `ToBase64String` | encode a byte array to a base-64 string |
| `FromBase64String` | decode a base-64 string to a byte array |
| `ToBase64CharArray` | encode a byte array subset to a base-64 char array |
| `FromBase64CharArray` | decode a base-64 char array subset to a byte array |

---

## System.GC

`ParentObject: "System.GC"` — use rarely, only when explicit memory management is required.

| MethodName | Intent |
|---|---|
| `Collect` | force garbage collection for all or a specified generation |
| `WaitForFullGCApproach` | get status indicating whether a full blocking GC is imminent |
| `WaitForFullGCComplete` | get status indicating whether a full blocking GC has completed |
