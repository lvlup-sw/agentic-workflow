using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Strategos.Ontology.Generators.Analyzers
{
    /// <summary>
    /// Analyzes Implements&lt;T&gt;() Via() calls for ONTO005 (incompatible property types).
    /// </summary>
    internal static class InterfaceAnalyzer
    {
        internal static void Register(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeViaInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeViaInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
            {
                return;
            }

            if (memberAccess.Name.Identifier.Text != "Via")
            {
                return;
            }

            // Check that this is within a DomainOntology Define method
            if (!AnalyzerHelper.IsWithinDefineMethod(invocation, context.SemanticModel))
            {
                return;
            }

            // Check this is a Via() call on IInterfaceMapping
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

            if (methodSymbol == null)
            {
                return;
            }

            var containingType = methodSymbol.ContainingType;
            if (containingType?.OriginalDefinition?.Name != "IInterfaceMapping")
            {
                return;
            }

            // Via() takes two lambda arguments: source and target
            if (invocation.ArgumentList.Arguments.Count < 2)
            {
                return;
            }

            var sourceLambda = invocation.ArgumentList.Arguments[0].Expression as SimpleLambdaExpressionSyntax;
            var targetLambda = invocation.ArgumentList.Arguments[1].Expression as SimpleLambdaExpressionSyntax;

            if (sourceLambda == null || targetLambda == null)
            {
                return;
            }

            var sourceType = ExtractPropertyType(sourceLambda.Body, context.SemanticModel);
            var targetType = ExtractPropertyType(targetLambda.Body, context.SemanticModel);

            if (sourceType == null || targetType == null)
            {
                return;
            }

            // Check type compatibility
            if (!IsAssignableFrom(targetType, sourceType, context.Compilation))
            {
                var objectTypeName = GetObjectTypeName(containingType);
                var interfaceTypeName = GetInterfaceTypeName(containingType);

                var diagnostic = Diagnostic.Create(
                    OntologyDiagnostics.ONTO005_IncompatiblePropertyType,
                    invocation.GetLocation(),
                    objectTypeName,
                    interfaceTypeName,
                    sourceType.ToDisplayString(),
                    targetType.ToDisplayString());
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static ITypeSymbol? ExtractPropertyType(CSharpSyntaxNode body, SemanticModel semanticModel)
        {
            // Handle cast expressions
            var expression = body;
            while (expression is CastExpressionSyntax castExpr)
            {
                expression = (CSharpSyntaxNode)castExpr.Expression;
            }

            if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                var symbol = semanticModel.GetSymbolInfo(memberAccess).Symbol;
                if (symbol is IPropertySymbol propertySymbol)
                {
                    return propertySymbol.Type;
                }

                if (symbol is IFieldSymbol fieldSymbol)
                {
                    return fieldSymbol.Type;
                }
            }

            return null;
        }

        private static bool IsAssignableFrom(ITypeSymbol targetType, ITypeSymbol sourceType, Compilation compilation)
        {
            // Same type
            if (SymbolEqualityComparer.Default.Equals(targetType, sourceType))
            {
                return true;
            }

            // Check implicit conversion
            var conversion = compilation.ClassifyConversion(sourceType, targetType);
            return conversion.IsImplicit;
        }

        private static string GetObjectTypeName(INamedTypeSymbol interfaceMappingType)
        {
            if (interfaceMappingType.IsGenericType && interfaceMappingType.TypeArguments.Length >= 1)
            {
                return interfaceMappingType.TypeArguments[0].Name;
            }

            return "unknown";
        }

        private static string GetInterfaceTypeName(INamedTypeSymbol interfaceMappingType)
        {
            if (interfaceMappingType.IsGenericType && interfaceMappingType.TypeArguments.Length >= 2)
            {
                return interfaceMappingType.TypeArguments[1].Name;
            }

            return "unknown";
        }
    }
}
