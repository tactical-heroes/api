using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.Delete;

[Mapper]
internal static partial class DeleteFileMapper
{
    internal static partial DeleteFileCommand ToCommand(Guid id);
}
