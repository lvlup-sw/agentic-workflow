// =============================================================================
// <copyright file="CoderWorkflow.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Builders;
using Agentic.Workflow.Definitions;
using AgenticCoder.State;
using AgenticCoder.Steps;

/// <summary>
/// Defines the AgenticCoder workflow using the fluent DSL.
/// </summary>
/// <remarks>
/// <para>
/// The workflow demonstrates:
/// <list type="bullet">
///   <item><description>RepeatUntil - Iterative refinement loop until tests pass</description></item>
///   <item><description>AwaitApproval - Human checkpoint before completion</description></item>
///   <item><description>Loop detection - Max 3 attempts prevents infinite loops</description></item>
///   <item><description>Audit trail - Attempts collection tracks all code generations</description></item>
/// </list>
/// </para>
/// <para>
/// Workflow structure:
/// <code>
/// AnalyzeTask -> PlanImplementation -> [GenerateCode -> RunTests -> ReviewResults] (loop) -> HumanCheckpoint -> Complete
///                                              ^                    |
///                                              |-- tests fail ------+
///                                              (max 3 attempts)
/// </code>
/// </para>
/// </remarks>
public static class CoderWorkflow
{
    /// <summary>
    /// Creates the AgenticCoder workflow definition.
    /// </summary>
    /// <returns>The workflow definition.</returns>
    public static WorkflowDefinition<CoderState> Create() =>
        Workflow<CoderState>
            .Create("agentic-coder")
            .StartWith<AnalyzeTask>()
            .Then<PlanImplementation>()
            .RepeatUntil(
                condition: state => state.LatestTestResults?.Passed == true,
                loopName: "Refinement",
                body: loop => loop
                    .Then<GenerateCode>()
                    .Then<RunTests>()
                    .Then<ReviewResults>(),
                maxIterations: 3)
            .AwaitApproval<HumanDeveloper>(approval => approval
                .WithContext("Please review the generated code before marking as complete.")
                .WithOption("approve", "Approve", "Accept the implementation")
                .WithOption("reject", "Reject", "Request changes"))
            .Finally<Complete>();
}
