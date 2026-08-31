using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Semantic.Generator;

namespace Sample.Tests;

/// <summary>
/// Drives SemanticGenerator over in-memory compilations to prove the build-time
/// diagnostics of the constrained scalar track without polluting the Sample
/// project with deliberately broken declarations.
/// </summary>
public class ConstrainedScalarDiagnosticsTests
{
    private const string AbstractionsSource = """
        using System;

        namespace Semantic.Abstractions;

        public enum ScalarConstraint
        {
            Positive,
        }

        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
        public sealed class ConstrainedScalarAttribute : Attribute
        {
            public ConstrainedScalarAttribute(Type underlyingType, ScalarConstraint constraint)
            {
            }
        }
        """;

    [Fact]
    public void Supported_declaration_emits_the_scalar_type()
    {
        var source = """
            namespace TestLib;

            [Semantic.Abstractions.ConstrainedScalar(typeof(int), Semantic.Abstractions.ScalarConstraint.Positive)]
            public readonly partial struct PositiveAmount
            {
            }
            """;

        var result = RunGenerator(AbstractionsSource, source);

        Assert.DoesNotContain(result.Diagnostics, d => d.Id == "VO002" || d.Id == "VO003");
        var generated = Assert.Single(
            result.GeneratedTrees,
            t => t.FilePath.Contains("PositiveAmount.Scalar.g.cs"));
        Assert.Contains("public PositiveAmount(int value)", generated.ToString());
    }

    [Fact]
    public void Unsupported_underlying_type_reports_VO002()
    {
        var source = """
            namespace TestLib;

            [Semantic.Abstractions.ConstrainedScalar(typeof(double), Semantic.Abstractions.ScalarConstraint.Positive)]
            public readonly partial struct BadAmount
            {
            }
            """;

        var result = RunGenerator(AbstractionsSource, source);

        var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == "VO002");
        Assert.Contains("BadAmount", diagnostic.GetMessage());
        Assert.Contains("double", diagnostic.GetMessage());
        Assert.DoesNotContain(result.GeneratedTrees, t => t.FilePath.Contains("BadAmount"));
    }

    [Fact]
    public void Non_partial_declaration_reports_VO003()
    {
        var source = """
            namespace TestLib;

            [Semantic.Abstractions.ConstrainedScalar(typeof(int), Semantic.Abstractions.ScalarConstraint.Positive)]
            public struct NotPartial
            {
            }
            """;

        var result = RunGenerator(AbstractionsSource, source);

        var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == "VO003");
        Assert.Contains("NotPartial", diagnostic.GetMessage());
        Assert.DoesNotContain(result.GeneratedTrees, t => t.FilePath.Contains("NotPartial"));
    }

    private static GeneratorDriverRunResult RunGenerator(string abstractionsSource, string testSource)
    {
        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(abstractionsSource),
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

        var generator = new SemanticGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()]);

        driver = driver.RunGenerators(compilation);
        return driver.GetRunResult();
    }
}
