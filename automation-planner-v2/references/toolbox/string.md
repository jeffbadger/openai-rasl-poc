# Toolbox: String and Regex

## String — ParentObject: "String"

Use for text manipulation, comparison, formatting, encoding, character inspection, and pattern-based operations.

| MethodName | Intent |
|---|---|
| `Concat` | join multiple string values into one |
| `Contains` | check if a string contains a substring |
| `ContainsAll` | check if a string contains all of a set of substrings |
| `ContainsAny` | check if a string contains any of a set of substrings |
| `StartsWith` | check if a string starts with a value |
| `EndsWith` | check if a string ends with a value |
| `DoesNotEndWith` | check that a string does not end with a pattern |
| `Equals` | compare two strings for equality with comparison type |
| `EqualsIgnoreCase` | compare two strings, ignoring case |
| `Format` | format a string using a format template and a typed value |
| `IndexOf` | find the first occurrence index of a substring |
| `LastIndexOf` | find the last occurrence index of a substring |
| `Insert` | insert a string at a given index position |
| `Remove` | remove characters from a string at a given index |
| `Replace` | replace all occurrences of a substring in a string |
| `RemovePunctuation` | remove all punctuation from a string |
| `Length` | get the character count of a string |
| `Left` | get the leftmost N characters of a string |
| `Right` | get the rightmost N characters of a string |
| `Substring` | extract a portion of a string by start index and length |
| `PadLeft` | left-pad a string to a given width |
| `PadRight` | right-pad a string to a given width |
| `Trim` | remove leading and trailing whitespace |
| `ToLower` | convert a string to lowercase |
| `ToUpper` | convert a string to uppercase |
| `ToProperCase` | convert a string to title/proper case |
| `Split` | split a string into an array using a delimiter |
| `Join` | join a list of strings with a separator |
| `GetCharacters` | get an array of characters in a range |
| `IsNullOrEmpty` | check if a string is null or empty |
| `IsDBNullOrEmpty` | check if an object is DBNull or empty |
| `IsIn` | check if a string is a member of a set of values |
| `IsControl` | check if character at position is a control character |
| `IsDigit` | check if character at position is a digit |
| `IsLetter` | check if character at position is a letter |
| `IsLetterOrDigit` | check if character at position is a letter or digit |
| `IsLower` | check if character at position is lowercase |
| `IsNumber` | check if character at position is a number |
| `IsPunctuation` | check if character at position is punctuation |
| `IsSeparator` | check if character at position is a separator |
| `IsSymbol` | check if character at position is a symbol |
| `IsUpper` | check if character at position is uppercase |
| `IsWhiteSpace` | check if character at position is whitespace |
| `EncodeBase64` | encode a string to Base64 |
| `DecodeBase64` | decode a Base64-encoded string |
| `GeneratePassword` | generate a random password string of a given length |
| `ToMd5Hash` | compute the MD5 hash of a string |
| `ToStream` | convert a string to a Stream |
| `ToTextReader` | convert a string to a TextReader |

---

## Regex — ParentObject: "Regex"

Use for pattern-based matching, extraction, replacement, splitting, and format validation. Load this file when the goal involves regex, pattern matching, or validating formats like email, URL, or IP address.

| MethodName | Intent |
|---|---|
| `IsRegexMatch` | check whether a pattern matches anywhere in a string |
| `RegexMatch` | apply a pattern and return the first Match object |
| `RegexMatches` | apply a pattern and return all Match objects |
| `ExtractGroup` | extract a capture group value from a string by group index |
| `ExtractMatchGroup` | extract a capture group string from an existing Match object |
| `ExtractMatchGroups` | extract all capture groups from an existing Match object |
| `RegexReplace` | replace pattern matches in a string with a replacement value |
| `RegexSplit` | split a string at pattern match positions |
| `BuildRegexOptions` | construct a RegexOptions value from individual boolean flags |
| `HasEmailAddress` | check whether a string contains a valid email address |
| `HasIPAddress` | check whether a string contains a valid IP address |
| `HasUrl` | check whether a string contains a valid URL |
