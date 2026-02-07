// -----------------------------------------------------------------------
// <copyright file="NamingHelper.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Agentic.Workflow.Generators.Polyfills;

namespace Agentic.Workflow.Generators.Helpers;

/// <summary>
/// Provides helper methods for generating consistent naming conventions in source generators.
/// </summary>
internal static class NamingHelper
{
    /// <summary>
    /// Gets the Start command name for a workflow.
    /// </summary>
    /// <param name="workflowPascalName">The Pascal-cased workflow name.</param>
    /// <returns>The Start command name (e.g., "StartProcessOrderCommand").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="workflowPascalName"/> is null.</exception>
    public static string GetStartCommandName(string workflowPascalName)
    {
        ThrowHelper.ThrowIfNull(workflowPascalName, nameof(workflowPascalName));
        return $"Start{workflowPascalName}Command";
    }

    /// <summary>
    /// Gets the StartStep command name for a step.
    /// </summary>
    /// <param name="stepName">The step name.</param>
    /// <returns>The StartStep command name (e.g., "StartValidateOrderCommand").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepName"/> is null.</exception>
    public static string GetStartStepCommandName(string stepName)
    {
        ThrowHelper.ThrowIfNull(stepName, nameof(stepName));
        return $"Start{stepName}Command";
    }

    /// <summary>
    /// Gets the Execute command name for a step.
    /// </summary>
    /// <param name="stepName">The step name.</param>
    /// <returns>The Execute command name (e.g., "ExecuteValidateOrderCommand").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepName"/> is null.</exception>
    public static string GetExecuteCommandName(string stepName)
    {
        ThrowHelper.ThrowIfNull(stepName, nameof(stepName));
        return $"Execute{stepName}Command";
    }

    /// <summary>
    /// Gets the Worker command name for a step.
    /// </summary>
    /// <param name="stepName">The step name.</param>
    /// <returns>The Worker command name (e.g., "ExecuteValidateOrderWorkerCommand").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepName"/> is null.</exception>
    public static string GetWorkerCommandName(string stepName)
    {
        ThrowHelper.ThrowIfNull(stepName, nameof(stepName));
        return $"Execute{stepName}WorkerCommand";
    }

    /// <summary>
    /// Gets the Completed event name for a step.
    /// </summary>
    /// <param name="stepName">The step name.</param>
    /// <returns>The Completed event name (e.g., "ValidateOrderCompleted").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepName"/> is null.</exception>
    public static string GetCompletedEventName(string stepName)
    {
        ThrowHelper.ThrowIfNull(stepName, nameof(stepName));
        return $"{stepName}Completed";
    }

    /// <summary>
    /// Gets the Saga class name for a workflow.
    /// </summary>
    /// <param name="pascalName">The Pascal-cased workflow name.</param>
    /// <param name="version">The workflow version.</param>
    /// <returns>The Saga class name (e.g., "ProcessOrderSaga" for v1, "ProcessOrderSagaV2" for v2+).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pascalName"/> is null.</exception>
    public static string GetSagaClassName(string pascalName, int version)
    {
        ThrowHelper.ThrowIfNull(pascalName, nameof(pascalName));
        return version == 1 ? $"{pascalName}Saga" : $"{pascalName}SagaV{version}";
    }

    /// <summary>
    /// Gets the Reducer type name for a state type.
    /// </summary>
    /// <param name="stateTypeName">The state type name.</param>
    /// <returns>The Reducer type name (e.g., "OrderStateReducer").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stateTypeName"/> is null.</exception>
    public static string GetReducerTypeName(string stateTypeName)
    {
        ThrowHelper.ThrowIfNull(stateTypeName, nameof(stateTypeName));
        return $"{stateTypeName}Reducer";
    }
}