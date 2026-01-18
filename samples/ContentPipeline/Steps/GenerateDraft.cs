// =============================================================================
// <copyright file="GenerateDraft.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Steps;
using ContentPipeline.Services;
using ContentPipeline.State;

namespace ContentPipeline.Steps;

/// <summary>
/// Workflow step that generates initial draft content using an LLM service.
/// </summary>
/// <remarks>
/// This step takes the content title and uses the LLM service to generate
/// a first draft of the content. The generated draft is stored in the
/// workflow state along with an audit entry.
/// </remarks>
public sealed class GenerateDraft : IWorkflowStep<ContentState>
{
    private readonly ILlmService _llmService;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateDraft"/> class.
    /// </summary>
    /// <param name="llmService">The LLM service for content generation.</param>
    /// <param name="timeProvider">The time provider for timestamps.</param>
    public GenerateDraft(ILlmService llmService, TimeProvider timeProvider)
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

        var prompt = $"Write an article about: {state.Title}";
        var draft = await _llmService.GenerateDraftAsync(prompt, cancellationToken);

        var timestamp = _timeProvider.GetUtcNow();
        var auditEntry = new AuditEntry(
            Timestamp: timestamp,
            Action: "Draft Generated",
            Actor: "AI",
            Details: $"Generated initial draft for '{state.Title}'");

        var updatedState = state with
        {
            Draft = draft,
            AuditEntries = [.. state.AuditEntries, auditEntry],
        };

        return StepResult<ContentState>.FromState(updatedState);
    }
}
