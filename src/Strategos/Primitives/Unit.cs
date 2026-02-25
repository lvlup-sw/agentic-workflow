// =============================================================================
// <copyright file="Unit.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Primitives;

/// <summary>
/// Represents a unit type (similar to void) for use in Result patterns.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Gets the single instance of the Unit type.
    /// </summary>
    public static readonly Unit Value = default;

    /// <summary>
    /// Determines whether two Unit instances are equal.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>Always returns true.</returns>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// Determines whether two Unit instances are not equal.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>Always returns false.</returns>
    public static bool operator !=(Unit left, Unit right) => false;

    /// <summary>
    /// Determines whether this instance is equal to another Unit instance.
    /// </summary>
    /// <param name="other">The other Unit instance.</param>
    /// <returns>Always returns true since all Unit instances are equal.</returns>
    public bool Equals(Unit other) => true;

    /// <summary>
    /// Determines whether this instance is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>True if the object is a Unit instance; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Gets the hash code for this instance.
    /// </summary>
    /// <returns>Always returns 0 for consistency.</returns>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Gets the string representation of this instance.
    /// </summary>
    /// <returns>Returns "()".</returns>
    public override string ToString() => "()";
}
