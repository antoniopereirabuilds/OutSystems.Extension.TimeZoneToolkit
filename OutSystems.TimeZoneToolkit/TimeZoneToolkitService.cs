using System;
using System.Collections.Generic;
using TimeZoneConverter;

namespace OutSystems.TimeZoneToolkit
{
    /// <summary>
    /// Default implementation of <see cref="ITimeZoneToolkit"/>.
    /// </summary>
    public class TimeZoneToolkitService : ITimeZoneToolkit
    {
        private const string IdentifierHint =
            "Provide a Windows identifier (for example, \"GMT Standard Time\") or an IANA name (for example, \"Europe/Lisbon\").";

        /// <inheritdoc />
        public DateTime ConvertFromTimeZone(DateTime SourceDateTime, string SourceTimeZone, string DestinationTimeZone)
        {
            TimeZoneInfo source = ResolveTimeZone(SourceTimeZone, nameof(SourceTimeZone));
            TimeZoneInfo destination = ResolveTimeZone(DestinationTimeZone, nameof(DestinationTimeZone));

            DateTime value = DateTime.SpecifyKind(SourceDateTime, DateTimeKind.Unspecified);
            GuardNonExistentTime(value, source, nameof(SourceDateTime));

            return TimeZoneInfo.ConvertTime(value, source, destination);
        }

        /// <inheritdoc />
        public DateTime ConvertToUTC(DateTime SourceDateTime, string SourceTimeZone)
        {
            TimeZoneInfo source = ResolveTimeZone(SourceTimeZone, nameof(SourceTimeZone));

            DateTime value = DateTime.SpecifyKind(SourceDateTime, DateTimeKind.Unspecified);
            GuardNonExistentTime(value, source, nameof(SourceDateTime));

            return TimeZoneInfo.ConvertTimeToUtc(value, source);
        }

        /// <inheritdoc />
        public DateTime ConvertFromUTC(DateTime DateTimeUTC, string TimeZone)
        {
            TimeZoneInfo destination = ResolveTimeZone(TimeZone, nameof(TimeZone));

            DateTime value = DateTime.SpecifyKind(DateTimeUTC, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeFromUtc(value, destination);
        }

        /// <inheritdoc />
        public TimeZoneRecord GetTimeZone(string Identifier)
        {
            return ToRecord(ResolveTimeZone(Identifier, nameof(Identifier)));
        }

        /// <inheritdoc />
        public List<TimeZoneRecord> GetSystemTimeZones()
        {
            List<TimeZoneInfo> timeZones = new(TimeZoneInfo.GetSystemTimeZones());

            // Sorted on the standard offset, which is the one printed inside DisplayName, so the
            // labels a user reads in a dropdown run in order. Sorting on the current offset
            // instead puts every zone that is on daylight saving time today out of place.
            timeZones.Sort((left, right) =>
                left.BaseUtcOffset == right.BaseUtcOffset
                    ? string.CompareOrdinal(left.DisplayName, right.DisplayName)
                    : left.BaseUtcOffset.CompareTo(right.BaseUtcOffset));

            List<TimeZoneRecord> records = new(timeZones.Count);
            foreach (TimeZoneInfo timeZone in timeZones)
            {
                records.Add(ToRecord(timeZone));
            }

            return records;
        }

        /// <inheritdoc />
        public TimeZoneRecord GetCurrentTimeZone()
        {
            return ToRecord(TimeZoneInfo.Local);
        }

        /// <inheritdoc />
        public string ConvertTimeZoneToIANA(string Timezone, string Country)
        {
            string windowsId = RequireText(Timezone, nameof(Timezone), IdentifierHint);
            string? territory = NormalizeCountry(Country, nameof(Country));

            bool converted = territory is null
                ? TZConvert.TryWindowsToIana(windowsId, out string? iana)
                : TZConvert.TryWindowsToIana(windowsId, territory, out iana);

            if (!converted || string.IsNullOrEmpty(iana))
            {
                throw new TimeZoneToolkitException(
                    $"{nameof(Timezone)} '{ForMessage(windowsId)}' is not a known Windows time zone identifier" +
                    (territory is null ? string.Empty : $" for country '{territory}'") +
                    ". Example: \"Eastern Standard Time\".");
            }

            return iana;
        }

        /// <inheritdoc />
        public TimeZoneRecord GetIANATimeZone(string IANATimezone)
        {
            string ianaName = RequireText(
                IANATimezone,
                nameof(IANATimezone),
                "Provide an IANA time zone name (for example, \"America/New_York\").");

            // Resolve the IANA name directly. Mapping it to a Windows identifier first would be
            // lossy, because one Windows identifier covers many IANA zones: Antarctica/Casey
            // would come back as Pacific/Guadalcanal, three hours away.
            if (!TZConvert.TryGetIanaCanonicalName(ianaName, out string? canonicalName)
                || string.IsNullOrEmpty(canonicalName))
            {
                throw new TimeZoneToolkitException(
                    $"{nameof(IANATimezone)} '{ForMessage(ianaName)}' is not a known IANA time zone name. Example: \"America/New_York\".");
            }

            return ToRecord(ResolveTimeZone(canonicalName, nameof(IANATimezone)));
        }

        /// <summary>
        /// The longest identifier in the database is 32 characters, so anything beyond this is
        /// not a near miss worth echoing back. The bound keeps a large payload out of the error
        /// message, and therefore out of the app's error log.
        /// </summary>
        private const int MaximumIdentifierLength = 64;

        /// <summary>
        /// Rejects empty or oversized input and returns the trimmed value.
        /// </summary>
        private static string RequireText(string value, string parameterName, string hint)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new TimeZoneToolkitException($"{parameterName} is required. {hint}");
            }

