// -----------------------------------------------------------------------
// <copyright file="IConversationThreadManager.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------


using Microsoft.Extensions.AI;

namespace Agentic.Workflow.Agents.Abstractions;
/// <summary>
/// Manages conversation threads for agent workflow execution.
/// </summary>
/// <remarks>
/// <para>
/// This interface enables conversation continuity across multiple invocations
/// of the same agent type within a workflow. Each agent type maintains its own
/// conversation thread, allowing specialists to build on previous context.
/// </para>
/// <para>
/// Implementations should handle thread serialization for workflow persistence
/// and restore previous conversations when the same agent is invoked again.
/// </para>
/// </remarks>
public interface IConversationThreadManager
{
    /// <summary>
    /// Creates a chat client with conversation thread restored from serialized state.
    /// </summary>
    /// <param name="agentType">The type identifier for the agent.</param>
    /// <param name="serializedThread">The serialized conversation history, or null for a new thread.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the chat client with restored or new thread.</returns>
    Task<IChatClient> CreateAgentWithThreadAsync(
        string agentType,
        string? serializedThread,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Serializes the current conversation thread for persistence.
    /// </summary>
    /// <param name="agentType">The type identifier for the agent.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the serialized thread state.</returns>
    Task<string> SerializeThreadAsync(
        string agentType,
        CancellationToken cancellationToken = default);
}