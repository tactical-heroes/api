using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.Rename;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Write.Rename;

[Mapper]
internal static partial class RenameFileMapper
{
    internal static partial RenameFileCommand ToCommand(RenameFileRequest request, Guid id);
}
