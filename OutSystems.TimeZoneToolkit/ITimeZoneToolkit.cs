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
        Description = "Convert date and time values between time zones and look up time zone information in ODC apps. Identifiers accept both the Windows style (\"GMT Standard Time\") and the IANA style (\"Europe/Lisbon\"). Invalid input raises an exception naming the parameter to fix.",
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
        [OSAction(
            Description = "Converts a date and time from one time zone to another. Raises an error if either identifier is unknown, or if the date and time does not exist in the source zone because a daylight saving change skips it.",
            ReturnName = "ConvertedDateTime",
            ReturnDescription = "The same instant expressed in DestinationTimeZone.")]
        DateTime ConvertFromTimeZone(
            [OSParameter(Description = "The date and time to convert, as read in SourceTimeZone.")]
            DateTime SourceDateTime,
            [OSParameter(Description = "The time zone SourceDateTime is expressed in. Accepts a Windows identifier (\"Hawaiian Standard Time\") or an IANA name (\"Pacific/Honolulu\").")]
            string SourceTimeZone,
            [OSParameter(Description = "The time zone to convert into. Accepts a Windows identifier (\"Tokyo Standard Time\") or an IANA name (\"Asia/Tokyo\").")]
            string DestinationTimeZone);

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
        [OSAction(
            Description = "Converts a date and time in a given time zone to UTC, ready to store. Raises an error if the identifier is unknown, or if the date and time does not exist because a daylight saving change skips it.",
            ReturnName = "DateTimeUTC",
            ReturnDescription = "The same instant expressed in UTC.")]
        DateTime ConvertToUTC(
            [OSParameter(Description = "The date and time to convert, as read in SourceTimeZone. This is typically what the user typed on screen.")]
            DateTime SourceDateTime,
            [OSParameter(Description = "The time zone SourceDateTime is expressed in. Accepts a Windows identifier (\"Hawaiian Standard Time\") or an IANA name (\"Pacific/Honolulu\").")]
            string SourceTimeZone);

        /// <summary>
        /// Converts a Coordinated Universal Time (UTC) date and time to a specific time zone.
        /// </summary>
        /// <param name="DateTimeUTC">The date and time to convert, expressed in UTC.</param>
        /// <param name="TimeZone">The time zone to convert to. Example: "Central Standard Time".</param>
        /// <returns>The equivalent date and time in TimeZone.</returns>
        /// <exception cref="TimeZoneToolkitException">TimeZone is empty or unknown.</exception>
        [OSAction(
            Description = "Converts a stored UTC date and time into a given time zone, ready to display. Raises an error if the identifier is unknown.",
            ReturnName = "DateTimeInTimeZone",
            ReturnDescription = "The same instant expressed in TimeZone.")]
        DateTime ConvertFromUTC(
            [OSParameter(Description = "The date and time to convert, expressed in UTC. This is typically the value held in the database.")]
            DateTime DateTimeUTC,
            [OSParameter(Description = "The time zone to convert into, usually the viewing user's own. Accepts a Windows identifier (\"Central Standard Time\") or an IANA name (\"America/Chicago\").")]
            string TimeZone);

        /// <summary>
        /// Gets the details of a time zone from its identifier.
        /// </summary>
        /// <param name="Identifier">
        /// A Windows identifier ("Tokyo Standard Time") or an IANA name ("Asia/Tokyo").
        /// </param>
        /// <returns>The matching time zone details.</returns>
        /// <exception cref="TimeZoneToolkitException">Identifier is empty or unknown.</exception>
        [OSAction(
            Description = "Gets the offset, names and daylight saving state of a single time zone. Raises an error if the identifier is unknown.",
            ReturnName = "TimeZone",
            ReturnDescription = "The details of the matching time zone.")]
        TimeZoneRecord GetTimeZone(
            [OSParameter(Description = "The time zone to look up. Accepts a Windows identifier (\"Tokyo Standard Time\") or an IANA name (\"Asia/Tokyo\").")]
            string Identifier);

        /// <summary>
        /// Lists every time zone available on the server, ordered by standard UTC offset and then by
        /// display name, matching the offset printed in DisplayName. Use it to fill a time zone dropdown.
        /// </summary>
        /// <returns>The list of available time zones.</returns>
        [OSAction(
            Description = "Lists every time zone available on the server, ordered by UTC offset and then by display name. Use it to fill a time zone dropdown. The list is effectively constant, so fetch it once and cache it rather than calling it on every request.",
            ReturnName = "TimeZones",
            ReturnDescription = "All available time zones, in display order.")]
        List<TimeZoneRecord> GetSystemTimeZones();

        /// <summary>
        /// Gets the details of the server's local time zone. In ODC this is always UTC, because
        /// app containers run with the UTC clock. Prefer storing the user's own time zone.
        /// </summary>
        /// <returns>The server's local time zone details.</returns>
        [OSAction(
            Description = "Gets the details of the server's own time zone. In ODC this is always UTC, because app containers run with the UTC clock, so store each user's time zone rather than relying on this.",
            ReturnName = "TimeZone",
            ReturnDescription = "The details of the server's local time zone.")]
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
        [OSAction(
            Description = "Translates a Windows time zone identifier into its IANA name. Unlike the other actions this one accepts Windows identifiers only. Raises an error if the identifier is not a Windows one.",
            ReturnName = "IANATimezone",
            ReturnDescription = "The matching IANA time zone name. Example: \"America/New_York\".")]
        string ConvertTimeZoneToIANA(
            [OSParameter(Description = "The Windows time zone identifier to translate. Example: \"Eastern Standard Time\". An IANA name is not accepted here.")]
            string Timezone,
            [OSParameter(Description = "Optional two-letter ISO 3166-1 country code used to pick a country specific mapping, such as \"CA\" to get America/Toronto. Leave empty for the default mapping.")]
            string Country);

        /// <summary>
        /// Gets the details of a time zone from its IANA name. Deprecated names are resolved to
        /// their current one, so "Asia/Calcutta" reports "Asia/Kolkata".
        /// </summary>
        /// <param name="IANATimezone">The IANA name to look up. Example: "America/New_York".</param>
        /// <returns>The matching time zone details.</returns>
        /// <exception cref="TimeZoneToolkitException">IANATimezone is empty or is not a known IANA name.</exception>
        [OSAction(
            Description = "Gets the offset, names and daylight saving state of a time zone from its IANA name. Unlike the other actions this one accepts IANA names only, and it is case sensitive. Raises an error if the name is not a known IANA one.",
            ReturnName = "TimeZone",
            ReturnDescription = "The details of the matching time zone.")]
        TimeZoneRecord GetIANATimeZone(
            [OSParameter(Description = "The IANA time zone name to look up. Example: \"America/New_York\". A Windows identifier is not accepted here.")]
            string IANATimezone);
    }
}
