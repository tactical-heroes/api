using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Write.Create;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Write.Create;

[Mapper]
internal static partial class CreateFolderMapper
{
    internal static partial CreateFolderCommand ToCommand(CreateFolderRequest request);

    internal static partial CreateFolderResponse ToResponse(Guid id);
}
