// =============================================================================
// <copyright file="IWorkflowAgentFactory.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Agents.Models;
using Microsoft.Extensions.AI;

namespace Agentic.Workflow.Agents.Abstractions;

/// <summary>
/// Defines the contract for creating workflow agents with conversation thread management.
/// </summary>
/// <remarks>
/// <para>
/// This factory is responsible for creating chat client instances configured for specific
/// specialist types, along with their associated conversation threads. It supports:
/// </para>
/// <list type="bullet">
///   <item><description>Creating new agents with empty conversation threads</description></item>
///   <item><description>Restoring agents with previously serialized conversation threads</description></item>
///   <item><description>Serializing conversation threads for persistence</description></item>
/// </list>
/// <para>
/// The factory enables conversation continuity across same-specialist invocations within
/// a workflow, while maintaining thread isolation between different specialist types as
/// recommended by MAF best practices.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Creating a new agent for a specialist
/// var context = await factory.CreateAgentWithThreadAsync(
///     SpecialistType.Coder,
///     existingSerializedThread, // null for new conversation
///     cancellationToken);
///
/// // Use the agent
/// var response = await context.ChatClient.GetResponseAsync(context.Messages, cancellationToken: ct);
/// context.Messages.Add(response.Message);
///
/// // Serialize the updated thread for persistence
/// var serializedThread = await factory.SerializeThreadAsync(context.Messages, ct);
/// state = state.WithSerializedThread(SpecialistType.Coder, serializedThread);
/// </code>
/// </example>
public interface IWorkflowAgentFactory
{
    /// <summary>
    /// Creates a workflow agent context with an optional restored conversation thread.
    /// </summary>
    /// <param name="specialistType">The type of specialist agent to create.</param>
    /// <param name="serializedThreadJson">
    /// The previously serialized conversation thread as JSON string to restore, or null to create a new thread.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    /// A <see cref="WorkflowAgentContext"/> containing the configured chat client and
    /// conversation messages.
    /// </returns>
    /// <remarks>
    /// <para>
    /// When <paramref name="serializedThreadJson"/> is provided, the factory deserializes
    /// the conversation history and populates the messages list. When null, an empty
    /// messages list is created for a new conversation.
    /// </para>
    /// <para>
    /// The returned chat client is configured with the appropriate system prompt and
    /// middleware for the specified specialist type.
    /// </para>
    /// </remarks>
    Task<WorkflowAgentContext> CreateAgentWithThreadAsync(
        SpecialistType specialistType,
        string? serializedThreadJson,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Serializes a conversation thread for persistence.
    /// </summary>
    /// <param name="messages">The conversation messages to serialize.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    /// A JSON string containing the serialized conversation thread.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The serialized thread can be stored in workflow state via
    /// <see cref="IConversationalState.WithSerializedThread"/> and later restored
    /// via <see cref="CreateAgentWithThreadAsync"/>.
    /// </para>
    /// </remarks>
    Task<string> SerializeThreadAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default);
}
