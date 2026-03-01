using System.Linq.Expressions;

namespace Strategos.Ontology.Builder;

public interface IPropertyBuilder<T> : IPropertyBuilder
    where T : class
{
    new IPropertyBuilder<T> Required();

    new IPropertyBuilder<T> Computed();

    IPropertyBuilder<T> DerivedFrom(params Expression<Func<T, object>>[] sources);

    IPropertyBuilder<T> DerivedFromExternal(string domain, string objectType, string property);
}
