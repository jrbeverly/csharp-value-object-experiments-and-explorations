using Semantic.Abstractions;

namespace Sample.Tests;

public class GenerationSmokeTests
{
    [Fact]
    public void Generator_emits_companion_type_when_attribute_is_present()
    {
        // StubValueSemantic only exists if Semantic.Generator ran over the
        // [SemanticValue] StubValue declaration in Sample at compile time.
        var value = StubValueSemantic.Instance;

        Assert.NotNull(value);
        Assert.IsAssignableFrom<ISemanticValue>(value);
        Assert.True(value.Validate().IsValid);
    }
}
