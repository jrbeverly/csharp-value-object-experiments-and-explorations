using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Semantic.Generator;

/// <summary>
/// Discovers declarations annotated with <c>[CompositeValue]</c> and completes
/// them with parse/format round-trip, equality, deconstruction, named
/// accessors, and JSON serialization. Invalid declarations (zero components,
/// an empty delimiter, a non-partial struct) are reported at build time
/// (VO004).
/// </summary>
[Generator]
public sealed class CompositeGenerator : IIncrementalGenerator
{
    private const string AttributeMetadataName = "Semantic.Abstractions.CompositeValueAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeMetadataName,
            predicate: static (node, _) => node is StructDeclarationSyntax,
            transform: static (ctx, _) => ToModel(ctx));

        context.RegisterSourceOutput(declarations, static (spc, model) => EmitComposite(spc, model));
    }

    private static CompositeModel? ToModel(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol type) return null;
        if (ctx.TargetNode is not StructDeclarationSyntax syntax) return null;

        var attribute = ctx.Attributes.FirstOrDefault();
        if (attribute is null || attribute.ConstructorArguments.Length != 3) return null;

        // Argument 0 is the delimiter string; argument 1 is the component name
        // array; argument 2 is a CompositeConstraint constant, boxed as the
        // enum instance or its underlying int.
        var delimiter = attribute.ConstructorArguments[0].Value as string ?? string.Empty;
        var components = attribute.ConstructorArguments[1].Values
            .Where(static v => v.Value is string)
            .Select(static v => (string)v.Value!)
            .ToArray();

        var ns = type.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : type.ContainingNamespace.ToDisplayString();

        // Mirror the declaration's modifiers (including 'readonly') so the
        // generated part stays consistent with the authored part.
        var modifiers = string.Join(" ", syntax.Modifiers.Select(m => m.Text));
        var isPartial = syntax.Modifiers.Any(SyntaxKind.PartialKeyword);

        return new CompositeModel(type.Name, ns, modifiers, syntax.GetLocation(), delimiter, components, isPartial);
    }

    private static void EmitComposite(SourceProductionContext spc, CompositeModel? model)
    {
        if (model is null) return;

        var reason = InvalidReason(model);
        if (reason is not null)
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.InvalidCompositeDeclaration,
                model.Location,
                model.Name,
                reason));
            return;
        }

        spc.AddSource($"{model.Name}.Composite.g.cs", EmitCompositeType(model));
        spc.AddSource($"{model.Name}JsonConverter.g.cs", EmitJsonConverter(model));
    }

    private static string? InvalidReason(CompositeModel model)
    {
        if (model.Components.Length == 0) return "must declare at least one component";
        if (model.Components.Any(static c => c.Length == 0)) return "must declare non-empty component names";
        if (model.Delimiter.Length == 0) return "must declare a non-empty delimiter";
        if (!model.IsPartial) return "must be a partial struct so the generator can complete it";
        return null;
    }

    private static string EmitCompositeType(CompositeModel model)
    {
        var name = model.Name;
        var ns = model.Namespace.Length > 0 ? $"namespace {model.Namespace};\n\n" : string.Empty;

        var parameters = model.Components.Select(Camel).ToArray();
        var fields = parameters.Select(p => "_" + p).ToArray();
        var parameterList = string.Join(", ", parameters.Select(p => $"string {p}"));
        var constructorArguments = string.Join(", ", Enumerable.Range(0, model.Components.Length).Select(i => $"parts[{i}]"));
        var validatedAssignments = string.Join("\n", parameters.Select(p => $"        _{p} = Validate({p}, nameof({p}));"));
        var accessors = string.Join("\n\n", model.Components.Select((c, i) =>
            $"    /// <summary>Gets the {parameters[i]} component.</summary>\n    public string {c} => _{parameters[i]};"));
        var deconstructParameters = string.Join(", ", parameters.Select(p => $"out string {p}"));
        var deconstructAssignments = string.Join("\n", parameters.Select(p => $"        {p} = _{p};"));
        var canonicalForm = string.Join(model.Delimiter, fields.Select(f => $"{{{f}}}"));
        var equalityBody = string.Join("\n        && ", fields.Select(f => $"{f} == other.{f}"));
        var hashArguments = string.Join(", ", fields);
        var expected = string.Join(model.Delimiter, parameters);

        return $@"// <auto-generated/>
#nullable enable

{ns}/// <summary>
/// Composite symbolic value emitted by Semantic.Generator for the
/// <c>[CompositeValue]</c> declaration <c>{name}</c>. Components are validated
/// at construction and parse boundaries for runtime inputs — it is not a
/// compile-time guarantee, and a default instance bypasses the check.
/// </summary>
[global::System.Text.Json.Serialization.JsonConverter(typeof({name}JsonConverter))]
{model.Modifiers} struct {name} :
    global::System.IEquatable<{name}>,
    global::Semantic.Abstractions.ISemanticValue
{{
    /// <summary>The delimiter separating components in the canonical form.</summary>
    public const string Delimiter = ""{model.Delimiter}"";

    private readonly string {string.Join(";\n    private readonly string ", fields)};

{accessors}

    /// <summary>
    /// True when <paramref name=""text""/> splits into {model.Components.Length}
    /// non-empty components, so it parses to a <see cref=""{name}""/>.
    /// </summary>
    public static bool IsValid(string? text) => TryParse(text, out _);

    /// <summary>Constructs a <see cref=""{name}""/> from its components.</summary>
    /// <exception cref=""global::System.ArgumentException"">
    /// A component is empty or contains the delimiter.
    /// </exception>
    public {name}({parameterList})
    {{
{validatedAssignments}
    }}

    private static string Validate(string component, string componentName)
    {{
        if (string.IsNullOrEmpty(component))
        {{
            throw new global::System.ArgumentException(
                $""{{componentName}} must be a non-empty component of {name}."",
                componentName);
        }}

        if (component.Contains(Delimiter))
        {{
            throw new global::System.ArgumentException(
                $""{{componentName}} must not contain the delimiter '{model.Delimiter}'."",
                componentName);
        }}

        return component;
    }}

    /// <summary>
    /// Tries to parse the canonical form <c>{expected}</c> into a
    /// <see cref=""{name}""/>. Fails when the component count differs or a
    /// component is empty.
    /// </summary>
    public static bool TryParse(string? text, out {name} value)
    {{
        if (text is not null)
        {{
            var parts = text.Split(new[] {{ Delimiter }}, global::System.StringSplitOptions.None);
            if (parts.Length == {model.Components.Length})
            {{
                for (var i = 0; i < parts.Length; i++)
                {{
                    if (parts[i].Length == 0)
                    {{
                        value = default;
                        return false;
                    }}
                }}

                value = new {name}({constructorArguments});
                return true;
            }}
        }}

        value = default;
        return false;
    }}

    /// <summary>Parses the canonical form into a <see cref=""{name}""/>.</summary>
    public static global::Semantic.Runtime.ParseResult<{name}> Parse(string? text) =>
        TryParse(text, out var value)
            ? global::Semantic.Runtime.ParseResult<{name}>.Success(value)
            : global::Semantic.Runtime.ParseResult<{name}>.Failure(
                $""'{{text}}' is not a valid {name} (expected {expected})."");

    /// <summary>Deconstructs into the named components.</summary>
    public void Deconstruct({deconstructParameters})
    {{
{deconstructAssignments}
    }}

    /// <summary>Renders the canonical form, which parses back to an equal value.</summary>
    public override string ToString() => $""{canonicalForm}"";

    public bool Equals({name} other) =>
        {equalityBody};

    public override bool Equals(object? obj) => obj is {name} other && Equals(other);

    public override int GetHashCode() =>
        global::System.HashCode.Combine({hashArguments});

    public static bool operator ==({name} left, {name} right) => left.Equals(right);

    public static bool operator !=({name} left, {name} right) => !left.Equals(right);
}}
";
    }

    private static string EmitJsonConverter(CompositeModel model)
    {
        var name = model.Name;
        var ns = model.Namespace.Length > 0 ? $"namespace {model.Namespace};\n\n" : string.Empty;

        return $@"// <auto-generated/>
#nullable enable

{ns}/// <summary>
/// JSON converter emitted by Semantic.Generator: a <see cref=""{name}""/> is
/// serialized as its canonical string form.
/// </summary>
public sealed class {name}JsonConverter : global::System.Text.Json.Serialization.JsonConverter<{name}>
{{
    public override {name} Read(
        ref global::System.Text.Json.Utf8JsonReader reader,
        global::System.Type typeToConvert,
        global::System.Text.Json.JsonSerializerOptions options)
    {{
        var text = reader.GetString();
        if ({name}.TryParse(text, out var value))
        {{
            return value;
        }}

        throw new global::System.Text.Json.JsonException($""'{{text}}' is not a valid {name}."");
    }}

    public override void Write(
        global::System.Text.Json.Utf8JsonWriter writer,
        {name} value,
        global::System.Text.Json.JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}}
";
    }

    private static string Camel(string name) => char.ToLowerInvariant(name[0]) + name.Substring(1);

    private sealed class CompositeModel
    {
        public CompositeModel(
            string name,
            string ns,
            string modifiers,
            Location location,
            string delimiter,
            string[] components,
            bool isPartial)
        {
            Name = name;
            Namespace = ns;
            Modifiers = modifiers;
            Location = location;
            Delimiter = delimiter;
            Components = components;
            IsPartial = isPartial;
        }

        public string Name { get; }

        public string Namespace { get; }

        public string Modifiers { get; }

        public Location Location { get; }

        public string Delimiter { get; }

        public string[] Components { get; }

        public bool IsPartial { get; }
    }
}
