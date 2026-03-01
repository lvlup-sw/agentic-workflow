using System.Linq.Expressions;
using Strategos.Ontology.ObjectSets;

namespace Strategos.Ontology.Tests.ObjectSets;

public class ObjectSetExpressionTests
{
    [Test]
    public async Task RootExpression_Create_HasObjectType()
    {
        // Arrange & Act
        var expression = new RootExpression(typeof(string));

        // Assert
        await Assert.That(expression.ObjectType).IsEqualTo(typeof(string));
    }

    [Test]
    public async Task FilterExpression_Create_HasPredicateAndObjectType()
    {
        // Arrange
        var root = new RootExpression(typeof(string));
        Expression<Func<string, bool>> predicate = s => s.Length > 0;

        // Act
        var filter = new FilterExpression(root, predicate);

        // Assert
        await Assert.That(filter.Predicate).IsNotNull();
        await Assert.That(filter.ObjectType).IsEqualTo(typeof(string));
        await Assert.That(filter.Source).IsEqualTo(root);
    }

    [Test]
    public async Task TraverseLinkExpression_Create_HasLinkNameAndSourceExpression()
    {
        // Arrange
        var root = new RootExpression(typeof(string));

        // Act
        var traverse = new TraverseLinkExpression(root, "Children", typeof(int));

        // Assert
        await Assert.That(traverse.LinkName).IsEqualTo("Children");
        await Assert.That(traverse.Source).IsEqualTo(root);
        await Assert.That(traverse.ObjectType).IsEqualTo(typeof(int));
    }

    [Test]
    public async Task InterfaceNarrowExpression_Create_HasInterfaceType()
    {
        // Arrange
        var root = new RootExpression(typeof(string));

        // Act
        var narrow = new InterfaceNarrowExpression(root, typeof(IDisposable));

        // Assert
        await Assert.That(narrow.InterfaceType).IsEqualTo(typeof(IDisposable));
        await Assert.That(narrow.ObjectType).IsEqualTo(typeof(IDisposable));
        await Assert.That(narrow.Source).IsEqualTo(root);
    }
}
