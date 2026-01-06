// =============================================================================
// <copyright file="ILoopBuilder.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Builders;

/// <summary>
/// Fluent builder interface for constructing loop body within a workflow.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <remarks>
/// <para>
/// Loop builders allow defining steps within a repeat-until loop:
/// <code>
/// .RepeatUntil(
///     condition: state => state.QualityScore >= 0.9m,
///     loopName: "Refinement",
///     body: loop => loop
///         .Then&lt;CritiqueStep&gt;()
///         .Then&lt;RefineStep&gt;(),
///     maxIterations: 5)
/// </code>
/// </para>
/// <para>
/// Steps added to the loop body will be prefixed with the loop name
/// for phase enum generation (e.g., Refinement_Critique).
/// </para>
/// <para>
/// Nested loops are supported for hierarchical prefixing:
/// <code>
/// .RepeatUntil(
///     condition: state => state.OuterDone,
///     loopName: "Outer",
///     body: outer => outer
///         .Then&lt;OuterStep&gt;()
///         .RepeatUntil(
///             condition: state => state.InnerDone,
///             loopName: "Inner",
///             body: inner => inner.Then&lt;InnerStep&gt;(),
///             maxIterations: 3),
///     maxIterations: 5)
/// </code>
/// This produces phase names: Outer_OuterStep, Outer_Inner_InnerStep.
/// </para>
/// </remarks>
public interface ILoopBuilder<TState>
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Adds a step to the loop body.
    /// </summary>
    /// <typeparam name="TStep">The step implementation type.</typeparam>
    /// <returns>The builder for fluent chaining.</returns>
    ILoopBuilder<TState> Then<TStep>()
        where TStep : class, IWorkflowStep<TState>;

    /// <summary>
    /// Adds a step to the loop body with an instance name.
    /// </summary>
    /// <typeparam name="TStep">The step implementation type.</typeparam>
    /// <param name="instanceName">
    /// The instance name for this step. Enables reusing the same step type
    /// in different contexts with distinct identities.
    /// </param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <remarks>
    /// <para>
    /// Instance names allow distinguishing same step types within a loop:
    /// <code>
    /// .RepeatUntil(state => state.Done, "Refinement", loop => loop
    ///     .Then&lt;EvaluateStep&gt;("PreEvaluation")
    ///     .Then&lt;ProcessStep&gt;()
    ///     .Then&lt;EvaluateStep&gt;("PostEvaluation"))
    /// </code>
    /// </para>
    /// </remarks>
    ILoopBuilder<TState> Then<TStep>(string instanceName)
        where TStep : class, IWorkflowStep<TState>;

    /// <summary>
    /// Adds a step to the loop body with configuration.
    /// </summary>
    /// <typeparam name="TStep">The step implementation type.</typeparam>
    /// <param name="configure">Action to configure the step behavior.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// Step configuration allows defining compensation, retries, timeouts, and validation:
    /// <code>
    /// .Then&lt;ProcessStep&gt;(step => step
    ///     .Compensate&lt;RollbackStep&gt;()
    ///     .WithRetry(3)
    ///     .WithTimeout(TimeSpan.FromMinutes(5)))
    /// </code>
    /// </para>
    /// </remarks>
    ILoopBuilder<TState> Then<TStep>(Action<IStepConfiguration<TState>> configure)
        where TStep : class, IWorkflowStep<TState>;

    /// <summary>
    /// Adds a nested repeat-until loop inside the current loop body.
    /// </summary>
    /// <param name="condition">The condition to evaluate after each iteration. Loop exits when true.</param>
    /// <param name="loopName">The name of the nested loop for phase enum prefixing.</param>
    /// <param name="body">Action to build the nested loop body.</param>
    /// <param name="maxIterations">Maximum iterations before forced exit. Default is 100.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <remarks>
    /// Nested loop steps are prefixed with both loop names: {OuterLoop}_{InnerLoop}_{StepName}.
    /// </remarks>
    ILoopBuilder<TState> RepeatUntil(
        Func<TState, bool> condition,
        string loopName,
        Action<ILoopBuilder<TState>> body,
        int maxIterations = 100);

    /// <summary>
    /// Initiates a fork point within the loop body where multiple paths execute in parallel.
    /// </summary>
    /// <param name="paths">Actions to configure each fork path. Must specify at least two paths.</param>
    /// <returns>A builder for specifying the join step.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when fewer than two paths are specified, or when any path has no steps.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Fork enables parallel execution of analyst agents within a loop iteration:
    /// <code>
    /// .RepeatUntil(state => state.AllTargetsProcessed(), "ProcessTargets", loop => loop
    ///     .Then&lt;SelectNextTarget&gt;()
    ///     .Fork(
    ///         news => news.Then&lt;ValidateThesis&gt;(),
    ///         technical => technical.Then&lt;AnalyzeTechnical&gt;(),
    ///         fundamental => fundamental.Then&lt;AnalyzeFundamental&gt;())
    ///     .Join&lt;AggregateVotes&gt;()
    ///     .Then&lt;ExecuteTrade&gt;())
    /// </code>
    /// </para>
    /// <para>
    /// Fork steps inside loops are marked as loop body steps with the parent loop ID,
    /// ensuring proper phase enum generation with loop-prefixed names.
    /// </para>
    /// </remarks>
    ILoopForkJoinBuilder<TState> Fork(params Action<IForkPathBuilder<TState>>[] paths);

    /// <summary>
    /// Adds a branch point within the loop body based on a discriminator value.
    /// </summary>
    /// <typeparam name="TDiscriminator">The type of the discriminator value.</typeparam>
    /// <param name="discriminator">Function that extracts the discriminator value from state.</param>
    /// <param name="cases">The branch cases defining conditional paths.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <remarks>
    /// <para>
    /// Branches inside loops support conditional execution within each iteration:
    /// <code>
    /// .RepeatUntil(state => state.AllTargetsProcessed(), "ProcessTargets", loop => loop
    ///     .Then&lt;SelectNextTarget&gt;()
    ///     .Branch(
    ///         state => state.RequiresApproval,
    ///         BranchCase&lt;MyState, bool&gt;.When(true, approval => approval
    ///             .Then&lt;PrepareApproval&gt;()
    ///             .Then&lt;ProcessApproval&gt;()),
    ///         BranchCase&lt;MyState, bool&gt;.Otherwise(_ => { }))
    ///     .Then&lt;ExecuteAction&gt;())
    /// </code>
    /// </para>
    /// <para>
    /// Branch steps inside loops are prefixed with the loop name for phase enum generation.
    /// </para>
    /// </remarks>
    ILoopBuilder<TState> Branch<TDiscriminator>(
        Func<TState, TDiscriminator> discriminator,
        params BranchCase<TState, TDiscriminator>[] cases);
}
