using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Semantic.Generator;

namespace Sample.Tests;

public class LocalizationTests
{
    [Fact]
    public void Typed_accessor_renders_with_culture_applied()
    {
        // French uses comma as decimal separator; English uses period.
        // Both calls use the same typed parameters — a wrong type is a compile error.
        var fr = GreetingMessage.Render(name: "Alice", score: 9.5m, culture: new CultureInfo("fr-FR"));
        var en = GreetingMessage.Render(name: "Alice", score: 9.5m, culture: new CultureInfo("en-US"));

        Assert.Equal("Hello, Alice! Score: 9,5", fr);
        Assert.Equal("Hello, Alice! Score: 9.5", en);
    }

    [Fact]
    public void Typed_accessor_falls_back_to_current_ui_culture_when_none_supplied()
    {
        var result = GreetingMessage.Render(name: "Bob", score: 1.0m);

        Assert.Contains("Bob", result);
    }

    [Fact]
    public void Mismatched_placeholder_reports_VO004_and_suppresses_emission()
    {
        // Template has {name} but the only parameter is declared as "string:other" — mismatch on both sides.
        var abstractionsSource = """
            using System;

            namespace Semantic.Abstractions;

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public sealed class LocalizedMessageAttribute : Attribute
            {
                public LocalizedMessageAttribute(string template, string[] parameters) { }
            }
            """;

        var source = """
            [Semantic.Abstractions.LocalizedMessage(
                "Hello, {name}!",
                new string[] { "string:other" }
            )]
            public partial class MismatchedMessage { }
            """;

        var result = RunGenerator(abstractionsSource, source);

        Assert.Contains(result.Diagnostics, d => d.Id == "VO004");
        Assert.DoesNotContain(result.GeneratedTrees, t => t.FilePath.Contains("MismatchedMessage.Localization.g.cs"));
    }

    // Negative compile-time evidence — these lines do not compile:
    //
    //   GreetingMessage.Render(name: 42, score: 9.5m);
    //   // error CS1503: Argument 1: cannot convert from 'int' to 'string'
    //
    //   GreetingMessage.Render("Alice");
    //   // error CS7036: There is no argument given that corresponds to the required parameter 'score'

    private static GeneratorDriverRunResult RunGenerator(string abstractionsSource, string testSource)
    {
        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(abstractionsSource),
            CSharpSyntaxTree.ParseText(testSource),
        };

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(
                Path.Combine(
                    Path.GetDirectoryName(typeof(object).Assembly.Location)!,
                    "System.Runtime.dll")),
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new LocalizationGenerator().AsSourceGenerator()]);

        driver = driver.RunGenerators(compilation);
        return driver.GetRunResult();
    }
}
