// -----------------------------------------------------------------------
// <copyright file="LoopModelFactoryTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Agentic.Workflow.Generators.Models;

namespace Agentic.Workflow.Generators.Tests.Models;

/// <summary>
/// Unit tests for <see cref="LoopModel.Create"/> factory method validation.
/// </summary>
public sealed class LoopModelFactoryTests
{
    [Test]
    public async Task Create_WithValidParameters_ReturnsModel()
    {
        // Arrange
        var loopName = "Refinement";
        var conditionId = "ProcessClaim-Refinement";
        var maxIterations = 10;
        var firstBodyStepName = "Refinement_Analyze";
        var lastBodyStepName = "Refinement_Validate";

        // Act
        var model = LoopModel.Create(
            loopName: loopName,
            conditionId: conditionId,
            maxIterations: maxIterations,
            firstBodyStepName: firstBodyStepName,
            lastBodyStepName: lastBodyStepName);

        // Assert
        await Assert.That(model.LoopName).IsEqualTo(loopName);
        await Assert.That(model.ConditionId).IsEqualTo(conditionId);
        await Assert.That(model.MaxIterations).IsEqualTo(maxIterations);
        await Assert.That(model.FirstBodyStepName).IsEqualTo(firstBodyStepName);
        await Assert.That(model.LastBodyStepName).IsEqualTo(lastBodyStepName);
        await Assert.That(model.ContinuationStepName).IsNull();
        await Assert.That(model.ParentLoopName).IsNull();
    }

    [Test]
    public async Task Create_WithNestedLoopParameters_ReturnsModel()
    {
        // Arrange
        var loopName = "Inner";
        var conditionId = "ProcessClaim-Inner";
        var maxIterations = 5;
        var firstBodyStepName = "Outer_Inner_Process";
        var lastBodyStepName = "Outer_Inner_Validate";
        var continuationStepName = "Outer_Continue";
        var parentLoopName = "Outer";

        // Act
        var model = LoopModel.Create(
            loopName: loopName,
            conditionId: conditionId,
            maxIterations: maxIterations,
            firstBodyStepName: firstBodyStepName,
            lastBodyStepName: lastBodyStepName,
            continuationStepName: continuationStepName,
            parentLoopName: parentLoopName);

        // Assert
        await Assert.That(model.ContinuationStepName).IsEqualTo(continuationStepName);
        await Assert.That(model.ParentLoopName).IsEqualTo(parentLoopName);
        await Assert.That(model.FullPrefix).IsEqualTo("Outer_Inner");
    }

    [Test]
    public async Task Create_WithNullLoopName_ThrowsArgumentNullException()
    {
        // Arrange
        string? loopName = null;
        var conditionId = "ProcessClaim-Refinement";
        var firstBodyStepName = "Refinement_Analyze";
        var lastBodyStepName = "Refinement_Validate";

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            LoopModel.Create(
                loopName: loopName!,
                conditionId: conditionId,
                maxIterations: 10,
                firstBodyStepName: firstBodyStepName,
                lastBodyStepName: lastBodyStepName);
        });

        // Assert
        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task Create_WithInvalidLoopName_ThrowsArgumentException()
    {
        // Arrange
        var loopName = "Invalid-Loop"; // Hyphen is not valid
        var conditionId = "ProcessClaim-Refinement";
        var firstBodyStepName = "Refinement_Analyze";
        var lastBodyStepName = "Refinement_Validate";

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            LoopModel.Create(
                loopName: loopName,
                conditionId: conditionId,
                maxIterations: 10,
                firstBodyStepName: firstBodyStepName,
                lastBodyStepName: lastBodyStepName);
        });

        // Assert
        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task Create_WithEmptyConditionId_ThrowsArgumentException()
    {
        // Arrange
        var loopName = "Refinement";
        var conditionId = "";
        var firstBodyStepName = "Refinement_Analyze";
        var lastBodyStepName = "Refinement_Validate";

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            LoopModel.Create(
                loopName: loopName,
                conditionId: conditionId,
                maxIterations: 10,
                firstBodyStepName: firstBodyStepName,
                lastBodyStepName: lastBodyStepName);
        });

        // Assert
        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task Create_WithMaxIterationsZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var loopName = "Refinement";
        var conditionId = "ProcessClaim-Refinement";
        var firstBodyStepName = "Refinement_Analyze";
        var lastBodyStepName = "Refinement_Validate";

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            LoopModel.Create(
                loopName: loopName,
                conditionId: conditionId,
                maxIterations: 0,
                firstBodyStepName: firstBodyStepName,
                lastBodyStepName: lastBodyStepName);
        });

        // Assert
        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task Create_WithEmptyFirstBodyStepName_ThrowsArgumentException()
    {
        // Arrange
        var loopName = "Refinement";
        var conditionId = "ProcessClaim-Refinement";
        var firstBodyStepName = "";
        var lastBodyStepName = "Refinement_Validate";

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            LoopModel.Create(
                loopName: loopName,
                conditionId: conditionId,
                maxIterations: 10,
                firstBodyStepName: firstBodyStepName,
                lastBodyStepName: lastBodyStepName);
        });

        // Assert
        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task Create_WithInvalidParentLoopName_ThrowsArgumentException()
    {
        // Arrange
        var loopName = "Inner";
        var conditionId = "ProcessClaim-Inner";
        var firstBodyStepName = "Outer_Inner_Process";
        var lastBodyStepName = "Outer_Inner_Validate";
        var parentLoopName = "Invalid-Parent"; // Hyphen is not valid

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            LoopModel.Create(
                loopName: loopName,
                conditionId: conditionId,
                maxIterations: 5,
                firstBodyStepName: firstBodyStepName,
                lastBodyStepName: lastBodyStepName,
                parentLoopName: parentLoopName);
        });

        // Assert
        await Assert.That(exception).IsNotNull();
    }
}
