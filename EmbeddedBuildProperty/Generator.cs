namespace EmbeddedBuildProperty;

using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

[Generator]
public sealed class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(AddAttribute);

        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsTargetSyntax(node),
                static (context, _) => GetMethodModel(context))
            .Where(x => x is not null)
            .Collect();

        var valueProvider = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) => SelectValues(provider));

        context.RegisterImplementationSourceOutput(
            provider.Combine(valueProvider),
            static (context, provider) => Execute(context, provider.Left!, provider.Right));
    }

    private static void AddAttribute(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("BuildPropertyAttributes", SourceText.From(AttributeSource, Encoding.UTF8));
    }

    private static Dictionary<string, string> SelectValues(AnalyzerConfigOptionsProvider provider)
    {
        if (provider.GlobalOptions.TryGetValue("build_property.EmbeddedBuildProperty", out var values))
        {
            return values
                .Split(',')
                .Select(x =>
                {
                    var index = x.IndexOf('=');
                    if (index > 0)
                    {
                        return new
                        {
                            Key = x.Substring(0, index).Trim(),
                            Value = x.Substring(index + 1).Trim()
                        };
                    }

                    return new { Key = string.Empty, Value = string.Empty };
                })
                .Where(x => !String.IsNullOrEmpty(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        return new Dictionary<string, string>();
    }

    private static bool IsTargetSyntax(SyntaxNode node) =>
        node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };

    private static MethodModel? GetMethodModel(GeneratorSyntaxContext context)
    {
        var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;

        if (methodDeclarationSyntax.ParameterList.Parameters.Count > 0)
        {
            return null;
        }

        if (context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        if (!methodSymbol.IsPartialDefinition || !methodSymbol.IsStatic)
        {
            return null;
        }

        if (methodSymbol.ReturnType is not INamedTypeSymbol returnTypeSymbol)
        {
            return null;
        }

        // TODO Extend supported types
        var returnType = returnTypeSymbol.ToDisplayString();
        if (returnType != "string")
        {
            return null;
        }

        var attribute = methodSymbol.GetAttributes()
            .FirstOrDefault(x => x.AttributeClass!.ToDisplayString() == "EmbeddedBuildProperty.BuildProperty");
        if (attribute is null)
        {
            return null;
        }

        var containingType = methodSymbol.ContainingType;
        var ns = String.IsNullOrEmpty(containingType.ContainingNamespace.Name)
            ? string.Empty
            : containingType.ContainingNamespace.ToDisplayString();
        var property = (attribute.ConstructorArguments.Length > 0) && (attribute.ConstructorArguments[0].Value is string value)
            ? value
            : methodSymbol.Name;

        return new MethodModel(
            ns,
            containingType.Name,
            containingType.IsValueType,
            methodSymbol.DeclaredAccessibility,
            returnType,
            methodSymbol.Name,
            property);
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<MethodModel> methods, Dictionary<string, string> values)
    {
        var buffer = new StringBuilder();
        foreach (var group in methods.GroupBy(x => new { x.Namespace, x.ClassName }))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var filename = MakeFilename(buffer, group.Key.Namespace, group.Key.ClassName);
            var source = GenerateSource(buffer, methods, values);
            context.AddSource(filename, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string MakeFilename(StringBuilder buffer, string ns, string className)
    {
        buffer.Clear();

        if (!String.IsNullOrEmpty(ns))
        {
            buffer.Append(ns.Replace('.', '_'));
            buffer.Append('_');
        }

        buffer.Append(className);
        buffer.Append(".g.cs");

        return buffer.ToString();
    }

    private static string GenerateSource(StringBuilder buffer, ImmutableArray<MethodModel> methods, Dictionary<string, string> values)
    {
        buffer.Clear();

        var ns = methods[0].Namespace;
        var className = methods[0].ClassName;
        var isValueType = methods[0].IsValueType;

        buffer.AppendLine("// <auto-generated />");

        // namespace
        if (!String.IsNullOrEmpty(ns))
        {
            buffer.Append("namespace ").Append(ns).AppendLine();
        }

        buffer.AppendLine("{");

        // class
        buffer.Append("    partial ").Append(isValueType ? "struct " : "class ").Append(className).AppendLine();
        buffer.AppendLine("    {");

        foreach (var method in methods)
        {
            buffer.Append("        ");
            buffer.Append(ToAccessibilityText(method.MethodAccessibility));
            buffer.Append(" static partial ");
            buffer.Append(method.ReturnType);
            buffer.Append(' ');
            buffer.Append(method.MethodName);
            buffer.Append("() => ");
            if (values.TryGetValue(method.PropertyName, out var value))
            {
                // TODO Extend supported types
                buffer.Append('"').Append(value).Append('"').Append(';');
            }
            else
            {
                buffer.Append("default!;");
            }

            buffer.AppendLine();

            buffer.AppendLine();
        }

        buffer.AppendLine("    }");

        buffer.AppendLine("}");

        return buffer.ToString();
    }

    private static string ToAccessibilityText(Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public => "public",
        Accessibility.Protected => "protected",
        Accessibility.Private => "private",
        Accessibility.Internal => "internal",
        Accessibility.ProtectedOrInternal => "protected internal",
        Accessibility.ProtectedAndInternal => "private protected",
        _ => throw new NotSupportedException()
    };

    private const string AttributeSource = @"// <auto-generated />
using System;

namespace EmbeddedBuildProperty
{
    [System.Diagnostics.Conditional(""COMPILE_TIME_ONLY"")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class BuildProperty : Attribute
    {
        public BuildProperty()
        {
        }

        public BuildProperty(string s)
        {
        }
    }
}
";

    internal sealed record MethodModel(
        string Namespace,
        string ClassName,
        bool IsValueType,
        Accessibility MethodAccessibility,
        string ReturnType,
        string MethodName,
        string PropertyName);
}
