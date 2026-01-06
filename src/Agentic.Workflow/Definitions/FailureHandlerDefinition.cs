// =============================================================================
// <copyright file="FailureHandlerDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Specifies the scope at which a failure handler operates.
/// </summary>
public enum FailureHandlerScope
{
    /// <summary>
    /// Workflow-level failure handler that catches any step failure.
    /// </summary>
    Workflow,

    /// <summary>
    /// Step-level failure handler that catches only specific step failures.
    /// </summary>
    Step,

    /// <summary>
    /// Fork path-level failure handler that catches failures within a fork path.
    /// </summary>
    ForkPath,
}

/// <summary>
/// Immutable definition of a failure handler within a workflow.
/// </summary>
/// <remarks>
/// <para>
/// A failure handler defines the recovery path when a step or workflow fails.
/// Handlers can be workflow-scoped (catch-all) or step-scoped (targeted).
/// </para>
/// </remarks>
public sealed record FailureHandlerDefinition
{
    /// <summary>
    /// Gets the unique identifier for this failure handler.
    /// </summary>
    public required string HandlerId { get; init; }

    /// <summary>
    /// Gets the scope at which this failure handler operates.
    /// </summary>
    public required FailureHandlerScope Scope { get; init; }

    /// <summary>
    /// Gets the identifier of the step that triggers this handler (for Step scope only).
    /// </summary>
    /// <remarks>
    /// This is null for workflow-scoped handlers and contains the triggering step's
    /// ID for step-scoped handlers.
    /// </remarks>
    public string? TriggerStepId { get; init; }

    /// <summary>
    /// Gets the steps in this failure handler path.
    /// </summary>
    public IReadOnlyList<StepDefinition> Steps { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether this handler terminates (does not rejoin).
    /// </summary>
    public bool IsTerminal { get; init; }

    /// <summary>
    /// Creates a new failure handler definition.
    /// </summary>
    /// <param name="scope">The scope of the failure handler.</param>
    /// <param name="steps">The steps in the failure handler path.</param>
    /// <param name="isTerminal">Whether this handler terminates without rejoining.</param>
    /// <param name="triggerStepId">The triggering step ID (for Step scope only).</param>
    /// <returns>A new failure handler definition.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="steps"/> is null.</exception>
    public static FailureHandlerDefinition Create(
        FailureHandlerScope scope,
        IReadOnlyList<StepDefinition> steps,
        bool isTerminal,
        string? triggerStepId = null)
    {
        ArgumentNullException.ThrowIfNull(steps, nameof(steps));

        return new FailureHandlerDefinition
        {
            HandlerId = Guid.NewGuid().ToString("N"),
            Scope = scope,
            TriggerStepId = triggerStepId,
            Steps = steps,
            IsTerminal = isTerminal,
        };
    }
}
