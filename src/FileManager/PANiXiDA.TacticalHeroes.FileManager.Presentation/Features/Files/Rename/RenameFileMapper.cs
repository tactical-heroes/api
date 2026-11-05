using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Rename;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Rename;

[Mapper]
internal static partial class RenameFileMapper
{
    internal static partial RenameFileCommand ToCommand(RenameFileRequest request, Guid id);
}
