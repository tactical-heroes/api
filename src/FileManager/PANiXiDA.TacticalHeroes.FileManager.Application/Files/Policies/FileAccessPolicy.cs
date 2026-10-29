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

    public bool CanRead(FilesFilter filter)
    {
        return CanRead(filter.Type, filter.UserId);
    }

    public Result<FilesFilter> ApplyReadRestrictions(FilesFilter filter)
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
        if (filter.UserId is null
            && filter.Type != FileType.Avatar.Name
            && !CanRead(FileType.Personal.Name, userId: null)
            && CanRead(FileType.Personal.Name, userId))
        {
            filter = filter with { UserId = userId };
        }

        if (!CanRead(filter))
        {
            return Result.Failure<FilesFilter>(Error.Forbidden("Access to files is denied."));
        }

        if (filter.Type is null)
        {
            if (!CanRead(FileType.Personal.Name, filter.UserId))
            {
                filter = filter with { Type = FileType.Avatar.Name };
            }
            else if (!CanRead(FileType.Avatar.Name, userId: null))
            {
                filter = filter with { Type = FileType.Personal.Name };
            }
        }

        return Result.Success(filter);
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
                || (userId is null && CanRead(FileType.Avatar.Name, userId: null));
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
