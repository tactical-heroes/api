using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.CompleteUpload;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Write.CompleteUpload;

[Mapper]
internal static partial class CompleteFileUploadMapper
{
    internal static partial CompleteFileUploadCommand ToCommand(Guid id);
}
