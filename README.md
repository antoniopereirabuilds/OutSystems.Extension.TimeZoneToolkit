# TimeZone Toolkit

Time zone conversion and lookup actions for OutSystems Developer Cloud (ODC) apps, packaged as an
external library. Built on .NET 10 and [TimeZoneConverter](https://github.com/mattjohnsonpint/TimeZoneConverter).

Based on [brunomachadoOS/OutSystems.TimeZoneConverter](https://github.com/brunomachadoOS/OutSystems.TimeZoneConverter),
rewritten with input validation, actionable error messages, and unit tests.

## Actions

| Action | Description |
| --- | --- |
| `ConvertFromTimeZone` | Converts a date and time from one time zone to another. |
| `ConvertToUTC` | Converts a date and time in a given time zone to UTC. |
| `ConvertFromUTC` | Converts a UTC date and time to a given time zone. |
| `GetTimeZone` | Gets the details of a time zone from its identifier. |
| `GetSystemTimeZones` | Lists every available time zone, ordered by UTC offset. Use it to fill a dropdown. |
| `GetCurrentTimeZone` | Gets the server's local time zone. |
| `ConvertTimeZoneToIANA` | Converts a Windows time zone identifier to its IANA name. |
| `GetIANATimeZone` | Gets the details of a time zone from its IANA name. |

### TimeZoneRecord structure

| Field | Type | Description |
| --- | --- | --- |
| `Identifier` | Text | The identifier accepted by the other actions. |
| `StandardName` | Text | The name of the zone during standard time. |
| `DisplayName` | Text | The name to show to end users. Example: `(UTC+09:00) Japan Standard Time (Tokyo)`. |
| `UtcOffset` | Integer | The current offset from UTC, in minutes. Example: `540`. |
| `SupportsDaylightSaving` | Boolean | Whether the zone has daylight saving rules. |
| `IsDaylightSaving` | Boolean | Whether the zone is observing daylight saving time right now. |

## Identifiers

Every action that takes a time zone identifier accepts **either** style:

- Windows: `GMT Standard Time`, `Eastern Standard Time`, `Tokyo Standard Time`
- IANA: `Europe/London`, `America/New_York`, `Asia/Tokyo`

Browsers report IANA names through `Intl.DateTimeFormat().resolvedOptions().timeZone`, so IANA is
usually the more convenient style to store.

`ConvertTimeZoneToIANA` and `GetIANATimeZone` are the exceptions: they translate between the two
naming schemes and each only accepts its own input style.

## Error handling

Invalid input raises a `TimeZoneToolkitException` carrying a message that names the parameter and
says what to fix. Catch it in OutSystems with an **Exception Handler** (All Exceptions) and read
`ExceptionMessage`.

| Situation | Message |
| --- | --- |
| Empty identifier | `SourceTimeZone is required. Provide a Windows identifier (for example, "GMT Standard Time") or an IANA name (for example, "Europe/Lisbon").` |
| Unknown identifier | `SourceTimeZone 'Middle Earth Standard Time' is not a valid time zone identifier. … Use GetSystemTimeZones to list the available identifiers.` |
| Time skipped by a DST start | `SourceDateTime '2026-03-08 02:30:00' does not exist in time zone 'America/New_York' because the clock moves forward for daylight saving time at that moment.` |
| Malformed country code | `Country 'USA' is not a valid two-letter ISO 3166-1 country code. Example: "CA".` |
| IANA name passed to `ConvertTimeZoneToIANA` | `Timezone 'America/New_York' is not a known Windows time zone identifier. Example: "Eastern Standard Time".` |
| Windows identifier passed to `GetIANATimeZone` | `IANATimezone 'Eastern Standard Time' is not a known IANA time zone name. Example: "America/New_York".` |

Identifiers and country codes are trimmed before use, so trailing whitespace is not an error.

## Behaviour worth knowing

- **Ambiguous times are not an error.** When clocks fall back, the repeated hour is resolved as
  *standard* time, matching the .NET default. Only times that do not exist at all are rejected.
- **Identifiers come back in IANA form.** ODC apps run on Linux, where the runtime always reports
  the IANA name even when a Windows identifier went in. `GetTimeZone("Tokyo Standard Time")`
  returns `Identifier = "Asia/Tokyo"`. The value it returns is always accepted back as input.
- **`SupportsDaylightSaving` includes historical rules.** On Linux, `Asia/Tokyo` reports `True`
  because Japan observed daylight saving time from 1948 to 1951. Use `IsDaylightSaving` for the
  current state.
- **`GetCurrentTimeZone` returns UTC on ODC**, because app containers run with the UTC clock. Store
  the user's own time zone instead of relying on the server's.
- **`UtcOffset` and `IsDaylightSaving` describe *now*, not the date being converted.** A record
  fetched in July reports the summer offset even if you are converting a date in January. The
  conversion actions themselves always apply the correct offset for the date given.

## Building

Requires the .NET 10 SDK.

```bash
dotnet test OutSystems.TimeZoneToolkit.UnitTests/OutSystems.TimeZoneToolkit.UnitTests.csproj
dotnet publish OutSystems.TimeZoneToolkit/OutSystems.TimeZoneToolkit.csproj \
  -c Release -r linux-x64 --self-contained false -o ./publish
```

Zip the contents of `./publish` and upload the ZIP in the ODC Portal under **External libraries**.
Pushing a `vX.Y.Z` tag runs the release workflow, which tests, publishes, zips, generates an SBOM
and a checksum, and attaches everything to a GitHub release.
