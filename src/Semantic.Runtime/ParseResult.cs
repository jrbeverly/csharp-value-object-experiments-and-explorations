namespace Semantic.Runtime;

/// <summary>
/// Outcome of parsing a raw textual representation into a semantic value.
/// </summary>
public readonly record struct ParseResult<T>(bool IsSuccess, T? Value, string? Error)
{
    public static ParseResult<T> Success(T value) => new(true, value, null);

    public static ParseResult<T> Failure(string error) => new(false, default, error);
}
