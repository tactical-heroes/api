using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;

public sealed class FileAccessPolicy(ICurrentUser currentUser)
{
    public bool CanRead(File file)
    {
        return CanRead(file.Type.Name, file.UserId?.Value);
    }

    public Result<FilesFilter> AuthorizeRead(FilesFilter filter)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
        {
            return Result.Failure<FilesFilter>(Error.Unauthorized("A user account is required."));
        }

        var type = FileType.CreateOptional(filter.Type);
        if (type.IsFailure)
        {
            return Result.Failure<FilesFilter>(type.Errors);
        }

        filter = filter with { Type = type.Value?.Name };
        var includesPersonal = type.Value is null || type.Value == FileType.Personal;
        var includesAvatars = type.Value is null || type.Value == FileType.Avatar;
        var canReadAllPersonal = includesPersonal
            && currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadAll.Code);
        var canReadOwnPersonal = includesPersonal
            && !canReadAllPersonal
            && currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadOwn.Code);
        var canReadPersonal = canReadAllPersonal || canReadOwnPersonal;
        var canReadAvatars = includesAvatars
            && currentUser.HasPermission(Permissions.FileManager.Avatars.Read.Code);

        if (includesPersonal
            && !canReadAllPersonal
            && filter.UserId is { } ownerId
            && (!canReadOwnPersonal || ownerId != userId))
        {
            return Result.Failure<FilesFilter>(Error.Forbidden("Access to files is denied."));
        }

        if (!canReadPersonal && !canReadAvatars)
        {
            return Result.Failure<FilesFilter>(Error.Forbidden("Access to files is denied."));
        }

        return Result.Success(ApplyReadRestrictions(
            filter: filter,
            userId: userId,
            canReadOwnPersonal: canReadOwnPersonal,
            canReadPersonal: canReadPersonal,
            canReadAvatars: canReadAvatars));
    }

    private static FilesFilter ApplyReadRestrictions(
        FilesFilter filter,
        Guid userId,
        bool canReadOwnPersonal,
        bool canReadPersonal,
        bool canReadAvatars)
    {
        if (canReadOwnPersonal && filter.UserId is null)
        {
            filter = filter with { UserId = userId };
        }

        if (filter.Type is null)
        {
            if (!canReadPersonal)
            {
                filter = filter with { Type = FileType.Avatar.Name };
            }
            else if (!canReadAvatars)
            {
                filter = filter with { Type = FileType.Personal.Name };
            }
        }

        return filter;
    }

    private bool CanRead(string type, Guid? userId)
    {
        if (!currentUser.IsAuthenticated)
        {
            return false;
        }

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
