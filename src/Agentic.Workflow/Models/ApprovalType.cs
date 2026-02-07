// =============================================================================
// <copyright file="ApprovalType.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Models;

/// <summary>
/// Defines the types of approval that can be requested from humans.
/// </summary>
public enum ApprovalType
{
    /// <summary>
    /// Escalation from loop detection recovery.
    /// </summary>
    /// <remarks>
    /// Triggered when the system detects repeated failures or semantic loops
    /// and determines that human intervention is needed to break the cycle.
    /// </remarks>
    LoopEscalation,

    /// <summary>
    /// Goal is ambiguous, need clarification.
    /// </summary>
    /// <remarks>
    /// Triggered when a specialist signals that the task goal is unclear
    /// and requires human clarification before proceeding.
    /// </remarks>
    GoalClarification,

    /// <summary>
    /// Missing data, need user to provide input.
    /// </summary>
    /// <remarks>
    /// Triggered when execution requires data that can only be provided
    /// by the user (e.g., credentials, preferences, missing context).
    /// </remarks>
    DataRequest,

    /// <summary>
    /// Safety check before sensitive operation.
    /// </summary>
    /// <remarks>
    /// Triggered before executing operations that could have significant
    /// impact (e.g., data deletion, external API calls with side effects).
    /// </remarks>
    SafetyCheck,

    /// <summary>
    /// General approval checkpoint.
    /// </summary>
    /// <remarks>
    /// Used for general-purpose approval requests that don't fit
    /// the other categories.
    /// </remarks>
    GeneralApproval
}