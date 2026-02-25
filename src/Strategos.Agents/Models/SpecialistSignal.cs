// =============================================================================
// <copyright file="SpecialistSignal.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Agents.Models;

/// <summary>
/// Represents a signal emitted by a specialist to communicate completion status to the Orchestrator.
/// </summary>
/// <remarks>
/// <para>
/// This is the unified signaling protocol used by all specialists. The signal type determines
/// which data property is populated: <see cref="SuccessData"/>, <see cref="FailureData"/>,
/// or <see cref="HelpNeededData"/>.
/// </para>
/// <para>
/// Signals are immutable records that become part of the progress ledger, providing
/// a complete audit trail of specialist communications.
/// </para>
/// </remarks>
public sealed record SpecialistSignal
{
    /// <summary>
    /// Gets the type of signal being emitted.
    /// </summary>
    public required SignalType Type { get; init; }

    /// <summary>
    /// Gets the specialist that emitted this signal.
    /// </summary>
    public required SpecialistType Specialist { get; init; }

    /// <summary>
    /// Gets the timestamp when the signal was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the success data when <see cref="Type"/> is <see cref="SignalType.Success"/>.
    /// </summary>
    /// <value>Success details, or <c>null</c> if the signal type is not Success.</value>
    public SuccessSignalData? SuccessData { get; init; }

    /// <summary>
    /// Gets the failure data when <see cref="Type"/> is <see cref="SignalType.Failure"/>.
    /// </summary>
    /// <value>Failure details, or <c>null</c> if the signal type is not Failure.</value>
    public FailureSignalData? FailureData { get; init; }

    /// <summary>
    /// Gets the help needed data when <see cref="Type"/> is <see cref="SignalType.HelpNeeded"/>.
    /// </summary>
    /// <value>Blocker details, or <c>null</c> if the signal type is not HelpNeeded.</value>
    public HelpNeededSignalData? HelpNeededData { get; init; }

    /// <summary>
    /// Gets the usage metrics collected during task execution.
    /// </summary>
    /// <remarks>
    /// Contains actual resource consumption (tokens, executions, tool calls, duration).
    /// Used by the Orchestrator for budget commit operations.
    /// </remarks>
    /// <value>Usage metrics, or <c>null</c> if not tracked.</value>
    public UsageMetrics? Usage { get; init; }

    /// <summary>
    /// Creates a success signal with the specified data.
    /// </summary>
    /// <param name="specialist">The specialist emitting the signal.</param>
    /// <param name="result">The result produced by the specialist.</param>
    /// <param name="confidence">Confidence score between 0.0 and 1.0.</param>
    /// <param name="artifacts">Paths to any artifacts produced.</param>
    /// <param name="nextSuggestion">Optional suggestion for the next action.</param>
    /// <param name="usage">Optional usage metrics from task execution.</param>
    /// <returns>A new success signal.</returns>
    public static SpecialistSignal Success(
        SpecialistType specialist,
        string result,
        double confidence,
        IReadOnlyList<string>? artifacts = null,
        string? nextSuggestion = null,
        UsageMetrics? usage = null)
    {
        return new SpecialistSignal
        {
            Type = SignalType.Success,
            Specialist = specialist,
            SuccessData = new SuccessSignalData
            {
                Result = result,
                Confidence = confidence,
                Artifacts = artifacts ?? [],
                NextSuggestion = nextSuggestion
            },
            Usage = usage
        };
    }

    /// <summary>
    /// Creates a failure signal with the specified data.
    /// </summary>
    /// <param name="specialist">The specialist emitting the signal.</param>
    /// <param name="reason">The reason for the failure.</param>
    /// <param name="partialResult">Any partial result that was produced.</param>
    /// <param name="recoveryHints">Hints for potential recovery strategies.</param>
    /// <param name="shouldRetry">Whether the operation should be retried.</param>
    /// <param name="usage">Optional usage metrics from partial execution.</param>
    /// <returns>A new failure signal.</returns>
    public static SpecialistSignal Failure(
        SpecialistType specialist,
        string reason,
        string? partialResult = null,
        IReadOnlyList<string>? recoveryHints = null,
        bool shouldRetry = false,
        UsageMetrics? usage = null)
    {
        return new SpecialistSignal
        {
            Type = SignalType.Failure,
            Specialist = specialist,
            FailureData = new FailureSignalData
            {
                Reason = reason,
                PartialResult = partialResult,
                RecoveryHints = recoveryHints ?? [],
                ShouldRetry = shouldRetry
            },
            Usage = usage
        };
    }

