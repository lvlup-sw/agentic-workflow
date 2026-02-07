// -----------------------------------------------------------------------
// <copyright file="BranchExtractor.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Agentic.Workflow.Generators.Models;
using Agentic.Workflow.Generators.Polyfills;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Agentic.Workflow.Generators.Helpers;

/// <summary>
/// Extracts branch models from a workflow definition.
/// </summary>
internal static class BranchExtractor
{
    /// <summary>
    /// Extracts branch models from the workflow DSL for saga handler generation.
    /// </summary>
    /// <param name="context">The parse context containing pre-computed lookups.</param>
    /// <returns>A list of branch models in the order they appear in the workflow.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static IReadOnlyList<BranchModel> Extract(FluentDslParseContext context)
    {
        ThrowHelper.ThrowIfNull(context, nameof(context));

        // Find all Branch() method calls
        // Sort by source position to ensure branches are in workflow order
        // This is critical for identifying consecutive branches (those immediately following other branches)
        var branchInvocations = context.AllInvocations
            .Where(inv => SyntaxHelper.IsMethodCall(inv, "Branch"))
            .OrderBy(inv => inv.Span.End) // Sort by End position for correct chain order (inner invocations have smaller End)
            .ToList();

        if (branchInvocations.Count == 0)
        {
            return [];
        }

        // Get all step names in order for determining previous/rejoin steps
        var stepInfos = StepExtractor.ExtractStepInfos(context);
        var stepNames = stepInfos.Select(s => s.PhaseName).ToList();

        var branches = new List<BranchModel>();
        var branchIndex = 0;

        foreach (var branchInvocation in branchInvocations)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (TryParseBranch(branchInvocation, context.SemanticModel, context.WorkflowName ?? string.Empty, branchIndex, stepNames, out var branchModel, context.CancellationToken))
            {
                branches.Add(branchModel);
                branchIndex++;
            }
        }

