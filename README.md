# TimeZone Toolkit

Time zone conversion and lookup actions for OutSystems Developer Cloud (ODC) apps, packaged as an
external library. Built on .NET 10 and [TimeZoneConverter](https://github.com/mattjohnsonpint/TimeZoneConverter).

Input is validated, errors say what to fix, and every action is covered by tests.

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
Input longer than 64 characters is rejected outright — no time zone identifier comes close to that
— and the value echoed back into a message is clipped and stripped of control characters, so a
large or crafted payload cannot inflate or forge a line in your error log.

### Matching identifiers

Several distinct strings resolve to the same zone, so **compare zones by the `Identifier` a lookup
returns, never by the string you sent in**. `GetTimeZone` accepts any casing and strips Unicode
whitespace, so `america/new_york` and ` America/New_York ` both resolve to
`America/New_York`. `GetIANATimeZone` is stricter — IANA names are officially case-sensitive, so it
accepts `America/New_York` but rejects `america/new_york`. Browsers always emit correct casing, so
this only bites when an identifier has been hand-typed or lower-cased somewhere in between.

## Behaviour worth knowing

- **Ambiguous times are not an error, and a round trip through them loses an hour.** When clocks
  fall back, the repeated local hour is resolved as *standard* time, matching the .NET default.
  Only times that do not exist at all are rejected. The consequence is that
  `UTC 05:30 → ConvertFromUTC → 01:30 local → ConvertToUTC → 06:30 UTC` on 1 November 2026 in
  New York: the instant moves forward by 60 minutes. Once a year, for one hour, a
  store-as-UTC / edit-as-local / save-as-UTC screen will shift a record. If an hour matters —
  a token expiry, an SLA deadline, an audit timestamp — keep the UTC value and never re-derive it
  from the displayed local time.
- **Identifiers come back in IANA form.** ODC apps run on Linux, where the runtime always reports
  the IANA name even when a Windows identifier went in. `GetTimeZone("Tokyo Standard Time")`
  returns `Identifier = "Asia/Tokyo"`. The value it returns is always accepted back as input.
- **`GetIANATimeZone` returns the canonical name.** IANA keeps retired names as aliases, so
  `Pacific/Midway` reports `Pacific/Pago_Pago` and `Asia/Calcutta` reports `Asia/Kolkata`. The zone
  is the same one — 107 of the 419 names are aliases, and all of them behave identically. Compare
  zones by the `Identifier` you get back, not by the string you sent in.
- **`SupportsDaylightSaving` includes historical rules.** On Linux, `Asia/Tokyo` reports `True`
  because Japan observed daylight saving time from 1948 to 1951. Use `IsDaylightSaving` for the
  current state.
- **`GetCurrentTimeZone` returns UTC on ODC**, because app containers run with the UTC clock. Store
  the user's own time zone instead of relying on the server's.
- **`UtcOffset` and `IsDaylightSaving` describe *now*, not the date being converted.** A record
  fetched in July reports the summer offset even if you are converting a date in January. The
  conversion actions themselves always apply the correct offset for the date given.
  For the same reason `UtcOffset` can disagree with the offset printed inside `DisplayName`, which
  is always the zone's standard offset: in July, `America/Chicago` reports `UtcOffset = -300` under
  the label `(UTC-06:00) Central Time (Chicago)`. `GetSystemTimeZones` is ordered by the label's
  offset so the list reads in order.
- **`DisplayName` can reach 64 characters** (`Indian/Kerguelen`), and 15 zones exceed 50. If you
  store it, give the attribute enough length — the OutSystems default of 50 truncates them.
- **`GetSystemTimeZones` returns 419 records every time it is called.** The list is effectively
  constant, so calling it in the preparation of a public screen makes every anonymous page load
  rebuild and serialize the whole thing. Fetch it once into a Static Entity or a cached Server
  Action rather than per request.
- **`DisplayName` and `StandardName` are localized** to the container's culture, and the runtime
  caches the first value it produces for the lifetime of that container. Do not parse them, and do
  not assume two containers return identical text. `Identifier` and `UtcOffset` are stable.
- **An empty OutSystems Date is `1900-01-01`, and converting it can underflow.**
  `ConvertToUTC(1900-01-01, "Asia/Tokyo")` returns `1899-12-31 15:00`, which is below the minimum
  date OutSystems accepts. Check for an unset date before calling the conversion actions.

## Building

Requires the .NET 10 SDK.

```bash
dotnet test OutSystems.TimeZoneToolkit.UnitTests/OutSystems.TimeZoneToolkit.UnitTests.csproj
dotnet publish OutSystems.TimeZoneToolkit/OutSystems.TimeZoneToolkit.csproj \
  -c Release -r linux-x64 --self-contained false -o ./publish
```

Zip the contents of `./publish` and upload the ZIP in the ODC Portal under **External libraries**.

Pushing a `vX.Y.Z` tag runs the release workflow. It refuses to release a tag that is not on
`main`, runs the tests and a vulnerability audit with read-only permissions, then hands the
artifact to a separate job that holds the signing identity and does nothing but attest and
publish. The ZIP, its SHA-256, and a CycloneDX SBOM are all covered by the attestation:

```bash
gh attestation verify TimeZoneToolkit_vX.Y.Z.zip \
  --repo antoniopereirabuilds/OutSystems.Extension.TimeZoneToolkit
```

## License

MIT — see [LICENSE](LICENSE). The release package redistributes TimeZoneConverter (MIT) and the
OutSystems External Libraries SDK (BSD-3-Clause); their notices are in [NOTICE](NOTICE).

## Credits

The action set was informed by a survey of
[brunomachadoOS/OutSystems.TimeZoneConverter](https://github.com/brunomachadoOS/OutSystems.TimeZoneConverter),
which mapped out the conversions an ODC app actually needs. This is an independent implementation;
no upstream source code was reused.
