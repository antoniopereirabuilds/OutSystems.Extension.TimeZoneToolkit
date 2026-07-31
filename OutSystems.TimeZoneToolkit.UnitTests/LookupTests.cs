using OutSystems.TimeZoneToolkit;

namespace OutSystems.TimeZoneToolkit.UnitTests
{
    /// <summary>
    /// Covers the lookup and naming actions.
    /// </summary>
    /// <remarks>
    /// The identifier a zone reports back is platform dependent: on Linux and macOS the runtime
    /// always answers with the IANA name, even when a Windows identifier went in. These tests
    /// therefore assert on offsets and on round trips rather than on a literal identifier.
    /// </remarks>
    public class LookupTests
    {
        private readonly ITimeZoneToolkit _toolkit = new TimeZoneToolkitService();

        [Fact]
        public void GetTimeZone_ReturnsOffsetAndDaylightSavingFlags()
        {
            TimeZoneRecord record = _toolkit.GetTimeZone("Tokyo Standard Time");

            Assert.Equal(540, record.UtcOffset);
            Assert.False(record.IsDaylightSaving);
            Assert.False(string.IsNullOrWhiteSpace(record.Identifier));
            Assert.False(string.IsNullOrWhiteSpace(record.StandardName));
            Assert.False(string.IsNullOrWhiteSpace(record.DisplayName));
        }

        [Fact]
        public void GetTimeZone_ReturnedIdentifier_ResolvesToTheSameZone()
        {
            TimeZoneRecord record = _toolkit.GetTimeZone("Tokyo Standard Time");

            Assert.Equal(record, _toolkit.GetTimeZone(record.Identifier));
        }

        [Fact]
        public void GetTimeZone_AcceptsBothIdentifierStyles()
        {
            Assert.Equal(_toolkit.GetTimeZone("Tokyo Standard Time"), _toolkit.GetTimeZone("Asia/Tokyo"));
        }

        [Fact]
        public void GetTimeZone_TrimsSurroundingWhitespace()
        {
            Assert.Equal(_toolkit.GetTimeZone("Asia/Tokyo"), _toolkit.GetTimeZone("  Asia/Tokyo  "));
        }

        [Fact]
        public void GetTimeZone_ReportsDaylightSavingSupport()
        {
            TimeZoneRecord record = _toolkit.GetTimeZone("America/New_York");

            Assert.True(record.SupportsDaylightSaving);
            Assert.Contains(record.UtcOffset, new[] { -300, -240 });
        }

        [Fact]
        public void GetTimeZone_Utc_NeverObservesDaylightSaving()
        {
            TimeZoneRecord record = _toolkit.GetTimeZone("UTC");

            Assert.Equal(0, record.UtcOffset);
            Assert.False(record.SupportsDaylightSaving);
            Assert.False(record.IsDaylightSaving);
        }

        [Fact]
        public void GetSystemTimeZones_ReturnsAPopulatedList()
        {
            List<TimeZoneRecord> records = _toolkit.GetSystemTimeZones();

            Assert.NotEmpty(records);
            Assert.All(records, record => Assert.False(string.IsNullOrWhiteSpace(record.Identifier)));
            Assert.All(records, record => Assert.False(string.IsNullOrWhiteSpace(record.DisplayName)));
        }

        [Fact]
        public void GetSystemTimeZones_IsOrderedByTheOffsetItReports()
        {
            List<TimeZoneRecord> records = _toolkit.GetSystemTimeZones();

            for (int i = 1; i < records.Count; i++)
            {
                Assert.True(
                    records[i - 1].UtcOffset <= records[i].UtcOffset,
                    $"'{records[i - 1].Identifier}' ({records[i - 1].UtcOffset}) sorted before " +
                    $"'{records[i].Identifier}' ({records[i].UtcOffset}).");
            }
        }

        [Fact]
        public void GetSystemTimeZones_EveryIdentifierIsResolvable()
        {
            List<TimeZoneRecord> records = _toolkit.GetSystemTimeZones();

            Assert.All(records, record => _toolkit.GetTimeZone(record.Identifier));
        }

        [Fact]
        public void GetCurrentTimeZone_ReturnsTheServerZone()
        {
            TimeZoneRecord record = _toolkit.GetCurrentTimeZone();

            Assert.Equal(TimeZoneInfo.Local.Id, record.Identifier);
            Assert.Equal((int)TimeZoneInfo.Local.GetUtcOffset(DateTimeOffset.UtcNow).TotalMinutes, record.UtcOffset);
        }

        [Fact]
        public void ConvertTimeZoneToIANA_WithoutCountry_UsesTheDefaultMapping()
        {
            Assert.Equal("America/New_York", _toolkit.ConvertTimeZoneToIANA("Eastern Standard Time", string.Empty));
        }

        [Theory]
        [InlineData("CA", "America/Toronto")]
        [InlineData("ca", "America/Toronto")]
        [InlineData(" us ", "America/New_York")]
        public void ConvertTimeZoneToIANA_WithCountry_UsesTheCountryMapping(string country, string expected)
        {
            Assert.Equal(expected, _toolkit.ConvertTimeZoneToIANA("Eastern Standard Time", country));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ConvertTimeZoneToIANA_TreatsBlankCountryAsNotSupplied(string? country)
        {
            Assert.Equal("America/New_York", _toolkit.ConvertTimeZoneToIANA("Eastern Standard Time", country!));
        }

        [Fact]
        public void ConvertTimeZoneToIANA_CountryWithNoMappingForTheZone_FallsBackToTheDefault()
        {
            // Portugal has no Eastern Standard Time mapping, so the default mapping is used
            // instead of failing. This documents the behaviour rather than prescribing it.
            Assert.Equal("America/New_York", _toolkit.ConvertTimeZoneToIANA("Eastern Standard Time", "PT"));
        }

        [Fact]
        public void GetIANATimeZone_ReturnsTheMatchingZone()
        {
            TimeZoneRecord record = _toolkit.GetIANATimeZone("America/New_York");

            Assert.True(record.SupportsDaylightSaving);
            Assert.Contains(record.UtcOffset, new[] { -300, -240 });
            Assert.Equal(_toolkit.GetTimeZone("Eastern Standard Time"), record);
        }

        [Fact]
        public void GetIANATimeZone_TrimsSurroundingWhitespace()
        {
            Assert.Equal(
                _toolkit.GetIANATimeZone("America/New_York"),
                _toolkit.GetIANATimeZone(" America/New_York "));
        }
    }
}
