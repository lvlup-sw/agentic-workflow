// -----------------------------------------------------------------------
// <copyright file="SagaLoopConditionsEmitter.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Text;
using Agentic.Workflow.Generators.Models;
using Agentic.Workflow.Generators.Polyfills;

namespace Agentic.Workflow.Generators.Emitters.Saga;

/// <summary>
/// Emits loop condition methods for all loops in a workflow saga.
/// </summary>
/// <remarks>
/// <para>
/// This emitter implements <see cref="ISagaComponentEmitter"/> to provide uniform
/// composition with other saga components. It delegates to <see cref="LoopConditionEmitter"/>
/// for the actual method generation.
/// </para>
/// <para>
/// When emitting, it iterates over all loops in the workflow model and emits
/// a corresponding condition method for each loop. If the model has no loops,
/// no output is generated.
/// </para>
/// </remarks>
internal sealed class SagaLoopConditionsEmitter : ISagaComponentEmitter
{
    private readonly LoopConditionEmitter _emitter = new();

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="sb"/> or <paramref name="model"/> is null.
    /// </exception>
    public void Emit(StringBuilder sb, WorkflowModel model)
    {
        ThrowHelper.ThrowIfNull(sb, nameof(sb));
        ThrowHelper.ThrowIfNull(model, nameof(model));

        if (!model.HasLoops)
        {
            return;
        }

        foreach (var loop in model.Loops!)
        {
            sb.AppendLine();
            _emitter.EmitConditionMethod(sb, model, loop);
        }
    }
}
