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
        return filter.Types.Count > 0
            && filter.Types.All(type => (filter.Type is null || type == filter.Type)
                && CanRead(type, filter.UserId));
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
