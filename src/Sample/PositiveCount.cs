using Semantic.Abstractions;

namespace Sample;

/// <summary>
/// A constrained scalar: a count that must be a positive integer. The
/// [ConstrainedScalar] declaration names the underlying primitive and the
/// constraint; Semantic.Generator completes this partial struct with
/// construction-time validation, parsing, formatting, and equality.
/// </summary>
[ConstrainedScalar(typeof(int), ScalarConstraint.Positive)]
public readonly partial struct PositiveCount
{
}
