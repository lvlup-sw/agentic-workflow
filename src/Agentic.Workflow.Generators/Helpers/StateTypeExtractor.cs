// -----------------------------------------------------------------------
// <copyright file="StateTypeExtractor.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Agentic.Workflow.Generators.Polyfills;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agentic.Workflow.Generators.Helpers;

/// <summary>
/// Extracts the state type name from a workflow definition.
/// </summary>
internal static class StateTypeExtractor
{
    /// <summary>
    /// Extracts the state type name from the workflow definition (e.g., "OrderState" from Workflow&lt;OrderState&gt;).
    /// </summary>
    /// <param name="context">The parse context containing pre-computed lookups.</param>
    /// <returns>The state type name, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static string? Extract(FluentDslParseContext context)
    {
        ThrowHelper.ThrowIfNull(context, nameof(context));

        context.CancellationToken.ThrowIfCancellationRequested();

        // Look for .Create("...") method call
        var createInvocation = context.AllInvocations
            .FirstOrDefault(inv => SyntaxHelper.IsMethodCall(inv, "Create"));

        if (createInvocation?.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return null;
        }

        // The receiver should be Workflow<TState> - look for the generic type argument
        if (memberAccess.Expression is GenericNameSyntax genericName)
        {
            // Get the first type argument (the state type)
            var typeArgument = genericName.TypeArgumentList.Arguments.FirstOrDefault();
            if (typeArgument is null)
            {
                return null;
            }

            // Resolve the symbol to get the type name
            var symbolInfo = context.SemanticModel.GetSymbolInfo(typeArgument);
            if (symbolInfo.Symbol is INamedTypeSymbol namedType)
            {
                return namedType.Name;
            }

            // Fallback to syntax-based name
            return SyntaxHelper.GetTypeNameFromSyntax(typeArgument);
        }

        return null;
    }
}
