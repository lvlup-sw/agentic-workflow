// =============================================================================
// <copyright file="AiReviewContent.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Steps;
using ContentPipeline.Services;
using ContentPipeline.State;

namespace ContentPipeline.Steps;

/// <summary>
/// Workflow step that reviews content using an AI service.
/// </summary>
/// <remarks>
/// This step takes the current draft and submits it to the LLM service
/// for quality review. The feedback and score are stored in the
/// workflow state for human reviewers to consider.
/// </remarks>
public sealed class AiReviewContent : IWorkflowStep<ContentState>
{
    private readonly ILlmService _llmService;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiReviewContent"/> class.
    /// </summary>
    /// <param name="llmService">The LLM service for content review.</param>
    /// <param name="timeProvider">The time provider for timestamps.</param>
    public AiReviewContent(ILlmService llmService, TimeProvider timeProvider)
    {
        _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <inheritdoc/>
    public async Task<StepResult<ContentState>> ExecuteAsync(
        ContentState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        var (feedback, score) = await _llmService.ReviewContentAsync(state.Draft, cancellationToken);

        var timestamp = _timeProvider.GetUtcNow();
        var auditEntry = new AuditEntry(
            Timestamp: timestamp,
            Action: "AI Review Completed",
            Actor: "AI",
            Details: $"Quality score: {score:P0}. Feedback: {feedback}");

        var updatedState = state with
        {
            AiReviewFeedback = feedback,
            AiQualityScore = score,
            AuditEntries = [.. state.AuditEntries, auditEntry],
        };

        return StepResult<ContentState>.FromState(updatedState);
    }
}
