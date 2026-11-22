using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.Update;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Write.Update;

[Mapper]
internal static partial class UpdateFileMapper
{
    internal static partial UpdateFileCommand ToCommand(UpdateFileRequest request, Guid id);
}
