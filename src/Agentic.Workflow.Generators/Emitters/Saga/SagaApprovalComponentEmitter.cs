// -----------------------------------------------------------------------
// <copyright file="SagaApprovalComponentEmitter.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agentic.Workflow.Generators.Models;
using Agentic.Workflow.Generators.Polyfills;

namespace Agentic.Workflow.Generators.Emitters.Saga;

/// <summary>
/// Component emitter that generates approval resume handlers for a Wolverine saga.
/// </summary>
/// <remarks>
/// <para>
/// This component emits handlers that process approval resume commands. For each
/// approval checkpoint in the workflow, it generates a handler that either:
/// <list type="bullet">
///   <item><description>Proceeds to the next step if approved</description></item>
///   <item><description>Transitions to Failed phase if rejected</description></item>
/// </list>
/// </para>
/// </remarks>
internal sealed class SagaApprovalComponentEmitter : ISagaComponentEmitter
{
    private readonly SagaApprovalHandlersEmitter _resumeHandlerEmitter = new();

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="sb"/> or <paramref name="model"/> is null.
    /// </exception>
    public void Emit(StringBuilder sb, WorkflowModel model)
    {
        ThrowHelper.ThrowIfNull(sb, nameof(sb));
        ThrowHelper.ThrowIfNull(model, nameof(model));

        if (!model.HasApprovalPoints)
        {
            return;
        }

        var context = SagaEmissionContext.Create(model);

        foreach (var approval in model.ApprovalPoints!)
        {
            var resumeContext = BuildApprovalResumeContext(context, approval);

            sb.AppendLine();
            _resumeHandlerEmitter.EmitResumeHandler(sb, model, approval, resumeContext);
        }
    }

    /// <summary>
    /// Builds the approval resume context for a specific approval checkpoint.
    /// </summary>
    /// <param name="ctx">The saga emission context.</param>
    /// <param name="approval">The approval model.</param>
    /// <returns>The context for emitting the resume handler.</returns>
    private static ApprovalResumeContext BuildApprovalResumeContext(
        SagaEmissionContext ctx,
        ApprovalModel approval)
    {
        // Find the index of the step that precedes this approval
        var stepIndex = FindStepIndex(ctx.Model.StepNames, approval.PrecedingStepName);

        // Check if this is the last step
        var isLastStep = stepIndex == ctx.Model.StepNames.Count - 1;

        // Get the next step name (if not last)
        var nextStepName = isLastStep ? null : ctx.Model.StepNames[stepIndex + 1];

        return new ApprovalResumeContext(
            IsLastStep: isLastStep,
            NextStepName: nextStepName);
    }

    /// <summary>
    /// Finds the index of a step name in the step names list.
    /// </summary>
    /// <param name="stepNames">The list of step names.</param>
    /// <param name="stepName">The step name to find.</param>
    /// <returns>The index of the step, or -1 if not found.</returns>
    private static int FindStepIndex(IReadOnlyList<string> stepNames, string stepName)
    {
        for (int i = 0; i < stepNames.Count; i++)
        {
            if (stepNames[i] == stepName)
            {
                return i;
            }
        }

        return -1;
    }
}
