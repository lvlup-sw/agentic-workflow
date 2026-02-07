// -----------------------------------------------------------------------
// <copyright file="MermaidEmitter.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Text;

using Agentic.Workflow.Generators.Models;
using Agentic.Workflow.Generators.Polyfills;

namespace Agentic.Workflow.Generators.Emitters;

/// <summary>
/// Emits Mermaid state diagram source for a workflow.
/// </summary>
internal static class MermaidEmitter
{
    /// <summary>
    /// Generates the Mermaid state diagram source for the given workflow model.
    /// </summary>
    /// <param name="model">The workflow model containing workflow structure information.</param>
    /// <returns>The generated Mermaid diagram source code.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is null.</exception>
    public static string Emit(WorkflowModel model)
    {
        ThrowHelper.ThrowIfNull(model, nameof(model));

        var sb = new StringBuilder();

        // Workflow name comment
        sb.AppendLine($"%% Workflow: {model.WorkflowName}");

        // Mermaid state diagram header
        sb.AppendLine("stateDiagram-v2");

        // Start transition to first step
        if (model.StepNames.Count > 0)
        {
            sb.AppendLine($"    [*] --> {model.StepNames[0]}");
        }

        // Build loop lookup for quick access
        var loopsByFirstStep = model.Loops?.ToDictionary(l => l.FirstBodyStepName, l => l)
            ?? new Dictionary<string, LoopModel>();
        var loopsByLastStep = model.Loops?.ToDictionary(l => l.LastBodyStepName, l => l)
            ?? new Dictionary<string, LoopModel>();

        // Build branch lookup structures
        // Filter out branches that follow other branches (empty PreviousStepName)
        var branchesByPreviousStep = model.Branches?
            .Where(b => !string.IsNullOrEmpty(b.PreviousStepName))
            .ToDictionary(b => b.PreviousStepName, b => b)
            ?? new Dictionary<string, BranchModel>();
        var branchCaseLastSteps = new Dictionary<string, (BranchModel Branch, BranchCaseModel Case)>();
        if (model.Branches is not null)
        {
            foreach (var branch in model.Branches)
            {
                foreach (var branchCase in branch.Cases)
                {
                    branchCaseLastSteps[branchCase.LastStepName] = (branch, branchCase);
                }
            }
        }

        // Sequential step transitions and failure paths
        for (var i = 0; i < model.StepNames.Count; i++)
        {
            var stepName = model.StepNames[i];

            // Check if this step has validation
            var stepModel = model.Steps?.FirstOrDefault(s => s.StepName == stepName || s.PhaseName == stepName);
            var hasValidation = stepModel?.HasValidation ?? false;

            // Validation guard failure transition (before normal transition)
            if (hasValidation)
            {
                sb.AppendLine($"    {stepName} --> ValidationFailed : guard failed");
            }

            // Check if this step is the first body step of a loop (add note)
            if (loopsByFirstStep.TryGetValue(stepName, out var loopAtFirst))
            {
                sb.AppendLine($"    note right of {stepName} : Loop: {loopAtFirst.LoopName} (max {loopAtFirst.MaxIterations})");
            }

            // Check if this step precedes a branch point
            if (branchesByPreviousStep.TryGetValue(stepName, out var branch))
            {
                // Transition to choice state
                var choiceName = $"BranchBy{branch.DiscriminatorPropertyPath}";
                sb.AppendLine($"    {stepName} --> {choiceName}");

                // Emit choice state and case transitions
                sb.AppendLine($"    state {choiceName} <<choice>>");
                foreach (var branchCase in branch.Cases)
                {
                    sb.AppendLine($"    {choiceName} --> {branchCase.FirstStepName} : {branchCase.BranchPathPrefix}");
                }
            }

            // Check if this step is the last step in a branch case
            else if (branchCaseLastSteps.TryGetValue(stepName, out var caseInfo))
            {
                var (parentBranch, branchCase) = caseInfo;
                if (branchCase.IsTerminal)
                {
                    // Terminal branch - transition to completion
                    sb.AppendLine($"    {stepName} --> [*]");
                }
                else if (parentBranch.RejoinStepName is not null)
                {
                    // Non-terminal branch - transition to rejoin step
                    sb.AppendLine($"    {stepName} --> {parentBranch.RejoinStepName}");
                }
            }

            // Check if this step is the last body step of a loop (special transitions)
            else if (loopsByLastStep.TryGetValue(stepName, out var loopAtLast))
            {
                // Loop back transition (continue)
                sb.AppendLine($"    {stepName} --> {loopAtLast.FirstBodyStepName} : continue");

                // Exit transition
                if (loopAtLast.ContinuationStepName is not null)
                {
                    sb.AppendLine($"    {stepName} --> {loopAtLast.ContinuationStepName} : exit");
                }
                else
                {
                    // Terminal loop - exit to completion
                    sb.AppendLine($"    {stepName} --> [*] : exit");
                }
            }
            else if (i < model.StepNames.Count - 1)
            {
                // Standard transition to next step (if not last and not a loop-back)
                sb.AppendLine($"    {stepName} --> {model.StepNames[i + 1]}");
            }

            // Every step can transition to Failed
            sb.AppendLine($"    {stepName} --> Failed");
        }

        // Completion transition from last step
        if (model.StepNames.Count > 0)
        {
            sb.AppendLine($"    {model.StepNames[model.StepNames.Count - 1]} --> [*]");
        }

        // Failed state
        sb.AppendLine("    state Failed");

        // ValidationFailed state (only when workflow has validation guards)
        if (model.HasAnyValidation)
        {
            sb.AppendLine("    state ValidationFailed");
        }

        return sb.ToString();
    }
}
