using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Tests.Descriptors;

public class PropertyDescriptorTests
{
    [Test]
    public async Task PropertyDescriptor_Create_HasNameAndType()
    {
        // Arrange & Act
        var descriptor = new PropertyDescriptor("Symbol", typeof(string));

        // Assert
        await Assert.That(descriptor.Name).IsEqualTo("Symbol");
        await Assert.That(descriptor.PropertyType).IsEqualTo(typeof(string));
    }

    [Test]
    public async Task PropertyDescriptor_Required_DefaultsFalse()
    {
        // Arrange & Act
        var descriptor = new PropertyDescriptor("Symbol", typeof(string));

        // Assert
        await Assert.That(descriptor.IsRequired).IsEqualTo(false);
    }

    [Test]
    public async Task PropertyDescriptor_Computed_DefaultsFalse()
    {
        // Arrange & Act
        var descriptor = new PropertyDescriptor("Symbol", typeof(string));

        // Assert
        await Assert.That(descriptor.IsComputed).IsEqualTo(false);
    }
}
