using System;

namespace Semantic.Abstractions;

/// <summary>
/// Marks a declaration as a semantic value object. Semantic.Generator discovers
/// these declarations and emits a companion type; the semantic tracks extend
/// the emission.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class SemanticValueAttribute : Attribute
{
}
