// -----------------------------------------------------------------------
// <copyright file="WorkflowModelFactoryTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Agentic.Workflow.Generators.Tests.Models;

using Agentic.Workflow.Generators.Models;

/// <summary>
/// Unit tests for <see cref="WorkflowModel.Create"/> factory method validation.
/// </summary>
public sealed class WorkflowModelFactoryTests
{
    [Test]
    public async Task Create_WithValidParameters_ReturnsModel()
    {
        // Arrange
        var workflowName = "process-order";
        var pascalName = "ProcessOrder";
        var @namespace = "MyCompany.Workflows";
        var stepNames = new[] { "ValidateOrder", "ProcessPayment", "ShipOrder" };

        // Act
        var model = WorkflowModel.Create(
            workflowName: workflowName,
            pascalName: pascalName,
            @namespace: @namespace,
            stepNames: stepNames);

        // Assert
        await Assert.That(model.WorkflowName).IsEqualTo(workflowName);
        await Assert.That(model.PascalName).IsEqualTo(pascalName);
        await Assert.That(model.Namespace).IsEqualTo(@namespace);
        await Assert.That(model.StepNames).IsEquivalentTo(stepNames);
        await Assert.That(model.Version).IsEqualTo(1);
        await Assert.That(model.StateTypeName).IsNull();
    }

    [Test]
    public async Task Create_WithAllOptionalParameters_ReturnsModel()
    {
        // Arrange
        var workflowName = "process-order";
        var pascalName = "ProcessOrder";
        var @namespace = "MyCompany.Workflows";
        var stepNames = new[] { "ValidateOrder" };
        var stateTypeName = "OrderState";
        var version = 2;
        var steps = new List<StepModel>
        {
            new("ValidateOrder", "MyCompany.Steps.ValidateOrderStep")
        };
        var loops = new List<LoopModel>();
        var branches = new List<BranchModel>();

        // Act
        var model = WorkflowModel.Create(
            workflowName: workflowName,
            pascalName: pascalName,
            @namespace: @namespace,
            stepNames: stepNames,
            stateTypeName: stateTypeName,
            version: version,
            steps: steps,
            loops: loops,
            branches: branches);

        // Assert
        await Assert.That(model.StateTypeName).IsEqualTo(stateTypeName);
        await Assert.That(model.Version).IsEqualTo(version);
        await Assert.That(model.Steps).IsNotNull();
        await Assert.That(model.Loops).IsNotNull();
        await Assert.That(model.Branches).IsNotNull();
    }

    [Test]
    public async Task Create_WithNullPascalName_ThrowsArgumentNullException()
    {
        // Arrange
        var workflowName = "process-order";
        string? pascalName = null;
        var @namespace = "MyCompany.Workflows";
        var stepNames = new[] { "ValidateOrder" };

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            WorkflowModel.Create(
                workflowName: workflowName,
                pascalName: pascalName!,
                @namespace: @namespace,
                stepNames: stepNames);
        });

        // Assert
        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task Create_WithInvalidPascalName_ThrowsArgumentException()
    {
        // Arrange
        var workflowName = "process-order";
        var pascalName = "123Invalid"; // Starts with number - invalid
        var @namespace = "MyCompany.Workflows";
        var stepNames = new[] { "ValidateOrder" };

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            WorkflowModel.Create(
                workflowName: workflowName,
                pascalName: pascalName,
                @namespace: @namespace,
                stepNames: stepNames);
        });

        // Assert
        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task Create_WithEmptyNamespace_ThrowsArgumentException()
    {
        // Arrange
        var workflowName = "process-order";
        var pascalName = "ProcessOrder";
        var @namespace = "";
        var stepNames = new[] { "ValidateOrder" };

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            WorkflowModel.Create(
                workflowName: workflowName,
                pascalName: pascalName,
                @namespace: @namespace,
                stepNames: stepNames);
        });

        // Assert
        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task Create_WithEmptyStepNames_ThrowsArgumentException()
    {
        // Arrange
        var workflowName = "process-order";
        var pascalName = "ProcessOrder";
        var @namespace = "MyCompany.Workflows";
        var stepNames = Array.Empty<string>();

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            WorkflowModel.Create(
                workflowName: workflowName,
                pascalName: pascalName,
                @namespace: @namespace,
                stepNames: stepNames);
        });

        // Assert
        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task Create_WithDuplicateStepNames_ThrowsArgumentException()
    {
        // Arrange
        var workflowName = "process-order";
        var pascalName = "ProcessOrder";
        var @namespace = "MyCompany.Workflows";
        var stepNames = new[] { "ValidateOrder", "ProcessPayment", "ValidateOrder" }; // Duplicate

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            WorkflowModel.Create(
                workflowName: workflowName,
                pascalName: pascalName,
                @namespace: @namespace,
                stepNames: stepNames);
        });

        // Assert
        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task Create_WithInvalidStepName_ThrowsArgumentException()
    {
        // Arrange
        var workflowName = "process-order";
        var pascalName = "ProcessOrder";
        var @namespace = "MyCompany.Workflows";
        var stepNames = new[] { "ValidateOrder", "Invalid-Step" }; // Hyphen is invalid

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            WorkflowModel.Create(
                workflowName: workflowName,
                pascalName: pascalName,
                @namespace: @namespace,
                stepNames: stepNames);
        });

        // Assert
        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task Create_WithVersionZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var workflowName = "process-order";
        var pascalName = "ProcessOrder";
        var @namespace = "MyCompany.Workflows";
        var stepNames = new[] { "ValidateOrder" };

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            WorkflowModel.Create(
                workflowName: workflowName,
                pascalName: pascalName,
                @namespace: @namespace,
                stepNames: stepNames,
                version: 0);
        });

        // Assert
        await Assert.That(exception).IsNotNull();
    }
}
