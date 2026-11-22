using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetDetails;

public sealed class GetFolderDetailsHandler(
    IFoldersReadRepository foldersReadRepository,
    FolderAccessPolicy folderAccessPolicy)
    : IQueryHandler<GetFolderDetailsQuery, Result<FolderDetailsReadModel>>, IRequireAuthorization
{
    public async Task<Result<FolderDetailsReadModel>> HandleAsync(
        GetFolderDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var folder = await foldersReadRepository.GetDetailsByIdAsync(query.Id, cancellationToken);

        return folder is null
            || !folderAccessPolicy.CanRead(new FoldersFilter(
                AllowedFileType: folder.AllowedFileType,
                UserId: folder.UserId))
            ? Result.Failure<FolderDetailsReadModel>(Error.NotFound("Folder was not found."))
            : Result.Success(folder);
    }
}
