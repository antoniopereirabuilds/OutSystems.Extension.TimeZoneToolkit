using System.Text.RegularExpressions;
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
        public void GetSystemTimeZones_IsOrderedByTheOffsetPrintedInTheLabel()
        {
            // A dropdown is read top to bottom, so the offset inside DisplayName is what has to
            // run in order. Sorting on the current UtcOffset instead displaces every zone that
            // happens to be on daylight saving time today.
            List<TimeZoneRecord> records = _toolkit.GetSystemTimeZones();

            for (int i = 1; i < records.Count; i++)
            {
                Assert.True(
                    LabelOffset(records[i - 1].DisplayName) <= LabelOffset(records[i].DisplayName),
                    $"'{records[i - 1].DisplayName}' sorted before '{records[i].DisplayName}'.");
            }
        }

        /// <summary>
        /// Reads the offset out of a label such as "(UTC-03:30) Newfoundland Time (St John's)".
        /// </summary>
        private static int LabelOffset(string displayName)
        {
            Match match = Regex.Match(displayName, @"^\(UTC(?:([+-])(\d{2}):(\d{2}))?\)");
            Assert.True(match.Success, $"Unrecognised label format: '{displayName}'.");

            if (!match.Groups[1].Success)
            {
                return 0;
            }

            int magnitude = (int.Parse(match.Groups[2].Value) * 60) + int.Parse(match.Groups[3].Value);
            return match.Groups[1].Value == "-" ? -magnitude : magnitude;
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

        [Theory]
        [InlineData("Antarctica/Casey", 480)]      // was returning Pacific/Guadalcanal, +3 hours out
        [InlineData("Antarctica/Vostok", 300)]     // was returning Asia/Bishkek, +1 hour out
        [InlineData("America/Scoresbysund", -60)]  // was returning Atlantic/Azores, +1 hour out
        public void GetIANATimeZone_ReturnsTheZoneAsked_NotItsWindowsApproximation(string iana, int expectedOffset)
        {
            // One Windows identifier stands in for many IANA zones, so resolving through a Windows
            // identifier silently substituted a different zone. These offsets are fixed year round.
            TimeZoneRecord record = _toolkit.GetIANATimeZone(iana);

            Assert.Equal(iana, record.Identifier);
            Assert.Equal(expectedOffset, record.UtcOffset);
        }

        [Fact]
        public void GetIANATimeZone_AgreesWithGetTimeZoneForTheSameName()
        {
            foreach (string iana in new[] { "America/Toronto", "Europe/Dublin", "Antarctica/Casey", "Asia/Tokyo" })
            {
                Assert.Equal(_toolkit.GetTimeZone(iana), _toolkit.GetIANATimeZone(iana));
            }
        }

        [Fact]
        public void GetIANATimeZone_AcceptsAZoneWithNoWindowsEquivalent()
        {
            // Antarctica/Troll has no Windows identifier, so the old Windows hop rejected it.
            TimeZoneRecord record = _toolkit.GetIANATimeZone("Antarctica/Troll");

            Assert.Equal("Antarctica/Troll", record.Identifier);
        }

        [Theory]
        [InlineData("Asia/Calcutta", "Asia/Kolkata")]
        [InlineData("Asia/Katmandu", "Asia/Kathmandu")]
        public void GetIANATimeZone_ResolvesDeprecatedNames(string deprecated, string current)
        {
            Assert.Equal(current, _toolkit.GetIANATimeZone(deprecated).Identifier);
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
