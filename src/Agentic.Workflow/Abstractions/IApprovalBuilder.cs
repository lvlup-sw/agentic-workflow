// =============================================================================
// <copyright file="IApprovalBuilder.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Definitions;
using Agentic.Workflow.Models;

namespace Agentic.Workflow.Builders;

/// <summary>
/// Fluent builder interface for constructing approval checkpoints within a workflow.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <typeparam name="TApprover">The marker type identifying the approver role.</typeparam>
/// <remarks>
/// <para>
/// Approval builders allow defining human-in-the-loop checkpoints:
/// <code>
/// .AwaitApproval&lt;ManagerApprover&gt;(approval => approval
///     .WithContext("Please review and approve this request")
///     .WithTimeout(TimeSpan.FromHours(4))
///     .WithOption("approve", "Approve", "Approve the request", isDefault: true)
///     .WithOption("reject", "Reject", "Reject the request")
///     .OnTimeout(escalation => escalation
///         .EscalateTo&lt;DirectorApprover&gt;(nested => nested
///             .WithContext("Escalated due to timeout")))
///     .OnRejection(rejection => rejection
///         .Then&lt;NotifyRequester&gt;()
///         .Complete()))
/// </code>
/// </para>
/// <para>
/// The <typeparamref name="TApprover"/> type parameter serves as a marker for:
/// <list type="bullet">
///   <item><description>Type-safe routing of approval requests</description></item>
///   <item><description>Dependency injection of approver-specific handlers</description></item>
///   <item><description>Configuration binding per approver type</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IApprovalBuilder<TState, TApprover>
    where TState : class, IWorkflowState
    where TApprover : class
{
    /// <summary>
    /// Sets the static context message for the approval request.
    /// </summary>
    /// <param name="context">The message explaining what needs approval.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> is null.
    /// </exception>
    IApprovalBuilder<TState, TApprover> WithContext(string context);

    /// <summary>
    /// Sets a dynamic context message derived from workflow state.
    /// </summary>
    /// <param name="contextFactory">
    /// A factory function that generates the context message from the workflow state.
    /// </param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="contextFactory"/> is null.
    /// </exception>
    /// <remarks>
    /// The expression is captured for source generator consumption.
    /// Example: <c>state => $"Claim {state.ClaimId} for ${state.Amount}"</c>
    /// </remarks>
    IApprovalBuilder<TState, TApprover> WithContextFrom(Func<TState, string> contextFactory);

    /// <summary>
    /// Sets the timeout duration for the approval request.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for a response.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <remarks>
    /// Default timeout is 24 hours. When timeout expires, the escalation handler
    /// is invoked if configured; otherwise the approval fails.
    /// </remarks>
    IApprovalBuilder<TState, TApprover> WithTimeout(TimeSpan timeout);

    /// <summary>
    /// Adds an approval option that the approver can select.
    /// </summary>
    /// <param name="optionId">Unique identifier for programmatic handling.</param>
    /// <param name="label">Short display text for the UI.</param>
    /// <param name="description">Detailed explanation of the option.</param>
    /// <param name="isDefault">Whether this option is pre-selected.</param>
    /// <returns>The builder for fluent chaining.</returns>
    IApprovalBuilder<TState, TApprover> WithOption(
        string optionId,
        string label,
        string description,
        bool isDefault = false);

    /// <summary>
    /// Adds static metadata to the approval request.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The builder for fluent chaining.</returns>
    IApprovalBuilder<TState, TApprover> WithMetadata(string key, object value);

    /// <summary>
    /// Adds dynamic metadata derived from workflow state.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="valueFactory">A factory function that generates the value from state.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <remarks>
    /// The expression is captured for source generator consumption.
    /// Example: <c>"amount", state => state.TotalAmount</c>
    /// </remarks>
    IApprovalBuilder<TState, TApprover> WithMetadataFrom(string key, Func<TState, object> valueFactory);

    /// <summary>
    /// Configures the escalation handler for timeout scenarios.
    /// </summary>
    /// <param name="configure">Action to configure the escalation builder.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configure"/> is null.
    /// </exception>
    /// <remarks>
    /// The escalation handler executes when the approval times out.
    /// It can include additional steps or escalate to another approver.
    /// </remarks>
    IApprovalBuilder<TState, TApprover> OnTimeout(
        Action<IApprovalEscalationBuilder<TState>> configure);

    /// <summary>
    /// Configures the rejection handler.
    /// </summary>
    /// <param name="configure">Action to configure the rejection builder.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configure"/> is null.
    /// </exception>
    /// <remarks>
    /// The rejection handler executes when an approver rejects the request.
    /// It can include cleanup steps or notify relevant parties.
    /// </remarks>
    IApprovalBuilder<TState, TApprover> OnRejection(
        Action<IApprovalRejectionBuilder<TState>> configure);

    /// <summary>
    /// Builds the immutable approval definition.
    /// </summary>
    /// <returns>The configured approval definition.</returns>
    ApprovalDefinition Build();
}