using System;

namespace Semantic.Abstractions;

/// <summary>
/// Kinds of constraints a constrained scalar can declare. This track proves one
/// kind; natural numbers, URL-safe, and prefix-constrained variants are
/// deferred to later tracks.
/// </summary>
public enum ScalarConstraint
{
    /// <summary>The underlying value must be greater than zero.</summary>
    Positive,
}

/// <summary>
/// Declares a constrained scalar value type: an underlying primitive plus the
/// constraint it must satisfy. Semantic.Generator completes the annotated
/// partial struct with construction-time validation, parsing, formatting, and
/// equality. Declarations with an unsupported underlying type are reported at
/// build time (VO002).
/// </summary>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class ConstrainedScalarAttribute : Attribute
{
    public ConstrainedScalarAttribute(Type underlyingType, ScalarConstraint constraint)
    {
        UnderlyingType = underlyingType;
        Constraint = constraint;
    }

    /// <summary>The primitive the scalar wraps (only <see cref="int"/> is supported).</summary>
    public Type UnderlyingType { get; }

    /// <summary>The constraint enforced at construction and parse boundaries.</summary>
    public ScalarConstraint Constraint { get; }
}
