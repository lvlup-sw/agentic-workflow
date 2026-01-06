// -----------------------------------------------------------------------
// <copyright file="ServiceExtensions.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Agentic.Workflow.Agents.Extensions;

using Agentic.Workflow.Agents.Abstractions;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering Agentic.Workflow.Agents services.
/// </summary>
/// <remarks>
/// <para>
/// This library contains only abstractions and models. Implementations
/// (SpecialistAgent, factories, middleware) are in Agentic.AgentHost.
/// </para>
/// <para>
/// Consumers should:
/// <list type="bullet">
///   <item><description>Reference Agentic.AgentHost for full implementation</description></item>
///   <item><description>Or provide their own implementations of the abstractions</description></item>
/// </list>
/// </para>
/// </remarks>
public static class ServiceExtensions
{
    /// <summary>
    /// Adds a conversation thread manager to the service collection.
    /// </summary>
    /// <typeparam name="TManager">The thread manager implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConversationThreadManager<TManager>(this IServiceCollection services)
        where TManager : class, IConversationThreadManager
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddScoped<IConversationThreadManager, TManager>();
        return services;
    }

    /// <summary>
    /// Adds a streaming callback to the service collection.
    /// </summary>
    /// <typeparam name="TCallback">The streaming callback implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddStreamingCallback<TCallback>(this IServiceCollection services)
        where TCallback : class, IStreamingCallback
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddScoped<IStreamingCallback, TCallback>();
        return services;
    }
}
