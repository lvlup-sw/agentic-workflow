using Strategos.Ontology.Builder;

namespace Strategos.Ontology.Tests.Builder;

public class PropertyBuilderTests
{
    [Test]
    public async Task PropertyBuilder_Build_ProducesDescriptorWithName()
    {
        var builder = new PropertyBuilder("Symbol", typeof(string));

        var descriptor = builder.Build();

        await Assert.That(descriptor.Name).IsEqualTo("Symbol");
        await Assert.That(descriptor.PropertyType).IsEqualTo(typeof(string));
    }

    [Test]
    public async Task PropertyBuilder_Required_SetsIsRequired()
    {
        var builder = new PropertyBuilder("Symbol", typeof(string));

        builder.Required();
        var descriptor = builder.Build();

        await Assert.That(descriptor.IsRequired).IsTrue();
    }

    [Test]
    public async Task PropertyBuilder_Computed_SetsIsComputed()
    {
        var builder = new PropertyBuilder("PnL", typeof(decimal));

        builder.Computed();
        var descriptor = builder.Build();

        await Assert.That(descriptor.IsComputed).IsTrue();
    }

    [Test]
    public async Task PropertyBuilder_ChainedCalls_AllApplied()
    {
        var builder = new PropertyBuilder("Symbol", typeof(string));

        builder.Required().Computed();
        var descriptor = builder.Build();

        await Assert.That(descriptor.IsRequired).IsTrue();
        await Assert.That(descriptor.IsComputed).IsTrue();
    }
}
