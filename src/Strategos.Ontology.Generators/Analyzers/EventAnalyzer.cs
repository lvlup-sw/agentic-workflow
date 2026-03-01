using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Strategos.Ontology.Generators.Analyzers
{
    /// <summary>
    /// Analyzes Event&lt;TEvent&gt;() MaterializesLink() calls for ONTO009 (undeclared link name).
    /// </summary>
    internal static class EventAnalyzer
    {
        internal static void Register(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeObjectTypeForEvents, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeObjectTypeForEvents(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            // Look for builder.Object<T>() calls
            if (!IsObjectBuilderCall(invocation, context.SemanticModel))
            {
                return;
            }

            if (!AnalyzerHelper.IsWithinDefineMethod(invocation, context.SemanticModel))
            {
                return;
            }

            // Get the lambda argument
            if (invocation.ArgumentList.Arguments.Count == 0)
            {
                return;
            }

            var lambdaArg = invocation.ArgumentList.Arguments[0].Expression;

            // Collect declared link names from HasOne, HasMany, ManyToMany calls
            var declaredLinks = new HashSet<string>();
            var nestedInvocations = lambdaArg.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();

            foreach (var nested in nestedInvocations)
            {
                if (nested.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    var methodName = memberAccess.Name.Identifier.Text;
                    if (methodName == "HasOne" || methodName == "HasMany" || methodName == "ManyToMany")
                    {
                        var linkName = ExtractFirstStringArgument(nested);
                        if (linkName != null)
                        {
                            declaredLinks.Add(linkName);
                        }
                    }
                }
            }

            // Find MaterializesLink calls and check link names
            foreach (var nested in nestedInvocations)
            {
                if (nested.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Name.Identifier.Text == "MaterializesLink")
                {
                    var linkName = ExtractFirstStringArgument(nested);
                    if (linkName != null && !declaredLinks.Contains(linkName))
                    {
                        // Get the entity type name from the Object<T> invocation
                        var entityTypeName = GetObjectTypeArg(invocation, context.SemanticModel);
                        var diagnostic = Diagnostic.Create(
                            OntologyDiagnostics.ONTO009_UndeclaredLink,
                            nested.GetLocation(),
                            linkName,
                            entityTypeName);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static bool IsObjectBuilderCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(invocation);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

            if (methodSymbol == null)
            {
                return false;
            }

            if (methodSymbol.Name != "Object" || !methodSymbol.IsGenericMethod || methodSymbol.TypeArguments.Length != 1)
            {
                return false;
            }

            var containingType = methodSymbol.ContainingType;
            return containingType?.Name == "IOntologyBuilder" || containingType?.Name == "OntologyBuilder";
        }

        private static string GetObjectTypeArg(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(invocation);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

            if (methodSymbol?.IsGenericMethod == true && methodSymbol.TypeArguments.Length > 0)
            {
                return methodSymbol.TypeArguments[0].Name;
            }

            return "unknown";
        }

        private static string? ExtractFirstStringArgument(InvocationExpressionSyntax invocation)
        {
            if (invocation.ArgumentList.Arguments.Count == 0)
            {
                return null;
            }

            var firstArg = invocation.ArgumentList.Arguments[0].Expression;
            if (firstArg is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return literal.Token.ValueText;
            }

            return null;
        }
    }
}
