// -----------------------------------------------------------------------
// <copyright file="FluentDslParseContext.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Agentic.Workflow.Generators.Polyfills;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agentic.Workflow.Generators.Helpers;

/// <summary>
/// Provides a pre-computed context for parsing fluent DSL workflow definitions.
/// Caches common lookups to avoid repeated traversal of the syntax tree.
/// </summary>
internal sealed class FluentDslParseContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FluentDslParseContext"/> class.
    /// </summary>
    /// <param name="typeDeclaration">The type declaration containing the workflow definition.</param>
    /// <param name="semanticModel">The semantic model for type resolution.</param>
    /// <param name="workflowName">The workflow name for condition ID generation (optional).</param>
    /// <param name="allInvocations">All invocation expressions in the type.</param>
    /// <param name="finallyInvocation">The terminal Finally call, if present.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private FluentDslParseContext(
        SyntaxNode typeDeclaration,
        SemanticModel semanticModel,
        string? workflowName,
        IReadOnlyList<InvocationExpressionSyntax> allInvocations,
        InvocationExpressionSyntax? finallyInvocation,
        CancellationToken cancellationToken)
    {
        TypeDeclaration = typeDeclaration;
        SemanticModel = semanticModel;
        WorkflowName = workflowName;
        AllInvocations = allInvocations;
        FinallyInvocation = finallyInvocation;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets the type declaration containing the workflow definition.
    /// </summary>
    public SyntaxNode TypeDeclaration { get; }

    /// <summary>
    /// Gets the semantic model for type resolution.
    /// </summary>
    public SemanticModel SemanticModel { get; }

    /// <summary>
    /// Gets the workflow name for condition ID generation.
    /// </summary>
    public string? WorkflowName { get; }

    /// <summary>
    /// Gets all invocation expressions in the type declaration.
    /// </summary>
    public IReadOnlyList<InvocationExpressionSyntax> AllInvocations { get; }

    /// <summary>
    /// Gets the terminal Finally call, or null if not found.
    /// </summary>
    public InvocationExpressionSyntax? FinallyInvocation { get; }

    /// <summary>
    /// Gets the cancellation token.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Creates a new parse context with pre-computed lookups.
    /// </summary>
    /// <param name="typeDeclaration">The type declaration containing the workflow definition.</param>
    /// <param name="semanticModel">The semantic model for type resolution.</param>
    /// <param name="workflowName">The workflow name for condition ID generation (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new <see cref="FluentDslParseContext"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="typeDeclaration"/> or <paramref name="semanticModel"/> is null.
    /// </exception>
    public static FluentDslParseContext Create(
        SyntaxNode typeDeclaration,
        SemanticModel semanticModel,
        string? workflowName,
        CancellationToken cancellationToken)
    {
        ThrowHelper.ThrowIfNull(typeDeclaration, nameof(typeDeclaration));
        ThrowHelper.ThrowIfNull(semanticModel, nameof(semanticModel));

        // Pre-compute all invocations once
        var allInvocations = typeDeclaration
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .ToList();

        // Pre-compute the Finally invocation
        var finallyInvocation = allInvocations
            .FirstOrDefault(inv => SyntaxHelper.IsMethodCall(inv, "Finally"));

        return new FluentDslParseContext(
            typeDeclaration,
            semanticModel,
            workflowName,
            allInvocations,
            finallyInvocation,
            cancellationToken);
    }
}