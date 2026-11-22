using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.Move;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Write.Move;

[Mapper]
internal static partial class MoveFileMapper
{
    internal static partial MoveFileCommand ToCommand(MoveFileRequest request, Guid id);
}
