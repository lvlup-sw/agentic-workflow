// =============================================================================
// <copyright file="LowConfidenceHandlerDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Immutable definition of a low confidence handler path.
/// </summary>
/// <remarks>
/// <para>
/// Low confidence handlers define alternative paths when agent confidence is below threshold:
/// <list type="bullet">
///   <item><description>HandlerId: Unique identifier for this handler</description></item>
///   <item><description>HandlerSteps: Steps to execute in the handler path</description></item>
///   <item><description>IsTerminal: Whether this handler terminates the workflow</description></item>
///   <item><description>RejoinStepId: The step to rejoin after handler completes (if not terminal)</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record LowConfidenceHandlerDefinition
{
    /// <summary>
    /// Gets the unique identifier for this handler.
    /// </summary>
    public required string HandlerId { get; init; }

    /// <summary>
    /// Gets the steps in the handler path.
    /// </summary>
    public IReadOnlyList<StepDefinition> HandlerSteps { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether this handler terminates the workflow.
    /// </summary>
    public bool IsTerminal { get; init; }

    /// <summary>
    /// Gets the step ID to rejoin after handler completes (null if terminal).
    /// </summary>
    public string? RejoinStepId { get; init; }

    /// <summary>
    /// Creates a low confidence handler definition.
    /// </summary>
    /// <param name="steps">The steps in the handler path.</param>
    /// <param name="isTerminal">Whether this handler terminates the workflow.</param>
    /// <returns>A new low confidence handler definition.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="steps"/> is null.</exception>
    public static LowConfidenceHandlerDefinition Create(
        IReadOnlyList<StepDefinition> steps,
        bool isTerminal = false)
    {
        ArgumentNullException.ThrowIfNull(steps, nameof(steps));

        return new LowConfidenceHandlerDefinition
        {
            HandlerId = Guid.NewGuid().ToString("N"),
            HandlerSteps = steps,
            IsTerminal = isTerminal,
        };
    }

    /// <summary>
    /// Creates a new handler definition with the specified rejoin step ID.
    /// </summary>
    /// <param name="stepId">The step ID to rejoin after handler completes.</param>
    /// <returns>A new handler definition with the rejoin step set.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepId"/> is null.</exception>
    public LowConfidenceHandlerDefinition WithRejoin(string stepId)
    {
        ArgumentNullException.ThrowIfNull(stepId, nameof(stepId));

        return this with { RejoinStepId = stepId };
    }
}