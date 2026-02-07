// =============================================================================
// <copyright file="LoopDetectionOptions.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace Agentic.Workflow.Configuration;

/// <summary>
/// Configuration options for workflow loop detection.
/// </summary>
/// <remarks>
/// <para>
/// Loop detection prevents workflows from consuming infinite resources by identifying
/// repetitive behavior patterns in the progress ledger.
/// </para>
/// <para>
/// Bind to configuration section: <c>"Workflow:LoopDetection"</c>.
/// </para>
/// </remarks>
public sealed class LoopDetectionOptions : IValidatableObject
{
    /// <summary>
    /// Gets the configuration section key for binding from appsettings.json.
    /// </summary>
    public static string Key => "Workflow:LoopDetection";

    /// <summary>
    /// Gets or sets the number of recent entries to analyze for loop detection.
    /// </summary>
    /// <value>Default is 5 entries (sliding window).</value>
    /// <remarks>
    /// Larger windows detect longer-period loops but increase computation cost.
    /// </remarks>
    public int WindowSize { get; set; } = 5;

    /// <summary>
    /// Gets or sets the cosine similarity threshold for semantic repetition detection.
    /// </summary>
    /// <value>Default is 0.85 (85% similarity).</value>
    /// <remarks>
    /// Higher values reduce false positives but may miss subtle repetition.
    /// </remarks>
    public double SimilarityThreshold { get; set; } = 0.85;

    /// <summary>
    /// Gets or sets the maximum number of workflow resets allowed.
    /// </summary>
    /// <value>Default is 3 resets.</value>
    /// <remarks>
    /// After exceeding this limit, the workflow transitions to FAILED state.
    /// </remarks>
    public int MaxResets { get; set; } = 3;

    /// <summary>
    /// Gets or sets the maximum retry attempts per specialist before reassignment.
    /// </summary>
    /// <value>Default is 3 retries.</value>
    /// <remarks>
    /// If a specialist fails this many times, the task is reassigned to another specialist.
    /// </remarks>
    public int MaxSpecialistRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the confidence threshold for triggering recovery.
    /// </summary>
    /// <value>Default is 0.7 (70% confidence).</value>
    /// <remarks>
    /// Loop detection uses confidence scoring. Values above this trigger recovery protocols.
    /// </remarks>
    public double RecoveryThreshold { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets the weight for repetition score in confidence calculation.
    /// </summary>
    /// <value>Default is 0.4 (40%).</value>
    public double RepetitionScoreWeight { get; set; } = 0.4;

    /// <summary>
    /// Gets or sets the weight for semantic similarity in confidence calculation.
    /// </summary>
    /// <value>Default is 0.3 (30%).</value>
    public double SemanticScoreWeight { get; set; } = 0.3;

    /// <summary>
    /// Gets or sets the weight for time since last artifact in confidence calculation.
    /// </summary>
    /// <value>Default is 0.2 (20%).</value>
    public double TimeScoreWeight { get; set; } = 0.2;

    /// <summary>
    /// Gets or sets the weight for specialist frustration in confidence calculation.
    /// </summary>
    /// <value>Default is 0.1 (10%).</value>
    public double FrustrationScoreWeight { get; set; } = 0.1;

    /// <summary>
    /// Gets or sets a value indicating whether loop detection is enabled.
    /// </summary>
    /// <value>Default is true.</value>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets environment-specific default configuration for development scenarios.
    /// </summary>
    /// <returns>Configuration with more aggressive detection for faster feedback.</returns>
    public static LoopDetectionOptions CreateDevelopmentDefaults() => new()
    {
        WindowSize = 3,
        SimilarityThreshold = 0.80,
        MaxResets = 2,
        MaxSpecialistRetries = 2,
        RecoveryThreshold = 0.6
    };

    /// <summary>
    /// Gets environment-specific default configuration for production scenarios.
    /// </summary>
    /// <returns>Configuration balanced for production workloads.</returns>
    public static LoopDetectionOptions CreateProductionDefaults() => new()
    {
        WindowSize = 5,
        SimilarityThreshold = 0.85,
        MaxResets = 3,
        MaxSpecialistRetries = 3,
        RecoveryThreshold = 0.7
    };

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (WindowSize <= 0)
        {
            yield return new ValidationResult(
                "WindowSize must be greater than 0",
                [nameof(WindowSize)]);
        }

        if (WindowSize > 20)
        {
            yield return new ValidationResult(
                "WindowSize should not exceed 20 to avoid excessive computation",
                [nameof(WindowSize)]);
        }

        if (SimilarityThreshold is <= 0 or >= 1)
        {
            yield return new ValidationResult(
                "SimilarityThreshold must be between 0 and 1 (exclusive)",
                [nameof(SimilarityThreshold)]);
        }

        if (MaxResets <= 0)
        {
            yield return new ValidationResult(
                "MaxResets must be greater than 0",
                [nameof(MaxResets)]);
        }

        if (MaxSpecialistRetries <= 0)
        {
            yield return new ValidationResult(
                "MaxSpecialistRetries must be greater than 0",
                [nameof(MaxSpecialistRetries)]);
        }

        if (RecoveryThreshold is <= 0 or >= 1)
        {
            yield return new ValidationResult(
                "RecoveryThreshold must be between 0 and 1 (exclusive)",
                [nameof(RecoveryThreshold)]);
        }

        // Validate weights sum to 1.0 (within tolerance)
        var weightSum = RepetitionScoreWeight + SemanticScoreWeight + TimeScoreWeight + FrustrationScoreWeight;
        if (Math.Abs(weightSum - 1.0) > 0.001)
        {
            yield return new ValidationResult(
                $"Score weights must sum to 1.0 (current sum: {weightSum:F3})",
                [nameof(RepetitionScoreWeight), nameof(SemanticScoreWeight), nameof(TimeScoreWeight), nameof(FrustrationScoreWeight)]);
        }

        // Validate individual weights are non-negative
        if (RepetitionScoreWeight < 0 || SemanticScoreWeight < 0 || TimeScoreWeight < 0 || FrustrationScoreWeight < 0)
        {
            yield return new ValidationResult(
                "Score weights cannot be negative",
                [nameof(RepetitionScoreWeight), nameof(SemanticScoreWeight), nameof(TimeScoreWeight), nameof(FrustrationScoreWeight)]);
        }
    }
}
