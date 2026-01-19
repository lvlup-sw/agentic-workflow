// =============================================================================
// <copyright file="ContentState.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Attributes;

namespace ContentPipeline.State;

/// <summary>
/// Represents the state of a content publishing workflow.
/// </summary>
/// <remarks>
/// <para>
/// This state record tracks the lifecycle of content from draft creation
/// through AI review, human approval, and publication.
/// </para>
/// <para>
/// Key state properties:
/// <list type="bullet">
///   <item><description>Title and Draft: The content being processed</description></item>
///   <item><description>AiReviewFeedback and AiQualityScore: AI review results</description></item>
///   <item><description>HumanDecision: Human approval/rejection with audit info</description></item>
///   <item><description>PublishedAt and PublishedUrl: Publication details</description></item>
///   <item><description>AuditEntries: Complete audit trail of all actions</description></item>
/// </list>
/// </para>
/// </remarks>
[WorkflowState]
public sealed record ContentState : IWorkflowState
{
    /// <inheritdoc/>
    public Guid WorkflowId { get; init; }

    /// <summary>
    /// Gets the title of the content.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current draft content.
    /// </summary>
    public string Draft { get; init; } = string.Empty;

    /// <summary>
    /// Gets the feedback from the AI review step.
    /// </summary>
    public string? AiReviewFeedback { get; init; }

    /// <summary>
    /// Gets the quality score assigned by AI review (0.0 to 1.0).
    /// </summary>
    public decimal AiQualityScore { get; init; }

    /// <summary>
    /// Gets the human approval decision.
    /// </summary>
    public ApprovalDecision? HumanDecision { get; init; }

    /// <summary>
    /// Gets the timestamp when content was published.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; init; }

    /// <summary>
    /// Gets the URL where content was published.
    /// </summary>
    public string? PublishedUrl { get; init; }

    /// <summary>
    /// Gets the audit trail of all actions taken on this content.
    /// </summary>
    [Append]
    public IReadOnlyList<AuditEntry> AuditEntries { get; init; } = [];
}
