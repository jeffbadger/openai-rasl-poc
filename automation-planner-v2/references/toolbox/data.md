# Toolbox: Data Services

Covers PegaTable, JSON, Guid, DataType, and OCR.

---

## PegaTable

Component instance — `ParentObject` = the instance name declared in locals or globals. Not a static service.

Use for cursor-based row iteration, cell read/write, and table metadata. See `references/surfaces/pega-table.md` for iteration patterns.

| MethodName | Intent |
|---|---|
| `MoveFirst` | move the row pointer to the first row |
| `MoveNext` | advance the row pointer to the next row |
| `GetCellStringValue` | read a cell value from the current row as a string |
| `GetCellValue` | read a cell value from the current row as an object |
| `SetCellValue` | write a value to a cell in the current row |
| `ReplaceTable` | replace the table schema and data from a source DataTable |
| `ImportTable` | replace the table schema and data from a source PegaTable |
| `GetTable` | get the underlying DataTable reference (non-storable) |
| `GetColumnNames` | get an array of all column names in the table |
| `RowCount` | number of rows in the table (property) |
| `ColumnCount` | number of columns in the table (property) |
| `CurrentRowIndex` | current row pointer index (property) |

---

## Json

`ParentObject: "Json"` — static service.

Use for JSON serialization, deserialization, and value extraction.

| MethodName | Intent |
|---|---|
| `SerializeObject` | serialize an object to a JSON string |
| `DeserializeObject` | deserialize a JSON string to a typed object |
| `GetValueFromJSON` | extract a single value from a JSON string using a JSONPath expression |
| `SetValueInJSON` | update a value in a JSON string using a JSONPath expression |

`SerializeObject` has ambiguous overloads — `@default=SingleOutput` variant used unless context requires otherwise.

---

## Guid

`ParentObject: "Guid"` — static service.

| MethodName | Intent |
|---|---|
| `NewGuid` | generate a new GUID string, optionally in a specified format |
| `GuidEquals` | compare two GUID strings for equality |
| `TryParse` | attempt to parse a GUID string to a Guid value |

---

## DataType

`ParentObject: "DataType"` — static service.

Use for constructing UI-related data types and null checks.

| MethodName | Intent |
|---|---|
| `GetPoint` | construct a Point from x and y coordinates |
| `GetSize` | construct a Size from width and height values |
| `GetRectangle` | construct a Rectangle from a point and size, or from x/y/width/height |
| `GetListViewItem` | construct a ListViewItem with text and sub-item values |
| `IsObjectNull` | check whether an object reference is null |

---

## DocumentOcr

`ParentObject: "DocumentOcr"` — static service.

Use for extracting text from images or documents.

| MethodName | Intent |
|---|---|
| `ProcessToText` | extract text from an image or document and return as a string |
| `ProcessToTextFile` | extract text from an image or document and save as a plain text file |
| `ProcessToPdfFile` | extract text from an image or document and save as a PDF file |
| `ProcessToXml` | extract text from an image or document and return as an XML string |
| `ProcessToXmlFile` | extract text from an image or document and save as an XML file |
| `GetProcessToXmlConfig` | build an XML configuration string for ProcessToXml output formatting |
