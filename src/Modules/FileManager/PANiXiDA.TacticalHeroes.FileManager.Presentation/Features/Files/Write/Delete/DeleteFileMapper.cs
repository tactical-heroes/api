using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Write.Delete;

[Mapper]
internal static partial class DeleteFileMapper
{
    internal static partial DeleteFileCommand ToCommand(Guid id);
}
