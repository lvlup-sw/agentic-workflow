// =============================================================================
// <copyright file="IBeliefPriorFactory.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Selection;

namespace Agentic.Workflow.Abstractions;

/// <summary>
/// Contract for creating prior beliefs for agents based on task features.
/// </summary>
/// <remarks>
/// <para>
/// Prior factories enable context-aware initialization of agent beliefs. The default
/// implementation uses fixed Beta distribution parameters, but the abstraction enables
/// future implementations using:
/// </para>
/// <list type="bullet">
///   <item><description>LLM-informed priors (query LLM for reasonable estimates)</description></item>
///   <item><description>Historical data priors (use past performance on similar tasks)</description></item>
///   <item><description>Complexity-adjusted priors (broader priors for complex tasks)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var factory = new DefaultBeliefPriorFactory(alpha: 2.0, beta: 2.0);
/// var features = new TaskFeatures { Category = TaskCategory.CodeGeneration };
/// var belief = factory.CreatePrior("agent-1", features);
/// // belief.Alpha == 2.0, belief.Beta == 2.0
/// </code>
/// </example>
public interface IBeliefPriorFactory
{
    /// <summary>
    /// Creates a prior belief for the specified agent based on task features.
    /// </summary>
    /// <param name="agentId">The unique identifier of the agent.</param>
    /// <param name="features">The extracted task features informing prior selection.</param>
    /// <returns>
    /// An <see cref="AgentBelief"/> instance initialized with appropriate prior parameters.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The returned belief should have:
    /// <list type="bullet">
    ///   <item><description><see cref="AgentBelief.AgentId"/> set to <paramref name="agentId"/></description></item>
    ///   <item><description><see cref="AgentBelief.TaskCategory"/> set to <paramref name="features"/>.Category.ToString()</description></item>
    ///   <item><description><see cref="AgentBelief.ObservationCount"/> set to 0</description></item>
    ///   <item><description><see cref="AgentBelief.Alpha"/> and <see cref="AgentBelief.Beta"/> set according to factory configuration</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="agentId"/> or <paramref name="features"/> is null.
    /// </exception>
    AgentBelief CreatePrior(string agentId, TaskFeatures features);
}