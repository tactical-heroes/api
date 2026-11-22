using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;

public sealed class FileAccessPolicy(ICurrentUser currentUser)
{
    public bool CanCreate(File file)
    {
        if (!currentUser.IsAuthenticated)
        {
            return false;
        }

        if (file.Type == FileType.Personal)
        {
            return currentUser.HasPermission(Permissions.FileManager.PersonalFiles.CreateAll.Code)
                || (file.UserId?.Value == currentUser.UserId
                    && currentUser.HasPermission(Permissions.FileManager.PersonalFiles.CreateOwn.Code));
        }

        return file.Type == FileType.Avatar
            && currentUser.HasPermission(Permissions.FileManager.Avatars.Create.Code);
    }

    public bool CanRead(File file)
    {
        return CanRead(file.Type.Name, file.UserId?.Value);
    }

    public bool CanRead(FilesFilter filter)
    {
        return CanRead(filter.Type, filter.UserId);
    }

    public bool CanUpdate(File file)
    {
        return CanUpdate(file.Type, file.UserId);
    }

    public bool CanUpdate(FileType type, UserId? userId)
    {
        if (!currentUser.IsAuthenticated)
        {
            return false;
        }

        if (type == FileType.Personal)
        {
            return currentUser.HasPermission(Permissions.FileManager.PersonalFiles.UpdateAll.Code)
                || (userId?.Value == currentUser.UserId
                    && currentUser.HasPermission(Permissions.FileManager.PersonalFiles.UpdateOwn.Code));
        }

        return type == FileType.Avatar
            && currentUser.HasPermission(Permissions.FileManager.Avatars.Update.Code);
    }

    public bool CanDelete(File file)
    {
        if (!currentUser.IsAuthenticated)
        {
            return false;
        }

        if (file.Type == FileType.Personal)
        {
            return currentUser.HasPermission(Permissions.FileManager.PersonalFiles.DeleteAll.Code)
                || (file.UserId?.Value == currentUser.UserId
                    && currentUser.HasPermission(Permissions.FileManager.PersonalFiles.DeleteOwn.Code));
        }

        return file.Type == FileType.Avatar
            && currentUser.HasPermission(Permissions.FileManager.Avatars.Delete.Code);
    }

    public Result<FilesFilter> ApplyReadRestrictions(FilesFilter filter)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
        {
            return Result.Failure<FilesFilter>(Error.Unauthorized("A user account is required."));
        }

        if (!CanRead(filter) && filter.UserId is null)
        {
            filter = filter with { UserId = userId };
        }

        return CanRead(filter)
            ? Result.Success(filter)
            : Result.Failure<FilesFilter>(Error.Forbidden("Access to files is denied."));
    }

    private bool CanRead(string? type, Guid? userId)
    {
        if (!currentUser.IsAuthenticated)
        {
            return false;
        }

        if (type is null)
        {
            return CanRead(FileType.Personal.Name, userId)
                && CanRead(FileType.Avatar.Name, userId: null);
        }

        type = type.Trim();

        if (type == FileType.Personal.Name)
        {
            return currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadAll.Code)
                || (userId is { } ownerId
                    && ownerId == currentUser.UserId
                    && currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadOwn.Code));
        }

        return type == FileType.Avatar.Name
            && currentUser.HasPermission(Permissions.FileManager.Avatars.Read.Code);
    }
}
