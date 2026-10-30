using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Create;

public sealed record CreateFolderCommand(
    string Name,
    string AllowedFileType,
    Guid? UserId,
    Guid? ParentId) : ICommand<Result<Guid>>
{
    public static string PersonalFileType => FileType.Personal.Name;
}
