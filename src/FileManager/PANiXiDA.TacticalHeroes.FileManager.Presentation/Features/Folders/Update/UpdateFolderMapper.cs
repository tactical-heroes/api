using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Update;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Update;

[Mapper]
internal static partial class UpdateFolderMapper
{
    internal static partial UpdateFolderCommand ToCommand(UpdateFolderRequest request, Guid id);
}
