// =============================================================================
// <copyright file="BranchCase.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Definitions;

/// <summary>
/// Represents a branch case for discriminator-based routing.
/// </summary>
/// <typeparam name="TState">The workflow state type.</typeparam>
/// <typeparam name="TDiscriminator">The discriminator type.</typeparam>
/// <remarks>
/// Branch cases map discriminator values to workflow paths:
/// <code>
/// .Branch(state => state.ClaimType,
///     BranchCase&lt;ClaimState, ClaimType&gt;.When(ClaimType.Auto, path => path.Then&lt;AutoProcess&gt;()),
///     BranchCase&lt;ClaimState, ClaimType&gt;.Otherwise(path => path.Then&lt;DefaultProcess&gt;()))
/// </code>
/// </remarks>
public sealed record BranchCase<TState, TDiscriminator>
    where TState : class, IWorkflowState
{
    /// <summary>
    /// Gets the discriminator value for this case.
    /// </summary>
    public required TDiscriminator Value { get; init; }

    /// <summary>
    /// Gets the action that builds the branch path.
    /// </summary>
    public required Action<IBranchBuilder<TState>> PathBuilder { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is the default (fallthrough) case.
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>
    /// Creates a branch case for a specific discriminator value.
    /// </summary>
    /// <param name="value">The discriminator value.</param>
    /// <param name="pathBuilder">The action that builds the branch path.</param>
    /// <returns>A branch case for the specified value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pathBuilder"/> is null.</exception>
    public static BranchCase<TState, TDiscriminator> When(
        TDiscriminator value,
        Action<IBranchBuilder<TState>> pathBuilder)
    {
        ArgumentNullException.ThrowIfNull(pathBuilder, nameof(pathBuilder));

        return new BranchCase<TState, TDiscriminator>
        {
            Value = value,
            PathBuilder = pathBuilder,
            IsDefault = false,
        };
    }

    /// <summary>
    /// Creates a default (otherwise) branch case.
    /// </summary>
    /// <param name="pathBuilder">The action that builds the default path.</param>
    /// <returns>A default branch case.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pathBuilder"/> is null.</exception>
    public static BranchCase<TState, TDiscriminator> Otherwise(
        Action<IBranchBuilder<TState>> pathBuilder)
    {
        ArgumentNullException.ThrowIfNull(pathBuilder, nameof(pathBuilder));

        return new BranchCase<TState, TDiscriminator>
        {
            Value = default!,
            PathBuilder = pathBuilder,
            IsDefault = true,
        };
    }
}
