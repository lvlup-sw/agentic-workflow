// -----------------------------------------------------------------------
// <copyright file="AgentStepContext.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Agentic.Workflow.Agents.Models;

using Agentic.Workflow.Agents.Abstractions;
using Microsoft.Extensions.AI;

/// <summary>
/// Provides context for agent step execution.
/// </summary>
/// <remarks>
/// This context extends the base step context with agent-specific
/// capabilities such as chat client access, streaming handler,
/// and conversation thread management.
/// </remarks>
/// <param name="ChatClient">The chat client for LLM interactions.</param>
/// <param name="WorkflowId">The workflow execution identifier.</param>
/// <param name="StepName">The current step name.</param>
/// <param name="StepExecutionId">Unique identifier for this step execution.</param>
/// <param name="StreamingCallback">Optional callback for streaming responses.</param>
/// <param name="ConversationThreadManager">Optional manager for conversation continuity.</param>
public sealed record AgentStepContext(
    IChatClient ChatClient,
    Guid WorkflowId,
    string StepName,
    Guid StepExecutionId,
    IStreamingCallback? StreamingCallback = null,
    IConversationThreadManager? ConversationThreadManager = null);
