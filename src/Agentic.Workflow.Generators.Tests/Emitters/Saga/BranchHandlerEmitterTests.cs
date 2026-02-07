// -----------------------------------------------------------------------
// <copyright file="BranchHandlerEmitterTests.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Text;

using Agentic.Workflow.Generators.Emitters.Saga;
using Agentic.Workflow.Generators.Models;

namespace Agentic.Workflow.Generators.Tests.Emitters.Saga;

/// <summary>
/// Unit tests for the <see cref="BranchHandlerEmitter"/> class.
/// </summary>
[Property("Category", "Unit")]
public class BranchHandlerEmitterTests
{
    // =============================================================================
    // A. Guard Tests - EmitRoutingHandler
    // =============================================================================

    /// <summary>
    /// Verifies that EmitRoutingHandler throws for null StringBuilder.
    /// </summary>
    [Test]
    public async Task EmitRoutingHandler_NullStringBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var model = CreateMinimalModel();
        var branch = CreateBranch();

        // Act & Assert
        await Assert.That(() => emitter.EmitRoutingHandler(null!, model, "ValidateStep", branch))
            .Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that EmitRoutingHandler throws for null model.
    /// </summary>
    [Test]
    public async Task EmitRoutingHandler_NullModel_ThrowsArgumentNullException()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var branch = CreateBranch();

        // Act & Assert
        await Assert.That(() => emitter.EmitRoutingHandler(sb, null!, "ValidateStep", branch))
            .Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that EmitRoutingHandler throws for null stepName.
    /// </summary>
    [Test]
    public async Task EmitRoutingHandler_NullStepName_ThrowsArgumentNullException()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();
        var branch = CreateBranch();

        // Act & Assert
        await Assert.That(() => emitter.EmitRoutingHandler(sb, model, null!, branch))
            .Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that EmitRoutingHandler throws for null branch.
    /// </summary>
    [Test]
    public async Task EmitRoutingHandler_NullBranch_ThrowsArgumentNullException()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();

