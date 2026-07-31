using OutSystems.TimeZoneToolkit;

namespace OutSystems.TimeZoneToolkit.UnitTests
{
    /// <summary>
    /// Every action must reject bad input with a <see cref="TimeZoneToolkitException"/> whose
    /// message names the offending parameter, so the OutSystems developer can act on it from an
    /// Exception Handler without reading a .NET stack trace.
    /// </summary>
    public class ValidationTests
    {
        private readonly ITimeZoneToolkit _toolkit = new TimeZoneToolkitService();

        private static readonly DateTime AnyDateTime = new(2026, 1, 15, 10, 0, 0);

        public static TheoryData<string?> BlankValues => new() { null, string.Empty, "   " };

        [Theory]
        [MemberData(nameof(BlankValues))]
        public void ConvertToUTC_BlankTimeZone_IsRejected(string? timeZone)
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.ConvertToUTC(AnyDateTime, timeZone!));

            Assert.Contains("SourceTimeZone is required", error.Message);
        }

        [Theory]
        [MemberData(nameof(BlankValues))]
        public void ConvertFromUTC_BlankTimeZone_IsRejected(string? timeZone)
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.ConvertFromUTC(AnyDateTime, timeZone!));

            Assert.Contains("TimeZone is required", error.Message);
        }

        [Theory]
        [MemberData(nameof(BlankValues))]
        public void GetTimeZone_BlankIdentifier_IsRejected(string? identifier)
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.GetTimeZone(identifier!));

            Assert.Contains("Identifier is required", error.Message);
        }

        [Theory]
        [MemberData(nameof(BlankValues))]
        public void ConvertTimeZoneToIANA_BlankTimeZone_IsRejected(string? timeZone)
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.ConvertTimeZoneToIANA(timeZone!, string.Empty));

            Assert.Contains("Timezone is required", error.Message);
        }

        [Theory]
        [MemberData(nameof(BlankValues))]
        public void GetIANATimeZone_BlankName_IsRejected(string? ianaName)
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.GetIANATimeZone(ianaName!));

            Assert.Contains("IANATimezone is required", error.Message);
        }

        [Fact]
        public void ConvertToUTC_UnknownTimeZone_IsRejected()
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.ConvertToUTC(AnyDateTime, "Middle Earth Standard Time"));

            Assert.Contains("SourceTimeZone 'Middle Earth Standard Time' is not a valid time zone identifier", error.Message);
        }

        [Fact]
        public void ConvertFromUTC_UnknownTimeZone_IsRejected()
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.ConvertFromUTC(AnyDateTime, "Mars/Olympus_Mons"));

            Assert.Contains("TimeZone 'Mars/Olympus_Mons' is not a valid time zone identifier", error.Message);
        }

        [Fact]
        public void GetTimeZone_UnknownIdentifier_IsRejected()
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.GetTimeZone("Nowhere/Nothing"));

            Assert.Contains("Identifier 'Nowhere/Nothing' is not a valid time zone identifier", error.Message);
        }

        [Fact]
        public void ConvertFromTimeZone_UnknownSource_NamesTheSourceParameter()
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.ConvertFromTimeZone(AnyDateTime, "Nowhere/Nothing", "Asia/Tokyo"));

            Assert.Contains("SourceTimeZone 'Nowhere/Nothing'", error.Message);
        }

        [Fact]
        public void ConvertFromTimeZone_UnknownDestination_NamesTheDestinationParameter()
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.ConvertFromTimeZone(AnyDateTime, "Asia/Tokyo", "Nowhere/Nothing"));

            Assert.Contains("DestinationTimeZone 'Nowhere/Nothing'", error.Message);
        }

        [Fact]
        public void ConvertToUTC_TimeSkippedByDaylightSaving_IsRejected()
        {
            // New York clocks jump from 02:00 to 03:00 on 8 March 2026, so 02:30 never happens.
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.ConvertToUTC(new DateTime(2026, 3, 8, 2, 30, 0), "America/New_York"));

            Assert.Contains("SourceDateTime '2026-03-08 02:30:00' does not exist", error.Message);
            Assert.Contains("daylight saving", error.Message);
        }

        [Fact]
        public void ConvertFromTimeZone_TimeSkippedByDaylightSaving_IsRejected()
        {
            // Lisbon clocks jump from 01:00 to 02:00 on 29 March 2026.
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.ConvertFromTimeZone(new DateTime(2026, 3, 29, 1, 30, 0), "Europe/Lisbon", "Asia/Tokyo"));

            Assert.Contains("does not exist", error.Message);
        }

        [Fact]
        public void ConvertTimeZoneToIANA_IanaNameInsteadOfWindowsIdentifier_IsRejected()
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.ConvertTimeZoneToIANA("America/New_York", string.Empty));

            Assert.Contains("is not a known Windows time zone identifier", error.Message);
        }

        [Theory]
        [InlineData("USA")]
        [InlineData("P")]
        [InlineData("1A")]
        [InlineData("P-")]
        public void ConvertTimeZoneToIANA_MalformedCountry_IsRejected(string country)
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.ConvertTimeZoneToIANA("Eastern Standard Time", country));

            Assert.Contains("is not a valid two-letter ISO 3166-1 country code", error.Message);
        }

        [Fact]
        public void ConvertTimeZoneToIANA_UnknownTimeZoneWithCountry_NamesTheCountry()
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.ConvertTimeZoneToIANA("Middle Earth Standard Time", "CA"));

            Assert.Contains("for country 'CA'", error.Message);
        }

        [Fact]
        public void GetIANATimeZone_UnknownName_IsRejected()
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.GetIANATimeZone("Mars/Olympus_Mons"));

            Assert.Contains("is not a known IANA time zone name", error.Message);
        }

        [Fact]
        public void GetIANATimeZone_WindowsIdentifierInsteadOfIanaName_IsRejected()
        {
            TimeZoneToolkitException error = Assert.Throws<TimeZoneToolkitException>(
                () => _toolkit.GetIANATimeZone("Eastern Standard Time"));

            Assert.Contains("is not a known IANA time zone name", error.Message);
        }
    }
}
