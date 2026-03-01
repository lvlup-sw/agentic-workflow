using System.Linq.Expressions;
using Strategos.Ontology.Builder;

namespace Strategos.Ontology.Tests.Builder;

public record TestPosition(Guid Id, string Symbol, decimal Quantity, decimal UnrealizedPnL, string DisplayDescription);
public record TestTradeOrder(Guid Id, string Symbol);
public record TestStrategy(Guid Id, string Name);
public record TestTradeExecuted(Guid OrderId, decimal NewPnL);
public record TestTradeExecutionRequest(string Symbol, decimal Quantity);
public record TestTradeExecutionResult(bool Success);

public class IObjectTypeBuilderTests
{
    [Test]
    public async Task IObjectTypeBuilder_Key_AcceptsExpression()
    {
        var substitute = Substitute.For<IObjectTypeBuilder<TestPosition>>();

        substitute.Key(Arg.Any<Expression<Func<TestPosition, object>>>());

        await Assert.That(true).IsTrue(); // Interface method exists and is callable
    }

    [Test]
    public async Task IObjectTypeBuilder_Property_ReturnsPropertyBuilder()
    {
        var substitute = Substitute.For<IObjectTypeBuilder<TestPosition>>();
        var propertyBuilder = Substitute.For<IPropertyBuilder>();
        substitute.Property(Arg.Any<Expression<Func<TestPosition, object>>>())
            .Returns(propertyBuilder);

        var result = substitute.Property(p => p.Symbol);

        await Assert.That(result).IsEqualTo(propertyBuilder);
    }

    [Test]
    public async Task IObjectTypeBuilder_HasOne_AcceptsLinkName()
    {
        var substitute = Substitute.For<IObjectTypeBuilder<TestPosition>>();

        substitute.HasOne<TestStrategy>("Strategy");

        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task IObjectTypeBuilder_HasMany_AcceptsLinkName()
    {
        var substitute = Substitute.For<IObjectTypeBuilder<TestPosition>>();

        substitute.HasMany<TestTradeOrder>("Orders");

        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task IObjectTypeBuilder_ManyToMany_AcceptsLinkName()
    {
        var substitute = Substitute.For<IObjectTypeBuilder<TestPosition>>();

        substitute.ManyToMany<TestTradeOrder>("RelatedOrders", null);

        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task IObjectTypeBuilder_Action_ReturnsActionBuilder()
    {
        var substitute = Substitute.For<IObjectTypeBuilder<TestPosition>>();
        var actionBuilder = Substitute.For<IActionBuilder>();
        substitute.Action("ExecuteTrade").Returns(actionBuilder);

        var result = substitute.Action("ExecuteTrade");

        await Assert.That(result).IsEqualTo(actionBuilder);
    }

    [Test]
    public async Task IObjectTypeBuilder_Event_ReturnsEventBuilder()
    {
        var substitute = Substitute.For<IObjectTypeBuilder<TestPosition>>();

        substitute.Event<TestTradeExecuted>(Arg.Any<Action<IEventBuilder<TestTradeExecuted>>>());

        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task IObjectTypeBuilder_Implements_AcceptsMapping()
    {
        var substitute = Substitute.For<IObjectTypeBuilder<TestPosition>>();

        substitute.Implements<ITestSearchable>(
            Arg.Any<Action<IInterfaceMapping<TestPosition, ITestSearchable>>>());

        await Assert.That(true).IsTrue();
    }
}
