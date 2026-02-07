// =============================================================================
// <copyright file="LoopDetected.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Orchestration.LoopDetection;

namespace Agentic.Workflow.Events;

/// <summary>
/// Event raised when the loop detector identifies repetitive behavior.
/// </summary>
/// <remarks>
/// <para>
/// This event captures the detection of execution loops, which can indicate
/// the workflow is stuck. It includes the loop type, confidence score, and
/// recommended recovery strategy.
/// </para>
/// </remarks>
/// <param name="WorkflowId">The unique identifier for the workflow.</param>
/// <param name="LoopType">The type of loop detected.</param>
/// <param name="Confidence">The confidence score of the detection (0.0 to 1.0).</param>
/// <param name="RecommendedStrategy">The recommended recovery strategy.</param>
/// <param name="Timestamp">The timestamp when the loop was detected.</param>
public sealed record LoopDetected(
    Guid WorkflowId,
    LoopType LoopType,
    double Confidence,
    LoopRecoveryStrategy RecommendedStrategy,
    DateTimeOffset Timestamp) : IWorkflowEvent;