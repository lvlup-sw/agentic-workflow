// =============================================================================
// <copyright file="ContentWorkflow.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Builders;
using Strategos.Definitions;
using ContentPipeline.State;
using ContentPipeline.Steps;

namespace ContentPipeline;

/// <summary>
/// Defines the content publishing workflow.
/// </summary>
/// <remarks>
/// <para>
/// This workflow demonstrates a content publishing pipeline with:
/// <list type="bullet">
///   <item><description>AI-powered draft generation</description></item>
///   <item><description>AI content review with quality scoring</description></item>
///   <item><description>Human-in-the-loop approval gate</description></item>
///   <item><description>Automated publishing with compensation support</description></item>
/// </list>
/// </para>
/// <para>
/// Workflow flow:
/// Draft -> AI Review -> Human Approval -> Publish
/// </para>
/// </remarks>
public static class ContentWorkflow
{
    /// <summary>
    /// Creates the content publishing workflow definition.
    /// </summary>
    /// <returns>The workflow definition.</returns>
    public static WorkflowDefinition<ContentState> Create() =>
        Workflow<ContentState>
            .Create("content-pipeline")
            .StartWith<GenerateDraft>()
            .Then<AiReviewContent>()
            .Then<AwaitHumanApproval>()
            .Finally<PublishContent>();
}
