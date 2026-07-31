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
            IReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();

            List<TimeZoneRecord> records = new(timeZones.Count);
            foreach (TimeZoneInfo timeZone in timeZones)
            {
                records.Add(ToRecord(timeZone));
            }

            // Sorted on the same offset the record reports, so a dropdown reads in order.
            records.Sort((left, right) =>
                left.UtcOffset == right.UtcOffset
                    ? string.CompareOrdinal(left.DisplayName, right.DisplayName)
                    : left.UtcOffset.CompareTo(right.UtcOffset));

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
                    $"{nameof(Timezone)} '{windowsId}' is not a known Windows time zone identifier" +
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

            if (!TZConvert.TryIanaToWindows(ianaName, out string? windowsId) || string.IsNullOrEmpty(windowsId))
            {
                throw new TimeZoneToolkitException(
                    $"{nameof(IANATimezone)} '{ianaName}' is not a known IANA time zone name. Example: \"America/New_York\".");
            }

            return ToRecord(ResolveTimeZone(windowsId, nameof(IANATimezone)));
        }

        /// <summary>
        /// Rejects empty input and returns the trimmed value.
        /// </summary>
        private static string RequireText(string value, string parameterName, string hint)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new TimeZoneToolkitException($"{parameterName} is required. {hint}");
            }

            return value.Trim();
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
                    $"{parameterName} '{value}' is not a valid time zone identifier. {IdentifierHint} " +
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
                    $"{parameterName} '{country}' is not a valid two-letter ISO 3166-1 country code. Example: \"CA\".");
            }

            return value.ToUpperInvariant();
        }
    }
}
