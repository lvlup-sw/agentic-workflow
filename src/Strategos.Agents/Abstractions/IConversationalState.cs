// -----------------------------------------------------------------------
// <copyright file="IConversationalState.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;

namespace Strategos.Agents.Abstractions;

/// <summary>
/// Marker interface for workflow states that support per-agent conversation continuity.
/// </summary>
/// <remarks>
/// <para>
/// Workflow states implementing this interface can persist conversation threads
/// for each agent type, enabling specialists to maintain context across multiple
/// invocations within the same workflow execution.
/// </para>
/// <para>
/// The <see cref="SerializedThreads"/> dictionary maps agent type identifiers
/// to their serialized conversation history. This enables:
/// </para>
/// <list type="bullet">
///   <item><description>Same-specialist context continuity</description></item>
///   <item><description>Durable conversation state via event sourcing</description></item>
///   <item><description>Immutable state updates for saga persistence</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// public record OrderState : IWorkflowState, IConversationalState
/// {
///     public Guid WorkflowId { get; init; }
///     public ImmutableDictionary&lt;string, string&gt; SerializedThreads { get; init; }
///         = ImmutableDictionary&lt;string, string&gt;.Empty;
///
///     public IConversationalState WithSerializedThread(string agentType, string thread)
///         => this with { SerializedThreads = SerializedThreads.SetItem(agentType, thread) };
/// }
/// </code>
/// </example>
public interface IConversationalState
{
    /// <summary>
    /// Gets the serialized conversation threads for each agent type.
    /// </summary>
    /// <remarks>
    /// Keys are agent type identifiers (e.g., "coder", "analyst").
    /// Values are serialized conversation histories.
    /// </remarks>
    ImmutableDictionary<string, string> SerializedThreads { get; }

    /// <summary>
    /// Creates a new state instance with the specified thread updated.
    /// </summary>
    /// <param name="agentType">The agent type identifier.</param>
    /// <param name="serializedThread">The serialized conversation thread.</param>
    /// <returns>A new state instance with the updated thread.</returns>
    IConversationalState WithSerializedThread(string agentType, string serializedThread);
}
