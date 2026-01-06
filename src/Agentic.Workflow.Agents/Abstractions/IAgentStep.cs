// -----------------------------------------------------------------------
// <copyright file="IAgentStep.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Agentic.Workflow.Agents.Abstractions;

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Steps;

/// <summary>
/// Represents a workflow step that is powered by an LLM agent.
/// </summary>
/// <typeparam name="TState">The type of workflow state.</typeparam>
/// <remarks>
/// <para>
/// Agent steps extend the base <see cref="IWorkflowStep{TState}"/> contract with
/// LLM-specific capabilities such as system prompts, output schema validation,
/// and streaming response handling.
/// </para>
/// <para>
/// Implementations should use the <see cref="AgentStepContext"/> to access
/// the chat client, conversation history, and execution context.
/// </para>
/// </remarks>
public interface IAgentStep<TState> : IWorkflowStep<TState>
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Gets the system prompt that defines the agent's behavior and capabilities.
    /// </summary>
    /// <returns>The system prompt string.</returns>
    string GetSystemPrompt();

    /// <summary>
    /// Gets the type used for structured output validation.
    /// </summary>
    /// <remarks>
    /// When set, the agent's response will be validated against this schema
    /// and deserialized into an instance of this type.
    /// </remarks>
    /// <returns>The output schema type, or null for unstructured text output.</returns>
    Type? GetOutputSchemaType();
}
