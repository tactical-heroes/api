using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Create;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.CreatePersonal;

[Mapper]
internal static partial class CreatePersonalFolderMapper
{
    [MapValue(nameof(CreateFolderCommand.AllowedFileType), Use = nameof(GetPersonalFileType))]
    internal static partial CreateFolderCommand ToCommand(CreatePersonalFolderRequest request, Guid userId);

    internal static partial CreatePersonalFolderResponse ToResponse(Guid id);

    private static string GetPersonalFileType() => CreateFolderCommand.PersonalFileType;
}
