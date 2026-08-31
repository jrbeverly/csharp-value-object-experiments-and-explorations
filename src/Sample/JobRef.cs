using Semantic.Abstractions;

namespace Sample;

/// <summary>
/// A composite symbolic value: a three-part job reference rendered as
/// <c>prefix::name::suffix</c> (e.g. <c>deploy::api::east</c>). The
/// [CompositeValue] declaration names the delimiter, the components, and the
/// per-component constraint; Semantic.Generator completes this partial struct
/// with parsing, formatting, equality, deconstruction, named accessors, and
/// serialization.
/// </summary>
[CompositeValue("::", new string[] { "Prefix", "Name", "Suffix" }, CompositeConstraint.NonEmpty)]
public readonly partial struct JobRef
{
}
