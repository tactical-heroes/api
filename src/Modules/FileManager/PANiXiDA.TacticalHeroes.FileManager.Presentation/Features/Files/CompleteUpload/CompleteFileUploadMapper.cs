using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.CompleteUpload;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.CompleteUpload;

[Mapper]
internal static partial class CompleteFileUploadMapper
{
    internal static partial CompleteFileUploadCommand ToCommand(Guid id);
}
