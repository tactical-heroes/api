using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Delete;

[Mapper]
internal static partial class DeleteFileMapper
{
    internal static partial DeleteFileCommand ToCommand(Guid id);
}
