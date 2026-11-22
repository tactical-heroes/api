using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Move;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Move;

[Mapper]
internal static partial class MoveFolderMapper
{
    internal static partial MoveFolderCommand ToCommand(MoveFolderRequest request, Guid id);
}
