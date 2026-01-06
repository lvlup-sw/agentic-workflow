// -----------------------------------------------------------------------
// <copyright file="ArgumentExceptionPolyfills.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// Polyfills for modern throw helper methods in netstandard2.0
// These methods were introduced in .NET 6+ but we need them for the source generator.
// Note: CallerArgumentExpression is not available in netstandard2.0, so paramName must be explicit.

namespace Agentic.Workflow.Generators.Polyfills;

/// <summary>
/// Polyfill extensions for <see cref="ArgumentNullException"/> in netstandard2.0.
/// </summary>
internal static class ThrowHelper
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.
    /// </summary>
    /// <param name="argument">The reference type argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    public static void ThrowIfNull(object? argument, string paramName)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null,
    /// or an <see cref="ArgumentException"/> if it is empty or whitespace.
    /// </summary>
    /// <param name="argument">The string argument to validate.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty or whitespace.</exception>
    public static void ThrowIfNullOrWhiteSpace(string? argument, string paramName)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(paramName);
        }

        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if <paramref name="argument"/> is empty or whitespace.
    /// </summary>
    /// <param name="argument">The string argument to validate.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty or whitespace.</exception>
    public static void ThrowIfEmpty(string? argument, string paramName)
    {
        if (string.IsNullOrEmpty(argument))
        {
            throw new ArgumentException("Value cannot be null or empty.", paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="other">The minimum value (exclusive lower bound).</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than <paramref name="other"/>.</exception>
    public static void ThrowIfLessThan(int value, int other, string paramName)
    {
        if (value < other)
        {
            throw new ArgumentOutOfRangeException(paramName, value, $"Value must be greater than or equal to {other}.");
        }
    }
}
