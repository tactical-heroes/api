using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CompleteUpload;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CompleteUpload;

[Mapper]
internal static partial class CompleteFileUploadMapper
{
    internal static partial CompleteFileUploadCommand ToCommand(Guid id);
}
