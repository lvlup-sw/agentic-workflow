// -----------------------------------------------------------------------
// <copyright file="IsExternalInit.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// Polyfill for init-only properties in netstandard2.0
// This type is normally provided by System.Runtime in .NET 5+
// For netstandard2.0 source generators, we need to define it ourselves.

namespace System.Runtime.CompilerServices;

/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.
/// </summary>
[ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never)]
internal static class IsExternalInit
{
}