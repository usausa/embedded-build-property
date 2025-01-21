namespace EmbeddedBuildProperty.Generator;

using System;
using System.Collections.Immutable;
using System.Text;

using EmbeddedBuildProperty.Generator.Helpers;
using EmbeddedBuildProperty.Generator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

[Generator]
public sealed class BuildPropertyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var valueProvider = context.AnalyzerConfigOptionsProvider
            .Select(SelectBuildProperty);

        var propertyProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "EmbeddedBuildProperty.BuildPropertyAttribute",
                static (syntax, _) => IsTargetSyntax(syntax),
                static (context, _) => GetPropertyModel(context))
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            .Where(static x => x.Value is not null) // TODO delete
            .Collect();

        // TODO Diagnostics? Errorsだけ分離する？ とりあえず定数定義にして、Modelもそうして、まとめて報告するか

        context.RegisterImplementationSourceOutput(
            valueProvider.Combine(propertyProvider),
            static (context, provider) => Execute(context, provider.Left, provider.Right));
    }

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
            // TODO シンボル取得失敗
            return null!;
        }

        if (!symbol.IsPartialDefinition || !symbol.IsStatic)
        {
            // TODO partial, staticチェック
            return null!;
        }

        if (symbol.GetMethod is null)
        {
            // TODO getterがない
            return null!;
        }

        var returnType = symbol.GetMethod.ReturnType.ToDisplayString();
        if (!Formatters.ContainsKey(returnType) &&
            returnType.EndsWith("?", StringComparison.InvariantCulture) &&
            !Formatters.ContainsKey(returnType.Substring(0, returnType.Length - 1)))
        {
            // TODO 未対応型
            return null!;
        }

        var containingType = symbol.ContainingType;
        var ns = String.IsNullOrEmpty(containingType.ContainingNamespace.Name)
            ? string.Empty
            : containingType.ContainingNamespace.ToDisplayString();
        var attribute = symbol.GetAttributes().First(static x => x.AttributeClass!.ToDisplayString() == "EmbeddedBuildProperty.BuildPropertyAttribute");
        var propertyName = (attribute.ConstructorArguments.Length > 0) && (attribute.ConstructorArguments[0].Value is string value)
            ? value
            : symbol.Name;

        var model = new PropertyModel(
            ns,
            containingType.Name,
            containingType.IsValueType,
            symbol.DeclaredAccessibility,
            returnType,
            symbol.Name,
            propertyName);
        return new(model, new([]));
    }

    private static void Execute(SourceProductionContext context, EquatableArray<BuildProperty> values, ImmutableArray<Result<PropertyModel>> properties)
    {
        // TODO Diagnostic

        var valueMap = values.ToArray().ToDictionary(static x => x.Key, static x => x.Value);

        var builder = new SourceBuilder();
        foreach (var group in properties.GroupBy(static x => new { x.Value.Namespace, x.Value.ClassName }))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            builder.Clear();
            BuildSource(builder, valueMap, group.Select(static x => x.Value).ToList());

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
        builder.Indent().Append("partial ").Append(isValueType ? "struct " : "class ").Append(className).NewLine();
        builder.BeginScope();

        foreach (var property in properties)
        {
            builder.Indent();
            builder.Append(property.MethodAccessibility.ToText());
            builder.Append(" static partial ");
            builder.Append(property.PropertyType);
            builder.Append(' ');
            builder.Append(property.PropertyName);
            builder.Append(" => ");
            if (values.TryGetValue(property.PropertyName, out var value))
            {
                var formatter = property.PropertyType.EndsWith("?", StringComparison.InvariantCulture)
                    ? Formatters[property.PropertyType.Substring(0, property.PropertyType.Length - 1)]
                    : Formatters[property.PropertyType];
                builder.Append(formatter(value)).Append(';');
            }
            else
            {
                builder.Append("default!;");
            }
            builder.NewLine();

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

        buffer.Append(className);
        buffer.Append(".g.cs");

        return buffer.ToString();
    }
}
