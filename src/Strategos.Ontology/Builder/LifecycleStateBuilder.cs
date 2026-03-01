using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Builder;

internal sealed class LifecycleStateBuilder(string name) : ILifecycleStateBuilder
{
    private string? _description;
    private bool _isInitial;
    private bool _isTerminal;

    public ILifecycleStateBuilder Description(string description)
    {
        _description = description;
        return this;
    }

    public ILifecycleStateBuilder Initial()
    {
        _isInitial = true;
        return this;
    }

    public ILifecycleStateBuilder Terminal()
    {
        _isTerminal = true;
        return this;
    }

    public LifecycleStateDescriptor Build() =>
        new()
        {
            Name = name,
            Description = _description,
            IsInitial = _isInitial,
            IsTerminal = _isTerminal,
        };
}
