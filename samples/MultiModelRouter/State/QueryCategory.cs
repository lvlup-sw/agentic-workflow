// =============================================================================
// <copyright file="QueryCategory.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace MultiModelRouter.State;

/// <summary>
/// Query category for classification.
/// </summary>
public enum QueryCategory
{
    /// <summary>
    /// Factual queries requiring accurate information retrieval.
    /// </summary>
    Factual,

    /// <summary>
    /// Creative queries requiring imaginative responses.
    /// </summary>
    Creative,

    /// <summary>
    /// Technical queries requiring domain expertise.
    /// </summary>
    Technical,

    /// <summary>
    /// Conversational queries for casual interaction.
    /// </summary>
    Conversational,
}

