using System.Linq.Expressions;
using Strategos.Ontology.Actions;
using Strategos.Ontology.Events;

namespace Strategos.Ontology.ObjectSets;

/// <summary>
/// A lazy, composable query expression over ontology-typed domain objects.
/// Analogous to IQueryable&lt;T&gt; but operating over the ontology graph.
/// Each operation returns a new immutable instance.
/// </summary>
/// <typeparam name="T">The domain object type.</typeparam>
public sealed class ObjectSet<T> where T : class
{
    private readonly IObjectSetProvider _provider;
    private readonly IActionDispatcher _actionDispatcher;
    private readonly IEventStreamProvider _eventStreamProvider;

    /// <summary>
    /// Creates a new root ObjectSet for the given type.
    /// </summary>
    public ObjectSet(IObjectSetProvider provider, IActionDispatcher actionDispatcher, IEventStreamProvider eventStreamProvider)
        : this(new RootExpression(typeof(T)), provider, actionDispatcher, eventStreamProvider)
    {
    }

    internal ObjectSet(ObjectSetExpression expression, IObjectSetProvider provider, IActionDispatcher actionDispatcher, IEventStreamProvider eventStreamProvider)
    {
        Expression = expression;
        _provider = provider;
        _actionDispatcher = actionDispatcher;
        _eventStreamProvider = eventStreamProvider;
    }

    /// <summary>
    /// The expression tree representing this query.
    /// </summary>
    public ObjectSetExpression Expression { get; }

    /// <summary>
    /// Filters the object set by the given predicate. Returns a new immutable ObjectSet.
    /// </summary>
    public ObjectSet<T> Where(Expression<Func<T, bool>> predicate)
    {
        var filterExpr = new FilterExpression(Expression, predicate);
        return new ObjectSet<T>(filterExpr, _provider, _actionDispatcher, _eventStreamProvider);
    }

    /// <summary>
    /// Traverses a named link to produce an ObjectSet of the linked type.
    /// </summary>
    public ObjectSet<TLinked> TraverseLink<TLinked>(string linkName) where TLinked : class
    {
        var traverseExpr = new TraverseLinkExpression(Expression, linkName, typeof(TLinked));
        return new ObjectSet<TLinked>(traverseExpr, _provider, _actionDispatcher, _eventStreamProvider);
    }

    /// <summary>
    /// Narrows the object set to objects implementing the given interface type.
    /// </summary>
    public ObjectSet<TInterface> OfInterface<TInterface>() where TInterface : class
    {
        var narrowExpr = new InterfaceNarrowExpression(Expression, typeof(TInterface));
        return new ObjectSet<TInterface>(narrowExpr, _provider, _actionDispatcher, _eventStreamProvider);
    }

    /// <summary>
    /// Specifies which data facets to include in the result. Returns a new immutable ObjectSet.
    /// </summary>
    public ObjectSet<T> Include(ObjectSetInclusion inclusion)
    {
        var includeExpr = new IncludeExpression(Expression, inclusion);
        return new ObjectSet<T>(includeExpr, _provider, _actionDispatcher, _eventStreamProvider);
    }
}
