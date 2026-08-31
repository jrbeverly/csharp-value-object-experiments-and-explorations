using System;

namespace Semantic.Abstractions;

/// <summary>
/// Declares a localized message with a named-placeholder template and typed parameter
/// declarations. Semantic.Generator emits a typed <c>Render</c> accessor on the annotated
/// partial class; a template/parameter mismatch is reported at build time (VO004).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class LocalizedMessageAttribute : Attribute
{
    public LocalizedMessageAttribute(string template, string[] parameters)
    {
        Template = template;
        Parameters = parameters;
    }

    /// <summary>Template with named placeholders, e.g. <c>"Hello, {name}!"</c>.</summary>
    public string Template { get; }

    /// <summary>
    /// Parameter declarations as <c>"type:name"</c> strings in placeholder order,
    /// e.g. <c>"string:name"</c>, <c>"decimal:amount"</c>.
    /// </summary>
    public string[] Parameters { get; }
}
