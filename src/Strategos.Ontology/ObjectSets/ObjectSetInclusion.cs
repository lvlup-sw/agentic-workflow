namespace Strategos.Ontology.ObjectSets;

/// <summary>
/// Flags controlling which data facets are included in object set results.
/// </summary>
[Flags]
public enum ObjectSetInclusion
{
    Properties = 1,
    Actions = 2,
    Links = 4,
    Events = 8,
    Interfaces = 16,
    LinkedObjects = 32,
    Schema = Properties | Actions | Links | Interfaces,
    Full = Schema | Events | LinkedObjects
}
