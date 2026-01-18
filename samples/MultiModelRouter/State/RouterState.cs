// =============================================================================
// <copyright file="RouterState.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;

namespace MultiModelRouter.State;

/// <summary>
/// Workflow state for the multi-model router sample.
/// </summary>
/// <remarks>
/// <para>
/// This state tracks the progression of a query through the multi-model router:
/// <list type="bullet">
///   <item><description>Classification: Determine query category</description></item>
///   <item><description>Model Selection: Use Thompson Sampling to pick optimal model</description></item>
///   <item><description>Response Generation: Generate response with selected model</description></item>
///   <item><description>Feedback Recording: Capture user feedback for learning</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record RouterState : IWorkflowState
{
    /// <inheritdoc/>
    public Guid WorkflowId { get; init; }

    /// <summary>
    /// Gets the user's input query.
    /// </summary>
    public string UserQuery { get; init; } = string.Empty;

    /// <summary>
    /// Gets the classified category of the query.
    /// </summary>
    public QueryCategory Category { get; init; }

    /// <summary>
    /// Gets the model selected for response generation.
    /// </summary>
    public string SelectedModel { get; init; } = string.Empty;

    /// <summary>
    /// Gets the generated response.
    /// </summary>
    public string Response { get; init; } = string.Empty;

    /// <summary>
    /// Gets the confidence score for the response (0.0 to 1.0).
    /// </summary>
    public decimal Confidence { get; init; }

    /// <summary>
    /// Gets the optional user feedback.
    /// </summary>
    public UserFeedback? Feedback { get; init; }
}

