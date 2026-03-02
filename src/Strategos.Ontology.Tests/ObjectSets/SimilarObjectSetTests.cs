using Strategos.Ontology.ObjectSets;

namespace Strategos.Ontology.Tests.ObjectSets;

public class SimilarObjectSetTests
{
    [Test]
    public async Task SimilarObjectSet_Create_HasExpression()
    {
        // Arrange
        var root = new RootExpression(typeof(string));
        var similarity = new SimilarityExpression(root, "query", 5, 0.7);
        var provider = Substitute.For<IObjectSetProvider>();

        // Act
        var similarSet = new SimilarObjectSet<string>(similarity, provider);

        // Assert
        await Assert.That(similarSet.Expression).IsEqualTo(similarity);
    }

    [Test]
    public async Task SimilarObjectSet_ExecuteAsync_DelegatesToProvider()
    {
        // Arrange
        var root = new RootExpression(typeof(string));
        var similarity = new SimilarityExpression(root, "query", 5, 0.7);
        var provider = Substitute.For<IObjectSetProvider>();
        var expected = new ScoredObjectSetResult<string>(
            ["a", "b"], 2, ObjectSetInclusion.Properties, [0.9, 0.8]);
        provider.ExecuteSimilarityAsync<string>(similarity, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var similarSet = new SimilarObjectSet<string>(similarity, provider);

        // Act
        var result = await similarSet.ExecuteAsync();

        // Assert
        await Assert.That(result.Items).HasCount().EqualTo(2);
        await Assert.That(result.Scores[0]).IsEqualTo(0.9);
        await Assert.That(result.Scores[1]).IsEqualTo(0.8);
    }

    [Test]
    public async Task SimilarObjectSet_ExecuteAsync_PassesCancellationToken()
    {
        // Arrange
        var root = new RootExpression(typeof(string));
        var similarity = new SimilarityExpression(root, "query", 5, 0.7);
        var provider = Substitute.For<IObjectSetProvider>();
        var expected = new ScoredObjectSetResult<string>(
            [], 0, ObjectSetInclusion.Properties, []);
        provider.ExecuteSimilarityAsync<string>(similarity, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var similarSet = new SimilarObjectSet<string>(similarity, provider);
        using var cts = new CancellationTokenSource();

        // Act
        await similarSet.ExecuteAsync(cts.Token);

        // Assert
        await provider.Received(1).ExecuteSimilarityAsync<string>(
            similarity, cts.Token);
    }
}
