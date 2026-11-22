using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Rename;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.Rename;

[Mapper]
internal static partial class RenameFileMapper
{
    internal static partial RenameFileCommand ToCommand(RenameFileRequest request, Guid id);
}
