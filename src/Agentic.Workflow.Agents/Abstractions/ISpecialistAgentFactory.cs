// =============================================================================
// <copyright file="ISpecialistAgentFactory.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

using Agentic.Workflow.Agents.Models;

namespace Agentic.Workflow.Agents.Abstractions;

/// <summary>
/// Factory for creating specialist agents from personas or predefined types.
/// </summary>
/// <remarks>
/// <para>
/// This factory enables dynamic specialist creation without requiring direct
/// dependency injection of concrete agent types. Use cases include:
/// </para>
/// <list type="bullet">
///   <item><description>Creating specialists based on workflow requirements</description></item>
///   <item><description>Orchestrator creating specialists dynamically</description></item>
///   <item><description>Testing with custom personas</description></item>
/// </list>
/// </remarks>
public interface ISpecialistAgentFactory
{
    /// <summary>
    /// Creates a specialist agent from the specified persona.
    /// </summary>
    /// <param name="persona">The persona defining the specialist's behavior.</param>
    /// <returns>A configured specialist agent.</returns>
    /// <exception cref="ArgumentNullException">Thrown when persona is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the persona is invalid.</exception>
    ISpecialistAgent Create(SpecialistPersona persona);

    /// <summary>
    /// Creates a specialist agent for the specified type using predefined personas.
    /// </summary>
    /// <param name="type">The type of specialist to create.</param>
    /// <returns>A configured specialist agent.</returns>
    /// <exception cref="ArgumentException">Thrown when no persona is defined for the type.</exception>
    ISpecialistAgent Create(SpecialistType type);

    /// <summary>
    /// Gets the predefined persona for a specialist type.
    /// </summary>
    /// <param name="type">The specialist type.</param>
    /// <returns>The persona, or null if no predefined persona exists.</returns>
    SpecialistPersona? GetPersona(SpecialistType type);
}