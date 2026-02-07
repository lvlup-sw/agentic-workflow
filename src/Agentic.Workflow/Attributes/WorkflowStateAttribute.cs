// -----------------------------------------------------------------------
// <copyright file="WorkflowStateAttribute.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Agentic.Workflow.Attributes;

/// <summary>
/// Marks a record or class as a workflow state type for reducer generation.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to a record or class that represents workflow state.
/// The source generator will produce a state reducer class with methods for
/// immutable state updates based on property attributes.
/// </para>
/// <para>
/// Properties marked with <see cref="AppendAttribute"/> will generate append
/// methods for collections. Properties marked with <see cref="MergeAttribute"/>
/// will generate merge methods for dictionaries.
/// </para>
/// <para>
/// Example usage:
/// </para>
/// <code>
/// [WorkflowState]
/// public record OrderState : IWorkflowState
/// {
///     public Guid WorkflowId { get; init; }
///     public string Status { get; init; } = "";
///
///     [Append]
///     public IReadOnlyList&lt;OrderItem&gt; Items { get; init; } = [];
///
///     [Merge]
///     public IReadOnlyDictionary&lt;string, string&gt; Metadata { get; init; } =
///         new Dictionary&lt;string, string&gt;();
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class WorkflowStateAttribute : Attribute
{
}