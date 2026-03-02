namespace Strategos.Ontology.Builder;

/// <summary>
/// Fluent builder for configuring ontology property descriptors.
/// </summary>
public interface IPropertyBuilder
{
    /// <summary>Marks the property as required.</summary>
    IPropertyBuilder Required();

    /// <summary>Marks the property as computed (derived at runtime, not stored).</summary>
    IPropertyBuilder Computed();

    /// <summary>Configures the property as a vector embedding with the specified number of dimensions.</summary>
    /// <param name="dimensions">The dimensionality of the vector (must be &gt;= 1).</param>
    IPropertyBuilder Vector(int dimensions);
}
