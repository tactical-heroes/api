using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Update;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Update;

[Mapper]
internal static partial class UpdateFileMapper
{
    internal static partial UpdateFileCommand ToCommand(UpdateFileRequest request, Guid id);
}
