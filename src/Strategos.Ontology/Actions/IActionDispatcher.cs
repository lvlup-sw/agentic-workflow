namespace Strategos.Ontology.Actions;

/// <summary>
/// Dispatches actions to ontology objects.
/// </summary>
public interface IActionDispatcher
{
    /// <summary>
    /// Dispatches an action to the target identified by the context.
    /// </summary>
    Task<ActionResult> DispatchAsync(ActionContext context, object request, CancellationToken ct = default);
}
