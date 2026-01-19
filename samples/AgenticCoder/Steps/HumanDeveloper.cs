// =============================================================================
// <copyright file="HumanDeveloper.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace AgenticCoder.Steps;

/// <summary>
/// Marker class identifying a human developer as the approver.
/// </summary>
/// <remarks>
/// This type is used with <c>AwaitApproval&lt;HumanDeveloper&gt;</c> to create
/// a type-safe approval checkpoint where a human developer must review
/// and approve the generated code before it is considered complete.
/// </remarks>
public sealed class HumanDeveloper
{
}
