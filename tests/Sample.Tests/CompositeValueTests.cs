using System.Text.Json;
using Semantic.Abstractions;

namespace Sample.Tests;

public class CompositeValueTests
{
    [Fact]
    public void Valid_components_construct_and_expose_named_accessors()
    {
        var value = new JobRef("deploy", "api", "east");

        Assert.Equal("deploy", value.Prefix);
        Assert.Equal("api", value.Name);
        Assert.Equal("east", value.Suffix);
        Assert.IsAssignableFrom<ISemanticValue>(value);
    }

    [Fact]
    public void Parse_round_trips_the_canonical_form()
    {
        var result = JobRef.Parse("deploy::api::east");

        Assert.True(result.IsSuccess);
        Assert.Equal(new JobRef("deploy", "api", "east"), result.Value);
        Assert.Equal("deploy::api::east", result.Value!.ToString());
        Assert.True(JobRef.TryParse("deploy::api::east", out var roundTripped));
        Assert.Equal(new JobRef("deploy", "api", "east"), roundTripped);
        Assert.True(JobRef.IsValid("deploy::api::east"));
    }

    [Theory]
    [InlineData("deploy::api")] // too few components
    [InlineData("a::b::c::d")] // too many components
    [InlineData("")] // no components
    [InlineData("::api::east")] // empty first component
    [InlineData("deploy::::east")] // empty middle component
    [InlineData("deploy::api::")] // empty last component
    public void Parse_rejects_malformed_text(string text)
    {
        var result = JobRef.Parse(text);

        Assert.False(result.IsSuccess);
        Assert.Contains("JobRef", result.Error);
        Assert.False(JobRef.TryParse(text, out var failed));
        Assert.Equal(default, failed);
        Assert.False(JobRef.IsValid(text));
    }

    [Fact]
    public void Parse_rejects_null()
    {
        Assert.False(JobRef.Parse(null).IsSuccess);
    }

    [Fact]
    public void Empty_component_is_rejected_at_construction()
    {
        // The guarantee boundary: a malformed component compiles fine as a
        // string — only the constructor rejects it.
        var exception = Assert.Throws<ArgumentException>(
            () => new JobRef("", "api", "east"));

        Assert.Equal("prefix", exception.ParamName);
    }

    [Fact]
    public void Component_containing_the_delimiter_is_rejected_at_construction()
    {
        // Structural rule: a component may not contain the delimiter, or the
        // canonical form becomes ambiguous and no longer round-trips.
        var exception = Assert.Throws<ArgumentException>(
            () => new JobRef("de::ploy", "api", "east"));

        Assert.Equal("prefix", exception.ParamName);
    }

    [Fact]
    public void Equality_is_value_semantic()
    {
        var a = new JobRef("deploy", "api", "east");
        var b = new JobRef("deploy", "api", "east");
        var c = new JobRef("deploy", "api", "west");

        Assert.True(a == b);
        Assert.False(a != b);
        Assert.False(a == c);
        Assert.True(a != c);
        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
        Assert.False(a.Equals("deploy::api::east"));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Deconstruct_exposes_the_components()
    {
        var (prefix, name, suffix) = new JobRef("deploy", "api", "east");

        Assert.Equal("deploy", prefix);
        Assert.Equal("api", name);
        Assert.Equal("east", suffix);
    }

    [Fact]
    public void Serialization_round_trips_as_a_string()
    {
        var value = new JobRef("deploy", "api", "east");

        var json = JsonSerializer.Serialize(value);

        Assert.Equal("\"deploy::api::east\"", json);
        Assert.Equal(value, JsonSerializer.Deserialize<JobRef>(json));
    }

    [Fact]
    public void Deserialization_rejects_malformed_text()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<JobRef>("\"deploy::api\""));

        Assert.Contains("JobRef", exception.Message);
    }
}
