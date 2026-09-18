using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Presentation;

public sealed class EndpointPaginationConventionTests
{
    [Fact(DisplayName = "Endpoints should pair pagination parameters and responses when mapped")]
    public async Task Endpoints_Should_PairPaginationParametersAndResponses_When_Mapped()
    {
        var endpoints = await ProductionSourceDocumentDiscovery.GetItemsAsync(GetEndpointsAsync);
        var violations = endpoints.Where(endpoint => endpoint.HasParameters != endpoint.DeclaresPagination ||
                endpoint.HasParameters != endpoint.ReturnsPagination)
            .Select(endpoint => $"{endpoint.Name}: PaginationParameters, Produces<PaginationResult<T>>, " +
                "and the paginated success response must occur together.")
            .ToArray();

        Assert.NotEmpty(endpoints);
        Assert.Contains(endpoints, endpoint => endpoint.HasParameters);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    private static async Task<EndpointPagination[]> GetEndpointsAsync(string repositoryRoot, Document document)
    {
        if (document.Project.AssemblyName?.EndsWith(".Presentation", StringComparison.Ordinal) != true)
        {
            return [];
        }

        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        if (root is null || semanticModel is null)
        {
            return [];
        }

        var endpoints = new List<EndpointPagination>();
        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol mapping ||
                mapping.ContainingType.ToDisplayString() != "PANiXiDA.Core.Presentation.Http.Endpoints.EndpointMapBuilder" ||
                !mapping.Name.StartsWith("Map", StringComparison.Ordinal))
            {
                continue;
            }

            var handler = invocation.ArgumentList.Arguments
                .Select(argument => semanticModel.GetSymbolInfo(argument.Expression))
                .SelectMany(info => info.Symbol is { } symbol ? [symbol] : info.CandidateSymbols)
                .OfType<IMethodSymbol>().SingleOrDefault()
                ?? throw new InvalidOperationException($"{document.FilePath}: cannot resolve the handler for {invocation}.");

            var hasParameters = handler.Parameters.Any(parameter => parameter.Type.ToDisplayString() ==
                "PANiXiDA.Core.Application.Querying.Pagination.PaginationParameters");
            var declaresPagination = invocation.Ancestors().OfType<InvocationExpressionSyntax>()
                .Select(call => semanticModel.GetSymbolInfo(call).Symbol).OfType<IMethodSymbol>()
                .Any(method => method.Name == "Produces" && method.TypeArguments.Any(IsPaginationResult));
            var returnsPagination = false;
            foreach (var reference in handler.DeclaringSyntaxReferences)
            {
                var declaration = await reference.GetSyntaxAsync();
                var handlerModel = semanticModel.Compilation.GetSemanticModel(declaration.SyntaxTree);
                returnsPagination |= declaration.DescendantNodes().OfType<InvocationExpressionSyntax>()
                    .Select(call => handlerModel.GetSymbolInfo(call).Symbol).OfType<IMethodSymbol>()
                    .Any(method => method.ContainingType.ToDisplayString() == "Microsoft.AspNetCore.Http.TypedResults" &&
                        method.Name == "Ok" && method.TypeArguments.Any(IsPaginationResult));
            }

            endpoints.Add(new EndpointPagination(
                $"{Path.GetRelativePath(repositoryRoot, document.FilePath!)}:{invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                hasParameters, declaresPagination, returnsPagination));
        }

        return [.. endpoints];
    }

    private static bool IsPaginationResult(ITypeSymbol type)
    {
        return type is INamedTypeSymbol { MetadataName: "PaginationResult`1" } named &&
            named.ContainingNamespace.ToDisplayString() == "PANiXiDA.Core.Application.Querying.Pagination";
    }

    private sealed record EndpointPagination(string Name, bool HasParameters, bool DeclaresPagination, bool ReturnsPagination);
}
