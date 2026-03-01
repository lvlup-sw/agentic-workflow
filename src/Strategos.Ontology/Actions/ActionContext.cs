namespace Strategos.Ontology.Actions;

/// <summary>
/// Identifies the target of an ontology action invocation.
/// </summary>
public sealed record ActionContext(
    string Domain,
    string ObjectType,
    string ObjectId,
    string ActionName);
