// -----------------------------------------------------------------------
// <copyright file="ValidationParser.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Agentic.Workflow.Generators.Polyfills;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agentic.Workflow.Generators.Helpers;

/// <summary>
/// Extracts validation information from ValidateState() invocations in fluent DSL.
/// </summary>
internal static class ValidationParser
{
    /// <summary>
    /// Extracts validation info from a ValidateState() invocation.
    /// </summary>
    /// <param name="invocation">The invocation expression to analyze.</param>
    /// <returns>
    /// A tuple containing the predicate expression string and error message,
    /// or (null, null) if the invocation is not a valid ValidateState call.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="invocation"/> is null.</exception>
    public static (string? Predicate, string? ErrorMessage) Extract(
        InvocationExpressionSyntax invocation)
    {
        ThrowHelper.ThrowIfNull(invocation, nameof(invocation));

        // Check if this is a ValidateState call
        if (!SyntaxHelper.IsMethodCall(invocation, "ValidateState"))
        {
            return (null, null);
        }

        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count < 2)
        {
            return (null, null);
        }

        string? predicate = null;
        string? errorMessage = null;

        // First argument: predicate lambda
        var predicateArg = arguments[0];
        if (predicateArg.Expression is LambdaExpressionSyntax lambdaExpression)
        {
            predicate = lambdaExpression.Body.ToString();
        }

        // Second argument: error message
        var errorArg = arguments[1];
        if (errorArg.Expression is LiteralExpressionSyntax literalExpression
            && literalExpression.Kind() == SyntaxKind.StringLiteralExpression)
        {
            errorMessage = literalExpression.Token.ValueText;
        }

        return (predicate, errorMessage);
    }
}