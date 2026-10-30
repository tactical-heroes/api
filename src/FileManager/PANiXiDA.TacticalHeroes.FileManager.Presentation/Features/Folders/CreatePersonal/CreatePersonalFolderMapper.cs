using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.CreatePersonal;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.CreatePersonal;

[Mapper]
internal static partial class CreatePersonalFolderMapper
{
    internal static partial CreatePersonalFolderCommand ToCommand(CreatePersonalFolderRequest request);

    internal static partial CreatePersonalFolderResponse ToResponse(Guid id);
}
