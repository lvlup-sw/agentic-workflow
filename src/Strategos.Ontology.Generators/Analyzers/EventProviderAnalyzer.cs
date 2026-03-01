using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Strategos.Ontology.Generators.Analyzers
{
    /// <summary>
    /// Analyzes Object&lt;T&gt;() registrations with events for ONTO010
    /// (events without IEventStreamProvider registration).
    /// </summary>
    internal static class EventProviderAnalyzer
    {
        internal static void Register(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeObjectType, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeObjectType(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (!IsObjectBuilderCall(invocation, context.SemanticModel, out var typeArg))
            {
                return;
            }

            if (!AnalyzerHelper.IsWithinDefineMethod(invocation, context.SemanticModel))
            {
                return;
            }

            if (invocation.ArgumentList.Arguments.Count == 0)
            {
                return;
            }

            var lambdaArg = invocation.ArgumentList.Arguments[0].Expression;

            // Check if there are any Event<T>() calls in the lambda
            var hasEvents = HasEventCalls(lambdaArg);
            if (!hasEvents)
            {
                return;
            }

            // Check if there's a UseEventStreamProvider call in the lambda
            var hasProvider = HasEventStreamProviderCall(lambdaArg);
            if (hasProvider)
            {
                return;
            }

            var diagnostic = Diagnostic.Create(
                OntologyDiagnostics.ONTO010_NoEventStreamProvider,
                invocation.GetLocation(),
                typeArg.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool HasEventCalls(ExpressionSyntax lambdaArg)
        {
            return lambdaArg.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(nested =>
                    nested.Expression is MemberAccessExpressionSyntax ma &&
                    ma.Name.Identifier.Text == "Event");
        }

        private static bool HasEventStreamProviderCall(ExpressionSyntax lambdaArg)
        {
            return lambdaArg.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(nested =>
                    nested.Expression is MemberAccessExpressionSyntax ma &&
                    ma.Name.Identifier.Text.Contains("EventStreamProvider"));
        }

        private static bool IsObjectBuilderCall(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel,
            out ITypeSymbol typeArg)
        {
            typeArg = null!;

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
            if (containingType?.Name != "IOntologyBuilder" &&
                containingType?.Name != "OntologyBuilder")
            {
                return false;
            }

            typeArg = methodSymbol.TypeArguments[0];
            return true;
        }
    }
}
