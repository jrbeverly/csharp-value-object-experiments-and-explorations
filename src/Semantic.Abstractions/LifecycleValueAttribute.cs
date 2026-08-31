using System;

namespace Semantic.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class LifecycleValueAttribute : Attribute
{
    public LifecycleValueAttribute(string[] states, string[] transitions)
    {
        States = states;
        Transitions = transitions;
    }

    /// <summary>Ordered state names, e.g. ["Metadata", "Open", "Closed"].</summary>
    public string[] States { get; }

    /// <summary>
    /// Transitions in "From:Method:To" form, e.g. "Metadata:Open:Open".
    /// The generator emits Method() on the From state returning the To state type.
    /// </summary>
    public string[] Transitions { get; }
}
