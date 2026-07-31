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

        // ODC Studio shows the SDK attribute descriptions, not the XML comments. An action or
        // parameter without one appears blank in the toolbox and the property pane.

        [Fact]
        public void EveryActionHasADescriptionAndANamedReturn()
        {
            foreach (MethodInfo action in typeof(ITimeZoneToolkit).GetMethods())
            {
                OSActionAttribute? attribute = action.GetCustomAttribute<OSActionAttribute>();
                Assert.True(attribute is not null, $"{action.Name} has no [OSAction].");

                Assert.False(string.IsNullOrWhiteSpace(attribute!.Description),
                    $"{action.Name} has no description.");
                Assert.False(string.IsNullOrWhiteSpace(attribute.ReturnName),
                    $"{action.Name} has no ReturnName.");
                Assert.False(string.IsNullOrWhiteSpace(attribute.ReturnDescription),
                    $"{action.Name} has no ReturnDescription.");
            }
        }

        [Fact]
        public void EveryParameterHasADescription()
        {
            foreach (MethodInfo action in typeof(ITimeZoneToolkit).GetMethods())
            {
                foreach (ParameterInfo parameter in action.GetParameters())
                {
                    OSParameterAttribute? attribute = parameter.GetCustomAttribute<OSParameterAttribute>();
                    Assert.True(attribute is not null,
                        $"{action.Name}.{parameter.Name} has no [OSParameter].");
                    Assert.False(string.IsNullOrWhiteSpace(attribute!.Description),
                        $"{action.Name}.{parameter.Name} has no description.");
                }
            }
        }

        [Fact]
        public void EveryStructureFieldHasADescription()
        {
            Assert.False(string.IsNullOrWhiteSpace(
                typeof(TimeZoneRecord).GetCustomAttribute<OSStructureAttribute>()!.Description));

            foreach (FieldInfo field in typeof(TimeZoneRecord).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                OSStructureFieldAttribute attribute = field.GetCustomAttribute<OSStructureFieldAttribute>()!;
                Assert.False(string.IsNullOrWhiteSpace(attribute.Description),
                    $"TimeZoneRecord.{field.Name} has no description.");
            }
        }

        [Fact]
        public void NoReturnNameCollidesWithAnInputParameter()
        {
            // ODC exposes the return as an output parameter, so a name shared with an input
            // is rejected when the library is uploaded.
            foreach (MethodInfo action in typeof(ITimeZoneToolkit).GetMethods())
            {
                string returnName = action.GetCustomAttribute<OSActionAttribute>()!.ReturnName!;
                string[] inputs = action.GetParameters().Select(p => p.Name!).ToArray();

                Assert.DoesNotContain(returnName, inputs, StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
