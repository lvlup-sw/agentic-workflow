// =============================================================================
// <copyright file="TaskFeatures.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Selection;

/// <summary>
/// Represents extracted features from a task description for contextual agent selection.
/// </summary>
/// <remarks>
/// <para>
/// TaskFeatures provides a rich representation of task characteristics beyond simple
/// category classification. This enables more sophisticated agent selection strategies
/// such as complexity-aware priors and contextual bandits.
/// </para>
/// <para>
/// The feature set is designed to be extensible via <see cref="CustomFeatures"/> for
/// future enhancements like embedding vectors or domain-specific attributes.
/// </para>
/// </remarks>
public sealed record TaskFeatures
{
    /// <summary>
    /// Complexity threshold below which a task is considered simple.
    /// </summary>
    public const double SimpleThreshold = 0.3;

    /// <summary>
    /// Complexity threshold above which a task is considered complex.
    /// </summary>
    public const double ComplexThreshold = 0.7;

    /// <summary>
    /// Gets the task category inferred from the task description.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The category determines which belief distribution to sample from when
    /// selecting agents. An agent may have different success rates across categories.
    /// </para>
    /// </remarks>
    public TaskCategory Category { get; init; } = TaskCategory.General;

    /// <summary>
    /// Gets the estimated complexity of the task, normalized to [0, 1].
    /// </summary>
    /// <remarks>
    /// <para>
    /// Complexity can inform prior selection (broader priors for complex tasks)
    /// and budget allocation (more tokens for complex tasks).
    /// </para>
    /// <list type="bullet">
    ///   <item><description>0.0 = trivial task</description></item>
    ///   <item><description>0.5 = moderate complexity</description></item>
    ///   <item><description>1.0 = highly complex task</description></item>
    /// </list>
    /// </remarks>
    public double Complexity { get; init; } = 0.0;

    /// <summary>
    /// Gets the keywords that matched during classification.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Useful for diagnostics and understanding why a particular category was chosen.
    /// </para>
    /// </remarks>
    public IReadOnlyList<string> MatchedKeywords { get; init; } = [];

    /// <summary>
    /// Gets optional custom features for extensibility.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Reserved for future enhancements such as:
    /// <list type="bullet">
    ///   <item><description>Embedding vectors from sentence transformers</description></item>
    ///   <item><description>Domain-specific feature flags</description></item>
    ///   <item><description>Historical task similarity scores</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public IReadOnlyDictionary<string, object>? CustomFeatures { get; init; }

    /// <summary>
    /// Gets a value indicating whether the task is considered simple.
    /// </summary>
    /// <remarks>
    /// Returns true if <see cref="Complexity"/> is below <see cref="SimpleThreshold"/>.
    /// </remarks>
    public bool IsSimple => Complexity < SimpleThreshold;

    /// <summary>
    /// Gets a value indicating whether the task is considered complex.
    /// </summary>
    /// <remarks>
    /// Returns true if <see cref="Complexity"/> is above <see cref="ComplexThreshold"/>.
    /// </remarks>
    public bool IsComplex => Complexity > ComplexThreshold;

    /// <summary>
    /// Creates a default <see cref="TaskFeatures"/> instance.
    /// </summary>
    /// <returns>A new instance with General category and zero complexity.</returns>
    public static TaskFeatures CreateDefault() => new();

    /// <summary>
    /// Creates a <see cref="TaskFeatures"/> instance with the specified category.
    /// </summary>
    /// <param name="category">The task category.</param>
    /// <returns>A new instance with the specified category and zero complexity.</returns>
    public static TaskFeatures CreateForCategory(TaskCategory category) => new()
    {
        Category = category,
    };
}
