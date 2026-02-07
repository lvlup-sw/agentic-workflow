// -----------------------------------------------------------------------
// <copyright file="ContextModel.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Agentic.Workflow.Generators.Models;

/// <summary>
/// Represents the context configuration for a workflow step.
/// </summary>
/// <param name="Sources">The ordered list of context sources.</param>
/// <remarks>
/// Context models aggregate multiple context sources that are assembled
/// at runtime to provide RAG context for agent steps.
/// </remarks>
internal sealed record ContextModel(IReadOnlyList<ContextSourceModel> Sources);