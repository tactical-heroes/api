using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Update;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.Update;

[Mapper]
internal static partial class UpdateFileMapper
{
    internal static partial UpdateFileCommand ToCommand(UpdateFileRequest request, Guid id);
}
