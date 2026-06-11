# Toolbox: ExcelConnector

Component instance — `ParentObject` = the connector instance name in its global container. Not a static service.

Use for all Excel workbook operations: opening files, reading/writing cells, managing sheets, iterating ranges, and importing/exporting PegaTables.

See `references/surfaces/excel.md` for surface-level planning guidance.

---

## Workbook Operations

| MethodName | Intent |
|---|---|
| `Open` | open an existing workbook file |
| `NewWorkbook` | create a new workbook file on disk |
| `Close` | close the workbook and clear all connector state |
| `Save` | save changes to the current workbook |
| `SaveToPDF` | save the workbook or selected sheets as a PDF file |
| `Reload` | reload workbook data from disk |
| `FileName` | workbook file path (property) |
| `Password` | workbook password — sensitive data (property) |
| `SaveAsName` | output file name for save operations (property) |
| `StringFormatting` | string formatting mode for cell values (property) |

---

## Sheet Management

| MethodName | Intent |
|---|---|
| `AddSheet` | add a new sheet to the workbook |
| `DeleteSheet` | delete a named sheet |
| `RenameSheet` | rename a sheet by current name or index |
| `InsertSheet` | insert a new sheet at a specific position |
| `HideSheet` | hide a named sheet |
| `UnhideSheet` | unhide a named sheet |
| `SheetExists` | check whether a named sheet exists |
| `IsSheetHidden` | check whether a named sheet is hidden |
| `GetAllSheets` | get a list of all sheet names |
| `GetSheetCount` | get the number of sheets |
| `GetSheetName` | get the sheet name at a given index |
| `Calculate` | recalculate formulas on the current or named sheet |
| `SheetName` | active sheet name; set to activate a sheet (property) |

---

## Cell Read / Write

| MethodName | Intent |
|---|---|
| `GetCellStringValue` | read a cell's raw string value |
| `GetCellFormattedValue` | read a cell's displayed formatted value as a string |
| `GetCellValue` | read a cell's value as an object |
| `SetCellStringValue` | write a string value to a cell |
| `SetCellValue` | write an object value to a cell |
| `GetCellFormula` | get the formula string from a cell |
| `SetCellFormula` | set a formula string on a cell |

---

## Range Boundaries

| MethodName | Intent |
|---|---|
| `GetLastRow` | find the index of the last row containing data or formatting |
| `GetFirstRow` | find the index of the first row containing data or formatting |
| `GetLastColumn` | find the index of the last column containing data or formatting |
| `GetFirstColumn` | find the index of the first column containing data or formatting |
| `GetLastCellAddress` | find the address of the last cell containing data or formatting |
| `GetFirstCellAddress` | find the address of the first cell containing data or formatting |
| `GetUsedCellRange` | returns the used cell range for the current sheet |
| `GetUsedCellAddresses` | returns the start and end addresses of the used cell range |
| `GetUsedCellIndexes` | returns the row and column indexes of the used cell range |

---

## Row and Column Operations

| MethodName | Intent |
|---|---|
| `AddRow` | insert a new row at a given index |
| `AddColumn` | insert a new column at a given index |
| `DeleteRows` | delete a range of rows |
| `DeleteColumns` | delete a range of columns |
| `ClearRow` | clear content and optionally formatting from a row |
| `ClearColumn` | clear content and optionally formatting from a column |
| `ClearCells` | clear content and optionally formatting from a cell range |
| `CopyRows` | copy rows to another sheet |
| `CopyColumns` | copy columns to another sheet |
| `CopyCells` | copy a cell range to another sheet |
| `MoveRows` | move rows to another sheet |
| `MoveColumns` | move columns to another sheet |
| `Sort` | sort a cell range by one or more columns |

---

## Search and Find

| MethodName | Intent |
|---|---|
| `FindCellAddress` | find the first cell matching a value; returns address or row/col |
| `FindCellAddresses` | find all cells matching a value; returns array of addresses |
| `FindRows` | find all rows containing a value; returns a DataTable |
| `FindRowAddresses` | find all rows containing a value; returns row index array |
| `FindColumns` | find all columns containing a value; returns a DataTable |
| `FindColumnAddresses` | find all columns containing a value; returns column index array |
| `FindRowsWithFilters` | find rows matching one or more filter conditions |
| `CreateRowFilter` | create a filter condition for use with FindRowsWithFilters or ExportToPegaTableWithFilters |

`FindCellAddress` has ambiguous overloads — `@default=address` variant used unless context requires row/col return.

---

## PegaTable Integration

| MethodName | Intent |
|---|---|
| `ExportToPegaTable` | export a range to a PegaTable for iteration or cross-automation transfer |
| `ExportToPegaTableWithFilters` | export rows matching filter conditions to a PegaTable |
| `ImportFromPegaTable` | import data from a PegaTable component into the spreadsheet |
| `ImportFromTable` | import data from a DataTable into the spreadsheet |

---

## Address Utilities

| MethodName | Intent |
|---|---|
| `AddressToRowColumn` | convert a cell address string to row and column indexes |
| `RowColumnToAddress` | convert row and column indexes to a cell address string |
| `SetCsvOptions` | configure CSV parsing and saving options |

---

## Cell Formatting

| MethodName | Intent |
|---|---|
| `GetCellBackgroundColor` | get the background color of a cell |
| `SetCellBackgroundColor` | set the background color of a cell |
| `GetCellForegroundColor` | get the foreground (text) color of a cell |
| `SetCellForegroundColor` | set the foreground (text) color of a cell |
| `GetCellFont` | get the font of a cell |
| `SetCellFont` | set the font of a cell |
| `GetCellFormat` | get the value format string of a cell |
| `SetCellFormat` | set the value format string of a cell |
| `GetCellHorizontalAlignment` | get the horizontal alignment of a cell |
| `SetCellHorizontalAlignment` | set the horizontal alignment of a cell |
| `GetCellVerticalAlignment` | get the vertical alignment of a cell |
| `SetCellVerticalAlignment` | set the vertical alignment of a cell |
| `GetColumnWidth` | get the width of a column |
| `SetColumnWidth` | set the width of a column |
| `GetRowHeight` | get the height of a row |
| `SetRowHeight` | set the height of a row |
| `SetColumnBackgroundColor` | set the background color of an entire column |
| `SetColumnForegroundColor` | set the foreground color of an entire column |
| `SetRowBackgroundColor` | set the background color of an entire row |
| `SetRowForegroundColor` | set the foreground color of an entire row |
