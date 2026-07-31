using OutSystems.TimeZoneToolkit;

namespace OutSystems.TimeZoneToolkit.UnitTests
{
    /// <summary>
    /// Covers the three conversion actions. Dates are picked so the expected offsets are
    /// stable: January is standard time and July is daylight saving time in the US zones,
    /// and Tokyo never observes daylight saving time.
    /// </summary>
    public class ConversionTests
    {
        private readonly ITimeZoneToolkit _toolkit = new TimeZoneToolkitService();

        [Fact]
        public void ConvertToUTC_WindowsIdentifier_SubtractsOffset()
        {
            DateTime result = _toolkit.ConvertToUTC(new DateTime(2026, 1, 15, 10, 0, 0), "Tokyo Standard Time");

            Assert.Equal(new DateTime(2026, 1, 15, 1, 0, 0), result);
        }

        [Fact]
        public void ConvertToUTC_IanaName_MatchesWindowsIdentifier()
        {
            DateTime source = new(2026, 1, 15, 10, 0, 0);

            Assert.Equal(
                _toolkit.ConvertToUTC(source, "Tokyo Standard Time"),
                _toolkit.ConvertToUTC(source, "Asia/Tokyo"));
        }

        [Fact]
        public void ConvertToUTC_InsideDaylightSaving_UsesDaylightOffset()
        {
            // 15 July 2026 is EDT (UTC-04:00), not EST (UTC-05:00).
            DateTime result = _toolkit.ConvertToUTC(new DateTime(2026, 7, 15, 10, 0, 0), "Eastern Standard Time");

            Assert.Equal(new DateTime(2026, 7, 15, 14, 0, 0), result);
        }

        [Fact]
        public void ConvertToUTC_AmbiguousTime_ResolvesToStandardTime()
        {
            // Clocks fall back on 1 November 2026, so 01:30 happens twice in New York.
            // The .NET rule is to treat the repeated hour as standard time (UTC-05:00).
            DateTime result = _toolkit.ConvertToUTC(new DateTime(2026, 11, 1, 1, 30, 0), "America/New_York");

            Assert.Equal(new DateTime(2026, 11, 1, 6, 30, 0), result);
        }

        [Fact]
        public void ConvertFromUTC_AddsOffset()
        {
            DateTime result = _toolkit.ConvertFromUTC(new DateTime(2026, 1, 15, 1, 0, 0), "Asia/Tokyo");

            Assert.Equal(new DateTime(2026, 1, 15, 10, 0, 0), result);
        }

        [Fact]
        public void ConvertFromUTC_InsideDaylightSaving_UsesDaylightOffset()
        {
            DateTime result = _toolkit.ConvertFromUTC(new DateTime(2026, 7, 15, 14, 0, 0), "Eastern Standard Time");

            Assert.Equal(new DateTime(2026, 7, 15, 10, 0, 0), result);
        }

        [Fact]
        public void ConvertFromUTC_UtcItself_ReturnsInput()
        {
            DateTime source = new(2026, 1, 15, 1, 0, 0);

            Assert.Equal(source, _toolkit.ConvertFromUTC(source, "UTC"));
        }

        [Fact]
        public void ConvertToUTC_ThenConvertFromUTC_RoundTrips()
        {
            DateTime source = new(2026, 5, 20, 8, 45, 0);

            DateTime utc = _toolkit.ConvertToUTC(source, "Europe/Lisbon");

            Assert.Equal(source, _toolkit.ConvertFromUTC(utc, "Europe/Lisbon"));
        }

        [Fact]
        public void ConvertFromTimeZone_CrossesTheDateBoundary()
        {
            // Tokyo 10:00 on 15 January is 01:00 UTC, which is 15:00 on 14 January in Hawaii.
            DateTime result = _toolkit.ConvertFromTimeZone(
                new DateTime(2026, 1, 15, 10, 0, 0),
                "Tokyo Standard Time",
                "Hawaiian Standard Time");

            Assert.Equal(new DateTime(2026, 1, 14, 15, 0, 0), result);
        }

        [Fact]
        public void ConvertFromTimeZone_SameZone_ReturnsInput()
        {
            DateTime source = new(2026, 1, 15, 10, 0, 0);

            Assert.Equal(source, _toolkit.ConvertFromTimeZone(source, "Asia/Tokyo", "Asia/Tokyo"));
        }

        [Fact]
        public void ConvertFromTimeZone_MixesIdentifierStyles()
        {
            DateTime source = new(2026, 1, 15, 10, 0, 0);

            Assert.Equal(
                _toolkit.ConvertFromTimeZone(source, "Tokyo Standard Time", "Hawaiian Standard Time"),
                _toolkit.ConvertFromTimeZone(source, "Asia/Tokyo", "Pacific/Honolulu"));
        }

        [Fact]
        public void Conversions_IgnoreTheKindOfTheInput()
        {
            DateTime unspecified = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Unspecified);
            DateTime utcKind = DateTime.SpecifyKind(unspecified, DateTimeKind.Utc);

            Assert.Equal(
                _toolkit.ConvertToUTC(unspecified, "Asia/Tokyo"),
                _toolkit.ConvertToUTC(utcKind, "Asia/Tokyo"));
        }
    }
}