        // Link consecutive branches and return only head branches
        return LinkConsecutiveBranches(branches);
    }

    /// <summary>
    /// Links consecutive branches together while preserving all branches in the output list.
    /// </summary>
    /// <param name="branches">The list of all parsed branches.</param>
    /// <returns>A list containing all branches, with head branches having NextConsecutiveBranch linked.</returns>
    /// <remarks>
    /// <para>
    /// Consecutive branches are identified by having an empty <c>PreviousStepName</c>.
    /// For example, in this workflow:
    /// <code>
    /// .Join&lt;AggregateVotes&gt;()
    /// .Branch(state => state.Cond1, ...)
    /// .Branch(state => state.Cond2, ...)
    /// .Branch(state => state.Cond3, ...)
    /// .Then&lt;NextStep&gt;()
    /// </code>
    /// Branch 1 has <c>PreviousStepName = "AggregateVotes"</c> (head branch).
    /// Branches 2 and 3 have <c>PreviousStepName = ""</c> (consecutive branches).
    /// </para>
    /// <para>
    /// This method links them: Branch1.NextConsecutiveBranch → Branch2.NextConsecutiveBranch → Branch3.
    /// ALL branches are returned in the output list (needed for step handler generation),
    /// but only head branches have NextConsecutiveBranch populated.
    /// </para>
    /// </remarks>
    private static IReadOnlyList<BranchModel> LinkConsecutiveBranches(List<BranchModel> branches)
    {
        if (branches.Count == 0)
        {
            return branches;
        }

        var result = new List<BranchModel>();
        var i = 0;

        while (i < branches.Count)
        {
            var current = branches[i];

            // If this is a consecutive branch (empty PreviousStepName), just add it as-is
            if (string.IsNullOrEmpty(current.PreviousStepName))
            {
                result.Add(current);
                i++;
                continue;
            }

            // This is a head branch - collect consecutive branches that follow it
            var consecutiveBranches = new List<BranchModel>();
            var j = i + 1;
            while (j < branches.Count && string.IsNullOrEmpty(branches[j].PreviousStepName))
            {
                consecutiveBranches.Add(branches[j]);
                j++;
            }

            // Link the head branch to its consecutive chain
            if (consecutiveBranches.Count > 0)
            {
                var linkedHead = BuildConsecutiveChain(current, consecutiveBranches);
                result.Add(linkedHead);
            }
            else
            {
                result.Add(current);
            }

            // Add all the consecutive branches as-is (for step handler generation)
            foreach (var consecutive in consecutiveBranches)
            {
                result.Add(consecutive);
            }

            // Move past all processed branches
            i = j;
        }

        return result;
    }

    /// <summary>
    /// Builds a linked chain of consecutive branches starting from the head branch.
    /// </summary>
    /// <param name="headBranch">The head branch.</param>
    /// <param name="consecutiveBranches">The consecutive branches that follow the head.</param>
    /// <returns>The head branch with consecutive branches linked via NextConsecutiveBranch.</returns>
    private static BranchModel BuildConsecutiveChain(BranchModel headBranch, List<BranchModel> consecutiveBranches)
    {
        if (consecutiveBranches.Count == 0)
        {
            return headBranch;
        }

        // Build the chain from the end backwards
        // e.g., if we have [Branch1, Branch2, Branch3], we build:
        // Branch2.NextConsecutiveBranch = Branch3
        // Branch1.NextConsecutiveBranch = Branch2
        var linkedTail = consecutiveBranches[consecutiveBranches.Count - 1];
        for (var k = consecutiveBranches.Count - 2; k >= 0; k--)
        {
            linkedTail = consecutiveBranches[k] with { NextConsecutiveBranch = linkedTail };
        }

        // Link head to the chain
        return headBranch with { NextConsecutiveBranch = linkedTail };
    }

    private static bool TryParseBranch(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        string workflowName,
        int branchIndex,
        List<string> stepNames,
        out BranchModel branchModel,
        CancellationToken cancellationToken)
    {
        branchModel = default!;

        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count < 2)
        {
            return false;
        }

        // Skip branches that immediately follow a RepeatUntil loop
        // These are handled by the loop exit handler via BranchOnExitId
        if (IsBranchAfterLoop(invocation))
        {
            return false;
        }

        // First argument: discriminator (lambda or method reference)
        var discriminatorArg = arguments[0];
        if (!TryExtractDiscriminatorInfo(discriminatorArg, semanticModel, out var propertyPath, out var typeName, out var isEnum, out var isMethod))
        {
            return false;
        }

        // Find previous step (the receiver of the Branch call)
        var previousStepName = FindPreviousStepName(invocation, semanticModel);

        // Find rejoin step (step after this branch in the chain)
        var rejoinStepName = FindRejoinStepName(invocation, semanticModel, stepNames);

        // Determine the loop prefix for this branch (if inside a loop)
        // Branch case steps need the same prefix as the branch's previous/rejoin steps
        var loopPrefix = DetermineLoopPrefix(invocation);

        // Parse branch cases from remaining arguments
        var cases = new List<BranchCaseModel>();
        for (var i = 1; i < arguments.Count; i++)
        {
            if (TryParseBranchCase(arguments[i], semanticModel, propertyPath, cancellationToken, out var caseModel))
            {
                cases.Add(caseModel);
            }
        }

        if (cases.Count == 0)
        {
            return false;
        }

        var branchId = $"{workflowName}-Branch{branchIndex}-{propertyPath}";

        branchModel = new BranchModel(
            BranchId: branchId,
            PreviousStepName: previousStepName ?? string.Empty,
            DiscriminatorPropertyPath: propertyPath,
            DiscriminatorTypeName: typeName,
            IsEnumDiscriminator: isEnum,
            IsMethodDiscriminator: isMethod,
            Cases: cases,
            RejoinStepName: rejoinStepName,
            LoopPrefix: loopPrefix);

        return true;
    }

    private static bool TryExtractDiscriminatorInfo(
        ArgumentSyntax discriminatorArg,
        SemanticModel semanticModel,
        out string propertyPath,
        out string typeName,
        out bool isEnum,
        out bool isMethod)
    {
        propertyPath = string.Empty;
        typeName = string.Empty;
        isEnum = false;
        isMethod = false;

        // Try to extract lambda: state => state.Property or state => state.Method()
        var lambda = discriminatorArg.Expression switch
        {
            SimpleLambdaExpressionSyntax simple => simple,
            ParenthesizedLambdaExpressionSyntax parens => (LambdaExpressionSyntax)parens,
            _ => null
        };

        if (lambda is not null)
        {
            // Get the body from the lambda
            var body = lambda switch
            {
                SimpleLambdaExpressionSyntax simple => simple.Body,
                ParenthesizedLambdaExpressionSyntax parens => parens.Body,
                _ => null
            };

            // Handle property access: state => state.GateRequiresHitl
            if (body is MemberAccessExpressionSyntax memberAccess)
            {
                // Extract property path
                propertyPath = SyntaxHelper.ExtractPropertyPath(memberAccess);

                // Get the type info for the property
                var typeInfo = semanticModel.GetTypeInfo(memberAccess);
                if (typeInfo.Type is INamedTypeSymbol namedType)
                {
                    typeName = namedType.Name;
                    isEnum = namedType.TypeKind == TypeKind.Enum;
                }
                else
                {
                    // Fallback to syntax-based extraction
                    typeName = "Object";
                }

                return !string.IsNullOrEmpty(propertyPath);
            }

            // Handle method invocation on state: state => state.IsInCrisisMode()
            // Treat like property access but include () in the path for method call syntax
            if (body is InvocationExpressionSyntax invocationBody &&
                invocationBody.Expression is MemberAccessExpressionSyntax invokedMember)
            {
                // Extract method name with parentheses for proper code generation
                // This generates: State.IsInCrisisMode() (like property but with method call syntax)
                propertyPath = invokedMember.Name.Identifier.Text + "()";
                isMethod = false; // Treat as property-style access (State.X rather than X(State))

                // Get the return type of the method
                var typeInfo = semanticModel.GetTypeInfo(invocationBody);
                if (typeInfo.Type is INamedTypeSymbol namedType)
                {
                    typeName = namedType.Name;
                    isEnum = namedType.TypeKind == TypeKind.Enum;
                }
                else
                {
                    typeName = "Boolean"; // Most method discriminators return bool
                }

                return !string.IsNullOrEmpty(propertyPath);
            }

            return false;
        }

        // Try to extract method reference: DetermineOutcome (IdentifierNameSyntax)
        if (discriminatorArg.Expression is IdentifierNameSyntax identifier)
        {
            propertyPath = identifier.Identifier.Text;
            isMethod = true;

            // Get the method symbol to determine return type
            var symbolInfo = semanticModel.GetSymbolInfo(identifier);
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                var returnType = methodSymbol.ReturnType;
                if (returnType is INamedTypeSymbol returnNamedType)
                {
                    typeName = returnNamedType.Name;
                    isEnum = returnNamedType.TypeKind == TypeKind.Enum;
                }
                else
                {
                    typeName = returnType.Name;
                }
            }
            else
            {
                // Fallback - assume enum based on common usage pattern
                typeName = "Object";
            }

            return !string.IsNullOrEmpty(propertyPath);
        }

        return false;
    }

    /// <summary>
    /// Checks if this Branch invocation immediately follows a RepeatUntil loop.
    /// </summary>
    /// <param name="branchInvocation">The Branch invocation to check.</param>
    /// <returns>True if the Branch's receiver is a RepeatUntil call.</returns>
    private static bool IsBranchAfterLoop(InvocationExpressionSyntax branchInvocation)
    {
        // Get the receiver of the Branch call (what .Branch() is called on)
        if (branchInvocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess.Expression is InvocationExpressionSyntax previousInvocation)
            {
                // Check if the receiver is a RepeatUntil call
                return SyntaxHelper.IsMethodCall(previousInvocation, "RepeatUntil");
            }
        }

        return false;
    }

    private static string? FindPreviousStepName(InvocationExpressionSyntax branchInvocation, SemanticModel semanticModel)
    {
        // Determine the loop prefix for this branch (if inside a loop)
        var loopPrefix = DetermineLoopPrefix(branchInvocation);

        // Walk backwards to find the previous step
        if (branchInvocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess.Expression is InvocationExpressionSyntax previousInvocation)
            {
                if (StepExtractor.TryGetStepName(previousInvocation, semanticModel, out var stepName))
                {
                    // Apply loop prefix to match the prefixed step names used in saga handlers
                    return ApplyPrefix(stepName, loopPrefix);
                }

                // If the previous call is also a Branch, don't recurse - this branch follows another branch
                // Return null to indicate this branch is part of a chain and should be evaluated
                // after the previous branch's rejoin (handled by RejoinStepName chain)
                if (SyntaxHelper.IsMethodCall(previousInvocation, "Branch"))
                {
                    return null;
                }

                // Recurse backwards if the previous call isn't a step and isn't a branch
                return FindPreviousStepName(previousInvocation, semanticModel);
            }
        }

        return null;
    }

    private static string? FindRejoinStepName(
        InvocationExpressionSyntax branchInvocation,
        SemanticModel semanticModel,
        List<string> stepNames)
    {
        // Determine if this branch is inside a loop and get the loop prefix
        var loopPrefix = DetermineLoopPrefix(branchInvocation);

        // Walk the chain of calls starting from this branch to find the next step
        // For consecutive branches like .Branch(cond1).Branch(cond2).Then<Step>(),
        // we need to walk through all the branches to find the final step
        var currentInvocation = branchInvocation;

        while (currentInvocation is not null)
        {
            // Look for a call that chains off the current invocation
            var parent = currentInvocation.Parent;
            while (parent is not null)
            {
                if (parent is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression == currentInvocation)
                {
                    // Found a call that chains off the current invocation
                    if (memberAccess.Parent is InvocationExpressionSyntax nextInvocation)
                    {
                        // If it's a step, we found the rejoin point
                        if (StepExtractor.TryGetStepName(nextInvocation, semanticModel, out var stepName))
                        {
                            // Apply loop prefix if branch is inside a loop
                            return ApplyPrefix(stepName, loopPrefix);
                        }

                        // If it's another branch, continue walking from that branch
                        if (SyntaxHelper.IsMethodCall(nextInvocation, "Branch"))
                        {
                            currentInvocation = nextInvocation;
                            break;
                        }
                    }
                }

                parent = parent.Parent;
            }

            // If we didn't find another invocation to continue from, stop
            if (parent is null)
            {
                break;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines the loop prefix for a branch invocation by walking up the syntax tree
    /// to find parent RepeatUntil lambda bodies.
    /// </summary>
    /// <param name="branchInvocation">The branch invocation syntax.</param>
    /// <returns>The combined loop prefix (e.g., "Outer_Inner"), or null if not in a loop.</returns>
    private static string? DetermineLoopPrefix(InvocationExpressionSyntax branchInvocation)
    {
        var loopNames = new List<string>();
        var current = branchInvocation.Parent;

        while (current is not null)
        {
            if (current is LambdaExpressionSyntax lambda)
            {
                var loopName = FindContainingLoopName(lambda);
                if (loopName is not null)
                {
                    loopNames.Insert(0, loopName);
                }
            }

            current = current.Parent;
        }

        if (loopNames.Count == 0)
        {
            return null;
        }

        return string.Join("_", loopNames);
    }

    /// <summary>
    /// Finds the loop name if the given lambda is the body argument of a RepeatUntil call.
    /// </summary>
    /// <param name="lambda">The lambda expression to check.</param>
    /// <returns>The loop name, or null if not a RepeatUntil body.</returns>
    private static string? FindContainingLoopName(LambdaExpressionSyntax lambda)
    {
        // The lambda's parent should be ArgumentSyntax -> ArgumentListSyntax -> InvocationExpressionSyntax (RepeatUntil)
        if (lambda.Parent is not ArgumentSyntax arg)
        {
            return null;
        }

        if (arg.Parent is not ArgumentListSyntax argList)
        {
            return null;
        }

        if (argList.Parent is not InvocationExpressionSyntax invocation)
        {
            return null;
        }

        if (!SyntaxHelper.IsMethodCall(invocation, "RepeatUntil"))
        {
            return null;
        }

        // RepeatUntil has 4 arguments: condition, loopName, body, maxIterations
        // The body is the 3rd argument (index 2)
        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count < 3)
        {
            return null;
        }

        // Check if this lambda is the body argument
        var bodyArgIndex = 2;
        if (arguments.Count <= bodyArgIndex || arguments[bodyArgIndex] != arg)
        {
            return null;
        }

        // Extract the loop name from the second argument
        var loopNameArg = arguments[1];
        if (loopNameArg.Expression is LiteralExpressionSyntax literal
            && literal.Kind() == SyntaxKind.StringLiteralExpression)
        {
            return literal.Token.ValueText;
        }

        return null;
    }

    /// <summary>
    /// Applies a loop prefix to a step name.
    /// </summary>
    /// <param name="stepName">The step name.</param>
    /// <param name="prefix">The loop prefix, or null for no prefix.</param>
    /// <returns>The prefixed step name, or the original if no prefix.</returns>
    private static string? ApplyPrefix(string? stepName, string? prefix)
    {
        if (prefix is null || stepName is null)
        {
            return stepName;
        }

        return $"{prefix}_{stepName}";
    }

    private static bool TryParseBranchCase(
        ArgumentSyntax caseArg,
        SemanticModel semanticModel,
        string branchPropertyPath,
        CancellationToken cancellationToken,
        out BranchCaseModel caseModel)
    {
        caseModel = default!;

        var expression = caseArg.Expression;

        if (expression is not InvocationExpressionSyntax caseInvocation)
        {
            return false;
        }

        // Check if it's When() or Otherwise()
        var isOtherwise = SyntaxHelper.IsMethodCall(caseInvocation, "Otherwise");
        var isWhen = SyntaxHelper.IsMethodCall(caseInvocation, "When");

        if (!isWhen && !isOtherwise)
        {
            return false;
        }

        var caseArgs = caseInvocation.ArgumentList.Arguments;
        string caseValueLiteral;
        ArgumentSyntax pathBuilderArg;

        if (isOtherwise)
        {
            if (caseArgs.Count < 1)
            {
                return false;
            }

            caseValueLiteral = "default";
            pathBuilderArg = caseArgs[0];
        }
        else
        {
            if (caseArgs.Count < 2)
            {
                return false;
            }

            caseValueLiteral = ExtractCaseValueLiteral(caseArgs[0]);
            pathBuilderArg = caseArgs[1];
        }

        // Extract path builder lambda
        var pathLambda = pathBuilderArg.Expression switch
        {
            SimpleLambdaExpressionSyntax simple => simple,
            ParenthesizedLambdaExpressionSyntax parens => (LambdaExpressionSyntax)parens,
            _ => null
        };

        if (pathLambda is null)
        {
            return false;
        }

        // Extract steps from the path builder
        var stepNames = new List<string>();
        var isTerminal = false;
        ParseBranchPathBody(pathLambda, semanticModel, stepNames, ref isTerminal, cancellationToken);

        if (stepNames.Count == 0)
        {
            return false;
        }

        var branchPathPrefix = $"{branchPropertyPath}_{caseValueLiteral.Replace(".", "_").Replace(" ", "_")}";

        caseModel = new BranchCaseModel(
            CaseValueLiteral: caseValueLiteral,
            BranchPathPrefix: branchPathPrefix,
            StepNames: stepNames,
            IsTerminal: isTerminal);

        return true;
    }

    private static string ExtractCaseValueLiteral(ArgumentSyntax valueArg)
    {
        var expression = valueArg.Expression;

        return expression switch
        {
            // Enum value: ClaimType.Auto
            MemberAccessExpressionSyntax memberAccess => memberAccess.ToString(),
            // String literal: "pdf"
            LiteralExpressionSyntax literal when literal.Kind() == SyntaxKind.StringLiteralExpression => literal.Token.ValueText,
            // Boolean: true/false
            LiteralExpressionSyntax literal when literal.Kind() == SyntaxKind.TrueLiteralExpression => "true",
            LiteralExpressionSyntax literal when literal.Kind() == SyntaxKind.FalseLiteralExpression => "false",
            // Numeric: 1, 2, etc.
            LiteralExpressionSyntax literal when literal.Kind() == SyntaxKind.NumericLiteralExpression => literal.Token.ValueText,
            // Default fallback
            _ => expression.ToString()
        };
    }

    private static void ParseBranchPathBody(
        LambdaExpressionSyntax pathLambda,
        SemanticModel semanticModel,
        List<string> stepNames,
        ref bool isTerminal,
        CancellationToken cancellationToken)
    {
        // Find all invocations in the path body, reversed for correct order
        var allInvocations = pathLambda
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Reverse()
            .ToList();

        foreach (var inv in allInvocations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (SyntaxHelper.IsMethodCall(inv, "Then"))
            {
                if (StepExtractor.TryGetStepName(inv, semanticModel, out var stepName))
                {
                    stepNames.Add(stepName);
                }
            }
            else if (SyntaxHelper.IsMethodCall(inv, "Complete"))
            {
                isTerminal = true;
            }
        }
    }
}
