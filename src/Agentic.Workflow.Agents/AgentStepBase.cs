// =============================================================================
// <copyright file="AgentStepBase.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Agents.Abstractions;
using Agentic.Workflow.Agents.Models;
using Agentic.Workflow.Steps;

using Microsoft.Extensions.AI;

namespace Agentic.Workflow.Agents;

/// <summary>
/// Base class for LLM-powered workflow steps with context assembly support.
/// </summary>
/// <typeparam name="TState">The type of workflow state.</typeparam>
/// <remarks>
/// <para>
/// AgentStepBase provides a structured approach to building LLM-powered steps:
/// <list type="bullet">
///   <item><description>Context assembly from multiple sources (state, RAG, literals)</description></item>
///   <item><description>Message building with system prompt, context, and user prompt</description></item>
///   <item><description>Response handling and state updates</description></item>
/// </list>
/// </para>
/// <para>
/// Subclasses must implement:
/// <list type="bullet">
///   <item><description><see cref="GetSystemPrompt"/> - Define agent behavior</description></item>
///   <item><description><see cref="GetUserPrompt"/> - Build the user query from state</description></item>
///   <item><description><see cref="ApplyResultAsync"/> - Update state with LLM response</description></item>
/// </list>
/// </para>
/// </remarks>
public abstract class AgentStepBase<TState> : IAgentStep<TState>
    where TState : class, IWorkflowState
{
    private readonly IChatClient _chatClient;
    private readonly IContextAssembler<TState>? _contextAssembler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentStepBase{TState}"/> class.
    /// </summary>
    /// <param name="chatClient">The chat client for LLM interactions.</param>
    /// <param name="contextAssembler">Optional context assembler for RAG support.</param>
    protected AgentStepBase(
        IChatClient chatClient,
        IContextAssembler<TState>? contextAssembler = null)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        _contextAssembler = contextAssembler;
    }

    /// <inheritdoc/>
    public abstract string GetSystemPrompt();

    /// <inheritdoc/>
    public abstract Type? GetOutputSchemaType();

    /// <summary>
    /// Gets the user prompt based on the current workflow state.
    /// </summary>
    /// <param name="state">The current workflow state.</param>
    /// <returns>The user prompt string.</returns>
    protected abstract string GetUserPrompt(TState state);

    /// <summary>
    /// Applies the LLM response to update the workflow state.
    /// </summary>
    /// <param name="state">The current workflow state.</param>
    /// <param name="response">The LLM response text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The step result with updated state.</returns>
    protected abstract Task<StepResult<TState>> ApplyResultAsync(
        TState state,
        string response,
        CancellationToken cancellationToken);

    /// <inheritdoc/>
    public async Task<StepResult<TState>> ExecuteAsync(
        TState state,
        StepContext context,
        CancellationToken cancellationToken)
    {
        // Assemble context if assembler is available
        var assembledContext = _contextAssembler != null
            ? await _contextAssembler.AssembleAsync(state, context, cancellationToken).ConfigureAwait(false)
            : AssembledContext.Empty;

        // Build messages for the LLM
        var messages = BuildMessages(state, assembledContext);

        // Get response from LLM
        var response = await _chatClient.GetResponseAsync(
            messages,
            options: null,
            cancellationToken).ConfigureAwait(false);

        var responseText = response.Text ?? string.Empty;

        // Apply result and return updated state
        return await ApplyResultAsync(state, responseText, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the message list for the LLM call.
    /// </summary>
    /// <param name="state">The current workflow state.</param>
    /// <param name="context">The assembled context.</param>
    /// <returns>The list of chat messages.</returns>
    /// <remarks>
    /// This method is internal for testing purposes.
    /// </remarks>
    internal IList<ChatMessage> BuildMessages(TState state, AssembledContext context)
    {
        var messages = new List<ChatMessage>();

        // Add system prompt
        messages.Add(new ChatMessage(ChatRole.System, GetSystemPrompt()));

        // Add context if non-empty
        if (!context.IsEmpty)
        {
            var contextText = context.ToPromptString();
            messages.Add(new ChatMessage(ChatRole.User, $"Context:\n{contextText}"));
        }

        // Add user prompt
        messages.Add(new ChatMessage(ChatRole.User, GetUserPrompt(state)));

        return messages;
    }
}
