// =============================================================================
// <copyright file="WorkflowBudget.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Orchestration.Budget;

namespace Agentic.Workflow.Infrastructure.Budget;

/// <summary>
/// Represents the aggregate budget across all resource types for a workflow.
/// </summary>
/// <remarks>
/// <para>
/// The workflow budget provides a unified view of resource consumption and enables
/// scarcity-aware decision making based on the worst-case resource availability.
/// </para>
/// <para>
/// This is part of the recoverable state tuple used for workflow checkpointing.
/// </para>
/// </remarks>
public sealed record WorkflowBudget : IWorkflowBudget
{
    private readonly Lazy<ScarcityLevel> _cachedScarcity;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowBudget"/> class.
    /// Initializes a new instance of the <see cref="WorkflowBudget"/> record.
    /// </summary>
    public WorkflowBudget()
    {
        _cachedScarcity = new Lazy<ScarcityLevel>(ComputeOverallScarcity);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowBudget"/> class.
    /// Copy constructor that reinitializes the lazy cache.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This constructor is required because C# record copy semantics (<c>this with { ... }</c>)
    /// copy the <see cref="Lazy{T}"/> field by reference. The original <c>Lazy</c> captures
    /// <c>this</c> in its closure, causing the copied record to compute scarcity from the
    /// original instance's resources instead of its own.
    /// </para>
    /// <para>
    /// By providing this copy constructor, we ensure each record instance has its own
    /// <see cref="Lazy{T}"/> that correctly references its own <see cref="Resources"/>.
    /// </para>
    /// </remarks>
    /// <param name="original">The original record being copied.</param>
    private WorkflowBudget(WorkflowBudget original)
    {
        BudgetId = original.BudgetId;
        WorkflowId = original.WorkflowId;
        Resources = original.Resources;
        CreatedAt = original.CreatedAt;
        UpdatedAt = original.UpdatedAt;
        _cachedScarcity = new Lazy<ScarcityLevel>(ComputeOverallScarcity);
    }

    /// <inheritdoc />
    public required string BudgetId { get; init; }

    /// <inheritdoc />
    public required string WorkflowId { get; init; }

    /// <inheritdoc />
    public required IReadOnlyDictionary<ResourceType, IResourceBudget> Resources { get; init; }

    /// <summary>
    /// Gets the timestamp when this budget was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the timestamp when this budget was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public ScarcityLevel OverallScarcity
    {
        get { return _cachedScarcity.Value; }
    }

    private ScarcityLevel ComputeOverallScarcity()
    {
        if (Resources is null || Resources.Count == 0)
        {
            return ScarcityLevel.Abundant;
        }

        // Return the most severe (highest ordinal) scarcity level
        return Resources.Values
            .Select(r => r.Scarcity)
            .Max();
    }

    /// <inheritdoc />
    public double ScarcityMultiplier => OverallScarcity switch
    {
        ScarcityLevel.Abundant => 1.0,
        ScarcityLevel.Normal => 1.5,
        ScarcityLevel.Scarce => 3.0,
        ScarcityLevel.Critical => 10.0,
        _ => 1.0
    };

    /// <summary>
    /// Creates a new workflow budget with default allocations.
    /// </summary>
    /// <param name="workflowId">The workflow identifier.</param>
    /// <param name="steps">Allocated steps budget.</param>
    /// <param name="tokens">Allocated tokens budget.</param>
    /// <param name="executions">Allocated executions budget.</param>
    /// <param name="toolCalls">Allocated tool calls budget.</param>
    /// <param name="wallTimeSeconds">Allocated wall time in seconds.</param>
    /// <returns>A new workflow budget with the specified allocations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when workflowId is null.</exception>
    public static WorkflowBudget Create(
        string workflowId,
        double steps = 25,
        double tokens = 50000,
        double executions = 15,
        double toolCalls = 40,
        double wallTimeSeconds = 300)
    {
        ArgumentNullException.ThrowIfNull(workflowId, nameof(workflowId));

        var budgetId = $"budget-{Guid.NewGuid():N}";
        var resources = new Dictionary<ResourceType, IResourceBudget>
        {
            [ResourceType.Steps] = ResourceBudget.Create(ResourceType.Steps, steps),
            [ResourceType.Tokens] = ResourceBudget.Create(ResourceType.Tokens, tokens),
            [ResourceType.Executions] = ResourceBudget.Create(ResourceType.Executions, executions),
            [ResourceType.ToolCalls] = ResourceBudget.Create(ResourceType.ToolCalls, toolCalls),
            [ResourceType.WallTime] = ResourceBudget.Create(ResourceType.WallTime, wallTimeSeconds)
        };

        return new WorkflowBudget
        {
            BudgetId = budgetId,
            WorkflowId = workflowId,
            Resources = resources
        };
    }

    /// <inheritdoc />
    public bool HasSufficientBudget(ResourceType resourceType, double amount)
    {
        return !Resources.TryGetValue(resourceType, out var budget) || budget.HasSufficient(amount);
    }

    /// <inheritdoc />
    public IWorkflowBudget WithConsumption(ResourceType resourceType, double amount)
    {
        if (!Resources.TryGetValue(resourceType, out var budget))
        {
            return this;
        }

        var updatedBudget = budget.WithConsumption(amount);
        var updatedResources = new Dictionary<ResourceType, IResourceBudget>(Resources)
        {
            [resourceType] = updatedBudget
        };

        return this with
        {
            Resources = updatedResources,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <inheritdoc />
    public IWorkflowBudget WithResource(ResourceType resourceType, IResourceBudget resource)
    {
        ArgumentNullException.ThrowIfNull(resource, nameof(resource));

        var updatedResources = new Dictionary<ResourceType, IResourceBudget>(Resources)
        {
            [resourceType] = resource
        };

        return this with
        {
            Resources = updatedResources,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
