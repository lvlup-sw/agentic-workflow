// =============================================================================
// <copyright file="ForkPathStatus.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Specifies the execution status of a fork path.
/// </summary>
/// <remarks>
/// <para>
/// Fork paths execute in parallel and each path independently tracks its status.
/// The join step waits until all paths reach a terminal status before executing.
/// </para>
/// </remarks>
public enum ForkPathStatus
{
    /// <summary>
    /// Path has not started execution.
    /// </summary>
    Pending,

    /// <summary>
    /// Path is currently executing.
    /// </summary>
    InProgress,

    /// <summary>
    /// Path completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// Path failed terminally (failure handler called <c>Complete()</c>).
    /// </summary>
    /// <remarks>
    /// When a path fails terminally, its state is not merged into the join result.
    /// </remarks>
    Failed,

    /// <summary>
    /// Path failed but recovered via failure handler (did not call <c>Complete()</c>).
    /// </summary>
    /// <remarks>
    /// When a path recovers, its state is preserved and merged into the join result.
    /// </remarks>
    FailedWithRecovery,
}