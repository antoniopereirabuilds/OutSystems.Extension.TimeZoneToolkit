using System.Reflection;
using OutSystems.ExternalLibraries.SDK;
using OutSystems.TimeZoneToolkit;

namespace OutSystems.TimeZoneToolkit.UnitTests
{
    /// <summary>
    /// Guards the parts of the library that ODC inspects at upload time. A mistake here is only
    /// visible when the ZIP is rejected in the ODC Portal, so it is cheaper to catch it in CI.
    /// </summary>
    public class OdcContractTests
    {
        private static readonly OSInterfaceAttribute Attribute =
            typeof(ITimeZoneToolkit).GetCustomAttribute<OSInterfaceAttribute>()!;

        [Fact]
        public void TheInterfaceIsExposedToOdc()
        {
            Assert.NotNull(Attribute);
            Assert.Equal("TimeZoneToolkit", Attribute.Name);
            Assert.False(string.IsNullOrWhiteSpace(Attribute.Description));
        }

        [Fact]
        public void TheIconIsEmbeddedUnderTheDeclaredResourceName()
        {
            string[] resources = typeof(ITimeZoneToolkit).Assembly.GetManifestResourceNames();

            Assert.Contains(Attribute.IconResourceName, resources);
        }

        [Fact]
        public void ExactlyOnePublicClassImplementsTheInterface()
        {
            Type[] implementations = typeof(ITimeZoneToolkit).Assembly
                .GetExportedTypes()
                .Where(type => type.IsClass && typeof(ITimeZoneToolkit).IsAssignableFrom(type))
                .ToArray();

            Assert.Single(implementations);
            Assert.NotNull(implementations[0].GetConstructor(Type.EmptyTypes));
        }

        [Fact]
        public void EveryActionIsDocumentedForOdcStudio()
        {
            // ODC reads the generated XML file to describe the actions in ODC Studio.
            string xmlPath = Path.ChangeExtension(typeof(ITimeZoneToolkit).Assembly.Location, ".xml");
            Assert.True(File.Exists(xmlPath), $"Missing XML documentation file at '{xmlPath}'.");

            string xml = File.ReadAllText(xmlPath);
            foreach (MethodInfo action in typeof(ITimeZoneToolkit).GetMethods())
            {
                Assert.Contains($"ITimeZoneToolkit.{action.Name}", xml);
            }
        }

        [Fact]
        public void TheStructureIsExposedToOdc()
        {
            Assert.NotNull(typeof(TimeZoneRecord).GetCustomAttribute<OSStructureAttribute>());
            Assert.All(
                typeof(TimeZoneRecord).GetFields(BindingFlags.Public | BindingFlags.Instance),
                field => Assert.NotNull(field.GetCustomAttribute<OSStructureFieldAttribute>()));
        }
    }
}
