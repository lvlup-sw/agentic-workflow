namespace Strategos.Ontology.Actions;

/// <summary>
/// Options controlling action dispatch behavior. Preconditions and postconditions
/// are metadata by default; enforcement is opt-in via these options.
/// </summary>
public sealed record ActionDispatchOptions
{
    /// <summary>
    /// Default options with no enforcement.
    /// </summary>
    public static readonly ActionDispatchOptions Default = new();

    /// <summary>
    /// When true, the dispatcher evaluates action preconditions before dispatch
    /// and returns a failure result if any precondition is unsatisfied.
    /// </summary>
    public bool EnforcePreconditions { get; init; }
}
