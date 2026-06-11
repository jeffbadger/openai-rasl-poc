# Toolbox: DateTime and TimeSpan

---

## DateTime

`ParentObject: "DateTime"` — static service.

Use for current date/time retrieval, arithmetic, formatting, parsing, and time zone operations.

| MethodName | Intent |
|---|---|
| `Now` | current local date and time |
| `UtcNow` | current UTC date and time; prefer for UTC intent |
| `Today` | current date with time set to midnight |
| `GetTimeStamp` | current date/time as a formatted timestamp string |
| `Add` | add a TimeSpan to a DateTime |
| `Subtract` | subtract two DateTimes to get a TimeSpan, or subtract a TimeSpan from a DateTime |
| `AddDays` | add a number of days to a DateTime |
| `AddHours` | add a number of hours to a DateTime |
| `AddMinutes` | add a number of minutes to a DateTime |
| `AddMonths` | add a number of months to a DateTime |
| `AddSeconds` | add a number of seconds to a DateTime |
| `AddYears` | add a number of years to a DateTime |
| `AddMilliseconds` | add a number of milliseconds to a DateTime |
| `AddTicks` | add a number of ticks to a DateTime |
| `Compare` | compare two DateTime values |
| `Equals` | check if two DateTime values are equal |
| `Date` | extract the date portion of a DateTime (time zeroed) |
| `Day` | extract the day-of-month component |
| `DayOfWeek` | extract the day-of-week as an integer |
| `DayOfYear` | extract the day-of-year as an integer |
| `Hour` | extract the hour component |
| `Minute` | extract the minute component |
| `Month` | extract the month component |
| `Second` | extract the second component |
| `Year` | extract the year component |
| `Millisecond` | extract the millisecond component |
| `Ticks` | extract the ticks value |
| `TimeOfDay` | extract the time-of-day as a TimeSpan |
| `DaysInMonth` | get the number of days in a given month and year |
| `IsLeapYear` | check if a year is a leap year |
| `IsDaylightSavingTime` | check if a DateTime falls in daylight saving time |
| `WeekOfYear` | get the calendar week number for a date |
| `Parse` | parse a date/time string to a DateTime |
| `ParseExact` | parse a date/time string using an exact format |
| `TryParse` | attempt to parse a date/time string; returns success flag |
| `ToString` | convert a DateTime to a string, optionally with a format |
| `ToShortDateString` | format a DateTime as a short date string |
| `ToLongDateString` | format a DateTime as a long date string |
| `ToShortTimeString` | format a DateTime as a short time string |
| `ToLongTimeString` | format a DateTime as a long time string |
| `DateTimeFromIso8601` | parse an ISO-8601 string to a DateTime |
| `DateTimeToIso8601` | format a DateTime as an ISO-8601 string |
| `DateTimeToUnixTimestampMilliseconds` | convert a DateTime to a Unix timestamp in milliseconds |
| `UnixTimestampToDateTimeMilliseconds` | convert a Unix timestamp in milliseconds to a DateTime |
| `FromExcelDate` | convert an Excel OLE Automation date number to a DateTime |
| `ToExcelDate` | convert a DateTime to an Excel OLE Automation date number |
| `ConvertTime` | convert a DateTime to a different time zone |
| `ConvertTimeFromUtc` | convert a UTC DateTime to local or target time zone |
| `ConvertTimeToUtc` | convert a DateTime to UTC |
| `FindTimeZone` | find a TimeZoneInfo by ID string |
| `GetTimeZones` | get all available time zones |
| `LocalTimeZone` | get the local system time zone |
| `TimeZoneToString` | convert a TimeZoneInfo to its string representation |

---

## TimeSpan

`ParentObject: "TimeSpan"` — static service.

Use for time duration creation, arithmetic, and component extraction.

| MethodName | Intent |
|---|---|
| `FromDays` | create a TimeSpan from a number of days |
| `FromHours` | create a TimeSpan from a number of hours |
| `FromMinutes` | create a TimeSpan from a number of minutes |
| `FromSeconds` | create a TimeSpan from a number of seconds |
| `FromMilliseconds` | create a TimeSpan from a number of milliseconds |
| `FromTicks` | create a TimeSpan from a number of ticks |
| `Add` | add two TimeSpan values |
| `Subtract` | subtract one TimeSpan from another |
| `Compare` | compare two TimeSpan values |
| `Equals` | check if two TimeSpan values are equal |
| `Parse` | parse a TimeSpan string |
| `Days` | extract the days component |
| `Hours` | extract the hours component |
| `Minutes` | extract the minutes component |
| `Seconds` | extract the seconds component |
| `Milliseconds` | extract the milliseconds component |
| `Ticks` | extract the ticks value |
| `TotalDays` | get the total value expressed in days |
| `TotalHours` | get the total value expressed in hours |
| `TotalMinutes` | get the total value expressed in minutes |
| `TotalSeconds` | get the total value expressed in seconds |
| `TotalMilliseconds` | get the total value expressed in milliseconds |
| `ToString` | convert a TimeSpan to its string representation |
| `Zero` | constant representing a zero-length TimeSpan |

---

## System.TimeSpan — ParentObject: "System.TimeSpan"

| MethodName | Intent |
|---|---|
| `TryParse` | attempt to parse a TimeSpan string; returns success flag and result via outputs |
