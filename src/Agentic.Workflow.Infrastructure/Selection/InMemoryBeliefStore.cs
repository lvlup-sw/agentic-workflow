// =============================================================================
// <copyright file="InMemoryBeliefStore.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using System.Collections.Concurrent;
using Agentic.Workflow.Primitives;
using Agentic.Workflow.Abstractions;
using Agentic.Workflow.Selection;

namespace Agentic.Workflow.Infrastructure.Selection;

/// <summary>
/// In-memory implementation of <see cref="IBeliefStore"/> for testing
/// and lightweight deployments without persistence requirements.
/// </summary>
/// <remarks>
/// <para>
/// Uses a <see cref="ConcurrentDictionary{TKey, TValue}"/> for thread-safe
/// belief storage. Beliefs are keyed by "{AgentId}_{TaskCategory}".
/// </para>
/// <para>
/// Secondary indices provide O(1) lookup by agent or category, avoiding
/// O(n) scans of all beliefs for common query patterns.
/// </para>
/// <para>
/// This implementation is suitable for:
/// <list type="bullet">
///   <item><description>Unit and integration testing</description></item>
///   <item><description>Single-instance deployments without durability needs</description></item>
///   <item><description>Development and prototyping</description></item>
/// </list>
/// </para>
/// <para>
/// For production use with persistence and audit trails, use the Marten-backed
/// implementation instead.
/// </para>
/// </remarks>
public sealed class InMemoryBeliefStore : IBeliefStore
{
    private readonly ConcurrentDictionary<string, AgentBelief> _beliefs = new();

    /// <summary>
    /// Secondary index: maps agent ID to set of composite keys for that agent's beliefs.
    /// </summary>
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _byAgent = new();

    /// <summary>
    /// Secondary index: maps task category to set of composite keys for that category's beliefs.
    /// </summary>
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _byCategory = new();

    /// <inheritdoc/>
    public ValueTask<Result<AgentBelief>> GetBeliefAsync(
        string agentId,
        string taskCategory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agentId);
        ArgumentNullException.ThrowIfNull(taskCategory);

        var key = GetKey(agentId, taskCategory);
        var belief = _beliefs.GetOrAdd(key, _ =>
        {
            var newBelief = AgentBelief.CreatePrior(agentId, taskCategory);
            AddToIndices(agentId, taskCategory, key);
            return newBelief;
        });

        return new ValueTask<Result<AgentBelief>>(Result<AgentBelief>.Success(belief));
    }

    /// <inheritdoc/>
    public ValueTask<Result<Unit>> UpdateBeliefAsync(
        string agentId,
        string taskCategory,
        bool success,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agentId);
        ArgumentNullException.ThrowIfNull(taskCategory);

        var key = GetKey(agentId, taskCategory);

        _beliefs.AddOrUpdate(
            key,
            _ =>
            {
                AddToIndices(agentId, taskCategory, key);
                return CreateUpdatedPrior(agentId, taskCategory, success);
            },
            (_, existing) => success ? existing.WithSuccess() : existing.WithFailure());

        return new ValueTask<Result<Unit>>(Result<Unit>.Success(Unit.Value));
    }

    /// <inheritdoc/>
    public ValueTask<Result<IReadOnlyList<AgentBelief>>> GetBeliefsForAgentAsync(
        string agentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agentId);

        if (!_byAgent.TryGetValue(agentId, out var keySet))
        {
            return new ValueTask<Result<IReadOnlyList<AgentBelief>>>(
                Result<IReadOnlyList<AgentBelief>>.Success(Array.Empty<AgentBelief>()));
        }

        var keys = keySet.Keys;
        var beliefs = new List<AgentBelief>(keys.Count);
        foreach (var key in keys)
        {
            if (_beliefs.TryGetValue(key, out var belief))
            {
                beliefs.Add(belief);
            }
        }

        return new ValueTask<Result<IReadOnlyList<AgentBelief>>>(
            Result<IReadOnlyList<AgentBelief>>.Success(beliefs));
    }

    /// <inheritdoc/>
    public ValueTask<Result<IReadOnlyList<AgentBelief>>> GetBeliefsForCategoryAsync(
        string taskCategory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskCategory);

        if (!_byCategory.TryGetValue(taskCategory, out var keySet))
        {
            return new ValueTask<Result<IReadOnlyList<AgentBelief>>>(
                Result<IReadOnlyList<AgentBelief>>.Success(Array.Empty<AgentBelief>()));
        }

        var keys = keySet.Keys;
        var beliefs = new List<AgentBelief>(keys.Count);
        foreach (var key in keys)
        {
            if (_beliefs.TryGetValue(key, out var belief))
            {
                beliefs.Add(belief);
            }
        }

        return new ValueTask<Result<IReadOnlyList<AgentBelief>>>(
            Result<IReadOnlyList<AgentBelief>>.Success(beliefs));
    }

    /// <inheritdoc/>
    public ValueTask<Result<Unit>> SaveBeliefAsync(
        AgentBelief belief,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(belief);

        var key = GetKey(belief.AgentId, belief.TaskCategory);
        _beliefs[key] = belief;
        AddToIndices(belief.AgentId, belief.TaskCategory, key);

        return new ValueTask<Result<Unit>>(Result<Unit>.Success(Unit.Value));
    }

    /// <summary>
    /// Creates the composite key for a belief.
    /// </summary>
    /// <param name="agentId">The agent identifier.</param>
    /// <param name="taskCategory">The task category.</param>
    /// <returns>The composite key string.</returns>
    private static string GetKey(string agentId, string taskCategory)
    {
        return $"{agentId}_{taskCategory}";
    }

    /// <summary>
    /// Adds a composite key to the secondary indices for agent and category.
    /// </summary>
    /// <param name="agentId">The agent identifier.</param>
    /// <param name="taskCategory">The task category.</param>
    /// <param name="key">The composite key.</param>
    private void AddToIndices(string agentId, string taskCategory, string key)
    {
        var agentKeys = _byAgent.GetOrAdd(agentId, _ => new ConcurrentDictionary<string, byte>());
        agentKeys.TryAdd(key, 0);

        var categoryKeys = _byCategory.GetOrAdd(taskCategory, _ => new ConcurrentDictionary<string, byte>());
        categoryKeys.TryAdd(key, 0);
    }

    /// <summary>
    /// Creates a new prior belief with a single update applied.
    /// </summary>
    /// <param name="agentId">The agent identifier.</param>
    /// <param name="taskCategory">The task category.</param>
    /// <param name="success">Whether the first observation was a success.</param>
    /// <returns>A new belief with the update applied.</returns>
    private static AgentBelief CreateUpdatedPrior(string agentId, string taskCategory, bool success)
    {
        var prior = AgentBelief.CreatePrior(agentId, taskCategory);
        return success ? prior.WithSuccess() : prior.WithFailure();
    }
}
