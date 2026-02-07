// -----------------------------------------------------------------------
// <copyright file="LoopConditionEmitter.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Text;

using Agentic.Workflow.Generators.Helpers;
using Agentic.Workflow.Generators.Models;
using Agentic.Workflow.Generators.Polyfills;

namespace Agentic.Workflow.Generators.Emitters.Saga;

/// <summary>
/// Emits virtual condition methods for loop constructs in a Wolverine saga.
/// </summary>
/// <remarks>
/// <para>
/// This emitter generates protected virtual methods that determine when a loop
/// should exit. Each loop gets a corresponding `ShouldExit{LoopName}Loop()` method.
/// </para>
/// <para>
/// The generated methods return false by default and are intended to be overridden
/// in partial classes or resolved via the workflow registry at runtime.
/// </para>
/// </remarks>
internal sealed class LoopConditionEmitter
{
    /// <summary>
    /// Emits a loop condition method for the specified loop.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append generated code to.</param>
    /// <param name="model">The workflow model containing state type information.</param>
    /// <param name="loop">The loop model.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is null.
    /// </exception>
    public void EmitConditionMethod(StringBuilder sb, WorkflowModel model, LoopModel loop)
    {
        ThrowHelper.ThrowIfNull(sb, nameof(sb));
        ThrowHelper.ThrowIfNull(model, nameof(model));
        ThrowHelper.ThrowIfNull(loop, nameof(loop));

        var methodName = loop.ConditionMethodName;
        var stateTypeName = model.StateTypeName ?? "object";

        XmlDocHelper.AppendSummary(sb, $"Evaluates whether the {loop.LoopName} loop should exit.", "    ");
        XmlDocHelper.AppendRemarks(
            sb,
            [
                $"Condition ID: {loop.ConditionId}",
                "This method delegates to the WorkflowConditionRegistry for runtime evaluation.",
                "The condition is registered when the workflow definition is accessed.",
            ],
            "    ");
        XmlDocHelper.AppendReturns(sb, "True if the loop should exit, false to continue iterating.", "    ");
        sb.AppendLine($"    protected virtual bool {methodName}()");
        sb.AppendLine("    {");
        sb.AppendLine($"        return Agentic.Workflow.Services.WorkflowConditionRegistry.Evaluate<{stateTypeName}>(\"{loop.ConditionId}\", State);");
        sb.AppendLine("    }");
    }
}
