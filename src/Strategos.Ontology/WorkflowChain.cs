using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology;

public sealed record WorkflowChain(
    string WorkflowName,
    ObjectTypeDescriptor ConsumedType,
    ObjectTypeDescriptor ProducedType);
