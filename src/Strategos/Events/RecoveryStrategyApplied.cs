// =============================================================================
// <copyright file="RecoveryStrategyApplied.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Orchestration.LoopDetection;

namespace Strategos.Events;

/// <summary>
/// Event raised when a recovery strategy is applied to escape a detected loop.
/// </summary>
/// <remarks>
/// <para>
/// This event follows a <see cref="LoopDetected"/> event and captures which
/// recovery strategy was applied and any contextual information about the
/// recovery action taken.
/// </para>
/// </remarks>
/// <param name="WorkflowId">The unique identifier for the workflow.</param>
/// <param name="Strategy">The recovery strategy that was applied.</param>
/// <param name="LoopType">The type of loop that triggered recovery.</param>
/// <param name="ActionTaken">A description of the specific action taken.</param>
/// <param name="Timestamp">The timestamp when the strategy was applied.</param>
public sealed record RecoveryStrategyApplied(
    Guid WorkflowId,
    LoopRecoveryStrategy Strategy,
    LoopType LoopType,
    string ActionTaken,
    DateTimeOffset Timestamp) : IWorkflowEvent;
