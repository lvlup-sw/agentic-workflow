// -----------------------------------------------------------------------
// <copyright file="SagaNotFoundHandlersEmitterTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------


using System.Text;

using Agentic.Workflow.Generators.Emitters.Saga;
using Agentic.Workflow.Generators.Models;

namespace Agentic.Workflow.Generators.Tests.Emitters.Saga;
/// <summary>
/// Unit tests for the <see cref="SagaNotFoundHandlersEmitter"/> class.
/// </summary>
[Property("Category", "Unit")]
public class SagaNotFoundHandlersEmitterTests
{
    // =============================================================================
    // A. Guard Tests
    // =============================================================================

    /// <summary>
    /// Verifies that Emit throws for null StringBuilder.
    /// </summary>
    [Test]
    public async Task Emit_NullStringBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var emitter = new SagaNotFoundHandlersEmitter();
        var model = CreateMinimalModel();

        // Act & Assert
        await Assert.That(() => emitter.Emit(null!, model))
            .Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that Emit throws for null model.
    /// </summary>
    [Test]
    public async Task Emit_NullModel_ThrowsArgumentNullException()
    {
        // Arrange
        var emitter = new SagaNotFoundHandlersEmitter();
        var sb = new StringBuilder();

        // Act & Assert
        await Assert.That(() => emitter.Emit(sb, null!))
            .Throws<ArgumentNullException>();
    }

    // =============================================================================
    // B. Start Command NotFound Handler Tests
    // =============================================================================

    /// <summary>
    /// Verifies that Emit generates NotFound handler for start command.
    /// </summary>
    [Test]
    public async Task Emit_ValidModel_GeneratesStartCommandNotFoundHandler()
    {
        // Arrange
        var emitter = new SagaNotFoundHandlersEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();

        // Act
        emitter.Emit(sb, model);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("public static void NotFound(StartTestWorkflowCommand command");
    }

    /// <summary>
    /// Verifies that NotFound handler takes logger parameter.
    /// </summary>
    [Test]
    public async Task Emit_ValidModel_NotFoundHandlerHasLoggerParameter()
    {
        // Arrange
        var emitter = new SagaNotFoundHandlersEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();

        // Act
        emitter.Emit(sb, model);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("ILogger<TestWorkflowSaga> logger");
    }

    /// <summary>
    /// Verifies that NotFound handler has guard clauses.
    /// </summary>
    [Test]
    public async Task Emit_ValidModel_NotFoundHandlerHasGuardClauses()
    {
        // Arrange
        var emitter = new SagaNotFoundHandlersEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();

        // Act
        emitter.Emit(sb, model);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("ArgumentNullException.ThrowIfNull(command, nameof(command))");
        await Assert.That(result).Contains("ArgumentNullException.ThrowIfNull(logger, nameof(logger))");
    }

    /// <summary>
    /// Verifies that NotFound handler logs warning.
    /// </summary>
    [Test]
    public async Task Emit_ValidModel_NotFoundHandlerLogsWarning()
    {
        // Arrange
        var emitter = new SagaNotFoundHandlersEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();

        // Act
        emitter.Emit(sb, model);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("logger.LogWarning(");
    }

    // =============================================================================
    // C. Step Completed NotFound Handler Tests
    // =============================================================================

    /// <summary>
    /// Verifies that Emit generates NotFound handler for each step.
    /// </summary>
    [Test]
    public async Task Emit_ValidModel_GeneratesNotFoundForEachStep()
    {
        // Arrange
        var emitter = new SagaNotFoundHandlersEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();

        // Act
        emitter.Emit(sb, model);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("public static void NotFound(ValidateStepCompleted evt");
        await Assert.That(result).Contains("public static void NotFound(ProcessStepCompleted evt");
    }

    /// <summary>
    /// Verifies that step NotFound handler has event guard clause.
    /// </summary>
    [Test]
    public async Task Emit_ValidModel_StepNotFoundHandlerHasEventGuardClause()
    {
        // Arrange
        var emitter = new SagaNotFoundHandlersEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();

        // Act
        emitter.Emit(sb, model);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("ArgumentNullException.ThrowIfNull(evt, nameof(evt))");
    }

    /// <summary>
    /// Verifies that step NotFound handler logs workflow id.
    /// </summary>
    [Test]
    public async Task Emit_ValidModel_StepNotFoundHandlerLogsWorkflowId()
    {
        // Arrange
        var emitter = new SagaNotFoundHandlersEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();

        // Act
        emitter.Emit(sb, model);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("evt.WorkflowId");
    }

    // =============================================================================
    // D. XML Documentation Tests
    // =============================================================================

    /// <summary>
    /// Verifies that Emit generates XML documentation for NotFound handlers.
    /// </summary>
    [Test]
    public async Task Emit_ValidModel_GeneratesXmlDocumentation()
    {
        // Arrange
        var emitter = new SagaNotFoundHandlersEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();

        // Act
        emitter.Emit(sb, model);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("/// <summary>");
        await Assert.That(result).Contains("/// </summary>");
    }

    /// <summary>
    /// Verifies that start command handler has meaningful documentation.
    /// </summary>
    [Test]
    public async Task Emit_ValidModel_StartHandlerHasMeaningfulDoc()
    {
        // Arrange
        var emitter = new SagaNotFoundHandlersEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();

        // Act
        emitter.Emit(sb, model);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("saga no longer exists");
    }

    // =============================================================================
    // E. Interface Implementation Tests
    // =============================================================================

    /// <summary>
    /// Verifies that SagaNotFoundHandlersEmitter implements ISagaComponentEmitter.
    /// </summary>
    [Test]
    public async Task Class_ImplementsISagaComponentEmitter()
    {
        // Arrange
        var emitter = new SagaNotFoundHandlersEmitter();

        // Assert
        ISagaComponentEmitter componentEmitter = emitter;
        await Assert.That(componentEmitter).IsNotNull();
    }

    // =============================================================================
    // F. Multiple Steps Tests
    // =============================================================================

    /// <summary>
    /// Verifies that Emit generates correct number of NotFound handlers for all steps.
    /// </summary>
    [Test]
    public async Task Emit_MultipleSteps_GeneratesHandlerForEachStep()
    {
        // Arrange
        var emitter = new SagaNotFoundHandlersEmitter();
        var sb = new StringBuilder();
        var model = new WorkflowModel(
            WorkflowName: "order-workflow",
            PascalName: "OrderWorkflow",
            Namespace: "Test",
            StepNames: ["Validate", "Process", "Ship", "Complete"],
            StateTypeName: null,
            Loops: null);

        // Act
        emitter.Emit(sb, model);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("NotFound(ValidateCompleted evt");
        await Assert.That(result).Contains("NotFound(ProcessCompleted evt");
        await Assert.That(result).Contains("NotFound(ShipCompleted evt");
        await Assert.That(result).Contains("NotFound(CompleteCompleted evt");
    }

    // =============================================================================
    // Helper Methods
    // =============================================================================

    private static WorkflowModel CreateMinimalModel()
    {
        return new WorkflowModel(
            WorkflowName: "test-workflow",
            PascalName: "TestWorkflow",
            Namespace: "TestNamespace",
            StepNames: ["ValidateStep", "ProcessStep"],
            StateTypeName: "TestState",
            Loops: null);
    }
}