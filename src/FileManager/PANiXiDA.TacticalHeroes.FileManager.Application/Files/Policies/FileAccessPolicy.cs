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
            && (file.Type == FileType.Avatar
                || access.Value.CanReadAllPersonalFiles
                || (access.Value.PersonalFilesUserId is { } userId && file.UserId?.Value == userId));
    }

    public Result<FileReadAccess> GetReadAccess(FileType? type = null)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Result.Failure<FileReadAccess>(Error.Unauthorized("Authentication is required."));
        }

        var includesPersonal = type is null || type == FileType.Personal;
        var canReadAllPersonal = includesPersonal
            && currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadAll.Code);
        var personalUserId = includesPersonal
            && !canReadAllPersonal
            && currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadOwn.Code)
                ? currentUser.UserId
                : null;
        var canReadAvatars = (type is null || type == FileType.Avatar)
            && currentUser.HasPermission(Permissions.FileManager.Avatars.Read.Code);

        if (!canReadAllPersonal && personalUserId is null && !canReadAvatars)
        {
            return Result.Failure<FileReadAccess>(Error.Forbidden("Access to files is denied."));
        }

        return Result.Success(new FileReadAccess(
            CanReadAllPersonalFiles: canReadAllPersonal,
            PersonalFilesUserId: personalUserId,
            CanReadAvatars: canReadAvatars));
    }
}
