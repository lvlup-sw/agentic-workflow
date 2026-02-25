// =============================================================================
// <copyright file="RouterWorkflow.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Builders;
using Strategos.Definitions;
using MultiModelRouter.State;
using MultiModelRouter.Steps;

namespace MultiModelRouter;

/// <summary>
/// Defines the multi-model router workflow using fluent DSL.
/// </summary>
/// <remarks>
/// <para>
/// This workflow demonstrates Thompson Sampling for model selection:
/// <list type="bullet">
///   <item><description>ClassifyQuery: Categorize the user query</description></item>
///   <item><description>SelectModel: Use Thompson Sampling to select optimal model</description></item>
///   <item><description>GenerateResponse: Generate response with selected model</description></item>
///   <item><description>RecordFeedback: Update beliefs based on user feedback</description></item>
/// </list>
/// </para>
/// </remarks>
public static class RouterWorkflow
{
    /// <summary>
    /// Creates the multi-model router workflow definition.
    /// </summary>
    /// <returns>A workflow definition for routing queries to optimal models.</returns>
    public static WorkflowDefinition<RouterState> Create() =>
        Workflow<RouterState>
            .Create("multi-model-router")
            .StartWith<ClassifyQuery>()
            .Then<SelectModel>()
            .Then<GenerateResponse>()
            .Finally<RecordFeedback>();
}
