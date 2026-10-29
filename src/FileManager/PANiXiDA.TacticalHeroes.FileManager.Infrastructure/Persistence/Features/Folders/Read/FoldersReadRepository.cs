using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetDetails;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetList;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.GetDetails;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.GetList;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read;

public sealed class FoldersReadRepository(FileManagerReadDbContext dbContext)
    :
        EfReadRepository<FileManagerReadDbContext, Guid, FolderReadDbModel>(dbContext),
        IFoldersReadRepository
{
    public Task<PaginationResult<FolderListItemReadModel>> GetPageAsync(
        FoldersFilter filter,
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilter(
            query: Query,
            filter: filter);

        return GetPagedResultAsync<
            FolderListItemReadModel,
            FolderListItemReadModelMapper,
            FolderListItemReadModelSorting>(
                query: query,
                paginationParameters: paginationParameters,
                sortingParameters: sortingParameters,
                cancellationToken: cancellationToken);
    }

    public Task<FolderDetailsReadModel?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return GetByIdAsync<FolderDetailsReadModel, FolderDetailsReadModelMapper>(id, cancellationToken);
    }

    private static IQueryable<FolderReadDbModel> ApplyFilter(
        IQueryable<FolderReadDbModel> query,
        FoldersFilter filter)
    {
        if (filter.UserId is { } userId)
        {
            query = query.Where(folder => folder.AllowedFileType != FileType.Personal.Name || folder.UserId == userId);
        }

        if (filter.AllowedFileType is { } type)
        {
            query = query.Where(folder => folder.AllowedFileType == type.Trim());
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(folder => EF.Functions.ILike(
                matchExpression: folder.Name,
                pattern: $"%{filter.Search.Trim()}%"));
        }

        return query;
    }
}
