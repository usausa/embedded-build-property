namespace EmbeddedBuildProperty;

using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

[Generator]
public sealed partial class Generator : IIncrementalGenerator
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

        var returnType = returnTypeSymbol.ToDisplayString();
        if (!Formatters.ContainsKey(returnType) &&
            returnType.EndsWith("?", StringComparison.InvariantCulture) &&
            !Formatters.ContainsKey(returnType.Substring(0, returnType.Length - 1)))
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
            var source = BuildSource(buffer, methods, values);
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
}
