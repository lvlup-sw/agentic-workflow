// Copyright (c) Levelup Software. All rights reserved.

using Strategos.Ontology.Descriptors;

namespace Strategos.Ontology.Query;

public sealed record ConstraintEvaluation(
    ActionPrecondition Precondition,
    bool IsSatisfied,
    ConstraintStrength Strength,
    string? FailureReason,
    IReadOnlyDictionary<string, object?>? ExpectedShape);