            string trimmed = value.Trim();
            if (trimmed.Length > MaximumIdentifierLength)
            {
                throw new TimeZoneToolkitException(
                    $"{parameterName} is {trimmed.Length} characters long, and no time zone " +
                    $"identifier is longer than {MaximumIdentifierLength}. {hint}");
            }

            return trimmed;
        }

        /// <summary>
        /// Prepares a caller supplied value for inclusion in an error message. The value is
        /// clipped, so a large payload cannot inflate the app's error log, and control characters
        /// are replaced, because a message carrying a newline can forge an entry in a log
        /// collector that reads one event per line.
        /// </summary>
        private static string ForMessage(string value)
        {
            string clipped = value.Length > MaximumIdentifierLength
                ? string.Concat(value.AsSpan(0, MaximumIdentifierLength), "…")
                : value;

            return string.Create(clipped.Length, clipped, static (destination, source) =>
            {
                for (int i = 0; i < source.Length; i++)
                {
                    destination[i] = char.IsControl(source[i]) ? '�' : source[i];
                }
            });
        }

        /// <summary>
        /// Resolves a Windows identifier or an IANA name into a time zone, on any host platform.
        /// </summary>
        private static TimeZoneInfo ResolveTimeZone(string identifier, string parameterName)
        {
            string value = RequireText(identifier, parameterName, IdentifierHint);

            if (!TZConvert.TryGetTimeZoneInfo(value, out TimeZoneInfo? timeZone))
            {
                throw new TimeZoneToolkitException(
                    $"{parameterName} '{ForMessage(value)}' is not a valid time zone identifier. {IdentifierHint} " +
                    "Use GetSystemTimeZones to list the available identifiers.");
            }

            return timeZone;
        }

        /// <summary>
        /// Rejects a date and time that the clock skips when daylight saving time starts.
        /// </summary>
        private static void GuardNonExistentTime(DateTime value, TimeZoneInfo timeZone, string parameterName)
        {
            if (timeZone.IsInvalidTime(value))
            {
                throw new TimeZoneToolkitException(
                    $"{parameterName} '{value:yyyy-MM-dd HH:mm:ss}' does not exist in time zone '{timeZone.Id}' " +
                    "because the clock moves forward for daylight saving time at that moment.");
            }
        }

        /// <summary>
        /// Projects a time zone into the structure returned to OutSystems, using the current instant
        /// for the offset and daylight saving flags.
        /// </summary>
        private static TimeZoneRecord ToRecord(TimeZoneInfo timeZone)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;

            return new TimeZoneRecord(
                timeZone.Id,
                timeZone.StandardName,
                timeZone.DisplayName,
                (int)timeZone.GetUtcOffset(now).TotalMinutes,
                timeZone.SupportsDaylightSavingTime,
                timeZone.IsDaylightSavingTime(now));
        }

        /// <summary>
        /// Validates the optional ISO 3166-1 alpha-2 country code and returns it upper cased,
        /// or null when it was not supplied.
        /// </summary>
        private static string? NormalizeCountry(string country, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                return null;
            }

            string value = country.Trim();
            if (value.Length != 2 || !char.IsAsciiLetter(value[0]) || !char.IsAsciiLetter(value[1]))
            {
                throw new TimeZoneToolkitException(
                    $"{parameterName} '{ForMessage(value)}' is not a valid two-letter ISO 3166-1 country code. Example: \"CA\".");
            }

            return value.ToUpperInvariant();
        }
    }
}
