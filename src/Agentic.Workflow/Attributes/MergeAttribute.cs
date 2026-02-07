// -----------------------------------------------------------------------
// <copyright file="MergeAttribute.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Agentic.Workflow.Attributes;

/// <summary>
/// Marks a dictionary property for merge semantics in state reduction.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to dictionary properties (IReadOnlyDictionary, IDictionary, etc.)
/// on workflow state types. When the state reducer is generated, it will create
/// methods that merge entries into the dictionary using last-write-wins semantics.
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
///     [Merge]
///     public IReadOnlyDictionary&lt;string, string&gt; Metadata { get; init; } =
///         new Dictionary&lt;string, string&gt;();
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class MergeAttribute : Attribute
{
}