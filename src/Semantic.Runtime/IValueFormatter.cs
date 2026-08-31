namespace Semantic.Runtime;

/// <summary>
/// Formatting contract consumed by generated formatting infrastructure.
/// </summary>
public interface IValueFormatter<in T>
{
    string Format(T value, string? format);
}
