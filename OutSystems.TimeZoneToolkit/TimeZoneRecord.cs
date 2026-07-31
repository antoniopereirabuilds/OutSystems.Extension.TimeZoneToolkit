using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.TimeZoneToolkit
{
    /// <summary>
    /// Describes a single time zone.
    /// </summary>
    [OSStructure(Description = "Describes a single time zone.")]
    public struct TimeZoneRecord
    {
        /// <summary>
        /// The time zone identifier, as accepted by the toolkit actions. Example: "Tokyo Standard Time".
        /// </summary>
        [OSStructureField(Description = "The time zone identifier, as accepted by the toolkit actions. Example: \"Tokyo Standard Time\".")]
        public string Identifier;

        /// <summary>
        /// The name of the time zone during standard time. Example: "Tokyo Standard Time".
        /// </summary>
        [OSStructureField(Description = "The name of the time zone during standard time. Example: \"Tokyo Standard Time\".")]
        public string StandardName;

        /// <summary>
        /// The name suitable for showing to end users. Example: "(UTC+09:00) Japan Standard Time (Tokyo)".
        /// Can reach 64 characters, so allow for that if you store it.
        /// </summary>
        [OSStructureField(Description = "The label to show end users, offset included. Example: \"(UTC+09:00) Japan Standard Time (Tokyo)\". Can reach 64 characters, so allow for that if you store it.")]
        public string DisplayName;

        /// <summary>
        /// The current offset from UTC, in minutes. Example: 540 for UTC+09:00.
        /// </summary>
        [OSStructureField(Description = "The current offset from UTC, in minutes. Example: 540 for UTC+09:00.")]
        public int UtcOffset;

        /// <summary>
        /// True when the time zone has daylight saving rules at any point in the year.
        /// </summary>
        [OSStructureField(Description = "True when the time zone has daylight saving rules at any point in the year.")]
        public bool SupportsDaylightSaving;

        /// <summary>
        /// True when the time zone is currently observing daylight saving time.
        /// </summary>
        [OSStructureField(Description = "True when the time zone is currently observing daylight saving time.")]
        public bool IsDaylightSaving;

        /// <summary>
        /// Creates a new <see cref="TimeZoneRecord"/>.
        /// </summary>
        /// <param name="identifier">The time zone identifier.</param>
        /// <param name="standardName">The name of the time zone during standard time.</param>
        /// <param name="displayName">The name suitable for showing to end users.</param>
        /// <param name="utcOffset">The current offset from UTC, in minutes.</param>
        /// <param name="supportsDaylightSaving">Whether the time zone has daylight saving rules.</param>
        /// <param name="isDaylightSaving">Whether the time zone is currently observing daylight saving time.</param>
        public TimeZoneRecord(
            string identifier,
            string standardName,
            string displayName,
            int utcOffset,
            bool supportsDaylightSaving,
            bool isDaylightSaving)
        {
            Identifier = identifier;
            StandardName = standardName;
            DisplayName = displayName;
            UtcOffset = utcOffset;
            SupportsDaylightSaving = supportsDaylightSaving;
            IsDaylightSaving = isDaylightSaving;
        }
    }
}
