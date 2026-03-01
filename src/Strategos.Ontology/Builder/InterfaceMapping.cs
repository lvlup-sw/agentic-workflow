using System.Linq.Expressions;

namespace Strategos.Ontology.Builder;

internal sealed class InterfaceMapping<TObject, TInterface> : IInterfaceMapping<TObject, TInterface>
{
    private readonly List<(string SourceName, string TargetName)> _mappings = [];

    public IInterfaceMapping<TObject, TInterface> Via(
        Expression<Func<TObject, object>> source,
        Expression<Func<TInterface, object>> target)
    {
        var sourceName = ExpressionHelper.ExtractMemberName(source);
        var targetName = ExpressionHelper.ExtractMemberName(target);
        _mappings.Add((sourceName, targetName));
        return this;
    }

    public IReadOnlyList<(string SourceName, string TargetName)> GetMappings() =>
        _mappings.AsReadOnly();
}
