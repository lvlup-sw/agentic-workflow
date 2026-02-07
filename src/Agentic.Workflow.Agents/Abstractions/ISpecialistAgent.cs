// =============================================================================
// <copyright file="ISpecialistAgent.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Agents.Models;
using Agentic.Workflow.Primitives;

namespace Agentic.Workflow.Agents.Abstractions;

/// <summary>
/// Defines the contract for specialist agents in the workflow system.
/// </summary>
/// <remarks>
/// <para>
/// Specialist agents execute discrete tasks within a workflow, following
/// a hierarchical state machine (HSM) pattern:
/// </para>
/// <list type="number">
///   <item><description>Receiving - Accept task from orchestrator</description></item>
///   <item><description>Reasoning - Analyze the task</description></item>
///   <item><description>Generating - Produce output (e.g., code)</description></item>
///   <item><description>Executing - Run the generated output</description></item>
///   <item><description>Interpreting - Analyze execution results</description></item>
///   <item><description>Signaling - Report completion to orchestrator</description></item>
/// </list>
/// <para>
/// Implementations provide domain-specific behavior (e.g., coding, analysis)
/// while the orchestrator coordinates multiple specialists.
/// </para>
/// </remarks>
public interface ISpecialistAgent
{
    /// <summary>
    /// Gets the current HSM state of the agent.
    /// </summary>
    /// <value>The current state in the specialist workflow.</value>
    SpecialistState CurrentState { get; }

    /// <summary>
    /// Gets the history of state transitions for debugging and audit.
    /// </summary>
    /// <value>An ordered list of state transitions with timestamps.</value>
    IReadOnlyList<StateTransition> StateHistory { get; }

    /// <summary>
    /// Gets the type of this specialist.
    /// </summary>
    /// <value>The specialist classification (e.g., Coder, Analyst).</value>
    SpecialistType SpecialistType { get; }

    /// <summary>
    /// Gets the version of this agent implementation.
    /// </summary>
    /// <value>A semantic version string (e.g., "v1.0.0").</value>
    /// <remarks>
    /// Used for A/B testing, regression debugging, and compliance reporting.
    /// </remarks>
    string AgentVersion { get; }

    /// <summary>
    /// Gets the model identifier used by this agent.
    /// </summary>
    /// <value>The LLM model ID, or null if not applicable.</value>
    /// <remarks>
    /// Used for compliance reporting and cost tracking.
    /// </remarks>
    string? ModelUsed { get; }

    /// <summary>
    /// Gets or sets the execution mode for LLM response handling.
    /// </summary>
    /// <value>
    /// <see cref="StreamingExecutionMode.Buffered"/> for complete responses,
    /// or <see cref="StreamingExecutionMode.Streaming"/> for token-by-token processing.
    /// </value>
    StreamingExecutionMode ExecutionMode { get; set; }

    /// <summary>
    /// Gets or sets the workflow identifier for event correlation.
    /// </summary>
    /// <value>The unique identifier of the parent workflow.</value>
    Guid WorkflowId { get; set; }

    /// <summary>
    /// Gets or sets the task identifier for event correlation.
    /// </summary>
    /// <value>The identifier of the current task within the workflow.</value>
    string TaskId { get; set; }

    /// <summary>
    /// Executes the specialist workflow for the given task.
    /// </summary>
    /// <param name="taskDescription">The natural language task description.</param>
    /// <param name="cancellationToken">Cancellation token for cooperative cancellation.</param>
    /// <returns>
    /// A result containing the specialist signal indicating success, failure, or help needed.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when taskDescription is null or whitespace.</exception>
    Task<Result<SpecialistSignal>> ExecuteAsync(
        string taskDescription,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Configures the streaming handler for real-time token processing.
    /// </summary>
    /// <param name="handler">The streaming handler implementation.</param>
    /// <returns>This agent instance for method chaining.</returns>
    /// <remarks>
    /// When configured with <see cref="ExecutionMode"/> set to
    /// <see cref="StreamingExecutionMode.Streaming"/>, tokens are published
    /// as they arrive from the LLM.
    /// </remarks>
    ISpecialistAgent WithStreamingHandler(IStreamingHandler handler);

    /// <summary>
    /// Configures the event store for conversation recording.
    /// </summary>
    /// <param name="eventStore">The progress event store for audit trails.</param>
    /// <returns>This agent instance for method chaining.</returns>
    /// <remarks>
    /// When configured, all LLM interactions are recorded for
    /// audit trails and time-travel debugging.
    /// </remarks>
    ISpecialistAgent WithEventStore(IProgressEventStore eventStore);
}