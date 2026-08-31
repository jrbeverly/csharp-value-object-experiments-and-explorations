using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Semantic.Generator;

namespace Sample.Tests;

/// <summary>
/// Drives CompositeGenerator over in-memory compilations to prove the VO004
/// build-time diagnostic without polluting the Sample project with deliberately
/// broken declarations.
/// </summary>
public class CompositeDiagnosticsTests
{
    private const string AbstractionsSource = """
        using System;

        namespace Semantic.Abstractions;

        public enum CompositeConstraint
        {
            NonEmpty,
        }

        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
        public sealed class CompositeValueAttribute : Attribute
        {
            public CompositeValueAttribute(string delimiter, string[] componentNames, CompositeConstraint constraint)
            {
            }
        }
        """;

    [Fact]
    public void Valid_declaration_emits_the_composite_type()
    {
        var source = """
            namespace TestLib;

            [Semantic.Abstractions.CompositeValue("::", new string[] { "Prefix", "Name", "Suffix" }, Semantic.Abstractions.CompositeConstraint.NonEmpty)]
            public readonly partial struct JobRef
            {
            }
            """;

        var result = RunGenerator(source);

        Assert.DoesNotContain(result.Diagnostics, d => d.Id == "VO004");
        var generated = Assert.Single(
            result.GeneratedTrees,
            t => t.FilePath.Contains("JobRef.Composite.g.cs"));
        Assert.Contains("public JobRef(string prefix, string name, string suffix)", generated.ToString());
        Assert.Contains("public void Deconstruct(out string prefix, out string name, out string suffix)", generated.ToString());
        Assert.Single(result.GeneratedTrees, t => t.FilePath.Contains("JobRefJsonConverter.g.cs"));
    }

    [Fact]
    public void Zero_components_reports_VO004()
    {
        var source = """
            namespace TestLib;

            [Semantic.Abstractions.CompositeValue("::", new string[0], Semantic.Abstractions.CompositeConstraint.NonEmpty)]
            public readonly partial struct Empty
            {
            }
            """;

        var result = RunGenerator(source);

        var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == "VO004");
        Assert.Contains("Empty", diagnostic.GetMessage());
        Assert.Contains("at least one component", diagnostic.GetMessage());
        Assert.DoesNotContain(result.GeneratedTrees, t => t.FilePath.Contains("Empty"));
    }

    [Fact]
    public void Empty_component_name_reports_VO004()
    {
        var source = """
            namespace TestLib;

            [Semantic.Abstractions.CompositeValue("::", new string[] { "" }, Semantic.Abstractions.CompositeConstraint.NonEmpty)]
            public readonly partial struct Nameless
            {
            }
            """;

        var result = RunGenerator(source);

        var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == "VO004");
        Assert.Contains("Nameless", diagnostic.GetMessage());
        Assert.Contains("component names", diagnostic.GetMessage());
    }

    [Fact]
    public void Empty_delimiter_reports_VO004()
    {
        var source = """
            namespace TestLib;

            [Semantic.Abstractions.CompositeValue("", new string[] { "Prefix", "Name" }, Semantic.Abstractions.CompositeConstraint.NonEmpty)]
            public readonly partial struct NoDelimiter
            {
            }
            """;

        var result = RunGenerator(source);

        var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == "VO004");
        Assert.Contains("NoDelimiter", diagnostic.GetMessage());
        Assert.Contains("delimiter", diagnostic.GetMessage());
    }

    [Fact]
    public void Non_partial_declaration_reports_VO004()
    {
        var source = """
            namespace TestLib;

            [Semantic.Abstractions.CompositeValue("::", new string[] { "Prefix", "Name" }, Semantic.Abstractions.CompositeConstraint.NonEmpty)]
            public struct NotPartial
            {
            }
            """;

        var result = RunGenerator(source);

        var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == "VO004");
        Assert.Contains("NotPartial", diagnostic.GetMessage());
        Assert.Contains("partial", diagnostic.GetMessage());
    }

    private static GeneratorDriverRunResult RunGenerator(string testSource)
    {
        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(AbstractionsSource),
            CSharpSyntaxTree.ParseText(testSource)
        };

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(
                Path.Combine(
                    Path.GetDirectoryName(typeof(object).Assembly.Location)!,
                    "System.Runtime.dll"))
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new CompositeGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()]);

        driver = driver.RunGenerators(compilation);
        return driver.GetRunResult();
    }
}
