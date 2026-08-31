using System;

namespace Semantic.Abstractions;

/// <summary>
/// Kinds of per-component constraints a composite value can declare. This track
/// proves one kind; length-, pattern-, and set-based variants are deferred to
/// later tracks.
/// </summary>
public enum CompositeConstraint
{
    /// <summary>Each component must be non-empty.</summary>
    NonEmpty,
}

/// <summary>
/// Declares a composite symbolic value: a delimiter, the names of its
/// components, and the constraint every component must satisfy. The canonical
/// form renders the components joined by the delimiter, e.g.
/// <c>prefix::name::suffix</c>. Semantic.Generator completes the annotated
/// partial struct with parsing, formatting, equality, deconstruction, named
/// accessors, and serialization. Invalid declarations (zero components, an
/// empty delimiter) are reported at build time (VO004).
/// </summary>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class CompositeValueAttribute : Attribute
{
    public CompositeValueAttribute(string delimiter, string[] componentNames, CompositeConstraint constraint)
    {
        Delimiter = delimiter;
        ComponentNames = componentNames;
        Constraint = constraint;
    }

    /// <summary>The separator between components in the canonical form, e.g. "::".</summary>
    public string Delimiter { get; }

    /// <summary>The component names in order, e.g. ["Prefix", "Name", "Suffix"].</summary>
    public string[] ComponentNames { get; }

    /// <summary>The constraint enforced on every component at construction and parse boundaries.</summary>
    public CompositeConstraint Constraint { get; }
}
