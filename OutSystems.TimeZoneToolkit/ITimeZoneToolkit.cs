using System;
using System.Collections.Generic;
using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.TimeZoneToolkit
{
    /// <summary>
    /// Time zone conversion and lookup actions for OutSystems Developer Cloud (ODC) apps.
    /// </summary>
    /// <remarks>
    /// Every action that takes a time zone identifier accepts either a Windows identifier
    /// ("GMT Standard Time") or an IANA name ("Europe/Lisbon"). Invalid input raises a
    /// <see cref="TimeZoneToolkitException"/> with a message explaining what to fix.
    /// </remarks>
    [OSInterface(
        Name = "TimeZoneToolkit",
        Description = "Convert date and time values between time zones and look up time zone information in ODC apps.",
        IconResourceName = "OutSystems.TimeZoneToolkit.resources.timezone.png")]
    public interface ITimeZoneToolkit
    {
        /// <summary>
        /// Converts a date and time from one time zone to another.
        /// </summary>
        /// <param name="SourceDateTime">The date and time to convert, expressed in SourceTimeZone.</param>
        /// <param name="SourceTimeZone">The time zone of SourceDateTime. Example: "Hawaiian Standard Time".</param>
        /// <param name="DestinationTimeZone">The time zone to convert to. Example: "Tokyo Standard Time".</param>
        /// <returns>The equivalent date and time in DestinationTimeZone.</returns>
        /// <exception cref="TimeZoneToolkitException">
        /// Either identifier is empty or unknown, or SourceDateTime does not exist in SourceTimeZone
        /// because of a daylight saving transition.
        /// </exception>
        DateTime ConvertFromTimeZone(DateTime SourceDateTime, string SourceTimeZone, string DestinationTimeZone);

        /// <summary>
        /// Converts a date and time to Coordinated Universal Time (UTC).
        /// </summary>
        /// <param name="SourceDateTime">The date and time to convert, expressed in SourceTimeZone.</param>
        /// <param name="SourceTimeZone">The time zone of SourceDateTime. Example: "Hawaiian Standard Time".</param>
        /// <returns>The equivalent date and time in UTC.</returns>
        /// <exception cref="TimeZoneToolkitException">
        /// SourceTimeZone is empty or unknown, or SourceDateTime does not exist in SourceTimeZone
        /// because of a daylight saving transition.
        /// </exception>
        DateTime ConvertToUTC(DateTime SourceDateTime, string SourceTimeZone);

        /// <summary>
        /// Converts a Coordinated Universal Time (UTC) date and time to a specific time zone.
        /// </summary>
        /// <param name="DateTimeUTC">The date and time to convert, expressed in UTC.</param>
        /// <param name="TimeZone">The time zone to convert to. Example: "Central Standard Time".</param>
        /// <returns>The equivalent date and time in TimeZone.</returns>
        /// <exception cref="TimeZoneToolkitException">TimeZone is empty or unknown.</exception>
        DateTime ConvertFromUTC(DateTime DateTimeUTC, string TimeZone);

        /// <summary>
        /// Gets the details of a time zone from its identifier.
        /// </summary>
        /// <param name="Identifier">
        /// A Windows identifier ("Tokyo Standard Time") or an IANA name ("Asia/Tokyo").
        /// </param>
        /// <returns>The matching time zone details.</returns>
        /// <exception cref="TimeZoneToolkitException">Identifier is empty or unknown.</exception>
        TimeZoneRecord GetTimeZone(string Identifier);

        /// <summary>
        /// Lists every time zone available on the server, ordered by UTC offset and then by display name.
        /// Use it to fill a time zone dropdown.
        /// </summary>
        /// <returns>The list of available time zones.</returns>
        List<TimeZoneRecord> GetSystemTimeZones();

        /// <summary>
        /// Gets the details of the server's local time zone. In ODC this is always UTC, because
        /// app containers run with the UTC clock. Prefer storing the user's own time zone.
        /// </summary>
        /// <returns>The server's local time zone details.</returns>
        TimeZoneRecord GetCurrentTimeZone();

        /// <summary>
        /// Converts a Windows time zone identifier to the equivalent IANA name.
        /// </summary>
        /// <param name="Timezone">The Windows identifier to convert. Example: "Eastern Standard Time".</param>
        /// <param name="Country">
        /// Optional two-letter ISO 3166-1 country code used to pick a country specific mapping.
        /// Example: "CA". Leave empty for the default mapping.
        /// </param>
        /// <returns>The IANA time zone name. Example: "America/New_York".</returns>
        /// <exception cref="TimeZoneToolkitException">
        /// Timezone is empty or is not a Windows identifier, or Country is not a two-letter code.
        /// </exception>
        string ConvertTimeZoneToIANA(string Timezone, string Country);

        /// <summary>
        /// Gets the details of a time zone from its IANA name, reported with the matching
        /// Windows identifier.
        /// </summary>
        /// <param name="IANATimezone">The IANA name to look up. Example: "America/New_York".</param>
        /// <returns>The matching time zone details.</returns>
        /// <exception cref="TimeZoneToolkitException">IANATimezone is empty or is not a known IANA name.</exception>
        TimeZoneRecord GetIANATimeZone(string IANATimezone);
    }
}
