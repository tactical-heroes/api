using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Common.Filters;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.GetDetails;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.GetList;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Persistence.Features.Files.Read.GetDetails;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Persistence.Features.Files.Read.GetList;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Persistence.Features.Files.Read;

public sealed class FilesReadRepository(FileManagerReadDbContext dbContext)
    :
        EfReadRepository<FileManagerReadDbContext, Guid, FileReadDbModel>(dbContext),
        IFilesReadRepository
{
    public Task<PaginationResult<FileListItemReadModel>> GetPageAsync(
        FilesFilter filter,
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilter(
            query: Query,
            filter: filter);

        return GetPagedResultAsync<FileListItemReadModel, FileListItemReadModelMapper, FileListItemReadModelSorting>(
            query: query,
            paginationParameters: paginationParameters,
            sortingParameters: sortingParameters,
            cancellationToken: cancellationToken);
    }

    public Task<FileDetailsReadModel?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return GetByIdAsync<FileDetailsReadModel, FileDetailsReadModelMapper>(id, cancellationToken);
    }

    private static IQueryable<FileReadDbModel> ApplyFilter(
        IQueryable<FileReadDbModel> query,
        FilesFilter filter)
    {
        if (filter.UserId is { } userId)
        {
            query = query.Where(file => file.Type != FileType.Personal.Name || file.UserId == userId);
        }

        if (filter.Type is { } type)
        {
            query = query.Where(file => file.Type == type.Trim());
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(file => EF.Functions.ILike(
                matchExpression: file.Name,
                pattern: $"%{filter.Search.Trim()}%"));
        }

        return query;
    }
}
