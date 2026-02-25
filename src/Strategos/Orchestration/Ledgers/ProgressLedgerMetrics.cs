// =============================================================================
// <copyright file="ProgressLedgerMetrics.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Orchestration.Ledgers;

/// <summary>
/// Aggregate metrics computed from a progress ledger.
/// </summary>
/// <remarks>
/// <para>
/// Metrics provide a summary view of workflow execution progress,
/// useful for monitoring, reporting, and budget tracking.
/// </para>
/// </remarks>
public sealed record ProgressLedgerMetrics
{
    /// <summary>
    /// Gets the total number of entries in the ledger.
    /// </summary>
    public required int TotalEntries { get; init; }

    /// <summary>
    /// Gets the total tokens consumed across all entries.
    /// </summary>
    public required int TotalTokensConsumed { get; init; }

    /// <summary>
    /// Gets the total measured duration across all entries.
    /// </summary>
    public required TimeSpan TotalDuration { get; init; }

    /// <summary>
    /// Gets the count of unique artifacts produced.
    /// </summary>
    public required int UniqueArtifactCount { get; init; }

    /// <summary>
    /// Gets the count of successful signals.
    /// </summary>
    public required int SuccessfulSignalCount { get; init; }

    /// <summary>
    /// Gets the count of failed signals.
    /// </summary>
    public required int FailedSignalCount { get; init; }

    /// <summary>
    /// Creates an empty metrics instance.
    /// </summary>
    /// <returns>Metrics with all values at zero.</returns>
    public static ProgressLedgerMetrics Empty => new()
    {
        TotalEntries = 0,
        TotalTokensConsumed = 0,
        TotalDuration = TimeSpan.Zero,
        UniqueArtifactCount = 0,
        SuccessfulSignalCount = 0,
        FailedSignalCount = 0
    };
}
