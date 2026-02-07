// -----------------------------------------------------------------------
// <copyright file="AppendAttribute.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Agentic.Workflow.Attributes;

/// <summary>
/// Marks a collection property for append semantics in state reduction.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to collection properties (IReadOnlyList, IList, etc.)
/// on workflow state types. When the state reducer is generated, it will create
/// methods that append new items to the collection rather than replacing it.
/// </para>
/// <para>
/// Example usage:
/// </para>
/// <code>
/// [WorkflowState]
/// public record OrderState : IWorkflowState
/// {
///     public Guid WorkflowId { get; init; }
///
///     [Append]
///     public IReadOnlyList&lt;OrderItem&gt; Items { get; init; } = [];
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class AppendAttribute : Attribute
{
}