namespace Strategos.Ontology.Builder;

public interface IOntologyBuilder
{
    void Object<T>(Action<IObjectTypeBuilder<T>> configure)
        where T : class;

    void Interface<T>(string name, Action<IInterfaceBuilder<T>> configure)
        where T : class;

    ICrossDomainLinkBuilder CrossDomainLink(string name);
}
