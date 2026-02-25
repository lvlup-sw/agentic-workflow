// -----------------------------------------------------------------------
// <copyright file="ISagaComponentEmitter.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Text;

using Strategos.Generators.Models;

namespace Strategos.Generators.Emitters.Saga;

/// <summary>
/// Defines a contract for components that emit portions of a Wolverine saga class.
/// </summary>
/// <remarks>
/// <para>
/// Implementations of this interface are responsible for generating specific
/// sections of a saga class, such as properties, start methods, or handlers.
/// </para>
/// <para>
/// Each component emitter focuses on a single responsibility following the
/// Single Responsibility Principle (SRP). The main <see cref="SagaEmitter"/>
/// orchestrates multiple component emitters to produce the complete saga.
/// </para>
/// </remarks>
internal interface ISagaComponentEmitter
{
    /// <summary>
    /// Emits the component's source code to the specified <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append generated code to.</param>
    /// <param name="model">The workflow model containing workflow information.</param>
    /// <remarks>
    /// Implementations should append their generated code to the provided
    /// <see cref="StringBuilder"/> rather than returning a new string.
    /// This enables efficient composition of multiple components.
    /// </remarks>
    void Emit(StringBuilder sb, WorkflowModel model);
}
