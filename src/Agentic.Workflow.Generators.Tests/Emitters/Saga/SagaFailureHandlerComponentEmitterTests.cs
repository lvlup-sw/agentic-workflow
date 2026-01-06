// -----------------------------------------------------------------------
// <copyright file="SagaFailureHandlerComponentEmitterTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Agentic.Workflow.Generators.Tests.Emitters.Saga;

using System.Text;
using Agentic.Workflow.Generators.Emitters.Saga;
using Agentic.Workflow.Generators.Models;
using TUnit.Core;

/// <summary>
/// Unit tests for <see cref="SagaFailureHandlerComponentEmitter"/>.
/// </summary>
[Property("Category", "Unit")]
public class SagaFailureHandlerComponentEmitterTests
{
    // ====================================================================
    // Section A: Guard Clause Tests
    // ====================================================================

    /// <summary>
    /// Verifies that Emit throws ArgumentNullException when StringBuilder is null.
    /// </summary>
    [Test]
    public async Task Emit_NullStringBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var model = CreateModelWithFailureHandler();

        // Act & Assert
        await Assert.That(() => emitter.Emit(null!, model))
            .Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that Emit throws ArgumentNullException when model is null.
    /// </summary>
    [Test]
    public async Task Emit_NullModel_ThrowsArgumentNullException()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var sb = new StringBuilder();

