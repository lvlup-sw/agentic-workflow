// =============================================================================
// <copyright file="ExecutorSignal.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using MemoryPack;

namespace Strategos.Orchestration.Ledgers;

/// <summary>
/// Represents a signal emitted by an executor during workflow execution.
/// </summary>
/// <remarks>
/// <para>
/// Signals are the structured way executors communicate their state and needs
/// back to the workflow orchestrator. Each signal includes the executor ID,
/// signal type, and optional context-specific data.
/// </para>
/// </remarks>
[MemoryPackable]
public partial record ExecutorSignal
{
    /// <summary>
    /// Gets the identifier of the executor that emitted this signal.
    /// </summary>
    public required string ExecutorId { get; init; }

    /// <summary>
    /// Gets the type of signal emitted.
    /// </summary>
    public required SignalType Type { get; init; }

    /// <summary>
    /// Gets the timestamp when this signal was emitted.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the success data if this is a success signal.
    /// </summary>
    public ExecutorSuccessData? SuccessData { get; init; }

    /// <summary>
    /// Gets the failure data if this is a failure signal.
    /// </summary>
    public ExecutorFailureData? FailureData { get; init; }

    /// <summary>
    /// Gets optional metadata associated with this signal.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Data accompanying a successful executor completion.
/// </summary>
[MemoryPackable]
public partial record ExecutorSuccessData
{
    /// <summary>
    /// Gets the result of the executor's work.
    /// </summary>
    public string? Result { get; init; }

    /// <summary>
    /// Gets the confidence score (0.0 - 1.0) if applicable.
    /// </summary>
    public double? Confidence { get; init; }

    /// <summary>
    /// Gets the list of artifacts produced.
    /// </summary>
    public IReadOnlyList<string> Artifacts { get; init; } = [];
}

/// <summary>
/// Data accompanying an executor failure.
/// </summary>
[MemoryPackable]
public partial record ExecutorFailureData
{
    /// <summary>
    /// Gets the reason for the failure.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Gets the error code if applicable.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Gets a value indicating whether the failure is recoverable.
    /// </summary>
    public bool IsRecoverable { get; init; }
}
