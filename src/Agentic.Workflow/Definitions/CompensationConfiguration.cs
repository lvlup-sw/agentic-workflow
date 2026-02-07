// =============================================================================
// <copyright file="CompensationConfiguration.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Immutable configuration for step compensation (rollback) behavior.
/// </summary>
/// <remarks>
/// <para>
/// Compensation configuration captures rollback step settings:
/// <list type="bullet">
///   <item><description>CompensationStepType: The step type to execute for rollback</description></item>
///   <item><description>RequiredOnFailure: Whether compensation is required when the step fails</description></item>
///   <item><description>Timeout: Optional timeout for compensation execution</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record CompensationConfiguration
{
    /// <summary>
    /// Gets the compensation step type.
    /// </summary>
    public required Type CompensationStepType { get; init; }

    /// <summary>
    /// Gets a value indicating whether compensation is required on failure.
    /// </summary>
    public bool RequiredOnFailure { get; init; } = true;

    /// <summary>
    /// Gets the timeout for compensation execution.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Creates a compensation configuration for the specified step type.
    /// </summary>
    /// <param name="stepType">The compensation step type.</param>
    /// <returns>A new compensation configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepType"/> is null.</exception>
    public static CompensationConfiguration Create(Type stepType)
    {
        ArgumentNullException.ThrowIfNull(stepType, nameof(stepType));

        return new CompensationConfiguration
        {
            CompensationStepType = stepType,
        };
    }

    /// <summary>
    /// Creates a compensation configuration for the specified step type.
    /// </summary>
    /// <typeparam name="TStep">The compensation step type.</typeparam>
    /// <returns>A new compensation configuration.</returns>
    public static CompensationConfiguration Create<TStep>()
        where TStep : class
    {
        return new CompensationConfiguration
        {
            CompensationStepType = typeof(TStep),
        };
    }

    /// <summary>
    /// Creates a new compensation configuration with the specified timeout.
    /// </summary>
    /// <param name="timeout">The timeout for compensation execution.</param>
    /// <returns>A new compensation configuration with the timeout set.</returns>
    public CompensationConfiguration WithTimeout(TimeSpan timeout)
    {
        return this with { Timeout = timeout };
    }
}