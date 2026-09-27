using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        if (member?.DeclaredAccessibility != Accessibility.Public ||
            returnType is null ||
            !DomainCollectionConvention.IsCollection(returnType, semanticModel.Compilation) ||
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
                                 semanticModel.GetOperation(expression),
                                 semanticModel.Compilation)));
    }

    private sealed record CollectionReturn(string Location, bool IsProtected);
}
