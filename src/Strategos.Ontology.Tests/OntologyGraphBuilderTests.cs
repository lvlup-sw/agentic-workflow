using Strategos.Ontology.Builder;
using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Tests;

public class TestPosition
{
    public string Id { get; set; } = "";
    public string Symbol { get; set; } = "";
}

public class TestInstrument
{
    public string Ticker { get; set; } = "";
    public decimal Price { get; set; }
}

public class TestTradingOntology : DomainOntology
{
    public override string DomainName => "trading";

    protected override void Define(IOntologyBuilder builder)
    {
        builder.Object<TestPosition>(obj =>
        {
            obj.Key(p => p.Id);
            obj.Property(p => p.Symbol).Required();
        });
    }
}

public class TestMarketDataOntology : DomainOntology
{
    public override string DomainName => "market-data";

    protected override void Define(IOntologyBuilder builder)
    {
        builder.Object<TestInstrument>(obj =>
        {
            obj.Key(i => i.Ticker);
            obj.Property(i => i.Price).Required();
        });
    }
}

public class OntologyGraphBuilderTests
{
    [Test]
    public async Task OntologyGraphBuilder_AddDomain_RegistersDomainOntology()
    {
        var graphBuilder = new OntologyGraphBuilder();

        graphBuilder.AddDomain<TestTradingOntology>();

        var graph = graphBuilder.Build();
        await Assert.That(graph.Domains).HasCount().EqualTo(1);
        await Assert.That(graph.Domains[0].DomainName).IsEqualTo("trading");
    }

    [Test]
    public async Task OntologyGraphBuilder_AddDomain_Multiple_AllRegistered()
    {
        var graphBuilder = new OntologyGraphBuilder();

        graphBuilder.AddDomain<TestTradingOntology>();
        graphBuilder.AddDomain<TestMarketDataOntology>();

        var graph = graphBuilder.Build();
        await Assert.That(graph.Domains).HasCount().EqualTo(2);
    }

    [Test]
    public async Task OntologyGraphBuilder_Build_ProducesOntologyGraph()
    {
        var graphBuilder = new OntologyGraphBuilder();
        graphBuilder.AddDomain<TestTradingOntology>();

        var graph = graphBuilder.Build();

        await Assert.That(graph).IsNotNull();
        await Assert.That(graph).IsTypeOf<OntologyGraph>();
    }

    [Test]
    public async Task OntologyGraphBuilder_Build_DomainDescriptorsPopulated()
    {
        var graphBuilder = new OntologyGraphBuilder();
        graphBuilder.AddDomain<TestTradingOntology>();

        var graph = graphBuilder.Build();

        await Assert.That(graph.Domains[0].ObjectTypes).HasCount().EqualTo(1);
        await Assert.That(graph.Domains[0].ObjectTypes[0].Name).IsEqualTo("TestPosition");
        await Assert.That(graph.ObjectTypes).HasCount().EqualTo(1);
        await Assert.That(graph.ObjectTypes[0].Name).IsEqualTo("TestPosition");
    }
}
