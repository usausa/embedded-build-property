namespace BunnyTail.EmbeddedBuildProperty.Generator;

using System;
using System.Collections.Immutable;
using System.Text;

using BunnyTail.EmbeddedBuildProperty.Generator.Helpers;
using BunnyTail.EmbeddedBuildProperty.Generator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

[Generator]
public sealed class BuildPropertyGenerator : IIncrementalGenerator
{
    private const string AttributeName = "BunnyTail.EmbeddedBuildProperty.BuildPropertyAttribute";

    // ------------------------------------------------------------
    // Initialize
    // ------------------------------------------------------------

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var valueProvider = context.AnalyzerConfigOptionsProvider
            .Select(SelectBuildProperty);

        var propertyProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeName,
                static (syntax, _) => IsTargetSyntax(syntax),
                static (context, _) => GetPropertyModel(context))
            .Collect();

        context.RegisterImplementationSourceOutput(
            valueProvider.Combine(propertyProvider),
            static (context, provider) => Execute(context, provider.Left, provider.Right));
    }

    // ------------------------------------------------------------
    // Parser
    // ------------------------------------------------------------

    private static EquatableArray<BuildProperty> SelectBuildProperty(AnalyzerConfigOptionsProvider provider, CancellationToken token)
    {
        var list = new List<BuildProperty>();

        if (provider.GlobalOptions.TryGetValue("build_property.EmbeddedBuildProperty", out var values))
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var entry in values.Split(','))
            {
                var index = entry.IndexOf('=');
                if (index > 0)
                {
                    var key = entry.Substring(0, index).Trim();
                    var value = entry.Substring(index + 1).Trim();
                    list.Add(new BuildProperty(key, value));
                }
            }
        }

        return new EquatableArray<BuildProperty>(list.ToArray());
    }

    private static bool IsTargetSyntax(SyntaxNode syntax) =>
        syntax is PropertyDeclarationSyntax;

    private static Result<PropertyModel> GetPropertyModel(GeneratorAttributeSyntaxContext context)
    {
        var syntax = (PropertyDeclarationSyntax)context.TargetNode;

        if (context.SemanticModel.GetDeclaredSymbol(syntax) is not IPropertySymbol symbol)
        {
            return Results.Error<PropertyModel>(null);
        }

        // Validate property definition
        if (!symbol.IsStatic || !symbol.IsPartialDefinition || (symbol.GetMethod is null))
        {
            return Results.Error<PropertyModel>(new DiagnosticInfo(Diagnostics.InvalidPropertyDefinition, syntax.GetLocation(), symbol.Name));
        }

        // Validate property type
        var returnType = symbol.GetMethod.ReturnType.ToDisplayString();
        if (!IsFormatSupported(returnType))
        {
            return Results.Error<PropertyModel>(new DiagnosticInfo(Diagnostics.UnsupportedPropertyType, syntax.GetLocation(), returnType));
        }

        var containingType = symbol.ContainingType;
        var ns = String.IsNullOrEmpty(containingType.ContainingNamespace.Name)
            ? string.Empty
            : containingType.ContainingNamespace.ToDisplayString();
        var attribute = symbol.GetAttributes().First(static x => x.AttributeClass!.ToDisplayString() == AttributeName);
        var propertyName = (attribute.ConstructorArguments.Length > 0) && (attribute.ConstructorArguments[0].Value is string value)
            ? value
            : symbol.Name;

        return Results.Success(new PropertyModel(
            ns,
            containingType.GetClassName(),
            containingType.IsValueType,
            symbol.DeclaredAccessibility,
            returnType,
            symbol.Name,
            propertyName));
    }

    // ------------------------------------------------------------
    // Generator
    // ------------------------------------------------------------

    private static void Execute(SourceProductionContext context, EquatableArray<BuildProperty> values, ImmutableArray<Result<PropertyModel>> properties)
    {
        foreach (var info in properties.SelectPart(static x => x.Error))
        {
            context.ReportDiagnostic(info);
        }

        var valueMap = values.ToArray().ToDictionary(static x => x.Key, static x => x.Value);

        var builder = new SourceBuilder();
        foreach (var group in properties.SelectPart(static x => x.Value).GroupBy(static x => new { x.Namespace, x.ClassName }))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            builder.Clear();
            BuildSource(builder, valueMap, group.ToList());

            var filename = MakeFilename(group.Key.Namespace, group.Key.ClassName);
            var source = builder.ToString();
            context.AddSource(filename, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static void BuildSource(SourceBuilder builder, Dictionary<string, string> values, List<PropertyModel> properties)
    {
        var ns = properties[0].Namespace;
        var className = properties[0].ClassName;
        var isValueType = properties[0].IsValueType;

        builder.AutoGenerated();
        builder.EnableNullable();
        builder.NewLine();

        // namespace
        if (!String.IsNullOrEmpty(ns))
        {
            builder.Namespace(ns);
            builder.NewLine();
        }

        // class
        builder
            .Indent()
            .Append("partial ")
            .Append(isValueType ? "struct " : "class ")
            .Append(className).
            NewLine();
        builder.BeginScope();

        var first = true;
        foreach (var property in properties)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.NewLine();
            }

            builder
                .Indent()
                .Append(property.MethodAccessibility.ToText())
                .Append(" static partial ")
                .Append(property.PropertyType)
                .Append(' ')
                .Append(property.PropertyName)
                .Append(" => ");
            if (values.TryGetValue(property.PropertyKey, out var value))
            {
                var formatter = GetFormatter(property.PropertyType);
                builder
                    .Append(formatter(value))
                    .Append(';');
            }
            else
            {
                builder.Append("default!;");
            }
            builder.NewLine();
        }

        builder.EndScope();
    }

    // ------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------

    private static readonly Dictionary<string, Func<string, string>> Formatters = new()
    {
        { "string", FormatString },
        { "int", FormatRaw },
        { "bool", FormatRaw }
    };

    private static bool IsFormatSupported(string type) =>
        Formatters.ContainsKey(type) ||
        (type.EndsWith("?", StringComparison.InvariantCulture) && Formatters.ContainsKey(type.Substring(0, type.Length - 1)));

    private static Func<string, string> GetFormatter(string type) =>
        type.EndsWith("?", StringComparison.InvariantCulture) ? Formatters[type.Substring(0, type.Length - 1)] : Formatters[type];

    private static string FormatRaw(string value) => value;

    private static string FormatString(string value) => $"@\"{value.Replace("\"", "\"\"")}\"";

    private static string MakeFilename(string ns, string className)
    {
        var buffer = new StringBuilder();

        if (!String.IsNullOrEmpty(ns))
        {
            buffer.Append(ns.Replace('.', '_'));
            buffer.Append('_');
        }

        buffer.Append(className.Replace('<', '[').Replace('>', ']'));
        buffer.Append(".g.cs");

        return buffer.ToString();
    }
}
