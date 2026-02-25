// =============================================================================
// <copyright file="ApprovalBuilder.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Strategos.Definitions;
using Strategos.Models;

namespace Strategos.Builders;

/// <summary>
/// Fluent builder for constructing approval checkpoints within a workflow.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <typeparam name="TApprover">The marker type identifying the approver role.</typeparam>
/// <remarks>
/// <para>
/// This builder creates <see cref="ApprovalDefinition"/> instances with:
/// <list type="bullet">
///   <item><description>Context messages (static or dynamic)</description></item>
///   <item><description>Timeout configuration</description></item>
///   <item><description>Approval options for the UI</description></item>
///   <item><description>Metadata for additional context</description></item>
///   <item><description>Escalation and rejection handlers</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class ApprovalBuilder<TState, TApprover> : IApprovalBuilder<TState, TApprover>
    where TState : class, IWorkflowState
    where TApprover : class
{
    private readonly string _precedingStepId;
    private readonly List<ApprovalOptionDefinition> _options = [];
    private readonly Dictionary<string, object> _staticMetadata = [];
    private readonly Dictionary<string, string> _dynamicMetadataExpressions = [];

    private string? _staticContext;
    private string? _contextFactoryExpression;
    private TimeSpan _timeout = TimeSpan.FromHours(24);
    private ApprovalEscalationDefinition? _escalationHandler;
    private ApprovalRejectionDefinition? _rejectionHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApprovalBuilder{TState, TApprover}"/> class.
    /// </summary>
    /// <param name="precedingStepId">The ID of the step that precedes this approval point.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="precedingStepId"/> is null.
    /// </exception>
    public ApprovalBuilder(string precedingStepId)
    {
        ArgumentNullException.ThrowIfNull(precedingStepId, nameof(precedingStepId));
        _precedingStepId = precedingStepId;
    }

    /// <inheritdoc/>
    public IApprovalBuilder<TState, TApprover> WithContext(string context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        _staticContext = context;
        return this;
    }

    /// <inheritdoc/>
    public IApprovalBuilder<TState, TApprover> WithContextFrom(Func<TState, string> contextFactory)
    {
        ArgumentNullException.ThrowIfNull(contextFactory, nameof(contextFactory));

        // Capture expression as string for source generator consumption
        _contextFactoryExpression = contextFactory.ToString() ?? "state => [dynamic]";
        return this;
    }

    /// <inheritdoc/>
    public IApprovalBuilder<TState, TApprover> WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    /// <inheritdoc/>
    public IApprovalBuilder<TState, TApprover> WithOption(
        string optionId,
        string label,
        string description,
        bool isDefault = false)
    {
        _options.Add(new ApprovalOptionDefinition(optionId, label, description, isDefault));
        return this;
    }

    /// <inheritdoc/>
    public IApprovalBuilder<TState, TApprover> WithMetadata(string key, object value)
    {
        _staticMetadata[key] = value;
        return this;
    }

    /// <inheritdoc/>
    public IApprovalBuilder<TState, TApprover> WithMetadataFrom(string key, Func<TState, object> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(valueFactory, nameof(valueFactory));

        // Capture expression as string for source generator consumption
        _dynamicMetadataExpressions[key] = valueFactory.ToString() ?? "state => [dynamic]";
        return this;
    }

    /// <inheritdoc/>
    public IApprovalBuilder<TState, TApprover> OnTimeout(
        Action<IApprovalEscalationBuilder<TState>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        var escalationBuilder = new ApprovalEscalationBuilder<TState>();
        configure(escalationBuilder);
        _escalationHandler = escalationBuilder.Build();
        return this;
    }

    /// <inheritdoc/>
    public IApprovalBuilder<TState, TApprover> OnRejection(
        Action<IApprovalRejectionBuilder<TState>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        var rejectionBuilder = new ApprovalRejectionBuilder<TState>();
        configure(rejectionBuilder);
        _rejectionHandler = rejectionBuilder.Build();
        return this;
    }

    /// <inheritdoc/>
    public ApprovalDefinition Build()
    {
        var configuration = new ApprovalConfiguration
        {
            Type = ApprovalType.GeneralApproval,
            StaticContext = _staticContext,
            ContextFactoryExpression = _contextFactoryExpression,
            Timeout = _timeout,
            Options = _options.ToList(),
            StaticMetadata = new Dictionary<string, object>(_staticMetadata),
            DynamicMetadataExpressions = new Dictionary<string, string>(_dynamicMetadataExpressions),
        };

        var definition = ApprovalDefinition.Create(
            typeof(TApprover),
            configuration,
            _precedingStepId);

        if (_escalationHandler is not null)
        {
            definition = definition.WithEscalation(_escalationHandler);
        }

        if (_rejectionHandler is not null)
        {
            definition = definition.WithRejection(_rejectionHandler);
        }

        return definition;
    }
}
