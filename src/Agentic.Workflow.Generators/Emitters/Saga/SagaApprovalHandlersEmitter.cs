// -----------------------------------------------------------------------
// <copyright file="SagaApprovalHandlersEmitter.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Text;

using Agentic.Workflow.Generators.Models;
using Agentic.Workflow.Generators.Polyfills;

namespace Agentic.Workflow.Generators.Emitters.Saga;

/// <summary>
/// Emits handler methods for approval resume commands in a Wolverine saga.
/// </summary>
/// <remarks>
/// <para>
/// This emitter generates handlers that process approval resume commands.
/// The behavior differs based on the approval result:
/// <list type="bullet">
///   <item><description>
///     Approved: Proceeds to the next step (or completes if final step)
///   </description></item>
///   <item><description>
///     Rejected: Transitions to Failed phase and completes the saga
///   </description></item>
/// </list>
/// </para>
/// </remarks>
internal sealed class SagaApprovalHandlersEmitter
{
    /// <summary>
    /// Emits a handler method for an approval resume command.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append generated code to.</param>
    /// <param name="model">The workflow model.</param>
    /// <param name="approval">The approval model.</param>
    /// <param name="context">The context with information about the next step.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is null.
    /// </exception>
    public void EmitResumeHandler(
        StringBuilder sb,
        WorkflowModel model,
        ApprovalModel approval,
        ApprovalResumeContext context)
    {
        ThrowHelper.ThrowIfNull(sb, nameof(sb));
        ThrowHelper.ThrowIfNull(model, nameof(model));
        ThrowHelper.ThrowIfNull(approval, nameof(approval));
        ThrowHelper.ThrowIfNull(context, nameof(context));

        var commandName = $"Resume{approval.ApprovalPointName}ApprovalCommand";

        // XML documentation
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Handles the approval resume command for {approval.ApprovalPointName}.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    /// <param name=\"cmd\">The approval resume command.</param>");

        if (context.IsLastStep)
        {
            EmitFinalStepResumeHandler(sb, model, approval, commandName);
        }
        else
        {
            EmitNonFinalStepResumeHandler(sb, model, approval, commandName, context.NextStepName!);
        }
    }

