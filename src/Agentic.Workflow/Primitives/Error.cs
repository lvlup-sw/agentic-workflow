// =============================================================================
// <copyright file="Error.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Primitives;

/// <summary>
/// Represents an error with a code, message, and type for categorization.
/// </summary>
/// <param name="Code">The error code identifying the specific error.</param>
/// <param name="Message">The human-readable error message.</param>
/// <param name="Type">The type of error for HTTP status mapping.</param>
public record Error(string Code, string Message, ErrorType Type = ErrorType.None)
{
    /// <summary>
    /// Represents no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A new Error instance with Type defaulted to None.</returns>
    public Error(string code, string message)
        : this(code, message, ErrorType.None)
    {
    }

    /// <summary>
    /// Creates a new Error instance.
    /// </summary>
    /// <param name="errorType">The error type for HTTP status mapping.</param>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A new Error instance.</returns>
    public static Error Create(ErrorType errorType, string code, string message) =>
        new(code, message, errorType);

    /// <summary>
    /// Creates a new Error instance with ErrorType.None (for backward compatibility).
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A new Error instance with Type defaulted to None.</returns>
    public static Error Create(string code, string message) =>
        new(code, message, ErrorType.None);
}
