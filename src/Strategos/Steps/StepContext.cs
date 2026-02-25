// =============================================================================
// <copyright file="StepContext.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Steps;

/// <summary>
/// Immutable context provided to workflow steps during execution.
/// </summary>
/// <remarks>
/// <para>
/// Step context provides execution metadata to steps:
/// <list type="bullet">
///   <item><description>CorrelationId: Distributed tracing correlation</description></item>
///   <item><description>WorkflowId: Parent workflow identifier</description></item>
///   <item><description>StepName: Current step being executed</description></item>
///   <item><description>CurrentPhase: Generated phase enum value</description></item>
///   <item><description>Timestamp: Execution start time</description></item>
///   <item><description>RetryCount: Number of retry attempts</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record StepContext
{
    /// <summary>
    /// Gets the correlation ID for distributed tracing.
    /// </summary>
    public required string CorrelationId { get; init; }

    /// <summary>
    /// Gets the parent workflow identifier.
    /// </summary>
    public required Guid WorkflowId { get; init; }

    /// <summary>
    /// Gets the name of the step being executed.
    /// </summary>
    public required string StepName { get; init; }

    /// <summary>
    /// Gets the timestamp when step execution started.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Gets the current workflow phase (maps to generated Phase enum).
    /// </summary>
    public required string CurrentPhase { get; init; }

    /// <summary>
    /// Gets the number of retry attempts for this step.
    /// </summary>
    public int RetryCount { get; init; }

    /// <summary>
    /// Creates a new step context with auto-generated correlation ID and timestamp.
    /// </summary>
    /// <param name="workflowId">The parent workflow identifier.</param>
    /// <param name="stepName">The step name.</param>
    /// <param name="currentPhase">The current workflow phase.</param>
    /// <returns>A new step context.</returns>
    public static StepContext Create(Guid workflowId, string stepName, string currentPhase)
    {
        return new StepContext
        {
            CorrelationId = Guid.NewGuid().ToString("N"),
            WorkflowId = workflowId,
            StepName = stepName,
            Timestamp = DateTimeOffset.UtcNow,
            CurrentPhase = currentPhase,
        };
    }
}
