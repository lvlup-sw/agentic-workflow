// =============================================================================
// <copyright file="StepConfigurationDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Definitions;

/// <summary>
/// Immutable configuration for a workflow step.
/// </summary>
/// <remarks>
/// <para>
/// Step configuration aggregates all configurable aspects of a workflow step:
/// <list type="bullet">
///   <item><description>ConfidenceThreshold: Minimum confidence for automatic continuation</description></item>
///   <item><description>OnLowConfidence: Handler path when confidence is below threshold</description></item>
///   <item><description>Compensation: Rollback step configuration</description></item>
///   <item><description>Retry: Retry policy configuration</description></item>
///   <item><description>Timeout: Maximum execution time</description></item>
///   <item><description>Validation: State validation guard before step execution</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record StepConfigurationDefinition
{
    /// <summary>
    /// Gets the confidence threshold for agent steps (0.0 to 1.0).
    /// </summary>
    public double? ConfidenceThreshold { get; init; }

    /// <summary>
    /// Gets the low confidence handler definition.
    /// </summary>
    public LowConfidenceHandlerDefinition? OnLowConfidence { get; init; }

    /// <summary>
    /// Gets the compensation configuration.
    /// </summary>
    public CompensationConfiguration? Compensation { get; init; }

    /// <summary>
    /// Gets the retry configuration.
    /// </summary>
    public RetryConfiguration? Retry { get; init; }

    /// <summary>
    /// Gets the step execution timeout.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets the validation configuration for state guards.
    /// </summary>
    /// <remarks>
    /// Validation guards run before step execution using the Guard-Then-Dispatch pattern.
    /// Failures transition to ValidationFailed phase instead of throwing exceptions.
    /// </remarks>
    public ValidationDefinition? Validation { get; init; }

    /// <summary>
    /// Gets the RAG context configuration for this step.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Context configuration specifies sources for RAG context assembly:
    /// <list type="bullet">
    ///   <item><description>State properties injected as context</description></item>
    ///   <item><description>Retrieval queries against RAG collections</description></item>
    ///   <item><description>Static literal context strings</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public ContextDefinition? Context { get; init; }

    /// <summary>
    /// Gets an empty step configuration.
    /// </summary>
    public static StepConfigurationDefinition Empty { get; } = new();

    /// <summary>
    /// Creates a step configuration with a confidence threshold.
    /// </summary>
    /// <param name="threshold">The confidence threshold (0.0 to 1.0).</param>
    /// <returns>A new step configuration with the confidence threshold set.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="threshold"/> is less than 0.0 or greater than 1.0.
    /// </exception>
    public static StepConfigurationDefinition WithConfidence(double threshold)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(threshold, 0.0, nameof(threshold));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(threshold, 1.0, nameof(threshold));

        return new StepConfigurationDefinition { ConfidenceThreshold = threshold };
    }

    /// <summary>
    /// Returns a new configuration with retry settings.
    /// </summary>
    /// <param name="retry">The retry configuration.</param>
    /// <returns>A new step configuration with retry settings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="retry"/> is null.</exception>
    public StepConfigurationDefinition WithRetry(RetryConfiguration retry)
    {
        ArgumentNullException.ThrowIfNull(retry, nameof(retry));

        return this with { Retry = retry };
    }

    /// <summary>
    /// Returns a new configuration with compensation settings.
    /// </summary>
    /// <param name="compensation">The compensation configuration.</param>
    /// <returns>A new step configuration with compensation settings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="compensation"/> is null.</exception>
    public StepConfigurationDefinition WithCompensation(CompensationConfiguration compensation)
    {
        ArgumentNullException.ThrowIfNull(compensation, nameof(compensation));

        return this with { Compensation = compensation };
    }

    /// <summary>
    /// Returns a new configuration with low confidence handler.
    /// </summary>
    /// <param name="handler">The low confidence handler definition.</param>
    /// <returns>A new step configuration with the handler set.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
    public StepConfigurationDefinition WithLowConfidenceHandler(LowConfidenceHandlerDefinition handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        return this with { OnLowConfidence = handler };
    }

    /// <summary>
    /// Returns a new configuration with a timeout.
    /// </summary>
    /// <param name="timeout">The step execution timeout.</param>
    /// <returns>A new step configuration with the timeout set.</returns>
    public StepConfigurationDefinition WithTimeout(TimeSpan timeout)
    {
        return this with { Timeout = timeout };
    }

    /// <summary>
    /// Returns a new configuration with validation settings.
    /// </summary>
    /// <param name="validation">The validation definition.</param>
    /// <returns>A new step configuration with validation settings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validation"/> is null.</exception>
    public StepConfigurationDefinition WithValidation(ValidationDefinition validation)
    {
        ArgumentNullException.ThrowIfNull(validation, nameof(validation));

        return this with { Validation = validation };
    }

    /// <summary>
    /// Returns a new configuration with RAG context settings.
    /// </summary>
    /// <param name="context">The context definition.</param>
    /// <returns>A new step configuration with context settings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public StepConfigurationDefinition WithContext(ContextDefinition context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        return this with { Context = context };
    }
}
