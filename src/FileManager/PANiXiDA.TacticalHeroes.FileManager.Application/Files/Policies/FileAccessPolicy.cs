using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;

public sealed class FileAccessPolicy(ICurrentUser currentUser)
{
    public bool CanRead(File file)
    {
        var access = GetReadAccess(file.Type);

        return access.IsSuccess
            && (access.Value.UserId is not { } userId || file.UserId?.Value == userId);
    }

    public Result<FileReadAccess> GetReadAccess(FileType type)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Result.Failure<FileReadAccess>(Error.Unauthorized("Authentication is required."));
        }

        if (type == FileType.Personal)
        {
            if (currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadAll.Code))
            {
                return Result.Success(new FileReadAccess(UserId: null));
            }

            if (currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadOwn.Code)
                && currentUser.UserId is { } userId)
            {
                return Result.Success(new FileReadAccess(userId));
            }
        }
        else if (type == FileType.Avatar
            && currentUser.HasPermission(Permissions.FileManager.Avatars.Read.Code))
        {
            return Result.Success(new FileReadAccess(UserId: null));
        }

        return Result.Failure<FileReadAccess>(Error.Forbidden("Access to this file type is denied."));
    }
}
