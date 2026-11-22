using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetDetails;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetList;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
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
    private readonly DatabaseFacade _database = dbContext.Database;

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

    public Task<bool> HasAncestorAsync(Guid id, Guid ancestorId, CancellationToken cancellationToken)
    {
        return _database.SqlQuery<bool>(
            $"""
            WITH RECURSIVE ancestors AS (
                SELECT parent_id AS id
                FROM file_manager.folders
                WHERE id = {id} AND parent_id IS NOT NULL
                UNION
                SELECT folder.parent_id
                FROM file_manager.folders AS folder
                JOIN ancestors ON folder.id = ancestors.id
                WHERE folder.parent_id IS NOT NULL
            )
            SELECT EXISTS (
                SELECT 1 FROM ancestors WHERE id = {ancestorId}
            ) AS "Value"
            """)
            .SingleAsync(cancellationToken);
    }

    public Task<bool> HasContentsAsync(Guid id, CancellationToken cancellationToken)
    {
        return Query
            .Where(folder => folder.Id == id)
            .AnyAsync(folder => folder.Children.Any() || folder.Files.Any(), cancellationToken);
    }

    public Task<List<Guid>> GetDeletedIdsWithActiveContentsAsync(CancellationToken cancellationToken)
    {
        return Query
            .IgnoreQueryFilters()
            .Where(folder => folder.DeletedAt != null
                && (folder.Children.Any(child => child.DeletedAt == null)
                    || folder.Files.Any(file => file.DeletedAt == null && file.Status != FileStatus.Deleting.Name)))
            .Select(folder => folder.Id)
            .ToListAsync(cancellationToken);
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
