using Semantic.Abstractions;
using Semantic.Runtime;

namespace Sample.Tests;

public class ConstrainedScalarTests
{
    [Fact]
    public void Positive_value_constructs_and_exposes_the_value()
    {
        var count = new PositiveCount(42);

        Assert.Equal(42, count.Value);
        Assert.IsAssignableFrom<ISemanticValue>(count);
    }

    [Fact]
    public void Boundary_value_one_is_accepted()
    {
        var count = new PositiveCount(1);

        Assert.Equal(1, count.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Non_positive_value_is_rejected_at_construction(int candidate)
    {
        // The guarantee boundary: PositiveCount is enforced at construction
        // time, not compile time. A runtime input like 0 or a negative number
        // compiles fine as an int — only the constructor rejects it.
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new PositiveCount(candidate));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Parse_round_trips_a_positive_value()
    {
        var result = PositiveCount.Parse("42");

        Assert.True(result.IsSuccess);
        Assert.Equal(new PositiveCount(42), result.Value);
        Assert.Equal("42", result.Value!.Value.ToString());
        Assert.True(PositiveCount.TryParse("42", out var roundTripped));
        Assert.Equal(new PositiveCount(42), roundTripped);
    }

    [Fact]
    public void Parse_rejects_non_positive_and_non_numeric_text()
    {
        var zero = PositiveCount.Parse("0");
        Assert.False(zero.IsSuccess);
        Assert.Contains("positive", zero.Error);

        Assert.False(PositiveCount.Parse("-5").IsSuccess);
        Assert.False(PositiveCount.Parse("abc").IsSuccess);

        Assert.False(PositiveCount.TryParse("0", out var failed));
        Assert.Equal(default, failed);
    }

    [Fact]
    public void Equality_is_value_semantic()
    {
        var a = new PositiveCount(7);
        var b = new PositiveCount(7);
        var c = new PositiveCount(8);

        Assert.True(a == b);
        Assert.False(a != b);
        Assert.False(a == c);
        Assert.True(a != c);
        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
        Assert.False(a.Equals("7"));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Default_instance_skips_construction_validation()
    {
        // Value types can be default-initialized without running the
        // constructor, so the invariant holds only for constructed instances.
        // This is the boundary construction-time validation cannot cross.
        PositiveCount count = default;

        Assert.False(PositiveCount.IsValid(count.Value));
    }
}
