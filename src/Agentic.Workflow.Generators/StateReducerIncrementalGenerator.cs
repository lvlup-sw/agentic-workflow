// -----------------------------------------------------------------------
// <copyright file="StateReducerIncrementalGenerator.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Agentic.Workflow.Generators.Diagnostics;
using Agentic.Workflow.Generators.Emitters;
using Agentic.Workflow.Generators.Models;

namespace Agentic.Workflow.Generators;

/// <summary>
/// Incremental source generator that produces state reducer classes
/// from records/structs marked with [WorkflowState] attribute.
/// </summary>
[Generator]
public sealed class StateReducerIncrementalGenerator : IIncrementalGenerator
{
    private const string WorkflowStateAttributeFullName = "Agentic.Workflow.Attributes.WorkflowStateAttribute";
    private const string AppendAttributeFullName = "Agentic.Workflow.Attributes.AppendAttribute";
    private const string MergeAttributeFullName = "Agentic.Workflow.Attributes.MergeAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all classes/records/structs with [WorkflowState] attribute
        var stateDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                WorkflowStateAttributeFullName,
                predicate: static (node, _) => IsValidTargetNode(node),
                transform: static (ctx, ct) => TransformToResult(ctx, ct));

        // Register source output for each state type
        context.RegisterSourceOutput(stateDeclarations, static (spc, result) =>
        {
            // Report diagnostics
            foreach (var diagnostic in result.Diagnostics)
            {
                spc.ReportDiagnostic(diagnostic);
            }

            // Generate source if model is valid
            if (result.Model is not null)
            {
                var source = StateReducerEmitter.Emit(result.Model);
                var hintName = $"{result.Model.ReducerClassName}.g.cs";
                spc.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
            }
        });
    }

    private static bool IsValidTargetNode(SyntaxNode node)
    {
        return node is RecordDeclarationSyntax or ClassDeclarationSyntax or StructDeclarationSyntax;
    }

    private static StateReducerGeneratorResult TransformToResult(
        GeneratorAttributeSyntaxContext context,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var diagnostics = new List<Diagnostic>();

        // Get type symbol
        var symbol = context.TargetSymbol as INamedTypeSymbol;
        if (symbol is null)
        {
            return new StateReducerGeneratorResult(null, diagnostics);
        }

        // Get namespace
        var ns = symbol.ContainingNamespace?.ToDisplayString();
        if (string.IsNullOrEmpty(ns) || ns == "<global namespace>")
        {
            return new StateReducerGeneratorResult(null, diagnostics);
        }

        // Get type name
        var typeName = symbol.Name;

        // Extract properties with their kinds (and validate attribute usage)
        var properties = ExtractProperties(symbol, context.SemanticModel.Compilation, diagnostics);

        var model = new StateModel(
            TypeName: typeName,
            Namespace: ns!,
            Properties: properties);

        return new StateReducerGeneratorResult(model, diagnostics);
    }

    private static IReadOnlyList<StatePropertyModel> ExtractProperties(
        INamedTypeSymbol symbol,
        Compilation compilation,
        List<Diagnostic> diagnostics)
    {
        var properties = new List<StatePropertyModel>();

        foreach (var member in symbol.GetMembers())
        {
            if (member is not IPropertySymbol property)
            {
                continue;
            }

            // Skip non-public properties
            if (property.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            // Skip write-only properties
            if (property.GetMethod is null)
            {
                continue;
            }

            // Determine property kind based on attributes (with validation)
            var kind = GetPropertyKind(property, compilation, diagnostics);

            properties.Add(new StatePropertyModel(
                Name: property.Name,
                TypeName: property.Type.ToDisplayString(),
                Kind: kind));
        }

        return properties;
    }

    private static StatePropertyKind GetPropertyKind(
        IPropertySymbol property,
        Compilation compilation,
        List<Diagnostic> diagnostics)
    {
        foreach (var attribute in property.GetAttributes())
        {
            var fullName = attribute.AttributeClass?.ToDisplayString();

            if (fullName == AppendAttributeFullName)
            {
                // Validate that the property type implements IEnumerable<T>
                if (!IsCollectionType(property.Type, compilation))
                {
                    var location = GetPropertyLocation(property);
                    diagnostics.Add(Diagnostic.Create(
                        StateReducerDiagnostics.AppendOnNonCollection,
                        location,
                        property.Name,
                        property.Type.ToDisplayString()));
                }

                return StatePropertyKind.Append;
            }

            if (fullName == MergeAttributeFullName)
            {
                // Validate that the property type is a dictionary type
                if (!IsDictionaryType(property.Type, compilation))
                {
                    var location = GetPropertyLocation(property);
                    diagnostics.Add(Diagnostic.Create(
                        StateReducerDiagnostics.MergeOnNonDictionary,
                        location,
                        property.Name,
                        property.Type.ToDisplayString()));
                }

                return StatePropertyKind.Merge;
            }
        }

        return StatePropertyKind.Standard;
    }

    private static bool IsCollectionType(ITypeSymbol type, Compilation compilation)
    {
        // Check if type implements IEnumerable<T> (but not string)
        if (type.SpecialType == SpecialType.System_String)
        {
            return false;
        }

        var enumerableT = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
        if (enumerableT is null)
        {
            return false;
        }

        // Check if type itself is IEnumerable<T> (for interface types)
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var originalDef = namedType.OriginalDefinition;
            if (originalDef.Equals(enumerableT, SymbolEqualityComparer.Default))
            {
                return true;
            }
        }

        // Check if the type implements IEnumerable<T>
        return type.AllInterfaces.Any(i =>
            i.OriginalDefinition.Equals(enumerableT, SymbolEqualityComparer.Default));
    }

    private static bool IsDictionaryType(ITypeSymbol type, Compilation compilation)
    {
        // Check if type implements IReadOnlyDictionary<TKey, TValue> or IDictionary<TKey, TValue>
        var readOnlyDict = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyDictionary`2");
        var dict = compilation.GetTypeByMetadataName("System.Collections.Generic.IDictionary`2");

        if (readOnlyDict is null && dict is null)
        {
            return false;
        }

        // Check if type itself is the dictionary type (for interface types)
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var originalDef = namedType.OriginalDefinition;
            if ((readOnlyDict is not null && originalDef.Equals(readOnlyDict, SymbolEqualityComparer.Default))
                || (dict is not null && originalDef.Equals(dict, SymbolEqualityComparer.Default)))
            {
                return true;
            }
        }

        // Check if type implements the dictionary interface
        return type.AllInterfaces.Any(i =>
            (readOnlyDict is not null && i.OriginalDefinition.Equals(readOnlyDict, SymbolEqualityComparer.Default))
            || (dict is not null && i.OriginalDefinition.Equals(dict, SymbolEqualityComparer.Default)));
    }

    private static Location GetPropertyLocation(IPropertySymbol property)
    {
        var syntaxRef = property.DeclaringSyntaxReferences.FirstOrDefault();
        return syntaxRef?.GetSyntax().GetLocation() ?? Location.None;
    }

    /// <summary>
    /// Result of transforming a state declaration, including model and diagnostics.
    /// </summary>
    private sealed record StateReducerGeneratorResult(
        StateModel? Model,
        IReadOnlyList<Diagnostic> Diagnostics);
}
