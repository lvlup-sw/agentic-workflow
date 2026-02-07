// =============================================================================
// <copyright file="BudgetOptions.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace Agentic.Workflow.Configuration;

/// <summary>
/// Configuration options for workflow budget allocation and management.
/// </summary>
/// <remarks>
/// <para>
/// The Budget Algebra provides a formal framework for resource tracking and scarcity-aware
/// decision making. These options configure default allocations and scaling factors.
/// </para>
/// <para>
/// Bind to configuration section: <c>"Workflow:Budget"</c>
/// </para>
/// </remarks>
public sealed class BudgetOptions : IValidatableObject
{
    /// <summary>
    /// Gets the configuration section key for binding from appsettings.json.
    /// </summary>
    public static string Key => "Workflow:Budget";

    /// <summary>
    /// Gets or sets the number of orchestrator steps allocated per complexity unit.
    /// </summary>
    /// <value>Default is 5 steps per complexity unit.</value>
    /// <remarks>
    /// Used when calculating initial budget based on task complexity estimation.
    /// </remarks>
    public int StepsPerComplexityUnit { get; set; } = 5;

    /// <summary>
    /// Gets or sets the average number of tokens consumed per orchestrator step.
    /// </summary>
    /// <value>Default is 2000 tokens per step.</value>
    /// <remarks>
    /// Includes tokens for agent prompts, tool descriptions, and model responses.
    /// </remarks>
    public int AverageTokensPerStep { get; set; } = 2000;

    /// <summary>
    /// Gets or sets the ratio of steps that involve code execution.
    /// </summary>
    /// <value>Default is 0.6 (60% of steps involve execution).</value>
    /// <remarks>
    /// Used to estimate execution budget from step budget.
    /// </remarks>
    public double ExecutionRatio { get; set; } = 0.6;

    /// <summary>
    /// Gets or sets the average number of tool calls per step.
    /// </summary>
    /// <value>Default is 1.5 tool calls per step.</value>
    /// <remarks>
    /// Used to estimate tool call budget from step budget.
    /// </remarks>
    public double ToolCallRatio { get; set; } = 1.5;

    /// <summary>
    /// Gets or sets the fraction of budget reserved for retries.
    /// </summary>
    /// <value>Default is 0.2 (20% reserved for retries).</value>
    /// <remarks>
    /// Budget is reduced by this fraction to ensure retry capacity.
    /// </remarks>
    public double RetryMargin { get; set; } = 0.2;

    /// <summary>
    /// Gets or sets the default step budget for tasks.
    /// </summary>
    /// <value>Default is 25 steps.</value>
    public int DefaultStepBudget { get; set; } = 25;

    /// <summary>
    /// Gets or sets the default token budget for tasks.
    /// </summary>
    /// <value>Default is 50,000 tokens.</value>
    public int DefaultTokenBudget { get; set; } = 50000;

    /// <summary>
    /// Gets or sets the default execution budget for tasks.
    /// </summary>
    /// <value>Default is 15 executions.</value>
    public int DefaultExecutionBudget { get; set; } = 15;

    /// <summary>
    /// Gets or sets the default tool call budget for tasks.
    /// </summary>
    /// <value>Default is 40 tool calls.</value>
    public int DefaultToolCallBudget { get; set; } = 40;

    /// <summary>
    /// Gets or sets the default wall time budget in seconds.
    /// </summary>
    /// <value>Default is 300 seconds (5 minutes).</value>
    public int DefaultWallTimeSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the scarcity multiplier for Abundant level.
    /// </summary>
    /// <value>Default is 1.0.</value>
    public double AbundantMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the scarcity multiplier for Normal level.
    /// </summary>
    /// <value>Default is 1.5.</value>
    public double NormalMultiplier { get; set; } = 1.5;

    /// <summary>
    /// Gets or sets the scarcity multiplier for Scarce level.
    /// </summary>
    /// <value>Default is 3.0.</value>
    public double ScarceMultiplier { get; set; } = 3.0;

    /// <summary>
    /// Gets or sets the scarcity multiplier for Critical level.
    /// </summary>
    /// <value>Default is 10.0.</value>
    public double CriticalMultiplier { get; set; } = 10.0;

    /// <summary>
    /// Gets environment-specific default configuration for development scenarios.
    /// </summary>
    /// <returns>Configuration optimized for development with conservative budgets.</returns>
    public static BudgetOptions CreateDevelopmentDefaults() => new()
    {
        DefaultStepBudget = 15,
        DefaultTokenBudget = 25000,
        DefaultExecutionBudget = 10,
        DefaultToolCallBudget = 25,
        DefaultWallTimeSeconds = 180
    };

    /// <summary>
    /// Gets environment-specific default configuration for production scenarios.
    /// </summary>
    /// <returns>Configuration optimized for production workloads.</returns>
    public static BudgetOptions CreateProductionDefaults() => new()
    {
        DefaultStepBudget = 25,
        DefaultTokenBudget = 50000,
        DefaultExecutionBudget = 15,
        DefaultToolCallBudget = 40,
        DefaultWallTimeSeconds = 300
    };

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StepsPerComplexityUnit <= 0)
        {
            yield return new ValidationResult(
                "StepsPerComplexityUnit must be greater than 0",
                [nameof(StepsPerComplexityUnit)]);
        }

        if (AverageTokensPerStep <= 0)
        {
            yield return new ValidationResult(
                "AverageTokensPerStep must be greater than 0",
                [nameof(AverageTokensPerStep)]);
        }

        if (ExecutionRatio is < 0 or > 1)
        {
            yield return new ValidationResult(
                "ExecutionRatio must be between 0 and 1",
                [nameof(ExecutionRatio)]);
        }

        if (ToolCallRatio < 0)
        {
            yield return new ValidationResult(
                "ToolCallRatio cannot be negative",
                [nameof(ToolCallRatio)]);
        }

        if (RetryMargin is < 0 or > 0.5)
        {
            yield return new ValidationResult(
                "RetryMargin must be between 0 and 0.5 (50% maximum)",
                [nameof(RetryMargin)]);
        }

        if (DefaultStepBudget <= 0)
        {
            yield return new ValidationResult(
                "DefaultStepBudget must be greater than 0",
                [nameof(DefaultStepBudget)]);
        }

        if (DefaultTokenBudget <= 0)
        {
            yield return new ValidationResult(
                "DefaultTokenBudget must be greater than 0",
                [nameof(DefaultTokenBudget)]);
        }

        if (DefaultWallTimeSeconds <= 0)
        {
            yield return new ValidationResult(
                "DefaultWallTimeSeconds must be greater than 0",
                [nameof(DefaultWallTimeSeconds)]);
        }

        // Validate multipliers are in increasing order
        if (AbundantMultiplier > NormalMultiplier ||
            NormalMultiplier > ScarceMultiplier ||
            ScarceMultiplier > CriticalMultiplier)
        {
            yield return new ValidationResult(
                "Scarcity multipliers must be in increasing order: Abundant < Normal < Scarce < Critical",
                [nameof(AbundantMultiplier), nameof(NormalMultiplier), nameof(ScarceMultiplier), nameof(CriticalMultiplier)]);
        }
    }
}