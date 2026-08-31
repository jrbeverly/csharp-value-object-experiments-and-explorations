namespace Semantic.Runtime;

/// <summary>
/// Outcome of validating a raw value at a construction boundary.
/// </summary>
public readonly record struct ValidationResult(bool IsValid, string? Error)
{
    public static ValidationResult Success() => new(true, null);

    public static ValidationResult Failure(string error) => new(false, error);
}
