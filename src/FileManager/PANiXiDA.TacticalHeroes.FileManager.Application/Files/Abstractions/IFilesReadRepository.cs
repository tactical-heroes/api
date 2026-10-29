using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDetails;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetList;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;

public interface IFilesReadRepository : IReadRepository<Guid>
{
    Task<PaginationResult<FileListItemReadModel>> GetPageAsync(
        FilesFilter filter,
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken);

    Task<FileDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
