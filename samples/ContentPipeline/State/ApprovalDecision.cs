// =============================================================================
// <copyright file="ApprovalDecision.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace ContentPipeline.State;

/// <summary>
/// Represents a human approval or rejection decision.
/// </summary>
/// <param name="Approved">Whether the content was approved.</param>
/// <param name="Feedback">Optional feedback from the reviewer.</param>
/// <param name="ReviewerId">The identifier of the reviewer.</param>
/// <param name="DecisionTime">When the decision was made.</param>
public sealed record ApprovalDecision(
    bool Approved,
    string? Feedback,
    string ReviewerId,
    DateTimeOffset DecisionTime);
