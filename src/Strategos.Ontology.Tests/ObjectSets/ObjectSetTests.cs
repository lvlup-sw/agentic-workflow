using System.Linq.Expressions;
using Strategos.Ontology.Actions;
using Strategos.Ontology.Events;
using Strategos.Ontology.ObjectSets;

namespace Strategos.Ontology.Tests.ObjectSets;

public class ObjectSetTests
{
    private IObjectSetProvider _provider = null!;
    private IActionDispatcher _dispatcher = null!;
    private IEventStreamProvider _eventProvider = null!;

    [Before(Test)]
    public Task Setup()
    {
        _provider = Substitute.For<IObjectSetProvider>();
        _dispatcher = Substitute.For<IActionDispatcher>();
        _eventProvider = Substitute.For<IEventStreamProvider>();
        return Task.CompletedTask;
    }

    [Test]
    public async Task ObjectSet_Create_HasRootExpression()
    {
        // Arrange & Act
        var set = new ObjectSet<string>(_provider, _dispatcher, _eventProvider);

        // Assert
        await Assert.That(set.Expression).IsTypeOf<RootExpression>();
        await Assert.That(set.Expression.ObjectType).IsEqualTo(typeof(string));
    }

    [Test]
    public async Task ObjectSet_Where_ReturnsNewObjectSetWithFilterExpression()
    {
        // Arrange
        var set = new ObjectSet<string>(_provider, _dispatcher, _eventProvider);

        // Act
        var filtered = set.Where(s => s.Length > 5);

        // Assert
        await Assert.That(filtered.Expression).IsTypeOf<FilterExpression>();
        var filterExpr = (FilterExpression)filtered.Expression;
        await Assert.That(filterExpr.Source).IsTypeOf<RootExpression>();
    }

    [Test]
    public async Task ObjectSet_Where_PreservesOriginalObjectSet()
    {
        // Arrange
        var set = new ObjectSet<string>(_provider, _dispatcher, _eventProvider);

        // Act
        var filtered = set.Where(s => s.Length > 5);

        // Assert — original is unchanged
        await Assert.That(set.Expression).IsTypeOf<RootExpression>();
        await Assert.That(filtered).IsNotEqualTo(set);
    }

    [Test]
    public async Task ObjectSet_MultipleWheres_ChainsExpressions()
    {
        // Arrange
        var set = new ObjectSet<string>(_provider, _dispatcher, _eventProvider);

        // Act
        var filtered = set
            .Where(s => s.Length > 5)
            .Where(s => s.StartsWith("A"));

        // Assert
        await Assert.That(filtered.Expression).IsTypeOf<FilterExpression>();
        var outerFilter = (FilterExpression)filtered.Expression;
        await Assert.That(outerFilter.Source).IsTypeOf<FilterExpression>();
        var innerFilter = (FilterExpression)outerFilter.Source;
        await Assert.That(innerFilter.Source).IsTypeOf<RootExpression>();
    }

    [Test]
    public async Task ObjectSet_SimilarTo_ReturnsSimilarObjectSet()
    {
        // Arrange
        var set = new ObjectSet<string>(_provider, _dispatcher, _eventProvider);

        // Act
        var similar = set.SimilarTo("search query");

        // Assert
        await Assert.That(similar).IsNotNull();
        await Assert.That(similar.Expression).IsTypeOf<SimilarityExpression>();
    }

    [Test]
    public async Task ObjectSet_SimilarTo_ExpressionHasCorrectProperties()
    {
        // Arrange
        var set = new ObjectSet<string>(_provider, _dispatcher, _eventProvider);

        // Act
        var similar = set.SimilarTo(
            "find related items",
            topK: 10,
            minRelevance: 0.8,
            metric: DistanceMetric.L2,
            embeddingPropertyName: "ContentEmbedding");

        // Assert
        var expr = similar.Expression;
        await Assert.That(expr.QueryText).IsEqualTo("find related items");
        await Assert.That(expr.TopK).IsEqualTo(10);
        await Assert.That(expr.MinRelevance).IsEqualTo(0.8);
        await Assert.That(expr.Metric).IsEqualTo(DistanceMetric.L2);
        await Assert.That(expr.EmbeddingPropertyName).IsEqualTo("ContentEmbedding");
        await Assert.That(expr.Source).IsTypeOf<RootExpression>();
    }

    [Test]
    public async Task ObjectSet_SimilarTo_DefaultParameters()
    {
        // Arrange
        var set = new ObjectSet<string>(_provider, _dispatcher, _eventProvider);

        // Act
        var similar = set.SimilarTo("query");

        // Assert
        var expr = similar.Expression;
        await Assert.That(expr.TopK).IsEqualTo(5);
        await Assert.That(expr.MinRelevance).IsEqualTo(0.7);
        await Assert.That(expr.Metric).IsEqualTo(DistanceMetric.Cosine);
        await Assert.That(expr.EmbeddingPropertyName).IsNull();
        await Assert.That(expr.QueryVector).IsNull();
    }

    [Test]
    public async Task ObjectSet_SimilarTo_WithQueryVector()
    {
        // Arrange
        var set = new ObjectSet<string>(_provider, _dispatcher, _eventProvider);
        var vector = new float[] { 0.1f, 0.2f, 0.3f };

        // Act
        var similar = set.SimilarTo("query", queryVector: vector);

        // Assert
        await Assert.That(similar.Expression.QueryVector).IsNotNull();
        await Assert.That(similar.Expression.QueryVector!.Length).IsEqualTo(3);
    }

    [Test]
    public async Task ObjectSet_SimilarTo_AfterWhere_ChainsExpressions()
    {
        // Arrange
        var set = new ObjectSet<string>(_provider, _dispatcher, _eventProvider);

        // Act
        var similar = set.Where(s => s.Length > 5).SimilarTo("query");

        // Assert
        var expr = similar.Expression;
        await Assert.That(expr.Source).IsTypeOf<FilterExpression>();
    }
}
