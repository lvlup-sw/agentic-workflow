// =============================================================================
// <copyright file="PublishContent.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Steps;
using ContentPipeline.Services;
using ContentPipeline.State;

namespace ContentPipeline.Steps;

/// <summary>
/// Workflow step that publishes approved content.
/// </summary>
/// <remarks>
/// This step publishes the content to the target platform. It only
/// executes if the human approval decision was positive. The published
/// URL and timestamp are recorded in the workflow state.
/// </remarks>
public sealed class PublishContent : IWorkflowStep<ContentState>
{
    private readonly IPublishingService _publishingService;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishContent"/> class.
    /// </summary>
    /// <param name="publishingService">The publishing service.</param>
    /// <param name="timeProvider">The time provider for timestamps.</param>
    public PublishContent(IPublishingService publishingService, TimeProvider timeProvider)
    {
        _publishingService = publishingService ?? throw new ArgumentNullException(nameof(publishingService));
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

        // Skip publishing if not approved
        if (state.HumanDecision?.Approved != true)
        {
            return StepResult<ContentState>.FromState(state);
        }

        var publishedUrl = await _publishingService.PublishAsync(
            state.Title,
            state.Draft,
            cancellationToken);

        var timestamp = _timeProvider.GetUtcNow();
        var auditEntry = new AuditEntry(
            Timestamp: timestamp,
            Action: "Content Published",
            Actor: "System",
            Details: $"Published to: {publishedUrl}");

        var updatedState = state with
        {
            PublishedUrl = publishedUrl,
            PublishedAt = timestamp,
            AuditEntries = [.. state.AuditEntries, auditEntry],
        };

        return StepResult<ContentState>.FromState(updatedState);
    }
}
