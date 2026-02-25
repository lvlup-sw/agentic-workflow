// =============================================================================
// <copyright file="ValidationDefinition.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Definitions;

/// <summary>
/// Immutable definition for workflow step state validation.
/// </summary>
/// <remarks>
/// <para>
/// Validation definitions capture guard conditions that run before step execution.
/// Per the Guard-Then-Dispatch pattern, validation failures:
/// <list type="bullet">
///   <item><description>Never throw exceptions (avoids useless Wolverine retries)</description></item>
///   <item><description>Transition to ValidationFailed phase</description></item>
///   <item><description>Emit audit events for observability</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record ValidationDefinition
{
    /// <summary>
    /// Gets the predicate expression text to evaluate.
    /// </summary>
    /// <remarks>
    /// This is the string representation of the lambda body, such as "state.Order.Items.Any()".
    /// The source generator uses this to emit the guard condition in generated saga handlers.
    /// </remarks>
    public required string PredicateExpression { get; init; }

    /// <summary>
    /// Gets the error message to use when validation fails.
    /// </summary>
    public required string ErrorMessage { get; init; }

    /// <summary>
    /// Creates a validation definition with the specified predicate and error message.
    /// </summary>
    /// <param name="predicateExpression">The predicate expression text to evaluate.</param>
    /// <param name="errorMessage">The error message when validation fails.</param>
    /// <returns>A new validation definition.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="predicateExpression"/> or <paramref name="errorMessage"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="errorMessage"/> is empty or whitespace.
    /// </exception>
    public static ValidationDefinition Create(string predicateExpression, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicateExpression, nameof(predicateExpression));
        ArgumentNullException.ThrowIfNull(errorMessage, nameof(errorMessage));
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage, nameof(errorMessage));

        return new ValidationDefinition
        {
            PredicateExpression = predicateExpression,
            ErrorMessage = errorMessage,
        };
    }
}
