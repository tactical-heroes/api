using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Write.Move;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Write.Move;

[Mapper]
internal static partial class MoveFolderMapper
{
    internal static partial MoveFolderCommand ToCommand(MoveFolderRequest request, Guid id);
}
