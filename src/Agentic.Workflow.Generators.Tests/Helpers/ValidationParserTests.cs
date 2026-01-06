// -----------------------------------------------------------------------
// <copyright file="ValidationParserTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Agentic.Workflow.Generators.Tests.Helpers;

using Agentic.Workflow.Generators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Unit tests for <see cref="ValidationParser"/>.
/// </summary>
[Property("Category", "Unit")]
public class ValidationParserTests
{
    // =============================================================================
    // A. Guard Clause Tests
    // =============================================================================

    /// <summary>
    /// Verifies that Extract throws ArgumentNullException when invocation is null.
    /// </summary>
    [Test]
    public void Extract_NullInvocation_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ValidationParser.Extract(null!));
    }

    // =============================================================================
    // B. Non-ValidateState Tests
    // =============================================================================

    /// <summary>
    /// Verifies that Extract returns null tuple when invocation is not ValidateState.
    /// </summary>
    [Test]
    public async Task Extract_NonValidateStateCall_ReturnsNullTuple()
    {
        // Arrange
        var invocation = ParseInvocation("builder.Then<Step>()");

        // Act
        var (predicate, errorMessage) = ValidationParser.Extract(invocation);

        // Assert
        await Assert.That(predicate).IsNull();
        await Assert.That(errorMessage).IsNull();
    }

    /// <summary>
    /// Verifies that Extract returns null tuple when ValidateState has no arguments.
    /// </summary>
    [Test]
    public async Task Extract_ValidateStateNoArguments_ReturnsNullTuple()
    {
        // Arrange
        var invocation = ParseInvocation("builder.ValidateState()");

        // Act
        var (predicate, errorMessage) = ValidationParser.Extract(invocation);

        // Assert
        await Assert.That(predicate).IsNull();
        await Assert.That(errorMessage).IsNull();
    }

    // =============================================================================
    // C. Predicate Extraction Tests
    // =============================================================================

    /// <summary>
    /// Verifies that Extract extracts predicate from ValidateState call.
    /// </summary>
    [Test]
    public async Task Extract_ValidateStateWithPredicate_ExtractsPredicate()
    {
        // Arrange
        var invocation = ParseInvocation("builder.ValidateState(state => state.IsValid, \"Invalid state\")");

        // Act
        var (predicate, errorMessage) = ValidationParser.Extract(invocation);

        // Assert
        await Assert.That(predicate).IsEqualTo("state.IsValid");
    }

    /// <summary>
    /// Verifies that Extract extracts error message from ValidateState call.
    /// </summary>
    [Test]
    public async Task Extract_ValidateStateWithErrorMessage_ExtractsMessage()
    {
        // Arrange
        var invocation = ParseInvocation("builder.ValidateState(state => state.IsValid, \"Invalid state\")");

        // Act
        var (predicate, errorMessage) = ValidationParser.Extract(invocation);

        // Assert
        await Assert.That(errorMessage).IsEqualTo("Invalid state");
    }

    /// <summary>
    /// Verifies that Extract extracts complex predicate expression.
    /// </summary>
    [Test]
    public async Task Extract_ComplexPredicate_ExtractsFullExpression()
    {
        // Arrange
        var invocation = ParseInvocation("builder.ValidateState(s => s.Amount > 0 && s.Status == \"Active\", \"Must have positive amount and active status\")");

        // Act
        var (predicate, errorMessage) = ValidationParser.Extract(invocation);

        // Assert
        await Assert.That(predicate).IsEqualTo("s.Amount > 0 && s.Status == \"Active\"");
        await Assert.That(errorMessage).IsEqualTo("Must have positive amount and active status");
    }

    // =============================================================================
    // Private Helpers
    // =============================================================================

    private static InvocationExpressionSyntax ParseInvocation(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText($"class C {{ void M() {{ {code}; }} }}");
        return syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .First();
    }
}
