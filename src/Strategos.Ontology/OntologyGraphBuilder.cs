using Strategos.Ontology.Builder;
using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology;

public sealed class OntologyGraphBuilder
{
    private readonly List<DomainOntology> _domainOntologies = [];

    public OntologyGraphBuilder AddDomain<T>()
        where T : DomainOntology, new()
    {
        _domainOntologies.Add(new T());
        return this;
    }

    public OntologyGraph Build()
    {
        var domains = new List<DomainDescriptor>();
        var allObjectTypes = new List<ObjectTypeDescriptor>();
        var allInterfaces = new List<InterfaceDescriptor>();
        var allCrossDomainLinkDescriptors = new List<(string SourceDomain, CrossDomainLinkDescriptor Descriptor)>();

        foreach (var domainOntology in _domainOntologies)
        {
            var ontologyBuilder = new OntologyBuilder(domainOntology.DomainName);
            domainOntology.Build(ontologyBuilder);

            var domainDescriptor = new DomainDescriptor(domainOntology.DomainName)
            {
                ObjectTypes = ontologyBuilder.ObjectTypes.ToArray(),
            };

            domains.Add(domainDescriptor);
            allObjectTypes.AddRange(ontologyBuilder.ObjectTypes);
            allInterfaces.AddRange(ontologyBuilder.Interfaces);

            foreach (var crossDomainLink in ontologyBuilder.CrossDomainLinks)
            {
                allCrossDomainLinkDescriptors.Add((domainOntology.DomainName, crossDomainLink));
            }
        }

        var domainLookup = domains.ToDictionary(d => d.DomainName);
        var objectTypeLookup = allObjectTypes
            .GroupBy(ot => ot.DomainName)
            .ToDictionary(g => g.Key, g => g.ToDictionary(ot => ot.Name));

        var resolvedLinks = ResolveCrossDomainLinks(
            allCrossDomainLinkDescriptors, domainLookup, objectTypeLookup, allObjectTypes);

        ValidateInterfaceImplementations(allObjectTypes, allInterfaces);

        var workflowChains = BuildWorkflowChains(allObjectTypes);

        return new OntologyGraph(
            domains: domains.ToArray(),
            objectTypes: allObjectTypes.ToArray(),
            interfaces: allInterfaces.ToArray(),
            crossDomainLinks: resolvedLinks.ToArray(),
            workflowChains: workflowChains.ToArray());
    }

    private static List<ResolvedCrossDomainLink> ResolveCrossDomainLinks(
        List<(string SourceDomain, CrossDomainLinkDescriptor Descriptor)> linkDescriptors,
        Dictionary<string, DomainDescriptor> domainLookup,
        Dictionary<string, Dictionary<string, ObjectTypeDescriptor>> objectTypeLookup,
        List<ObjectTypeDescriptor> allObjectTypes)
    {
        var resolved = new List<ResolvedCrossDomainLink>();

        foreach (var (sourceDomain, descriptor) in linkDescriptors)
        {
            if (!domainLookup.ContainsKey(descriptor.TargetDomain))
            {
                throw new OntologyCompositionException(
                    $"Cross-domain link '{descriptor.Name}' references unresolvable domain '{descriptor.TargetDomain}'.");
            }

            if (!objectTypeLookup.TryGetValue(descriptor.TargetDomain, out var targetDomainTypes)
                || !targetDomainTypes.TryGetValue(descriptor.TargetTypeName, out var targetObjectType))
            {
                throw new OntologyCompositionException(
                    $"Cross-domain link '{descriptor.Name}' references unresolvable object type '{descriptor.TargetTypeName}' in domain '{descriptor.TargetDomain}'.");
            }

            var sourceObjectType = allObjectTypes.FirstOrDefault(
                ot => ot.DomainName == sourceDomain && ot.ClrType == descriptor.SourceType);

            resolved.Add(new ResolvedCrossDomainLink(
                Name: descriptor.Name,
                SourceDomain: sourceDomain,
                SourceObjectType: sourceObjectType!,
                TargetDomain: descriptor.TargetDomain,
                TargetObjectType: targetObjectType,
                Cardinality: descriptor.Cardinality,
                EdgeProperties: descriptor.EdgeProperties));
        }

        return resolved;
    }

    private static void ValidateInterfaceImplementations(
        List<ObjectTypeDescriptor> allObjectTypes,
        List<InterfaceDescriptor> allInterfaces)
    {
        var interfaceLookup = allInterfaces.ToDictionary(i => i.Name);

        foreach (var objectType in allObjectTypes)
        {
            foreach (var implementedInterface in objectType.ImplementedInterfaces)
            {
                if (!interfaceLookup.TryGetValue(implementedInterface.Name, out var interfaceDescriptor))
                {
                    continue;
                }

                var objectPropertyLookup = objectType.Properties
                    .ToDictionary(p => p.Name, p => p.PropertyType);

                foreach (var interfaceProperty in interfaceDescriptor.Properties)
                {
                    if (!objectPropertyLookup.TryGetValue(interfaceProperty.Name, out var objectPropertyType))
                    {
                        throw new OntologyCompositionException(
                            $"Object type '{objectType.Name}' implements interface '{implementedInterface.Name}' but is missing property '{interfaceProperty.Name}'.");
                    }

                    if (!interfaceProperty.PropertyType.IsAssignableFrom(objectPropertyType))
                    {
                        throw new OntologyCompositionException(
                            $"Object type '{objectType.Name}' implements interface '{implementedInterface.Name}' but property '{interfaceProperty.Name}' has incompatible type. Expected '{interfaceProperty.PropertyType.Name}', found '{objectPropertyType.Name}'.");
                    }
                }
            }
        }
    }

    private static List<WorkflowChain> BuildWorkflowChains(List<ObjectTypeDescriptor> allObjectTypes)
    {
        // Workflow chain validation infrastructure - will be populated in Phase 3
        // when Produces<T>/Consumes<T> extension methods are available.
        return [];
    }
}
