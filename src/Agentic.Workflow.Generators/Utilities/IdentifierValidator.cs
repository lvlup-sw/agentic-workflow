// -----------------------------------------------------------------------
// <copyright file="IdentifierValidator.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Agentic.Workflow.Generators.Polyfills;

namespace Agentic.Workflow.Generators.Utilities;

/// <summary>
/// Provides validation methods for C# identifiers and property paths used in code generation.
/// </summary>
internal static class IdentifierValidator
{
    /// <summary>
    /// Determines whether the specified string is a valid C# identifier.
    /// </summary>
    /// <param name="identifier">The string to validate.</param>
    /// <returns><c>true</c> if the string is a valid C# identifier; otherwise, <c>false</c>.</returns>
    public static bool IsValidIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return false;
        }

        return SyntaxFacts.IsValidIdentifier(identifier);
    }

    /// <summary>
    /// Validates that the specified string is a valid C# identifier.
    /// </summary>
    /// <param name="identifier">The identifier to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="identifier"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="identifier"/> is not a valid C# identifier.</exception>
    public static void ValidateIdentifier(string identifier, string paramName)
    {
        ThrowHelper.ThrowIfNull(identifier, paramName);

        if (!IsValidIdentifier(identifier))
        {
            throw new ArgumentException(
                $"'{identifier}' is not a valid C# identifier.",
                paramName);
        }
    }

    /// <summary>
    /// Validates that the specified string is a valid property path (dot-separated identifiers).
    /// </summary>
    /// <param name="path">The property path to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is empty, whitespace, or contains invalid segments.</exception>
    public static void ValidatePropertyPath(string path, string paramName)
    {
        ThrowHelper.ThrowIfNull(path, paramName);

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "Property path cannot be empty.",
                paramName);
        }

        var parts = path.Split('.');
        foreach (var part in parts)
        {
            if (!IsValidIdentifier(part))
            {
                throw new ArgumentException(
                    $"'{part}' in property path '{path}' is not a valid C# identifier.",
                    paramName);
            }
        }
    }
}
