using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

using PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Domain;

public sealed class DomainCollectionExposureConventionTests
{
    [Fact(DisplayName = "Aggregate roots and entities should return protected collections when collections are exposed")]
    public async Task AggregateRootsAndEntities_Should_ReturnProtectedCollections_When_CollectionsAreExposed()
    {
        var returns = await ProductionSourceDocumentDiscovery.GetItemsAsync(GetCollectionReturnsAsync);
        var violations = returns
            .Where(result => !result.IsProtected)
            .Select(result => result.Location)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(returns);
        Assert.True(
            violations.Length == 0,
            "Domain collections must be returned through standard read-only wrappers, " +
            "immutable collections, or frozen collections. A read-only interface " +
            $"alone does not protect mutable storage:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Theory(DisplayName = "Collection exposure should validate protection when member shapes vary")]
    [InlineData("public IReadOnlyCollection<int> Values => _values;", false)]
    [InlineData("public IReadOnlyCollection<int> Values => _values.AsReadOnly();", true)]
    [InlineData("internal IReadOnlyCollection<int> Values => _values;", false)]
    [InlineData("IEnumerable<int> IView.Values => _values;", false)]
    [InlineData("public object Values => _values;", false)]
    [InlineData("public object Values => _values.AsReadOnly();", true)]
    [InlineData("public Task<IReadOnlyCollection<int>> GetValues() => Task.FromResult<IReadOnlyCollection<int>>(_values);", false)]
    [InlineData("public Task<ReadOnlyCollection<int>> GetValues() => Task.FromResult(_values.AsReadOnly());", true)]
    [InlineData("public IReadOnlyCollection<IReadOnlyCollection<int>> Values => ImmutableArray.Create<IReadOnlyCollection<int>>(_values);", false)]
    [InlineData("public IReadOnlyCollection<ReadOnlyCollection<int>> Values => ImmutableArray.Create(_values.AsReadOnly());", true)]
    [InlineData("public (IReadOnlyCollection<int> Items, int Count) GetValues() => (_values, _values.Count);", false)]
    [InlineData("public (IReadOnlyCollection<int> Items, int Count) GetValues() => (_values.AsReadOnly(), _values.Count);", true)]
    [InlineData("public IReadOnlyCollection<int> Values => GetRaw(); private List<int> GetRaw() => _values;", false)]
    [InlineData("public IReadOnlyCollection<int> Values => GetProtected(); private ReadOnlyCollection<int> GetProtected() => _values.AsReadOnly();", true)]
    [InlineData("public IReadOnlyCollection<int> Values => _values.Count > 0 ? _values.AsReadOnly() : null;", true)]
    [InlineData("public Memory<int> Values => new Memory<int>(_values.ToArray());", false)]
    [InlineData("public Span<int> Values => System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_values);", false)]
    [InlineData("public ReadOnlyMemory<int> Values => new ReadOnlyMemory<int>(_values.ToArray());", true)]
    [InlineData("public ReadOnlySpan<int> Values => new ReadOnlySpan<int>(_values.ToArray());", true)]
    [InlineData("public ReadOnlySpan<int> Values => System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_values);", true)]
    [InlineData("public ReadOnlyMemory<int> Values => new Memory<int>(_values.ToArray());", true)]
    [InlineData("public ReadOnlySpan<int> Values => _values.ToArray();", true)]
    public void CollectionExposure_Should_ValidateProtection_When_MemberShapesVary(string member, bool expected)
    {
        var source = $$"""
                       using System;
                       using System.Collections.Generic;
                       using System.Collections.Immutable;
                       using System.Collections.ObjectModel;
                       using System.Threading.Tasks;

                       namespace PANiXiDA.Core.Domain.Entities { public interface IEntity { } }

                       public interface IView { IEnumerable<int> Values => Array.Empty<int>(); }

                       public sealed class Sample : PANiXiDA.Core.Domain.Entities.IEntity, IView
                       {
                           private readonly List<int> _values = [1];
                           {{member}}
                       }
                       """;
        var cancellationToken = TestContext.Current.CancellationToken;
        var repositoryRoot = Path.GetTempPath();
        var tree = CSharpSyntaxTree.ParseText(
            source,
            path: Path.Combine(repositoryRoot, "CollectionExposureProbe.cs"),
            cancellationToken: cancellationToken);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            assemblyName: "CollectionExposureProbe",
            syntaxTrees: [tree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var semanticModel = compilation.GetSemanticModel(tree);

        var returns = tree.GetRoot(cancellationToken).DescendantNodes()
            .Select(node => GetCollectionReturn(node, semanticModel, repositoryRoot))
            .OfType<CollectionReturn>()
            .ToArray();

        Assert.DoesNotContain(compilation.GetDiagnostics(cancellationToken), diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.NotEmpty(returns);
        Assert.Equal(expected, returns.All(result => result.IsProtected));
    }

    private static async Task<CollectionReturn[]> GetCollectionReturnsAsync(
        string repositoryRoot,
        Document document)
    {
        if (!document.Project.Name.EndsWith(".Domain", StringComparison.Ordinal))
        {
            return [];
        }

        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        if (root is null || semanticModel is null)
        {
            return [];
        }

        return
        [
            .. root.DescendantNodes()
                .Select(node => GetCollectionReturn(node, semanticModel, repositoryRoot))
                .OfType<CollectionReturn>()
        ];
    }

    private static CollectionReturn? GetCollectionReturn(
        SyntaxNode node,
        SemanticModel semanticModel,
        string repositoryRoot)
    {
        var expression = node switch
        {
            ArrowExpressionClauseSyntax arrow => arrow.Expression,
            ReturnStatementSyntax statement => statement.Expression,
            EqualsValueClauseSyntax initializer => initializer.Value,
            _ => null
        };

        var member = node switch
        {
            EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax variable } =>
                semanticModel.GetDeclaredSymbol(variable),
            EqualsValueClauseSyntax { Parent: PropertyDeclarationSyntax propertyDeclaration } =>
                semanticModel.GetDeclaredSymbol(propertyDeclaration),
            PropertyDeclarationSyntax { ExpressionBody: null, Initializer: null } propertyDeclaration
                when propertyDeclaration.AccessorList?.Accessors.All(accessor =>
                    accessor.Body is null && accessor.ExpressionBody is null) == true =>
                semanticModel.GetDeclaredSymbol(propertyDeclaration),
            VariableDeclaratorSyntax
            { Initializer: null, Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax } } variable =>
                semanticModel.GetDeclaredSymbol(variable),
            _ when expression is not null => semanticModel.GetEnclosingSymbol(expression.SpanStart),
            _ => null
        };

        if (member is IMethodSymbol { AssociatedSymbol: IPropertySymbol property })
        {
            member = property;
        }

        var returnType = member switch
        {
            IPropertySymbol propertySymbol => propertySymbol.Type,
            IFieldSymbol field => field.Type,
            IMethodSymbol method => method.ReturnType,
            _ => null
        };
        var entity = semanticModel.Compilation.GetTypeByMetadataName("PANiXiDA.Core.Domain.Entities.IEntity");

        var operation = expression is null ? null : semanticModel.GetOperation(expression);

        while (operation?.Parent is IConversionOperation conversion)
        {
            operation = conversion;
        }

        if (member is null || !IsExposed(member) ||
            returnType is null ||
            !DomainCollectionConvention.ContainsCollection(returnType, semanticModel.Compilation) &&
            !DomainCollectionConvention.ReturnsCollection(operation, semanticModel.Compilation) ||
            !member.ContainingType.AllInterfaces.Any(type =>
                SymbolEqualityComparer.Default.Equals(type, entity)))
        {
            return null;
        }

        return new CollectionReturn(
            Location: $"{Path.GetRelativePath(repositoryRoot, node.SyntaxTree.FilePath)}:" +
                      $"{node.GetLocation().GetLineSpan().StartLinePosition.Line + 1} " +
                      $"{member.Name}",
            IsProtected: member is not IFieldSymbol { IsReadOnly: false } &&
                         (expression is null
                             ? DomainCollectionConvention.IsProtectedCollectionType(returnType, semanticModel.Compilation)
                             : DomainCollectionConvention.IsProtectedCollection(
                                 operation,
                                 semanticModel.Compilation)));
    }

    private static bool IsExposed(ISymbol member)
    {
        return member.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or
                   Accessibility.ProtectedOrInternal ||
               member is IPropertySymbol { ExplicitInterfaceImplementations.IsEmpty: false } or
                   IMethodSymbol { ExplicitInterfaceImplementations.IsEmpty: false };
    }

    private sealed record CollectionReturn(string Location, bool IsProtected);
}