    /// <summary>
    /// Creates a help needed signal with the specified data.
    /// </summary>
    /// <param name="specialist">The specialist emitting the signal.</param>
    /// <param name="blocker">The type of blocker preventing progress.</param>
    /// <param name="context">Additional context about the blocker.</param>
    /// <param name="suggestions">Suggested resolutions for the blocker.</param>
    /// <param name="canProceedPartial">Whether partial progress can be made without full resolution.</param>
    /// <returns>A new help needed signal.</returns>
    public static SpecialistSignal HelpNeeded(
        SpecialistType specialist,
        BlockerType blocker,
        string context,
        IReadOnlyList<string>? suggestions = null,
        bool canProceedPartial = false)
    {
        return new SpecialistSignal
        {
            Type = SignalType.HelpNeeded,
            Specialist = specialist,
            HelpNeededData = new HelpNeededSignalData
            {
                Blocker = blocker,
                Context = context,
                Suggestions = suggestions ?? [],
                CanProceedPartial = canProceedPartial
            }
        };
    }
}

/// <summary>
/// Data associated with a successful task completion signal.
/// </summary>
public sealed record SuccessSignalData
{
    /// <summary>
    /// Gets the result produced by the specialist.
    /// </summary>
    /// <remarks>
    /// This may be a summary, extracted data, or a reference to a larger artifact.
    /// </remarks>
    public required string Result { get; init; }

    /// <summary>
    /// Gets the confidence score for the result (0.0 to 1.0).
    /// </summary>
    /// <remarks>
    /// A confidence below 0.7 typically indicates the Orchestrator should verify
    /// the result or seek additional confirmation.
    /// </remarks>
    public required double Confidence { get; init; }

    /// <summary>
    /// Gets the paths to any artifacts produced during task execution.
    /// </summary>
    /// <remarks>
    /// Artifacts are stored in the shared volume and referenced by path.
    /// Examples include generated files, charts, or exported data.
    /// </remarks>
    public IReadOnlyList<string> Artifacts { get; init; } = [];

    /// <summary>
    /// Gets an optional suggestion for the next action.
    /// </summary>
    /// <remarks>
    /// The specialist may suggest a follow-up task based on its findings.
    /// The Orchestrator considers but is not bound by this suggestion.
    /// </remarks>
    public string? NextSuggestion { get; init; }
}

/// <summary>
/// Data associated with a task failure signal.
/// </summary>
public sealed record FailureSignalData
{
    /// <summary>
    /// Gets the reason for the failure.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Gets any partial result that was produced before failure.
    /// </summary>
    /// <remarks>
    /// May be useful for debugging or as context for retry attempts.
    /// </remarks>
    public string? PartialResult { get; init; }

    /// <summary>
    /// Gets hints for potential recovery strategies.
    /// </summary>
    /// <remarks>
    /// Suggestions from the specialist about how to retry or work around the failure.
    /// </remarks>
    public IReadOnlyList<string> RecoveryHints { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the operation should be retried.
    /// </summary>
    /// <remarks>
    /// True for transient failures that may succeed on retry (e.g., timeouts).
    /// False for permanent failures that require a different approach.
    /// </remarks>
    public bool ShouldRetry { get; init; }
}

/// <summary>
/// Data associated with a help needed signal.
/// </summary>
public sealed record HelpNeededSignalData
{
    /// <summary>
    /// Gets the type of blocker preventing progress.
    /// </summary>
    public required BlockerType Blocker { get; init; }

    /// <summary>
    /// Gets additional context about the blocker.
    /// </summary>
    /// <remarks>
    /// Human-readable description of why the specialist is blocked
    /// and what information or action is needed.
    /// </remarks>
    public required string Context { get; init; }

    /// <summary>
    /// Gets suggested resolutions for the blocker.
    /// </summary>
    /// <remarks>
    /// The specialist's suggestions for how to resolve the blocker,
    /// which the Orchestrator may act upon.
    /// </remarks>
    public IReadOnlyList<string> Suggestions { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether partial progress can be made.
    /// </summary>
    /// <remarks>
    /// If true, the specialist can continue with reduced scope or
    /// quality while waiting for full resolution.
    /// </remarks>
    public bool CanProceedPartial { get; init; }
}
