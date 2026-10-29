using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Common.Filters;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;

public sealed class FolderAccessPolicy(ICurrentUser currentUser, FileAccessPolicy fileAccessPolicy)
{
    public bool CanRead(FoldersFilter filter)
    {
        return fileAccessPolicy.CanRead(new FilesFilter(Type: filter.AllowedFileType, UserId: filter.UserId));
    }

    public Result<FoldersFilter> ApplyReadRestrictions(FoldersFilter filter)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
        {
            return Result.Failure<FoldersFilter>(Error.Unauthorized("A user account is required."));
        }

        if (!CanRead(filter) && filter.UserId is null)
        {
            filter = filter with { UserId = userId };
        }

        return CanRead(filter)
            ? Result.Success(filter)
            : Result.Failure<FoldersFilter>(Error.Forbidden("Access to folders is denied."));
    }
}
