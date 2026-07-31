## What it does

TimeZone Toolkit gives an ODC app the time zone conversions and lookups the platform does not provide out of the box. It converts date and time values between zones and to or from UTC, and returns the details of a zone, including its current offset and daylight saving state.

Identifiers accept either the Windows style, such as "GMT Standard Time", or the IANA style, such as "Europe/Lisbon". Browsers report IANA names, so IANA is usually easier to store.

## Actions

| Action | What it does | Key inputs | Key outputs |
| --- | --- | --- | --- |
| ConvertFromTimeZone | Converts a date and time from one zone to another | SourceDateTime, SourceTimeZone, DestinationTimeZone | Date Time |
| ConvertToUTC | Converts a date and time in a zone to UTC | SourceDateTime, SourceTimeZone | Date Time |
| ConvertFromUTC | Converts a UTC date and time into a zone | DateTimeUTC, TimeZone | Date Time |
| GetTimeZone | Looks up a zone from a Windows or IANA identifier | Identifier | TimeZoneRecord |
| GetSystemTimeZones | Lists all available zones, ordered by standard UTC offset | None | List of TimeZoneRecord |
| GetCurrentTimeZone | Returns the server zone | None | TimeZoneRecord |
| ConvertTimeZoneToIANA | Translates a Windows identifier to its IANA name | Timezone, Country (optional) | Text |
| GetIANATimeZone | Looks up a zone from an IANA name | IANATimezone | TimeZoneRecord |

The last two actions are the exception to the dual naming rule: ConvertTimeZoneToIANA only accepts Windows identifiers, and GetIANATimeZone only accepts IANA names.

## TimeZoneRecord

| Field | Type | What it holds |
| --- | --- | --- |
| Identifier | Text | The value the other actions accept as input |
| StandardName | Text | The zone name during standard time |
| DisplayName | Text | The label to show users, offset included |
| UtcOffset | Integer | Current offset from UTC, in minutes |
| SupportsDaylightSaving | Boolean | Whether the zone has daylight saving rules |
| IsDaylightSaving | Boolean | Whether the zone is on daylight saving now |

## How to use it

Capture the user's zone once, from the browser or from a dropdown filled by GetSystemTimeZones, and save the Identifier.

On save, drag ConvertToUTC into the flow, map the local value the user typed and the stored Identifier, and persist the result.

On display, drag ConvertFromUTC into the flow, map the stored UTC value and the same Identifier, and bind the result to the screen.

Add an Exception Handler for All Exceptions. Invalid input raises a TimeZoneToolkitException whose ExceptionMessage names the parameter to fix.

## Constraints and gotchas

Keep the stored UTC value. A UTC to local to UTC round trip through the repeated hour at a daylight saving end shifts the instant forward by 60 minutes, once a year, so never re-derive UTC from a displayed local time.

Times that a daylight saving start skips are rejected. Repeated times are not rejected and resolve to standard time.

On ODC the returned Identifier is always the IANA name, even when a Windows identifier went in. Compare zones by the Identifier you get back, not the string you sent.

GetCurrentTimeZone returns UTC on ODC, because app containers run the UTC clock. Store the user's own zone instead.

GetSystemTimeZones returns 419 records on every call. Fetch it once and cache it rather than calling it per request.

Each call is independent, with no state kept between invocations.