        // Act & Assert
        await Assert.That(() => emitter.EmitRoutingHandler(sb, model, "ValidateStep", null!))
            .Throws<ArgumentNullException>();
    }

    // =============================================================================
    // B. Routing Handler Tests
    // =============================================================================

    /// <summary>
    /// Verifies that EmitRoutingHandler generates object return type.
    /// </summary>
    [Test]
    public async Task EmitRoutingHandler_ValidInput_GeneratesObjectReturnType()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();
        var branch = CreateBranch();

        // Act
        emitter.EmitRoutingHandler(sb, model, "ValidateStep", branch);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("public object Handle(");
    }

    /// <summary>
    /// Verifies that EmitRoutingHandler generates switch expression.
    /// </summary>
    [Test]
    public async Task EmitRoutingHandler_ValidInput_GeneratesSwitchExpression()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();
        var branch = CreateBranch();

        // Act
        emitter.EmitRoutingHandler(sb, model, "ValidateStep", branch);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("return State.Status switch");
    }

    /// <summary>
    /// Verifies that EmitRoutingHandler generates cases.
    /// </summary>
    [Test]
    public async Task EmitRoutingHandler_ValidInput_GeneratesCases()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();
        var branch = CreateBranch();

        // Act
        emitter.EmitRoutingHandler(sb, model, "ValidateStep", branch);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("OrderStatus.Approved => new StartApproved_ProcessCommand(WorkflowId)");
        await Assert.That(result).Contains("OrderStatus.Rejected => new StartRejected_HandleCommand(WorkflowId)");
    }

    /// <summary>
    /// Verifies that EmitRoutingHandler generates default throw when no otherwise case.
    /// </summary>
    [Test]
    public async Task EmitRoutingHandler_NoOtherwiseCase_GeneratesDefaultThrow()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();
        var branch = CreateBranch();

        // Act
        emitter.EmitRoutingHandler(sb, model, "ValidateStep", branch);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("_ => throw new InvalidOperationException");
    }

    /// <summary>
    /// Verifies that EmitRoutingHandler uses otherwise case when present.
    /// </summary>
    [Test]
    public async Task EmitRoutingHandler_WithOtherwiseCase_GeneratesOtherwiseBranch()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();
        var branch = CreateBranchWithOtherwise();

        // Act
        emitter.EmitRoutingHandler(sb, model, "ValidateStep", branch);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("_ => new StartDefault_HandleCommand(WorkflowId)");
        await Assert.That(result).DoesNotContain("throw new InvalidOperationException");
    }

    /// <summary>
    /// Verifies that EmitRoutingHandler applies reducer.
    /// </summary>
    [Test]
    public async Task EmitRoutingHandler_WithStateType_AppliesReducer()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();
        var branch = CreateBranch();

        // Act
        emitter.EmitRoutingHandler(sb, model, "ValidateStep", branch);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("State = TestStateReducer.Reduce(State, evt.UpdatedState)");
    }

    // =============================================================================
    // C. Guard Tests - EmitPathEndHandler
    // =============================================================================

    /// <summary>
    /// Verifies that EmitPathEndHandler throws for null StringBuilder.
    /// </summary>
    [Test]
    public async Task EmitPathEndHandler_NullStringBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var model = CreateMinimalModel();
        var branch = CreateBranch();
        var branchCase = CreateBranchCase();

        // Act & Assert
        await Assert.That(() => emitter.EmitPathEndHandler(null!, model, "Approved_Complete", branch, branchCase))
            .Throws<ArgumentNullException>();
    }

    // =============================================================================
    // D. Path End Handler with Rejoin Tests
    // =============================================================================

    /// <summary>
    /// Verifies that EmitPathEndHandler generates rejoin command when branch has rejoin point.
    /// </summary>
    [Test]
    public async Task EmitPathEndHandler_WithRejoinPoint_GeneratesRejoinCommand()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();
        var branch = CreateBranchWithRejoin();
        var branchCase = CreateBranchCase();

        // Act
        emitter.EmitPathEndHandler(sb, model, "Approved_Complete", branch, branchCase);
        var result = sb.ToString();

        // Assert - Handler now uses method injection for ILogger (multiline signature)
        await Assert.That(result).Contains("public StartFinalizeStepCommand Handle(");
        await Assert.That(result).Contains("ILogger<TestWorkflowSaga> logger)");
        await Assert.That(result).Contains("return new StartFinalizeStepCommand(WorkflowId)");
    }

    // =============================================================================
    // E. Path End Handler without Rejoin Tests
    // =============================================================================

    /// <summary>
    /// Verifies that EmitPathEndHandler generates void handler when no rejoin.
    /// </summary>
    [Test]
    public async Task EmitPathEndHandler_NoRejoinPoint_GeneratesVoidHandler()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();
        var branch = CreateBranch(); // No rejoin
        var branchCase = CreateBranchCase();

        // Act
        emitter.EmitPathEndHandler(sb, model, "Approved_Complete", branch, branchCase);
        var result = sb.ToString();

        // Assert - Handler now uses method injection for ILogger (multiline signature)
        await Assert.That(result).Contains("public void Handle(");
        await Assert.That(result).Contains("ILogger<TestWorkflowSaga> logger)");
    }

    /// <summary>
    /// Verifies that EmitPathEndHandler marks completed when no rejoin.
    /// </summary>
    [Test]
    public async Task EmitPathEndHandler_NoRejoinPoint_MarksCompleted()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();
        var branch = CreateBranch(); // No rejoin
        var branchCase = CreateBranchCase();

        // Act
        emitter.EmitPathEndHandler(sb, model, "Approved_Complete", branch, branchCase);
        var result = sb.ToString();

        // Assert
        await Assert.That(result).Contains("Phase = TestWorkflowPhase.Completed");
        await Assert.That(result).Contains("MarkCompleted()");
    }

    // =============================================================================
    // F. XML Documentation Tests
    // =============================================================================

    /// <summary>
    /// Verifies that EmitRoutingHandler generates XML documentation including logger param.
    /// </summary>
    [Test]
    public async Task EmitRoutingHandler_ValidInput_GeneratesXmlDocumentation()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();
        var branch = CreateBranch();

        // Act
        emitter.EmitRoutingHandler(sb, model, "ValidateStep", branch);
        var result = sb.ToString();

        // Assert - XML docs include logger param
        await Assert.That(result).Contains("/// <summary>");
        await Assert.That(result).Contains("/// </summary>");
        await Assert.That(result).Contains("/// <param name=\"logger\">");
        await Assert.That(result).Contains("/// <returns>");
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
            StepNames: ["ValidateStep", "Approved_Process", "Rejected_Handle", "FinalizeStep"],
            StateTypeName: "TestState",
            Loops: null);
    }

    private static BranchModel CreateBranch()
    {
        var case1 = BranchCaseModel.Create(
            caseValueLiteral: "OrderStatus.Approved",
            branchPathPrefix: "Approved",
            stepNames: ["Approved_Process", "Approved_Complete"],
            isTerminal: false);
        var case2 = BranchCaseModel.Create(
            caseValueLiteral: "OrderStatus.Rejected",
            branchPathPrefix: "Rejected",
            stepNames: ["Rejected_Handle"],
            isTerminal: false);

        return BranchModel.Create(
            branchId: "Test-Status",
            previousStepName: "ValidateStep",
            discriminatorPropertyPath: "Status",
            discriminatorTypeName: "OrderStatus",
            isEnumDiscriminator: true,
            isMethodDiscriminator: false,
            cases: [case1, case2]);
    }

    private static BranchModel CreateBranchWithOtherwise()
    {
        var case1 = BranchCaseModel.Create(
            caseValueLiteral: "OrderStatus.Approved",
            branchPathPrefix: "Approved",
            stepNames: ["Approved_Process"],
            isTerminal: false);
        var defaultCase = BranchCaseModel.Create(
            caseValueLiteral: "_",
            branchPathPrefix: "Default",
            stepNames: ["Default_Handle"],
            isTerminal: false);

        return BranchModel.Create(
            branchId: "Test-Status",
            previousStepName: "ValidateStep",
            discriminatorPropertyPath: "Status",
            discriminatorTypeName: "OrderStatus",
            isEnumDiscriminator: true,
            isMethodDiscriminator: false,
            cases: [case1, defaultCase]);
    }

    private static BranchModel CreateBranchWithRejoin()
    {
        var case1 = BranchCaseModel.Create(
            caseValueLiteral: "OrderStatus.Approved",
            branchPathPrefix: "Approved",
            stepNames: ["Approved_Process", "Approved_Complete"],
            isTerminal: false);

        return BranchModel.Create(
            branchId: "Test-Status",
            previousStepName: "ValidateStep",
            discriminatorPropertyPath: "Status",
            discriminatorTypeName: "OrderStatus",
            isEnumDiscriminator: true,
            isMethodDiscriminator: false,
            cases: [case1],
            rejoinStepName: "FinalizeStep");
    }

    private static BranchCaseModel CreateBranchCase()
    {
        return BranchCaseModel.Create(
            caseValueLiteral: "OrderStatus.Approved",
            branchPathPrefix: "Approved",
            stepNames: ["Approved_Process", "Approved_Complete"],
            isTerminal: false);
    }

    // =============================================================================
    // G. Loop Prefix Tests
    // =============================================================================

    /// <summary>
    /// Verifies that EmitRoutingHandler applies loop prefix to Start commands when branch is inside a loop.
    /// </summary>
    [Test]
    public async Task EmitRoutingHandler_WithLoopPrefix_GeneratesPrefixedStartCommands()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();
        var branch = CreateBranchWithLoopPrefix();

        // Act
        emitter.EmitRoutingHandler(sb, model, "TargetLoop_ValidateStep", branch);
        var result = sb.ToString();

        // Assert - Start commands should use loop-prefixed step names
        await Assert.That(result).Contains("StartTargetLoop_Approved_ProcessCommand");
        await Assert.That(result).Contains("StartTargetLoop_Rejected_HandleCommand");
    }

    /// <summary>
    /// Verifies that EmitRoutingHandler does NOT apply prefix when branch is NOT inside a loop.
    /// </summary>
    [Test]
    public async Task EmitRoutingHandler_WithoutLoopPrefix_GeneratesUnprefixedStartCommands()
    {
        // Arrange
        var emitter = new BranchHandlerEmitter();
        var sb = new StringBuilder();
        var model = CreateMinimalModel();
        var branch = CreateBranch(); // No loop prefix

        // Act
        emitter.EmitRoutingHandler(sb, model, "ValidateStep", branch);
        var result = sb.ToString();

        // Assert - Start commands should use unprefixed step names
        await Assert.That(result).Contains("StartApproved_ProcessCommand");
        await Assert.That(result).Contains("StartRejected_HandleCommand");
        await Assert.That(result).DoesNotContain("StartTargetLoop_");
    }

    private static BranchModel CreateBranchWithLoopPrefix()
    {
        var case1 = BranchCaseModel.Create(
            caseValueLiteral: "OrderStatus.Approved",
            branchPathPrefix: "Approved",
            stepNames: ["Approved_Process", "Approved_Complete"],
            isTerminal: false);
        var case2 = BranchCaseModel.Create(
            caseValueLiteral: "OrderStatus.Rejected",
            branchPathPrefix: "Rejected",
            stepNames: ["Rejected_Handle"],
            isTerminal: false);

        return BranchModel.Create(
            branchId: "Test-Status",
            previousStepName: "TargetLoop_ValidateStep",
            discriminatorPropertyPath: "Status",
            discriminatorTypeName: "OrderStatus",
            isEnumDiscriminator: true,
            isMethodDiscriminator: false,
            cases: [case1, case2],
            loopPrefix: "TargetLoop");
    }
}
