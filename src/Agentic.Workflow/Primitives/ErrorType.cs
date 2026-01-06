// =============================================================================
// <copyright file="ErrorType.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Agentic.Workflow.Primitives;

/// <summary>
/// Defines the types of errors that can occur in the application.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// No error type specified (default).
    /// </summary>
    None,

    /// <summary>
    /// Validation error (400 Bad Request).
    /// </summary>
    Validation,

    /// <summary>
    /// Authentication error (401 Unauthorized).
    /// </summary>
    Unauthorized,

    /// <summary>
    /// Authorization error (403 Forbidden).
    /// </summary>
    Forbidden,

    /// <summary>
    /// Resource not found error (404 Not Found).
    /// </summary>
    NotFound,

    /// <summary>
    /// Conflict error (409 Conflict).
    /// </summary>
    Conflict,

    /// <summary>
    /// Internal server error (500 Internal Server Error).
    /// </summary>
    Internal,

    /// <summary>
    /// Service unavailable error (503 Service Unavailable).
    /// </summary>
    ServiceUnavailable,

    /// <summary>
    /// Bad gateway error (502 Bad Gateway).
    /// </summary>
    BadGateway,

    /// <summary>
    /// Timeout error (504 Gateway Timeout).
    /// </summary>
    Timeout,

    /// <summary>
    /// Too many requests error (429 Too Many Requests).
    /// </summary>
    TooManyRequests,

    /// <summary>
    /// Configuration error.
    /// </summary>
    Configuration,

    /// <summary>
    /// Loop detection error when workflow gets stuck in repetitive behavior.
    /// </summary>
    LoopDetection,

    /// <summary>
    /// Budget exhausted error when workflow resources are depleted.
    /// </summary>
    /// <remarks>
    /// Returned when a budget reservation fails due to insufficient remaining resources.
    /// Signals that the workflow cannot continue without additional allocation.
    /// </remarks>
    BudgetExhausted,

    /// <summary>
    /// Network connectivity error.
    /// </summary>
    /// <remarks>
    /// Indicates transient network issues such as DNS resolution failures,
    /// connection refused, or network unreachable errors.
    /// </remarks>
    Network,

    /// <summary>
    /// External service or dependency error.
    /// </summary>
    /// <remarks>
    /// Indicates an error from an external service or API that the application depends on.
    /// </remarks>
    External,

    /// <summary>
    /// Invalid operation error.
    /// </summary>
    /// <remarks>
    /// Indicates an operation was attempted in an invalid state or context.
    /// </remarks>
    InvalidOperation,

    /// <summary>
    /// Rate limited error (alias for TooManyRequests).
    /// </summary>
    /// <remarks>
    /// Indicates the client has been rate limited and should retry after backoff.
    /// Semantically equivalent to TooManyRequests but more explicit for retry logic.
    /// </remarks>
    RateLimited,
}
