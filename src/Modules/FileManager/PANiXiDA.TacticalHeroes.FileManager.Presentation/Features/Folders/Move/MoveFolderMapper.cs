using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Move;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Folders.Move;

[Mapper]
internal static partial class MoveFolderMapper
{
    internal static partial MoveFolderCommand ToCommand(MoveFolderRequest request, Guid id);
}
