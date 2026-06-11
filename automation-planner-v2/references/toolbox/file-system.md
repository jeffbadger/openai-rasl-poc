# Toolbox: File System

Covers file operations, directory operations, zip archives, and path utilities.

---

## File — ParentObject: "File"

High-level file utility service. Use for file copy, move, delete, existence checks, and path utilities.

| MethodName | Intent |
|---|---|
| `CopyFile` | copy a file to a destination path |
| `MoveFile` | move a file to a new location |
| `DeleteFile` | delete a file, optionally sending to recycle bin |
| `CreateFile` | create a new empty file at a path |
| `FileExists` | check whether a file exists at a path |
| `IsFileReadOnly` | check whether a file is read-only |
| `GetFileInfo` | get file metadata including created and modified dates |
| `TouchFile` | update the timestamp of a file to a given time |
| `CombinePaths` | combine multiple path segments into one path |
| `GetDirectoryName` | get the directory portion of a file path |
| `GetFileExtension` | get the file extension from a path |
| `GetFileName` | get the file name including extension from a path |
| `GetFileNameWithoutExtension` | get the file name without its extension |
| `GetFullPath` | resolve a path to its full absolute form |
| `FindInFiles` | search for text within files in a directory |
| `CopyDirectory` | copy a directory and its contents to a destination |

---

## System.IO.File — ParentObject: "System.IO.File"

Lower-level BCL file read/write operations. Use when you need raw text, line, or byte access to files.

| MethodName | Intent |
|---|---|
| `ReadAllText` | read all text content from a file into a string |
| `ReadAllLines` | read all lines from a text file into a string array |
| `ReadAllBytes` | read all bytes from a file into a byte array |
| `ReadLines` | read lines from a file as an enumerable sequence |
| `WriteAllText` | write text to a file, overwriting existing content |
| `WriteAllLines` | write an array of lines to a file |
| `WriteAllBytes` | write a byte array to a file |
| `AppendAllText` | append text to a file, creating it if it does not exist |
| `AppendAllLines` | append an array of lines to a file, creating it if it does not exist |
| `AppendText` | open a StreamWriter that appends to a file |
| `GetAttributes` | get the FileAttributes of a file |
| `SetAttributes` | set the FileAttributes of a file |
| `Encrypt` | encrypt a file so only the current account can decrypt it |
| `Decrypt` | decrypt a file encrypted by the current account |
| `Replace` | replace a file's contents with another file, creating a backup |

---

## Directory

`ParentObject: "Directory"` — static service.

Use for directory creation, deletion, listing, and navigation.

| MethodName | Intent |
|---|---|
| `CreateDirectory` | create a new directory at a path |
| `DeleteDirectory` | delete a directory, optionally recycling contents |
| `DirectoryExists` | check whether a directory exists at a path |
| `MoveDirectory` | move a directory to a new location |
| `RenameDirectory` | rename a directory |
| `CopyDirectory` | copy a directory and its contents to a destination |
| `GetFiles` | list files within a directory |
| `GetFilesInDirectory` | list files in a directory with optional pattern filtering |
| `GetDirectoriesInDirectory` | list subdirectories within a directory |
| `GetParentDirectory` | get the parent directory of a path |
| `IsFileUnderDirectory` | check whether a file path is under a given directory |

---

## Zip

`ParentObject: "Zip"` — static service (System.IO.Compression.ZipFile).

| MethodName | Intent |
|---|---|
| `CreateFromDirectory` | create a ZIP archive from the contents of a directory |
| `ExtractToDirectory` | extract all files from a ZIP archive to a directory |
| `Open` | open a ZIP archive at a path in a specified mode (Read/Create/Update) |
| `OpenRead` | open a ZIP archive for reading |
