using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.Application.Querying;
using PANiXiDA.Core.Application.Querying.Cursor;
using PANiXiDA.Core.Application.Querying.Pagination;
using PANiXiDA.Core.Application.Storage.Models;
using PANiXiDA.Core.ResultPattern;
using PANiXiDA.TacticalHeroes.ArchitectureTests.Global;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDetails;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDownloadUrl;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Application;

public sealed class ReadModelConventionTests
{
    private const string ApplicationAssemblySuffix = ".Application";
    private const string ReadModelSuffix = "ReadModel";

    [Fact(DisplayName = "Read models should use sealed record declarations without class or struct when declared")]
    public async Task ReadModels_Should_UseSealedRecordDeclarations_When_Declared()
    {
        var readModels = await ProductionSourceDocumentDiscovery.GetItemsAsync<INamedTypeSymbol>(async (_, document) =>
        {
            if (document.Project.AssemblyName?.EndsWith(ApplicationAssemblySuffix, StringComparison.Ordinal) != true)
            {
                return [];
            }

            var syntaxRoot = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();
            var readModelContract = semanticModel?.Compilation.GetTypeByMetadataName(
                "PANiXiDA.Core.Application.Querying.IReadModel");

            return syntaxRoot is null || semanticModel is null
                ? []
                : [.. syntaxRoot.DescendantNodes()
                    .OfType<TypeDeclarationSyntax>()
                    .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
                    .OfType<INamedTypeSymbol>()
                    .Where(type => type.TypeKind is TypeKind.Class or TypeKind.Struct
                        && (type.Name.EndsWith(ReadModelSuffix, StringComparison.Ordinal)
                            || type.AllInterfaces.Any(contract =>
                                SymbolEqualityComparer.Default.Equals(contract, readModelContract))))];
        });
        var violations = readModels
            .Where(type => !type.IsRecord
                || type.TypeKind != TypeKind.Class
                || !type.IsSealed
                || type.DeclaringSyntaxReferences.Any(reference =>
                    reference.GetSyntax() is RecordDeclarationSyntax declaration
                        && declaration.ClassOrStructKeyword.RawKind != 0))
            .Select(type => $"{type.ToDisplayString()} must use 'sealed record' without 'class' or 'struct'.")
            .ToArray();

        Assert.NotEmpty(readModels);
        Assert.True(
            violations.Length == 0,
            $"Read model record violations:{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Read models should end with ReadModel when declared")]
    public void ReadModels_Should_EndWithReadModel_When_Declared()
    {
        var readModels = GetApplicationTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false }
                    && typeof(IReadModel).IsAssignableFrom(type))
            .ToArray();
        var violations = readModels
            .Where(type => !type.Name.EndsWith(
                ReadModelSuffix,
                StringComparison.Ordinal))
            .Select(type =>
                $"{type.FullName} implements IReadModel and must end with "
                    + $"'{ReadModelSuffix}'.")
            .ToArray();

        Assert.NotEmpty(readModels);
        Assert.True(
            violations.Length == 0,
            $"Read model naming violations:{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Types ending with ReadModel should implement IReadModel when declared")]
    public void TypesEndingWithReadModel_Should_ImplementIReadModel_When_Declared()
    {
        var namedReadModels = GetApplicationTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false }
                    && type.Name.EndsWith(
                        ReadModelSuffix,
                        StringComparison.Ordinal))
            .ToArray();
        var violations = namedReadModels
            .Where(type => !typeof(IReadModel).IsAssignableFrom(type))
            .Select(type =>
                $"{type.FullName} ends with '{ReadModelSuffix}' and must "
                    + $"implement IReadModel.")
            .ToArray();

        Assert.NotEmpty(namedReadModels);
        Assert.True(
            violations.Length == 0,
            $"Read model inheritance violations:{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Query handlers should return read models or Core Application types when declared")]
    public void QueryHandlers_Should_ReturnReadModelsOrCoreApplicationTypes_When_Declared()
    {
        var queryHandlers = GetApplicationTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type
                .GetInterfaces()
                .Where(candidate =>
                    candidate.IsGenericType
                        && candidate.GetGenericTypeDefinition()
                            == typeof(IQueryHandler<,>))
                .Select(contract => new QueryHandler(
                    type,
                    contract.GetGenericArguments()[1])))
            .ToArray();
        var violations = queryHandlers
            .Where(handler => !ReadSideConvention.IsQueryHandlerResult(
                handler.ResultType))
            .Select(handler =>
                $"{handler.Type.FullName} must return an IReadModel or a type from PANiXiDA.Core.Application, "
                    + $"optionally wrapped in Task, Result, a collection, or a "
                    + $"pagination model; found '{handler.ResultType}'.")
            .ToArray();

        Assert.NotEmpty(queryHandlers);
        Assert.True(
            violations.Length == 0,
            $"Query handler result type violations:{Environment.NewLine}"
                + string.Join(Environment.NewLine, violations));
    }

    [Theory(DisplayName = "Query result check should validate payload origin when result type is examined")]
    [InlineData(typeof(PresignedDownloadUrl), true)]
    [InlineData(typeof(PresignedUploadUrl), true)]
    [InlineData(typeof(FileMetadata), true)]
    [InlineData(typeof(Result<PresignedDownloadUrl>), true)]
    [InlineData(typeof(Task<Result<PresignedDownloadUrl>>), true)]
    [InlineData(typeof(ValueTask<FileMetadata>), true)]
    [InlineData(typeof(PresignedDownloadUrl[]), true)]
    [InlineData(typeof(IReadOnlyList<PresignedDownloadUrl>), true)]
    [InlineData(typeof(PaginationResult<PresignedDownloadUrl>), true)]
    [InlineData(typeof(CursorPaginationResult<FileMetadata>), true)]
    [InlineData(typeof(FileDetailsReadModel), true)]
    [InlineData(typeof(Task<Result<PaginationResult<FileDetailsReadModel>>>), true)]
    [InlineData(typeof(IQuery<Result<PresignedDownloadUrl>>), true)]
    [InlineData(typeof(File), false)]
    [InlineData(typeof(Result<File>), false)]
    [InlineData(typeof(PaginationResult<File>), false)]
    [InlineData(typeof(CursorPaginationResult<File>), false)]
    [InlineData(typeof(Task<Result<IReadOnlyList<File>>>), false)]
    [InlineData(typeof(IQuery<Result<File>>), false)]
    [InlineData(typeof(FileReadDbModel), false)]
    [InlineData(typeof(GetFileDownloadUrlResponse), false)]
    [InlineData(typeof(Guid), false)]
    [InlineData(typeof(Result<Guid>), false)]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(Task), false)]
    public void IsQueryHandlerResult_Should_ValidatePayloadOrigin_When_ResultTypeIsExamined(Type type, bool expected)
    {
        var result = ReadSideConvention.IsQueryHandlerResult(type);

        Assert.Equal(expected, result);
    }

    [Theory(DisplayName = "Read model result check should preserve repository rules when core models are examined")]
    [InlineData(typeof(PresignedDownloadUrl))]
    [InlineData(typeof(Result<PresignedDownloadUrl>))]
    [InlineData(typeof(PaginationResult<FileMetadata>))]
    public void IsReadModelResult_Should_PreserveRepositoryRules_When_CoreModelsAreExamined(Type type)
    {
        var result = ReadSideConvention.IsReadModelResult(type);

        Assert.False(result);
    }

    private static Type[] GetApplicationTypes()
    {
        return
        [
            .. ArchitectureDefinition.s_productionAssemblies
                .Where(assembly => assembly.GetName().Name?.EndsWith(
                    ApplicationAssemblySuffix,
                    StringComparison.Ordinal) == true)
                .SelectMany(assembly => assembly.GetTypes())
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
        ];
    }

    private sealed record QueryHandler(
        Type Type,
        Type ResultType);
}
