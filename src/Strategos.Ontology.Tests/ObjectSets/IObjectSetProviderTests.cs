using Strategos.Ontology.ObjectSets;

namespace Strategos.Ontology.Tests.ObjectSets;

public class IObjectSetProviderTests
{
    [Test]
    public async Task IObjectSetProvider_ExecuteAsync_MethodSignatureExists()
    {
        // Arrange
        var provider = Substitute.For<IObjectSetProvider>();
        var expression = new RootExpression(typeof(string));
        var expected = new ObjectSetResult<string>(["hello"], 1, ObjectSetInclusion.Properties);
        provider.ExecuteAsync<string>(expression, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await provider.ExecuteAsync<string>(expression, CancellationToken.None);

        // Assert
        await Assert.That(result.Items).HasCount().EqualTo(1);
        await Assert.That(result.TotalCount).IsEqualTo(1);
    }

    [Test]
    public async Task IObjectSetProvider_StreamAsync_MethodSignatureExists()
    {
        // Arrange
        var provider = Substitute.For<IObjectSetProvider>();
        var expression = new RootExpression(typeof(string));

        static async IAsyncEnumerable<string> CreateStream()
        {
            yield return "a";
            yield return "b";
            await Task.CompletedTask;
        }

        provider.StreamAsync<string>(expression, Arg.Any<CancellationToken>())
            .Returns(CreateStream());

        // Act
        var items = new List<string>();
        await foreach (var item in provider.StreamAsync<string>(expression, CancellationToken.None))
        {
            items.Add(item);
        }

        // Assert
        await Assert.That(items).HasCount().EqualTo(2);
    }
}
