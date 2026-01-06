// =============================================================================
// <copyright file="WorkflowAgentContext.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Microsoft.Extensions.AI;

namespace Agentic.Workflow.Agents.Models;

/// <summary>
/// Represents the context for a workflow agent, including the chat client and conversation thread.
/// </summary>
/// <remarks>
/// <para>
/// This record encapsulates the state needed to execute a specialist agent within a workflow,
/// including the underlying chat client and the conversation history (messages).
/// </para>
/// <para>
/// The messages list represents the conversation thread and can be modified during agent
/// execution. After execution, the updated messages can be serialized for persistence
/// via <see cref="Abstractions.IWorkflowAgentFactory.SerializeThreadAsync"/>.
/// </para>
/// </remarks>
/// <param name="ChatClient">The chat client for agent interactions.</param>
/// <param name="Messages">The conversation history/thread as a mutable list of messages.</param>
public sealed record WorkflowAgentContext(
    IChatClient ChatClient,
    IList<ChatMessage> Messages);