        // Act & Assert
        await Assert.That(() => emitter.Emit(sb, null!))
            .Throws<ArgumentNullException>();
    }

    // ====================================================================
    // Section B: Interface Implementation Tests
    // ====================================================================

    /// <summary>
    /// Verifies that the class implements ISagaComponentEmitter.
    /// </summary>
    [Test]
    public async Task Class_ImplementsISagaComponentEmitter()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();

        // Assert
        await Assert.That(emitter is ISagaComponentEmitter).IsTrue();
    }

    // ====================================================================
    // Section C: No Failure Handlers Tests
    // ====================================================================

    /// <summary>
    /// Verifies that Emit produces no output when workflow has no failure handlers.
    /// </summary>
    [Test]
    public async Task Emit_NoFailureHandlers_ProducesNoOutput()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel(); // No failure handlers

        // Act
        emitter.Emit(sb, model);

        // Assert
        var output = sb.ToString();
        await Assert.That(output).IsEmpty();
    }

    // ====================================================================
    // Section D: Terminal Failure Handler Tests
    // ====================================================================

    /// <summary>
    /// Verifies that Emit generates trigger, start, and completed handlers for terminal failure handler.
    /// </summary>
    [Test]
    public async Task Emit_TerminalFailureHandler_GeneratesTriggerStartCompletedHandlers()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var sb = new StringBuilder();
        var handler = FailureHandlerModel.Create(
            handlerId: "workflow-failure",
            scope: FailureHandlerScope.Workflow,
            stepNames: ["LogFailure", "NotifyAdmin"],
            isTerminal: true);
        var model = CreateMinimalModel(failureHandlers: [handler]);

        // Act
        emitter.Emit(sb, model);

        // Assert
        var output = sb.ToString();

        // Trigger handler
        await Assert.That(output).Contains("TriggerTestWorkflowFailureHandlerCommand");

        // Start handler for first step
        await Assert.That(output).Contains("StartFailureHandler_workflow_failure_LogFailureCommand");
        await Assert.That(output).Contains("ExecuteFailureHandler_workflow_failure_LogFailureWorkerCommand");

        // Completed handler for first step chains to second
        await Assert.That(output).Contains("FailureHandler_workflow_failure_LogFailureCompleted");

        // Start handler for second step
        await Assert.That(output).Contains("StartFailureHandler_workflow_failure_NotifyAdminCommand");

        // Final completed handler
        await Assert.That(output).Contains("FailureHandler_workflow_failure_NotifyAdminCompleted");
    }

    /// <summary>
    /// Verifies that terminal failure handler marks workflow as Failed and completed.
    /// </summary>
    [Test]
    public async Task Emit_TerminalFailureHandler_MarksFailedAndCompletes()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var sb = new StringBuilder();
        var handler = FailureHandlerModel.Create(
            handlerId: "failure-handler",
            scope: FailureHandlerScope.Workflow,
            stepNames: ["CleanupStep"],
            isTerminal: true);
        var model = CreateMinimalModel(failureHandlers: [handler]);

        // Act
        emitter.Emit(sb, model);

        // Assert
        var output = sb.ToString();
        await Assert.That(output).Contains("Phase = TestWorkflowPhase.Failed;");
        await Assert.That(output).Contains("MarkCompleted();");
    }

    // ====================================================================
    // Section E: Multi-Step Failure Handler Tests
    // ====================================================================

    /// <summary>
    /// Verifies that multi-step failure handler chains steps correctly.
    /// </summary>
    [Test]
    public async Task Emit_MultiStepFailureHandler_ChainsStepsCorrectly()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var sb = new StringBuilder();
        var handler = FailureHandlerModel.Create(
            handlerId: "multi-step",
            scope: FailureHandlerScope.Workflow,
            stepNames: ["Step1", "Step2", "Step3"],
            isTerminal: true);
        var model = CreateMinimalModel(failureHandlers: [handler]);

        // Act
        emitter.Emit(sb, model);

        // Assert
        var output = sb.ToString();

        // Step1 completed should return start command for Step2
        await Assert.That(output).Contains("return new StartFailureHandler_multi_step_Step2Command");

        // Step2 completed should return start command for Step3
        await Assert.That(output).Contains("return new StartFailureHandler_multi_step_Step3Command");

        // Step3 is last - no chaining, just mark completed
        await Assert.That(output).Contains("FailureHandler_multi_step_Step3Completed");
    }

    /// <summary>
    /// Verifies that each step in failure handler gets phase transition.
    /// </summary>
    [Test]
    public async Task Emit_MultiStepFailureHandler_EmitsPhaseTransitionsForEachStep()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var sb = new StringBuilder();
        var handler = FailureHandlerModel.Create(
            handlerId: "multi-phase",
            scope: FailureHandlerScope.Workflow,
            stepNames: ["Alpha", "Beta"],
            isTerminal: true);
        var model = CreateMinimalModel(failureHandlers: [handler]);

        // Act
        emitter.Emit(sb, model);

        // Assert
        var output = sb.ToString();
        await Assert.That(output).Contains("Phase = TestWorkflowPhase.FailureHandler_multi_phase_Alpha;");
        await Assert.That(output).Contains("Phase = TestWorkflowPhase.FailureHandler_multi_phase_Beta;");
    }

    // ====================================================================
    // Section F: Non-Terminal Failure Handler Tests
    // ====================================================================

    /// <summary>
    /// Verifies that non-terminal failure handler does not call MarkCompleted.
    /// </summary>
    [Test]
    public async Task Emit_NonTerminalFailureHandler_DoesNotCallMarkCompleted()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var sb = new StringBuilder();
        var handler = FailureHandlerModel.Create(
            handlerId: "non-terminal",
            scope: FailureHandlerScope.Workflow,
            stepNames: ["RecoverStep"],
            isTerminal: false);
        var model = CreateMinimalModel(failureHandlers: [handler]);

        // Act
        emitter.Emit(sb, model);

        // Assert
        var output = sb.ToString();

        // Should NOT contain MarkCompleted for non-terminal handler
        // The last step handler should just update state without marking completed
        var lines = output.Split('\n');
        var recoverCompletedHandlerStart = -1;
        var recoverCompletedHandlerEnd = -1;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("FailureHandler_non_terminal_RecoverStepCompleted evt)"))
            {
                recoverCompletedHandlerStart = i;
            }

            if (recoverCompletedHandlerStart > 0 && lines[i].Trim() == "}")
            {
                recoverCompletedHandlerEnd = i;
                break;
            }
        }

        // Extract just the handler method
        if (recoverCompletedHandlerStart > 0 && recoverCompletedHandlerEnd > recoverCompletedHandlerStart)
        {
            var handlerCode = string.Join("\n", lines[recoverCompletedHandlerStart..recoverCompletedHandlerEnd]);
            await Assert.That(handlerCode).DoesNotContain("MarkCompleted");
        }
    }

    /// <summary>
    /// Verifies that non-terminal failure handler still updates state.
    /// </summary>
    [Test]
    public async Task Emit_NonTerminalFailureHandler_UpdatesState()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var sb = new StringBuilder();
        var handler = FailureHandlerModel.Create(
            handlerId: "recovery",
            scope: FailureHandlerScope.Workflow,
            stepNames: ["HandleError"],
            isTerminal: false);
        var model = CreateMinimalModel(failureHandlers: [handler]);

        // Act
        emitter.Emit(sb, model);

        // Assert
        var output = sb.ToString();
        await Assert.That(output).Contains("State = TestStateReducer.Reduce(State, evt.UpdatedState);");
    }

    // ====================================================================
    // Section G: Exception Context Tests
    // ====================================================================

    /// <summary>
    /// Verifies that trigger handler stores failed step name and exception message.
    /// </summary>
    [Test]
    public async Task Emit_WithExceptionContext_StoresFailedStepAndMessage()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var sb = new StringBuilder();
        var handler = FailureHandlerModel.Create(
            handlerId: "ctx-handler",
            scope: FailureHandlerScope.Workflow,
            stepNames: ["LogError"],
            isTerminal: true);
        var model = CreateMinimalModel(failureHandlers: [handler]);

        // Act
        emitter.Emit(sb, model);

        // Assert
        var output = sb.ToString();

        // Trigger handler should store exception context
        await Assert.That(output).Contains("FailedStepName = cmd.FailedStepName;");
        await Assert.That(output).Contains("FailureExceptionMessage = cmd.ExceptionMessage;");
    }

    /// <summary>
    /// Verifies that worker command includes exception context.
    /// </summary>
    [Test]
    public async Task Emit_WorkerCommand_IncludesExceptionContext()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var sb = new StringBuilder();
        var handler = FailureHandlerModel.Create(
            handlerId: "worker-ctx",
            scope: FailureHandlerScope.Workflow,
            stepNames: ["ProcessError"],
            isTerminal: true);
        var model = CreateMinimalModel(failureHandlers: [handler]);

        // Act
        emitter.Emit(sb, model);

        // Assert
        var output = sb.ToString();

        // Worker command should receive failure context
        await Assert.That(output).Contains("FailedStepName!,");
        await Assert.That(output).Contains("FailureExceptionMessage);");
    }

    // ====================================================================
    // Section H: Handler ID Sanitization Tests
    // ====================================================================

    /// <summary>
    /// Verifies that handler ID with dashes is sanitized for identifiers.
    /// </summary>
    [Test]
    public async Task Emit_HandlerIdWithDashes_SanitizesForIdentifiers()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var sb = new StringBuilder();
        var handler = FailureHandlerModel.Create(
            handlerId: "my-error-handler",
            scope: FailureHandlerScope.Workflow,
            stepNames: ["Handle"],
            isTerminal: true);
        var model = CreateMinimalModel(failureHandlers: [handler]);

        // Act
        emitter.Emit(sb, model);

        // Assert
        var output = sb.ToString();

        // Dashes should be replaced with underscores
        await Assert.That(output).Contains("FailureHandler_my_error_handler_Handle");
        await Assert.That(output).DoesNotContain("my-error-handler");
    }

    // ====================================================================
    // Section I: XML Documentation Tests
    // ====================================================================

    /// <summary>
    /// Verifies that emitted handlers include XML documentation.
    /// </summary>
    [Test]
    public async Task Emit_AllHandlers_IncludeXmlDocumentation()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var sb = new StringBuilder();
        var handler = FailureHandlerModel.Create(
            handlerId: "documented",
            scope: FailureHandlerScope.Workflow,
            stepNames: ["DocStep"],
            isTerminal: true);
        var model = CreateMinimalModel(failureHandlers: [handler]);

        // Act
        emitter.Emit(sb, model);

        // Assert
        var output = sb.ToString();
        await Assert.That(output).Contains("/// <summary>");
        await Assert.That(output).Contains("/// <param name=\"cmd\">");
        await Assert.That(output).Contains("/// <returns>");
    }

    // ====================================================================
    // Section J: Reducer Integration Tests
    // ====================================================================

    /// <summary>
    /// Verifies that completed handlers use reducer for state updates.
    /// </summary>
    [Test]
    public async Task Emit_CompletedHandlers_UseReducerForStateUpdate()
    {
        // Arrange
        var emitter = new SagaFailureHandlerComponentEmitter();
        var sb = new StringBuilder();
        var handler = FailureHandlerModel.Create(
            handlerId: "reducer-test",
            scope: FailureHandlerScope.Workflow,
            stepNames: ["ReduceStep"],
            isTerminal: true);
        var model = CreateMinimalModel(failureHandlers: [handler]);

        // Act
        emitter.Emit(sb, model);

        // Assert
        var output = sb.ToString();
        await Assert.That(output).Contains("TestStateReducer.Reduce(State, evt.UpdatedState)");
    }

    // ====================================================================
    // Helper Methods
    // ====================================================================

    private static WorkflowModel CreateMinimalModel(
        IReadOnlyList<string>? stepNames = null,
        IReadOnlyList<FailureHandlerModel>? failureHandlers = null)
    {
        return new WorkflowModel(
            WorkflowName: "test-workflow",
            PascalName: "TestWorkflow",
            Namespace: "TestNamespace",
            StepNames: stepNames ?? ["Step1", "Step2"],
            StateTypeName: "TestState",
            Version: 1,
            FailureHandlers: failureHandlers);
    }

    private static WorkflowModel CreateModelWithFailureHandler()
    {
        var handler = FailureHandlerModel.Create(
            handlerId: "default-handler",
            scope: FailureHandlerScope.Workflow,
            stepNames: ["HandleFailure"],
            isTerminal: true);
        return CreateMinimalModel(failureHandlers: [handler]);
    }
}
