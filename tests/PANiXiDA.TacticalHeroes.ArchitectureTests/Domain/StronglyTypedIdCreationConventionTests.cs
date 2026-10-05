using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

using PANiXiDA.Core.Domain.Identifiers;
using PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Domain;

public sealed class StronglyTypedIdCreationConventionTests
{
    [Fact(DisplayName = "Strongly typed ids should avoid default values when production code is declared")]
    public async Task StronglyTypedIds_Should_AvoidDefaultValues_When_ProductionCodeIsDeclared()
    {
        var violations = await ProductionSourceDocumentDiscovery.GetItemsAsync(GetDocumentViolationsAsync);

        Assert.True(violations.Length == 0,
            $"Strongly typed IDs must be created through their factories:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Theory(DisplayName = "ID creation should detect default values when source expressions vary")]
    [InlineData("public static SampleId Get() => default;", true)]
    [InlineData("public static object Get() => default(SampleId);", true)]
    [InlineData("public static SampleId? Get() => default(SampleId);", true)]
    [InlineData("public static SampleId Get() => new SampleId();", true)]
    [InlineData("public static SampleId Get() => new();", true)]
    [InlineData("public static IdAlias Get() => default;", true)]
    [InlineData("public static SampleId Value = default;", true)]
    [InlineData("public static SampleId Value { get; } = default;", true)]
    [InlineData("public static SampleId Get() { SampleId id = default; return id; }", true)]
    [InlineData("public static void Use() => Consume(default); private static void Consume(SampleId id) { }", true)]
    [InlineData("public static void Use(SampleId id = default) { }", true)]
    [InlineData("public static SampleId Get(bool choice) => choice ? SampleId.New() : default;", true)]
    [InlineData("public static T Get<T>() where T : struct, IStronglyTypedId => default;", true)]
    [InlineData("public static T Get<T>() where T : struct, IStronglyTypedId => new();", true)]
    [InlineData("public static T Get<T>() where T : IStronglyTypedId => default;", true)]
    [InlineData("public static SampleId[] Get() => new SampleId[1];", true)]
    [InlineData("public static SampleId[] Get(int size) => new SampleId[size];", true)]
    [InlineData("public static SampleId[] Get() => [default];", true)]
    [InlineData("public static SampleId Get(SampleId? id) => id.GetValueOrDefault();", true)]
    [InlineData("public static SampleId Get(SampleId[] ids) => ids.FirstOrDefault();", true)]
    [InlineData("public static SampleId Get(SampleId[] ids) => ids.SingleOrDefault(id => true);", true)]
    [InlineData("public static SampleId Get(SampleId[] ids) => ids.LastOrDefault();", true)]
    [InlineData("public static SampleId Get(IQueryable<SampleId> ids) => ids.FirstOrDefault();", true)]
    [InlineData("public static SampleId Get(SampleId[] ids) => ids.ElementAtOrDefault(0);", true)]
    [InlineData("public static IEnumerable<SampleId> Get(SampleId[] ids) => ids.DefaultIfEmpty();", true)]
    [InlineData("public static SampleId Get() => Activator.CreateInstance<SampleId>();", true)]
    [InlineData("public static object Get() => Activator.CreateInstance(typeof(SampleId));", true)]
    [InlineData("public static SampleId Get() => SampleId.New();", false)]
    [InlineData("public static SampleId? Get() => default;", false)]
    [InlineData("public static SampleId? Get() => default(SampleId?);", false)]
    [InlineData("public static SampleId? Get() => null;", false)]
    [InlineData("public static CancellationToken Get() => default;", false)]
    [InlineData("public static int Get() => default;", false)]
    [InlineData("public static T Get<T>() where T : struct => default;", false)]
    [InlineData("public static SampleId[] Get() => new SampleId[0];", false)]
    [InlineData("public static SampleId[] Get() => new SampleId[1] { SampleId.New() };", false)]
    [InlineData("public static SampleId[] Get() => [SampleId.New()];", false)]
    [InlineData("public static SampleId[] Get() => Array.Empty<SampleId>();", false)]
    [InlineData("public static SampleId Get(SampleId? id) => id.GetValueOrDefault(SampleId.New());", false)]
    [InlineData("public static SampleId Get(SampleId[] ids) => ids.FirstOrDefault(SampleId.New());", false)]
    [InlineData("public static SampleId? Get(SampleId?[] ids) => ids.FirstOrDefault();", false)]
    [InlineData("public static IEnumerable<SampleId> Get(SampleId[] ids) => ids.DefaultIfEmpty(SampleId.New());", false)]
    [InlineData("public static int Get() => Activator.CreateInstance<int>();", false)]
    public void IdCreation_Should_DetectDefaultValues_When_SourceExpressionsVary(string member, bool expected)
    {
        var source = $$"""
                       using System;
                       using System.Collections.Generic;
                       using System.Linq;
                       using System.Threading;
                       using PANiXiDA.Core.Domain.Identifiers;
                       using IdAlias = SampleId;

                       public static class Sample { {{member}} }
                       public readonly record struct SampleId(Guid Value) : IStronglyTypedId
                       {
                           public static SampleId New() => new(Guid.NewGuid());
                       }
                       """;
        var cancellationToken = TestContext.Current.CancellationToken;
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator).Append(typeof(IStronglyTypedId).Assembly.Location)
            .Distinct(StringComparer.OrdinalIgnoreCase).Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            assemblyName: "IdCreationProbe", syntaxTrees: [tree], references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.DoesNotContain(compilation.GetDiagnostics(cancellationToken), diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        var violations = GetViolations(tree.GetRoot(cancellationToken), compilation.GetSemanticModel(tree));

        Assert.Equal(expected, violations.Length > 0);
    }

    private static async Task<string[]> GetDocumentViolationsAsync(string repositoryRoot, Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        var model = await document.GetSemanticModelAsync();

        if (root is null || model is null || document.FilePath is null)
        {
            return [];
        }

        var relativePath = Path.GetRelativePath(repositoryRoot, document.FilePath);
        return [.. GetViolations(root, model).Select(expression =>
            $"{relativePath}:{expression.GetLocation().GetLineSpan().StartLinePosition.Line + 1}: " +
            $"'{expression}' can produce a default strongly typed ID.")];
    }

    private static ExpressionSyntax[] GetViolations(SyntaxNode root, SemanticModel model)
    {
        var identifier = model.Compilation.GetTypeByMetadataName(typeof(IStronglyTypedId).FullName!);
        if (identifier is null)
        {
            return [];
        }

        return [.. root.DescendantNodes().OfType<ExpressionSyntax>()
            .Where(expression => ProducesDefaultId(expression, model, identifier))];
    }

    private static bool ProducesDefaultId(ExpressionSyntax expression, SemanticModel model, INamedTypeSymbol identifier)
    {
        switch (expression)
        {
            case DefaultExpressionSyntax:
            case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.DefaultLiteralExpression):
            case BaseObjectCreationExpressionSyntax { ArgumentList.Arguments.Count: 0 }:
                return IsIdentifier(model.GetTypeInfo(expression).Type, identifier);
            case ArrayCreationExpressionSyntax:
                return model.GetOperation(expression) is IArrayCreationOperation
                {
                    Type: IArrayTypeSymbol array,
                    Initializer: null
                } creation && IsIdentifier(array.ElementType, identifier) &&
                    !creation.DimensionSizes.Any(size => size.ConstantValue is { HasValue: true, Value: 0 });
            case InvocationExpressionSyntax:
                return model.GetOperation(expression) is IInvocationOperation invocation &&
                    InvocationProducesDefaultId(invocation, model.Compilation, identifier);
            default:
                return false;
        }
    }

    private static bool InvocationProducesDefaultId(
        IInvocationOperation invocation, Compilation compilation, INamedTypeSymbol identifier)
    {
        var method = invocation.TargetMethod;

        if (method.ContainingType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return method.Name == "GetValueOrDefault" && method.Parameters.Length == 0 &&
                IsIdentifier(method.ReturnType, identifier);
        }

        if (IsType(method.ContainingType, compilation, "System.Linq.Enumerable") ||
            IsType(method.ContainingType, compilation, "System.Linq.Queryable"))
        {
            return method.Name is "FirstOrDefault" or "LastOrDefault" or "SingleOrDefault" or
                "ElementAtOrDefault" or "DefaultIfEmpty" &&
                method.TypeArguments.Any(type => IsIdentifier(type, identifier)) &&
                !method.Parameters.Any(parameter => parameter.Name == "defaultValue");
        }

        return method.Name == "CreateInstance" && IsType(method.ContainingType, compilation, "System.Activator") &&
            (method.TypeArguments.Any(type => IsIdentifier(type, identifier)) ||
             invocation.Arguments.Any(argument => argument.Value is ITypeOfOperation typeOf &&
                 IsIdentifier(typeOf.TypeOperand, identifier)));
    }

    private static bool IsIdentifier(ITypeSymbol? type, INamedTypeSymbol identifier)
    {
        if (type is ITypeParameterSymbol parameter)
        {
            return parameter.ConstraintTypes.Any(constraint => IsIdentifier(constraint, identifier));
        }

        return type is not null &&
            (SymbolEqualityComparer.Default.Equals(type, identifier) ||
             type.AllInterfaces.Any(candidate => SymbolEqualityComparer.Default.Equals(candidate, identifier)));
    }

    private static bool IsType(ITypeSymbol type, Compilation compilation, string metadataName)
    {
        return SymbolEqualityComparer.Default.Equals(type, compilation.GetTypeByMetadataName(metadataName));
    }
}
