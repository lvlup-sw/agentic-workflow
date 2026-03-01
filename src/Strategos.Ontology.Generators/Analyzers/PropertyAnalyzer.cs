using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Strategos.Ontology.Generators.Analyzers
{
    /// <summary>
    /// Analyzes Property() and Key() expression arguments for ONTO002 (non-existent member).
    /// </summary>
    internal static class PropertyAnalyzer
    {
        internal static void Register(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzePropertyInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzePropertyInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
            {
                return;
            }

            var methodName = memberAccess.Name.Identifier.Text;
            if (methodName != "Property" && methodName != "Key")
            {
                return;
            }

            // Check that this is within a DomainOntology Define method
            if (!IsWithinDefineMethod(invocation, context.SemanticModel))
            {
                return;
            }

            // Check that this call is on an IObjectTypeBuilder
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

            if (methodSymbol == null)
            {
                return;
            }

            var containingType = methodSymbol.ContainingType;
            if (containingType == null)
            {
                return;
            }

            if (containingType.Name != "IObjectTypeBuilder" && containingType.OriginalDefinition?.Name != "IObjectTypeBuilder")
            {
                return;
            }

            // Get the expression argument (first argument to Property/Key)
            if (invocation.ArgumentList.Arguments.Count == 0)
            {
                return;
            }

            var argExpression = invocation.ArgumentList.Arguments[0].Expression;

            // The argument should be a lambda expression like e => e.SomeProperty
            if (!(argExpression is SimpleLambdaExpressionSyntax lambda))
            {
                return;
            }

            // Check if the lambda body resolves to a valid property member access
            if (!IsValidPropertyMemberAccess(lambda.Body, context.SemanticModel))
            {
                var expressionText = lambda.Body.ToString();
                var entityTypeName = GetEntityTypeName(containingType);
                var diagnostic = Diagnostic.Create(
                    OntologyDiagnostics.ONTO002_InvalidProperty,
                    invocation.GetLocation(),
                    expressionText,
                    entityTypeName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsValidPropertyMemberAccess(CSharpSyntaxNode body, SemanticModel semanticModel)
        {
            // Handle conversion expressions like (object)e.Id
            var expression = body;
            while (expression is CastExpressionSyntax castExpr)
            {
                expression = (CSharpSyntaxNode)castExpr.Expression;
            }

            // The body should be a member access expression (e.g., e.Id)
            if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                var symbol = semanticModel.GetSymbolInfo(memberAccess).Symbol;
                if (symbol is IPropertySymbol)
                {
                    return true;
                }

                if (symbol is IFieldSymbol)
                {
                    return true;
                }

                return false;
            }

            // Handle simple identifier (unlikely in lambda body but cover it)
            if (expression is IdentifierNameSyntax)
            {
                return true;
            }

            return false;
        }

        private static string GetEntityTypeName(INamedTypeSymbol objectTypeBuilderType)
        {
            if (objectTypeBuilderType.IsGenericType && objectTypeBuilderType.TypeArguments.Length > 0)
            {
                return objectTypeBuilderType.TypeArguments[0].Name;
            }

            return "unknown";
        }

        private static bool IsWithinDefineMethod(SyntaxNode node, SemanticModel semanticModel)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current is MethodDeclarationSyntax methodDecl &&
                    methodDecl.Identifier.Text == "Define" &&
                    methodDecl.Modifiers.Any(SyntaxKind.OverrideKeyword))
                {
                    var containingType = semanticModel.GetDeclaredSymbol(methodDecl)?.ContainingType;
                    if (containingType != null && IsDomainOntologySubclass(containingType))
                    {
                        return true;
                    }
                }

                current = current.Parent;
            }

            return false;
        }

        private static bool IsDomainOntologySubclass(INamedTypeSymbol typeSymbol)
        {
            var baseType = typeSymbol.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == "DomainOntology" &&
                    baseType.ContainingNamespace?.ToDisplayString() == "Strategos.Ontology")
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }
    }
}
