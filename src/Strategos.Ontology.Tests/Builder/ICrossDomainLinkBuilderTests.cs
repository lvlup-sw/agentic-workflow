using Strategos.Ontology.Builder;

namespace Strategos.Ontology.Tests.Builder;

public record TestAtomicNote(Guid Id, string Content);

public class ICrossDomainLinkBuilderTests
{
    [Test]
    public async Task ICrossDomainLinkBuilder_From_SetsSourceType()
    {
        var substitute = Substitute.For<ICrossDomainLinkBuilder>();
        substitute.From<TestAtomicNote>().Returns(substitute);

        var result = substitute.From<TestAtomicNote>();

        await Assert.That(result).IsEqualTo(substitute);
    }

    [Test]
    public async Task ICrossDomainLinkBuilder_ToExternal_SetsDomainAndType()
    {
        var substitute = Substitute.For<ICrossDomainLinkBuilder>();
        substitute.ToExternal("trading", "Strategy").Returns(substitute);

        var result = substitute.ToExternal("trading", "Strategy");

        await Assert.That(result).IsEqualTo(substitute);
    }

    [Test]
    public async Task ICrossDomainLinkBuilder_ManyToMany_SetsCardinality()
    {
        var substitute = Substitute.For<ICrossDomainLinkBuilder>();
        substitute.ManyToMany().Returns(substitute);

        var result = substitute.ManyToMany();

        await Assert.That(result).IsEqualTo(substitute);
    }

    [Test]
    public async Task ICrossDomainLinkBuilder_WithEdge_AllowsEdgeProperties()
    {
        var substitute = Substitute.For<ICrossDomainLinkBuilder>();
        substitute.WithEdge(Arg.Any<Action<IEdgeBuilder>>()).Returns(substitute);

        var result = substitute.WithEdge(edge =>
        {
            edge.Property<double>("Relevance");
            edge.Property<string>("Rationale");
        });

        await Assert.That(result).IsEqualTo(substitute);
    }
}
