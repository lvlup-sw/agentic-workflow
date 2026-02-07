// =============================================================================
// <copyright file="IContextAssembler.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================


using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Agents.Models;
using Agentic.Workflow.Steps;

namespace Agentic.Workflow.Agents.Abstractions;
/// <summary>
/// Assembles runtime context for agent step execution.
/// </summary>
/// <typeparam name="TState">The type of workflow state.</typeparam>
/// <remarks>
/// <para>
/// Context assemblers gather information from multiple sources to build
/// a comprehensive context for LLM prompts:
/// <list type="bullet">
///   <item><description>State values from the workflow state</description></item>
///   <item><description>Retrieved documents from vector search</description></item>
///   <item><description>Literal context injections</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IContextAssembler<TState>
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Assembles context from various sources for the current step execution.
    /// </summary>
    /// <param name="state">The current workflow state.</param>
    /// <param name="stepContext">The step execution context.</param>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown.</param>
    /// <returns>The assembled context containing all gathered information.</returns>
    Task<AssembledContext> AssembleAsync(
        TState state,
        StepContext stepContext,
        CancellationToken cancellationToken);
}