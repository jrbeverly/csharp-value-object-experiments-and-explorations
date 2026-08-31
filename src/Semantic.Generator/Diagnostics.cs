using Microsoft.CodeAnalysis;

namespace Semantic.Generator;

/// <summary>
/// Build-time diagnostic descriptors for the semantic value platform. Codes are
/// reserved as VO0xx; the semantic tracks populate real usage.
/// </summary>
internal static class DiagnosticDescriptors
{
    public const string Category = "SemanticValueGeneration";

    /// <summary>VO001: a declaration cannot be processed as a semantic value.</summary>
    public static readonly DiagnosticDescriptor InvalidDeclaration = new(
        id: "VO001",
        title: "Invalid semantic value declaration",
        messageFormat: "The declaration '{0}' is not a valid semantic value declaration",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>VO002: a constrained scalar declares an underlying type the track cannot emit for.</summary>
    public static readonly DiagnosticDescriptor UnsupportedUnderlyingType = new(
        id: "VO002",
        title: "Unsupported constrained scalar underlying type",
        messageFormat: "Constrained scalar '{0}' declares the unsupported underlying type '{1}'; only 'int' is supported",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>VO003: a constrained scalar declaration is not partial, so the generator cannot complete it.</summary>
    public static readonly DiagnosticDescriptor NotPartialDeclaration = new(
        id: "VO003",
        title: "Constrained scalar declaration must be partial",
        messageFormat: "Constrained scalar declaration '{0}' must be a partial struct so the generator can complete it",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>VO004: a composite declaration is invalid, so the generator cannot complete it.</summary>
    public static readonly DiagnosticDescriptor InvalidCompositeDeclaration = new(
        id: "VO004",
        title: "Invalid composite value declaration",
        messageFormat: "Composite declaration '{0}' is invalid: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>VO004: a localized message has a placeholder that has no matching parameter, or vice versa.</summary>
    public static readonly DiagnosticDescriptor LocalizationParameterMismatch = new(
        id: "VO004",
        title: "Localized message template/parameter mismatch",
        messageFormat: "Localized message '{0}' has a template/parameter mismatch: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
