using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetList;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.GetList;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.GetPersonalDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read;

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

    public Task<PersonalFileDetailsReadModel?> GetPersonalDetailsByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var query = Query.Where(file =>
            file.Id == id
                && file.Type == FileType.Personal.Name
                && file.UserId == userId);

        return PersonalFileDetailsReadModelMapper.ProjectTo(query)
            .FirstOrDefaultAsync(cancellationToken);
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
            query = query.Where(file => file.Type == type);
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
