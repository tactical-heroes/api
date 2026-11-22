using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Create;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Folders.Create;

[Mapper]
internal static partial class CreateFolderMapper
{
    internal static partial CreateFolderCommand ToCommand(CreateFolderRequest request);

    internal static partial CreateFolderResponse ToResponse(Guid id);
}
