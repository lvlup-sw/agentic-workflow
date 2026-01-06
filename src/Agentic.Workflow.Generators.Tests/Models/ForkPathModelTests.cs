// -----------------------------------------------------------------------
// <copyright file="ForkPathModelTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Agentic.Workflow.Generators.Tests.Models;

using Agentic.Workflow.Generators.Models;

/// <summary>
/// Unit tests for <see cref="ForkPathModel"/>.
/// </summary>
[Property("Category", "Unit")]
public class ForkPathModelTests
{
    // =============================================================================
    // A. Factory Method Tests
    // =============================================================================

    /// <summary>
    /// Verifies that Create with valid params returns a model.
    /// </summary>
    [Test]
    public async Task Create_WithValidParams_ReturnsModel()
    {
        // Arrange
        var stepNames = new List<string> { "ProcessPayment", "ChargeCard" };

        // Act
        var model = ForkPathModel.Create(
            pathIndex: 0,
            stepNames: stepNames,
            hasFailureHandler: false,
            isTerminalOnFailure: false);

        // Assert
        await Assert.That(model.PathIndex).IsEqualTo(0);
        await Assert.That(model.StepNames.Count).IsEqualTo(2);
        await Assert.That(model.HasFailureHandler).IsFalse();
        await Assert.That(model.IsTerminalOnFailure).IsFalse();
    }

    /// <summary>
    /// Verifies that Create throws for null step names.
    /// </summary>
    [Test]
    public async Task Create_WithNullStepNames_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.That(() => ForkPathModel.Create(
            pathIndex: 0,
            stepNames: null!,
            hasFailureHandler: false,
            isTerminalOnFailure: false))
            .Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that Create throws for empty step names.
    /// </summary>
    [Test]
    public async Task Create_WithEmptyStepNames_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.That(() => ForkPathModel.Create(
            pathIndex: 0,
            stepNames: [],
            hasFailureHandler: false,
            isTerminalOnFailure: false))
            .Throws<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws for negative path index.
    /// </summary>
    [Test]
    public async Task Create_WithNegativePathIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var stepNames = new List<string> { "Step1" };

        // Act & Assert
        await Assert.That(() => ForkPathModel.Create(
            pathIndex: -1,
            stepNames: stepNames,
            hasFailureHandler: false,
            isTerminalOnFailure: false))
            .Throws<ArgumentOutOfRangeException>();
    }

    // =============================================================================
    // B. Property Tests
    // =============================================================================

    /// <summary>
    /// Verifies that FirstStepName returns the first step.
    /// </summary>
    [Test]
    public async Task FirstStepName_ReturnsFirstStep()
    {
        // Arrange
        var stepNames = new List<string> { "First", "Second", "Third" };
        var model = ForkPathModel.Create(0, stepNames, false, false);

        // Assert
        await Assert.That(model.FirstStepName).IsEqualTo("First");
    }

    /// <summary>
    /// Verifies that LastStepName returns the last step.
    /// </summary>
    [Test]
    public async Task LastStepName_ReturnsLastStep()
    {
        // Arrange
        var stepNames = new List<string> { "First", "Second", "Third" };
        var model = ForkPathModel.Create(0, stepNames, false, false);

        // Assert
        await Assert.That(model.LastStepName).IsEqualTo("Third");
    }

    /// <summary>
    /// Verifies that HasFailureHandler returns true when present.
    /// </summary>
    [Test]
    public async Task HasFailureHandler_WhenPresent_ReturnsTrue()
    {
        // Arrange
        var model = ForkPathModel.Create(
            pathIndex: 0,
            stepNames: new List<string> { "Step1" },
            hasFailureHandler: true,
            isTerminalOnFailure: false);

        // Assert
        await Assert.That(model.HasFailureHandler).IsTrue();
    }

    /// <summary>
    /// Verifies that StatusPropertyName returns correct name.
    /// </summary>
    [Test]
    public async Task StatusPropertyName_ReturnsCorrectName()
    {
        // Arrange
        var model = ForkPathModel.Create(0, new List<string> { "Step1" }, false, false);

        // Assert
        await Assert.That(model.StatusPropertyName).IsEqualTo("Path0Status");
    }

    /// <summary>
    /// Verifies that StatePropertyName returns correct name.
    /// </summary>
    [Test]
    public async Task StatePropertyName_ReturnsCorrectName()
    {
        // Arrange
        var model = ForkPathModel.Create(1, new List<string> { "Step1" }, false, false);

        // Assert
        await Assert.That(model.StatePropertyName).IsEqualTo("Path1State");
    }

    /// <summary>
    /// Verifies that failure handler step names are included when present.
    /// </summary>
    [Test]
    public async Task Create_WithFailureHandlerSteps_IncludesSteps()
    {
        // Arrange
        var stepNames = new List<string> { "Process" };
        var failureSteps = new List<string> { "Recover", "Cleanup" };

        // Act
        var model = ForkPathModel.Create(
            pathIndex: 0,
            stepNames: stepNames,
            hasFailureHandler: true,
            isTerminalOnFailure: false,
            failureHandlerStepNames: failureSteps);

        // Assert
        await Assert.That(model.FailureHandlerStepNames).IsNotNull();
        await Assert.That(model.FailureHandlerStepNames!.Count).IsEqualTo(2);
    }
}
