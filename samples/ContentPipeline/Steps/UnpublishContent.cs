// =============================================================================
// <copyright file="UnpublishContent.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Abstractions;
using Strategos.Steps;
using ContentPipeline.Services;
using ContentPipeline.State;

namespace ContentPipeline.Steps;

/// <summary>
/// Compensation step that unpublishes content if issues arise post-publication.
/// </summary>
/// <remarks>
/// This step is the compensating action for <see cref="PublishContent"/>.
/// It removes published content from the target platform when post-publication
/// issues are detected or when the workflow needs to be rolled back.
/// </remarks>
public sealed class UnpublishContent : IWorkflowStep<ContentState>
{
    private readonly IPublishingService _publishingService;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnpublishContent"/> class.
    /// </summary>
    /// <param name="publishingService">The publishing service.</param>
    /// <param name="timeProvider">The time provider for timestamps.</param>
    public UnpublishContent(IPublishingService publishingService, TimeProvider timeProvider)
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

        // Skip if content is not published
        if (string.IsNullOrEmpty(state.PublishedUrl))
        {
            return StepResult<ContentState>.FromState(state);
        }

        var unpublished = await _publishingService.UnpublishAsync(state.PublishedUrl, cancellationToken);
        var timestamp = _timeProvider.GetUtcNow();

        if (!unpublished)
        {
            var failedAudit = new AuditEntry(
                Timestamp: timestamp,
                Action: "Content Unpublish Failed (Compensation)",
                Actor: "System",
                Details: $"Failed to remove: {state.PublishedUrl}");

            var failedState = state with
            {
                AuditEntries = [.. state.AuditEntries, failedAudit],
            };

            return StepResult<ContentState>.FromState(failedState);
        }

        var auditEntry = new AuditEntry(
            Timestamp: timestamp,
            Action: "Content Unpublished (Compensation)",
            Actor: "System",
            Details: $"Removed from: {state.PublishedUrl}");

        var updatedState = state with
        {
            PublishedUrl = null,
            PublishedAt = null,
            AuditEntries = [.. state.AuditEntries, auditEntry],
        };

        return StepResult<ContentState>.FromState(updatedState);
    }
}
