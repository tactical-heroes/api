using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Write.Update;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Write.Update;

[Mapper]
internal static partial class UpdateFolderMapper
{
    internal static partial UpdateFolderCommand ToCommand(UpdateFolderRequest request, Guid id);
}
