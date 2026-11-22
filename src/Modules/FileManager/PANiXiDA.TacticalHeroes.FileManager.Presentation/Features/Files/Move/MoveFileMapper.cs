using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Move;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.Move;

[Mapper]
internal static partial class MoveFileMapper
{
    internal static partial MoveFileCommand ToCommand(MoveFileRequest request, Guid id);
}
