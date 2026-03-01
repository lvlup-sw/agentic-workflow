using Strategos.Ontology.Builder;
using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Tests.Builder;

public class ObjectTypeBuilderTests
{
    [Test]
    public async Task ObjectTypeBuilder_Key_SetsKeyProperty()
    {
        var builder = new ObjectTypeBuilder<TestPosition>("Trading");

        builder.Key(p => p.Id);
        var descriptor = builder.Build();

        await Assert.That(descriptor.KeyProperty).IsNotNull();
        await Assert.That(descriptor.KeyProperty!.Name).IsEqualTo("Id");
    }

    [Test]
    public async Task ObjectTypeBuilder_Property_AddsPropertyDescriptor()
    {
        var builder = new ObjectTypeBuilder<TestPosition>("Trading");

        builder.Property(p => p.Symbol).Required();
        var descriptor = builder.Build();

        await Assert.That(descriptor.Properties.Count).IsEqualTo(1);
        await Assert.That(descriptor.Properties[0].Name).IsEqualTo("Symbol");
        await Assert.That(descriptor.Properties[0].IsRequired).IsTrue();
    }

    [Test]
    public async Task ObjectTypeBuilder_HasOne_AddsLinkWithOneToOneCardinality()
    {
        var builder = new ObjectTypeBuilder<TestPosition>("Trading");

        builder.HasOne<TestStrategy>("Strategy");
        var descriptor = builder.Build();

        await Assert.That(descriptor.Links.Count).IsEqualTo(1);
        await Assert.That(descriptor.Links[0].Name).IsEqualTo("Strategy");
        await Assert.That(descriptor.Links[0].Cardinality).IsEqualTo(LinkCardinality.OneToOne);
        await Assert.That(descriptor.Links[0].TargetTypeName).IsEqualTo("TestStrategy");
    }

    [Test]
    public async Task ObjectTypeBuilder_HasMany_AddsLinkWithOneToManyCardinality()
    {
        var builder = new ObjectTypeBuilder<TestPosition>("Trading");

        builder.HasMany<TestTradeOrder>("Orders");
        var descriptor = builder.Build();

        await Assert.That(descriptor.Links.Count).IsEqualTo(1);
        await Assert.That(descriptor.Links[0].Name).IsEqualTo("Orders");
        await Assert.That(descriptor.Links[0].Cardinality).IsEqualTo(LinkCardinality.OneToMany);
    }

    [Test]
    public async Task ObjectTypeBuilder_ManyToMany_AddsLinkWithManyToManyCardinality()
    {
        var builder = new ObjectTypeBuilder<TestPosition>("Trading");

        builder.ManyToMany<TestTradeOrder>("RelatedOrders", null);
        var descriptor = builder.Build();

        await Assert.That(descriptor.Links.Count).IsEqualTo(1);
        await Assert.That(descriptor.Links[0].Name).IsEqualTo("RelatedOrders");
        await Assert.That(descriptor.Links[0].Cardinality).IsEqualTo(LinkCardinality.ManyToMany);
    }

    [Test]
    public async Task ObjectTypeBuilder_ManyToManyWithEdge_RecordsEdgeProperties()
    {
        var builder = new ObjectTypeBuilder<TestPosition>("Trading");

        builder.ManyToMany<TestTradeOrder>("RelatedOrders", edge =>
        {
            edge.Property<double>("Relevance");
            edge.Property<string>("Rationale");
        });
        var descriptor = builder.Build();

        await Assert.That(descriptor.Links[0].EdgeProperties.Count).IsEqualTo(2);
        await Assert.That(descriptor.Links[0].EdgeProperties[0].Name).IsEqualTo("Relevance");
        await Assert.That(descriptor.Links[0].EdgeProperties[1].Name).IsEqualTo("Rationale");
    }

    [Test]
    public async Task ObjectTypeBuilder_Action_AddsActionDescriptor()
    {
        var builder = new ObjectTypeBuilder<TestPosition>("Trading");

        builder.Action("ExecuteTrade")
            .Description("Open a new position")
            .BoundToWorkflow("execute-trade");
        var descriptor = builder.Build();

        await Assert.That(descriptor.Actions.Count).IsEqualTo(1);
        await Assert.That(descriptor.Actions[0].Name).IsEqualTo("ExecuteTrade");
        await Assert.That(descriptor.Actions[0].BindingType).IsEqualTo(ActionBindingType.Workflow);
    }

    [Test]
    public async Task ObjectTypeBuilder_Event_AddsEventDescriptor()
    {
        var builder = new ObjectTypeBuilder<TestPosition>("Trading");

        builder.Event<TestTradeExecuted>(evt =>
        {
            evt.Description("A trade was executed");
        });
        var descriptor = builder.Build();

        await Assert.That(descriptor.Events.Count).IsEqualTo(1);
        await Assert.That(descriptor.Events[0].EventType).IsEqualTo(typeof(TestTradeExecuted));
        await Assert.That(descriptor.Events[0].Description).IsEqualTo("A trade was executed");
    }

    [Test]
    public async Task ObjectTypeBuilder_Implements_RecordsInterfaceMapping()
    {
        var builder = new ObjectTypeBuilder<TestPosition>("Trading");

        builder.Implements<ITestSearchable>(map =>
        {
            map.Via(p => p.Symbol, s => s.Title);
        });
        var descriptor = builder.Build();

        await Assert.That(descriptor.ImplementedInterfaces.Count).IsEqualTo(1);
        await Assert.That(descriptor.ImplementedInterfaces[0].InterfaceType).IsEqualTo(typeof(ITestSearchable));
    }
}
