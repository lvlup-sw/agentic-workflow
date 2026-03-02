using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Tests.Descriptors;

public class LinkDescriptorTests
{
    [Test]
    public async Task LinkCardinality_HasExpectedValues()
    {
        // Assert
        await Assert.That(Enum.IsDefined(LinkCardinality.OneToOne)).IsTrue();
        await Assert.That(Enum.IsDefined(LinkCardinality.OneToMany)).IsTrue();
        await Assert.That(Enum.IsDefined(LinkCardinality.ManyToMany)).IsTrue();
    }

    [Test]
    public async Task LinkDescriptor_Create_HasNameCardinalityAndTargetType()
    {
        // Arrange & Act
        var descriptor = new LinkDescriptor("Orders", "TradeOrder", LinkCardinality.OneToMany);

        // Assert
        await Assert.That(descriptor.Name).IsEqualTo("Orders");
        await Assert.That(descriptor.TargetTypeName).IsEqualTo("TradeOrder");
        await Assert.That(descriptor.Cardinality).IsEqualTo(LinkCardinality.OneToMany);
    }

    [Test]
    public async Task LinkDescriptor_EdgeProperties_DefaultsEmpty()
    {
        // Arrange & Act
        var descriptor = new LinkDescriptor("Orders", "TradeOrder", LinkCardinality.OneToMany);

        // Assert
        await Assert.That(descriptor.EdgeProperties).IsNotNull();
        await Assert.That(descriptor.EdgeProperties.Count).IsEqualTo(0);
    }
}
