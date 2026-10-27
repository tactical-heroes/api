using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;

public sealed class FileAccessPolicy(ICurrentUser currentUser)
{
    public bool CanRead(File file)
    {
        if (!currentUser.IsAuthenticated)
        {
            return false;
        }

        if (file.Type == FileType.Personal)
        {
            return currentUser.HasPermission(Permissions.FileManager.PersonalFiles.Read.Code)
                && file.UserId is { } ownerId
                && currentUser.UserId == ownerId.Value;
        }

        return file.Type == FileType.Avatar
            && currentUser.HasPermission(Permissions.FileManager.Avatars.Read.Code);
    }
}
