using Strategos.Ontology.Actions;
using Strategos.Ontology.Descriptors;
using Strategos.Ontology.ObjectSets;

namespace Strategos.Ontology.MCP;

/// <summary>
/// MCP tool for ontology write operations.
/// Routes action requests to IActionDispatcher with ActionContext.
/// </summary>
public sealed class OntologyActionTool
{
    private readonly OntologyGraph _graph;
    private readonly IActionDispatcher _actionDispatcher;
    private readonly IObjectSetProvider _objectSetProvider;

    public OntologyActionTool(
        OntologyGraph graph,
        IActionDispatcher actionDispatcher,
        IObjectSetProvider objectSetProvider)
    {
        _graph = graph;
        _actionDispatcher = actionDispatcher;
        _objectSetProvider = objectSetProvider;
    }

    /// <summary>
    /// Executes an action on a single object or a filtered set of objects.
    /// </summary>
    public async Task<ActionToolResult> ExecuteAsync(
        string objectType,
        string action,
        object request,
        string? domain = null,
        string? objectId = null,
        string? filter = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);

        var resolvedDomain = domain ?? ResolveDomain(objectType);

        var objectTypeDescriptor = resolvedDomain is not null
            ? _graph.GetObjectType(resolvedDomain, objectType)
            : null;

        if (objectTypeDescriptor is null)
        {
            return new ActionToolResult(
            [
                new ActionResult(false, Error: $"Object type '{objectType}' not found in domain '{resolvedDomain ?? "unknown"}'."),
            ]);
        }

        if (!HasAction(objectTypeDescriptor, action))
        {
            return new ActionToolResult(
            [
                new ActionResult(false, Error: $"Action '{action}' not found on object type '{objectType}'."),
            ]);
        }

        if (objectId is not null)
        {
            return await DispatchSingleAsync(resolvedDomain!, objectType, objectId, action, request, ct)
                .ConfigureAwait(false);
        }

        return await DispatchBatchAsync(resolvedDomain!, objectType, action, request, filter, ct)
            .ConfigureAwait(false);
    }

    private async Task<ActionToolResult> DispatchSingleAsync(
        string domain,
        string objectType,
        string objectId,
        string action,
        object request,
        CancellationToken ct)
    {
        var context = new ActionContext(domain, objectType, objectId, action);
        var result = await _actionDispatcher.DispatchAsync(context, request, ct).ConfigureAwait(false);
        return new ActionToolResult([result]);
    }

    private async Task<ActionToolResult> DispatchBatchAsync(
        string domain,
        string objectType,
        string action,
        object request,
        string? filter,
        CancellationToken ct)
    {
        var clrType = _graph.GetObjectType(domain, objectType)?.ClrType ?? typeof(object);
        ObjectSetExpression expression = new RootExpression(clrType);

        if (filter is not null)
        {
            expression = new RawFilterExpression(expression, filter);
        }

        var queryResult = await _objectSetProvider.ExecuteAsync<object>(expression, ct).ConfigureAwait(false);

        var results = new List<ActionResult>(queryResult.Items.Count);
        foreach (var item in queryResult.Items)
        {
            var itemId = item?.ToString() ?? string.Empty;
            var context = new ActionContext(domain, objectType, itemId, action);
            try
            {
                var actionResult = await _actionDispatcher.DispatchAsync(context, request, ct).ConfigureAwait(false);
                results.Add(actionResult);
            }
            catch (Exception ex)
            {
                results.Add(new ActionResult(false, Error: $"Dispatch failed for '{itemId}': {ex.Message}"));
            }
        }

        return new ActionToolResult(results);
    }

    private static bool HasAction(ObjectTypeDescriptor objectType, string actionName) =>
        objectType.Actions.Any(a => a.Name == actionName);

    private string? ResolveDomain(string objectType)
    {
        foreach (var domain in _graph.Domains)
        {
            foreach (var ot in domain.ObjectTypes)
            {
                if (ot.Name == objectType)
                {
                    return domain.DomainName;
                }
            }
        }

        return null;
    }
}