    private static void EmitFinalStepResumeHandler(
        StringBuilder sb,
        WorkflowModel model,
        ApprovalModel approval,
        string commandName)
    {
        // Final step - void return, sets Completed on approval or Failed on rejection
        sb.AppendLine("    public void Handle(");
        sb.AppendLine($"        {commandName} cmd)");
        sb.AppendLine("    {");
        sb.AppendLine("        ArgumentNullException.ThrowIfNull(cmd, nameof(cmd));");
        sb.AppendLine("        PendingApprovalRequestId = null;");
        sb.AppendLine();
        sb.AppendLine("        switch (cmd.Decision)");
        sb.AppendLine("        {");
        sb.AppendLine("            case Agentic.Core.Models.ApprovalDecision.Approved:");
        sb.AppendLine("                if (!string.IsNullOrEmpty(cmd.Instructions))");
        sb.AppendLine("                {");
        sb.AppendLine("                    ApprovalInstructions = cmd.Instructions;");
        sb.AppendLine("                }");
        sb.AppendLine();
        sb.AppendLine($"                Phase = {model.PhaseEnumName}.Completed;");
        sb.AppendLine("                MarkCompleted();");
        sb.AppendLine("                break;");
        sb.AppendLine();
        sb.AppendLine("            case Agentic.Core.Models.ApprovalDecision.Rejected:");
        EmitRejectionHandling(sb, model, approval, isVoidHandler: true);
        sb.AppendLine("                break;");
        sb.AppendLine();
        sb.AppendLine("            case Agentic.Core.Models.ApprovalDecision.Deferred:");
        sb.AppendLine("                // Stay in approval phase, await another response");
        sb.AppendLine("                break;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
    }

    private static void EmitNonFinalStepResumeHandler(
        StringBuilder sb,
        WorkflowModel model,
        ApprovalModel approval,
        string commandName,
        string nextStepName)
    {
        // Non-final step - returns nullable object to allow multiple command types
        sb.AppendLine($"    /// <returns>The command to start the next step, or null if deferred.</returns>");
        sb.AppendLine($"    public object? Handle(");
        sb.AppendLine($"        {commandName} cmd)");
        sb.AppendLine("    {");
        sb.AppendLine("        ArgumentNullException.ThrowIfNull(cmd, nameof(cmd));");
        sb.AppendLine("        PendingApprovalRequestId = null;");
        sb.AppendLine();
        sb.AppendLine("        switch (cmd.Decision)");
        sb.AppendLine("        {");
        sb.AppendLine("            case Agentic.Core.Models.ApprovalDecision.Approved:");
        sb.AppendLine("                if (!string.IsNullOrEmpty(cmd.Instructions))");
        sb.AppendLine("                {");
        sb.AppendLine("                    ApprovalInstructions = cmd.Instructions;");
        sb.AppendLine("                }");
        sb.AppendLine();
        sb.AppendLine($"                return new Start{nextStepName}Command(WorkflowId);");
        sb.AppendLine();
        sb.AppendLine("            case Agentic.Core.Models.ApprovalDecision.Rejected:");
        EmitRejectionHandling(sb, model, approval, isVoidHandler: false);
        sb.AppendLine();
        sb.AppendLine("            case Agentic.Core.Models.ApprovalDecision.Deferred:");
        sb.AppendLine("                // Stay in approval phase, await another response");
        sb.AppendLine("                return null;");
        sb.AppendLine();
        sb.AppendLine("            default:");
        sb.AppendLine("                return null;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
    }

    /// <summary>
    /// Emits a handler method for an approval timeout command.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append generated code to.</param>
    /// <param name="model">The workflow model.</param>
    /// <param name="approval">The approval model.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is null.
    /// </exception>
    public void EmitTimeoutHandler(
        StringBuilder sb,
        WorkflowModel model,
        ApprovalModel approval)
    {
        ThrowHelper.ThrowIfNull(sb, nameof(sb));
        ThrowHelper.ThrowIfNull(model, nameof(model));
        ThrowHelper.ThrowIfNull(approval, nameof(approval));

        var commandName = $"{approval.ApprovalPointName}ApprovalTimeoutCommand";

        // XML documentation
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Handles the timeout command for {approval.ApprovalPointName} approval.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    /// <param name=\"cmd\">The timeout command.</param>");
        sb.AppendLine($"    /// <returns>The command to start escalation, or null if approval already received.</returns>");
        sb.AppendLine($"    public object? Handle(");
        sb.AppendLine($"        {commandName} cmd)");
        sb.AppendLine("    {");
        sb.AppendLine("        ArgumentNullException.ThrowIfNull(cmd, nameof(cmd));");
        sb.AppendLine();
        sb.AppendLine("        // Race condition guard: check if approval was already received");
        sb.AppendLine("        if (PendingApprovalRequestId != cmd.ApprovalRequestId)");
        sb.AppendLine("        {");
        sb.AppendLine("            return null; // Approval already received");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        PendingApprovalRequestId = null;");
        sb.AppendLine();

        // Handle escalation path
        if (approval.EscalationSteps is not null && approval.EscalationSteps.Count > 0)
        {
            // Escalation steps configured - transition to first step
            var firstStep = approval.EscalationSteps[0].StepName;
            sb.AppendLine($"        Phase = {model.PhaseEnumName}.{firstStep};");
            sb.AppendLine($"        return new Start{firstStep}Command(WorkflowId);");
        }
        else if (approval.NestedEscalationApprovals is not null && approval.NestedEscalationApprovals.Count > 0)
        {
            // Nested approval configured - transition to escalated approval phase
            var nestedApproval = approval.NestedEscalationApprovals[0];
            sb.AppendLine($"        Phase = {model.PhaseEnumName}.{nestedApproval.PhaseName};");
            sb.AppendLine($"        return new Request{nestedApproval.ApprovalPointName}ApprovalEvent(");
            sb.AppendLine("            WorkflowId,");
            sb.AppendLine($"            \"{nestedApproval.ApprovalPointName}\",");
            sb.AppendLine("            \"Escalated from timeout\",");
            sb.AppendLine("            TimeSpan.FromHours(4),");
            sb.AppendLine("            null);");
        }
        else if (approval.IsEscalationTerminal)
        {
            // Terminal escalation - fail workflow
            sb.AppendLine($"        Phase = {model.PhaseEnumName}.Failed;");
            sb.AppendLine("        MarkCompleted();");
            sb.AppendLine("        return null;");
        }
        else
        {
            // No escalation configured - fail workflow
            sb.AppendLine($"        Phase = {model.PhaseEnumName}.Failed;");
            sb.AppendLine("        MarkCompleted();");
            sb.AppendLine("        return null;");
        }

        sb.AppendLine("    }");
    }

    /// <summary>
    /// Emits a handler method for setting the pending approval request ID.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append generated code to.</param>
    /// <param name="model">The workflow model.</param>
    /// <param name="approval">The approval model.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is null.
    /// </exception>
    public void EmitSetPendingHandler(
        StringBuilder sb,
        WorkflowModel model,
        ApprovalModel approval)
    {
        ThrowHelper.ThrowIfNull(sb, nameof(sb));
        ThrowHelper.ThrowIfNull(model, nameof(model));
        ThrowHelper.ThrowIfNull(approval, nameof(approval));

        var commandName = $"Set{approval.ApprovalPointName}PendingApprovalCommand";

        // XML documentation
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Handles the set pending approval command for {approval.ApprovalPointName}.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"cmd\">The set pending approval command.</param>");
        sb.AppendLine("    public void Handle(");
        sb.AppendLine($"        {commandName} cmd)");
        sb.AppendLine("    {");
        sb.AppendLine("        ArgumentNullException.ThrowIfNull(cmd, nameof(cmd));");
        sb.AppendLine("        PendingApprovalRequestId = cmd.ApprovalRequestId;");
        sb.AppendLine("    }");
    }

    private static void EmitRejectionHandling(
        StringBuilder sb,
        WorkflowModel model,
        ApprovalModel approval,
        bool isVoidHandler)
    {
        // Check if approval has rejection steps
        if (approval.HasRejection)
        {
            var firstRejectionStep = approval.RejectionSteps![0].StepName;
            sb.AppendLine($"                Phase = {model.PhaseEnumName}.{firstRejectionStep};");
            if (isVoidHandler)
            {
                sb.AppendLine($"                // Note: Execute rejection step in subsequent handler");
            }
            else
            {
                sb.AppendLine($"                return new Start{firstRejectionStep}Command(WorkflowId);");
            }
        }
        else
        {
            // No rejection steps - go directly to Failed
            sb.AppendLine($"                Phase = {model.PhaseEnumName}.Failed;");
            sb.AppendLine("                MarkCompleted();");
            if (!isVoidHandler)
            {
                sb.AppendLine("                return null;");
            }
        }
    }
}