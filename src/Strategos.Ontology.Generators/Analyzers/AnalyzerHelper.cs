using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Strategos.Ontology.Generators.Analyzers
{
    /// <summary>
    /// Shared helper methods for ontology analyzers.
    /// </summary>
    internal static class AnalyzerHelper
    {
        internal static bool IsWithinDefineMethod(SyntaxNode node, SemanticModel semanticModel)
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

        internal static bool IsDomainOntologySubclass(INamedTypeSymbol typeSymbol)
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
