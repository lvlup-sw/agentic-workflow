// =============================================================================
// <copyright file="RetryConfiguration.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Immutable configuration for step retry behavior.
/// </summary>
/// <remarks>
/// <para>
/// Retry configuration captures retry policy settings for workflow steps:
/// <list type="bullet">
///   <item><description>MaxAttempts: Maximum number of retry attempts</description></item>
///   <item><description>InitialDelay: Initial delay between retries</description></item>
///   <item><description>BackoffMultiplier: Multiplier for exponential backoff</description></item>
///   <item><description>MaxDelay: Maximum delay cap for backoff</description></item>
///   <item><description>UseJitter: Whether to add random jitter to delays</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record RetryConfiguration
{
    /// <summary>
    /// Gets the maximum number of retry attempts.
    /// </summary>
    public required int MaxAttempts { get; init; }

    /// <summary>
    /// Gets the initial delay between retries.
    /// </summary>
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets the delay multiplier for exponential backoff.
    /// </summary>
    public double BackoffMultiplier { get; init; } = 2.0;

    /// <summary>
    /// Gets the maximum delay between retries.
    /// </summary>
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets a value indicating whether to use jitter.
    /// </summary>
    public bool UseJitter { get; init; } = true;

    /// <summary>
    /// Creates a retry configuration with sensible defaults.
    /// </summary>
    /// <param name="maxAttempts">The maximum number of retry attempts.</param>
    /// <returns>A new retry configuration.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxAttempts"/> is less than 1.</exception>
    public static RetryConfiguration Create(int maxAttempts)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1, nameof(maxAttempts));

        return new RetryConfiguration { MaxAttempts = maxAttempts };
    }

    /// <summary>
    /// Creates a retry configuration with exponential backoff.
    /// </summary>
    /// <param name="maxAttempts">The maximum number of retry attempts.</param>
    /// <param name="initialDelay">The initial delay between retries.</param>
    /// <param name="multiplier">The backoff multiplier (must be greater than 1).</param>
    /// <returns>A new retry configuration with exponential backoff.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxAttempts"/> is less than 1 or
    /// <paramref name="multiplier"/> is less than or equal to 1.
    /// </exception>
    public static RetryConfiguration WithExponentialBackoff(
        int maxAttempts,
        TimeSpan initialDelay,
        double multiplier = 2.0)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1, nameof(maxAttempts));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(multiplier, 1.0, nameof(multiplier));

        return new RetryConfiguration
        {
            MaxAttempts = maxAttempts,
            InitialDelay = initialDelay,
            BackoffMultiplier = multiplier,
        };
    }
}