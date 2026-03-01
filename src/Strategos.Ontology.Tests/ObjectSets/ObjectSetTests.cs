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
}
